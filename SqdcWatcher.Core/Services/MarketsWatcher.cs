using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XFactory.SqdcWatcher.Core.Exceptions;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Utils;

namespace XFactory.SqdcWatcher.Core.Services
{
    public enum WatcherState
    {
        [UsedImplicitly] Idle = 0,
        Running,
        Stopped
    }

    public class MarketsWatcher : ISqdcWatcher
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ILogger<MarketsWatcher> logger;
        private readonly IEnumerable<Func<IScanOperation>> scanOperationsFactories;
        private readonly TimeSpan loopInterval = TimeSpan.FromMinutes(6);

        private readonly ManualResetEventSlim resumeLoopEvent;
        private readonly Mutex runningMutex;

        private volatile bool isFullRefreshRequested;
        private Stopwatch loopStopwatch;

        public MarketsWatcher(
            ILogger<MarketsWatcher> logger,
            IEnumerable<Func<IScanOperation>> scanOperationsFactories,
            IHostApplicationLifetime applicationLifetime)
        {
            this.logger = logger;
            this.scanOperationsFactories = scanOperationsFactories;
            this.applicationLifetime = applicationLifetime;
            resumeLoopEvent = new ManualResetEventSlim(true);
            runningMutex = new Mutex(false);
        }

        public WatcherState State { get; private set; }

        public Task Start(CancellationToken cancellationToken)
        {
            SetStartedState();

            return Task.Run(async () => await TryLoop(cancellationToken), CancellationToken.None)
                .ContinueWith(t => StopWorker(), CancellationToken.None);
        }

        public void RequestRefresh(bool forceFullRefresh = false)
        {
            isFullRefreshRequested = forceFullRefresh;
            resumeLoopEvent.Set();
        }

        private void SetStartedState()
        {
            if (!runningMutex.WaitOne(TimeSpan.Zero))
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            State = WatcherState.Running;
        }

        private async Task TryLoop(CancellationToken cancellationToken)
        {
            try
            {
                await Loop(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Let the thread end normally
            }
            catch (Exception e)
            {
                HandleLoopException(e);
            }
        }

        private async Task Loop(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                PrepareLoopIteration();
                await TryExecuteScan(cancelToken);
                LogLoopIterationCompleted();
                BlockFor(loopInterval, cancelToken);
            }
        }

        private void PrepareLoopIteration()
        {
            loopStopwatch = Stopwatch.StartNew();
        }

        private async Task TryExecuteScan(CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteScan(cancellationToken);
                ResetManualRefreshState();
            }
            catch (Exception e) when (!(e is OperationCanceledException))
            {
                logger.LogError(e, "Error while executing scan within the watcher loop");
                throw;
            }
        }

        private void LogLoopIterationCompleted()
        {
            DateTime nextExecutionTime = DateTime.Now + loopInterval;
            logger.LogInformation(
                $"Scan completed in {loopStopwatch.Elapsed.ToSmartFormat()}. Next execution: {nextExecutionTime:H\\hmm}");
        }

        private async Task ExecuteScan(CancellationToken cancelToken)
        {
            logger.LogInformation("--- Watcher - Refresh products started ---");
            foreach (Func<IScanOperation> factory in scanOperationsFactories)
            {
                IScanOperation scanOperation = factory.Invoke();
                await scanOperation.Execute(isFullRefreshRequested, cancelToken);
            }
        }

        private void StopWorker()
        {
            State = WatcherState.Stopped;
            runningMutex.ReleaseMutex();
            logger.LogInformation("SqdcWatcher stopped");
        }

        private void HandleLoopException(Exception exception)
        {
            logger.LogError(exception, exception.Message);
            StopWorker();

            logger.LogInformation("Stopping the application because of an error");
            applicationLifetime.StopApplication();
        }

        private void BlockFor(TimeSpan timeToBlock, CancellationToken cancelToken)
        {
            resumeLoopEvent.Reset();
            resumeLoopEvent.Wait(timeToBlock, cancelToken);
        }

        private void ResetManualRefreshState()
        {
            isFullRefreshRequested = false;
        }
    }
}