using System;
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

    public class SqdcHttpWatcher : ISqdcWatcher
    {
        private readonly ILogger<SqdcHttpWatcher> logger;
        private readonly TimeSpan loopInterval = TimeSpan.FromMinutes(6);
        private readonly Func<IScanOperation> scanOperationFactory;
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ManualResetEventSlim resumeLoopEvent;
        
        private volatile bool isFullRefreshRequested;
        private Stopwatch loopStopwatch;

        public SqdcHttpWatcher(
            ILogger<SqdcHttpWatcher> logger,
            Func<IScanOperation> scanOperationFactory,
            IHostApplicationLifetime applicationLifetime)
        {
            this.logger = logger;
            this.scanOperationFactory = scanOperationFactory;
            this.applicationLifetime = applicationLifetime;
            resumeLoopEvent = new ManualResetEventSlim(true);
        }

        public WatcherState State { get; private set; }

        public Task Start(CancellationToken cancellationToken)
        {
            SetStartedState();
            
            return Task.Run(async () => await TryLoop(cancellationToken), CancellationToken.None)
                .ContinueWith(t => StopWorker(), CancellationToken.None);
        }

        private void SetStartedState()
        {
            if (State == WatcherState.Running)
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

        private void LogLoopIterationCompleted()
        {
            DateTime nextExecutionTime = DateTime.Now + loopInterval;
            logger.LogInformation(
                $"Scan completed in {loopStopwatch.Elapsed.ToSmartFormat()}. Next execution: {nextExecutionTime:H\\hmm}");
        }

        private void PrepareLoopIteration()
        {
            loopStopwatch = Stopwatch.StartNew();
        }

        private void StopWorker()
        {
            State = WatcherState.Stopped;
            logger.LogInformation("SqdcWatcher stopped");
        }

        private void HandleLoopException(Exception exception)
        {
            logger.LogError(exception, exception.Message);

            logger.LogInformation("Stopping the application because of an error");
            applicationLifetime.StopApplication();
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

        private async Task ExecuteScan(CancellationToken cancelToken)
        {
            logger.LogInformation("--- Watcher - Refresh products started ---");
            IScanOperation scanOperation = scanOperationFactory.Invoke();
            await scanOperation.Execute(isFullRefreshRequested, cancelToken);
        }

        public void RequestRefresh(bool forceFullRefresh = false)
        {
            isFullRefreshRequested = forceFullRefresh;
            resumeLoopEvent.Set();
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