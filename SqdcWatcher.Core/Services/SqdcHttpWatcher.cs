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
        
        private volatile bool isFullRefreshRequested;
        private volatile bool isRefreshInProgress;
        private volatile bool isRefreshRequested;

        public SqdcHttpWatcher(
            ILogger<SqdcHttpWatcher> logger,
            Func<IScanOperation> scanOperationFactory,
            IHostApplicationLifetime applicationLifetime)
        {
            this.logger = logger;
            this.scanOperationFactory = scanOperationFactory;
            this.applicationLifetime = applicationLifetime;
        }

        public WatcherState State { get; private set; }

        public Task Start(CancellationToken cancellationToken)
        {
            if (State == WatcherState.Running)
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            State = WatcherState.Running;
            return Task.Run(async () =>
            {
                try
                {
                    await Loop(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Let the thread end normally
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    HandleLoopException(e);
                }
                finally
                {
                    StopWorker();
                }
            }, CancellationToken.None);
        }

        private async Task Loop(CancellationToken cancelToken)
        {
            isRefreshInProgress = true;

            while (!cancelToken.IsCancellationRequested)
            {
                logger.LogInformation("--- Watcher - Refresh products started ---");

                Stopwatch sw = Stopwatch.StartNew();

                try
                {
                    await ExecuteScan(cancelToken);
                }
                catch (Exception e) when (!(e is OperationCanceledException))
                {
                    logger.LogError(e, "Error while executing scan within the watcher loop");
                    throw;
                }
                finally
                {
                    isFullRefreshRequested = false;
                }

                DateTime nextExecutionTime = DateTime.Now + loopInterval;
                logger.LogInformation(
                    $"Scan completed in {sw.Elapsed.ToSmartFormat()}. Next execution: {nextExecutionTime:H\\hmm}");
                await SleepChunksAsync(loopInterval, cancelToken);
            }
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

        public void RequestRefresh(bool fullRefresh = false)
        {
            if (isRefreshInProgress)
            {
                logger.LogInformation("Ignoring a manual refresh request made while a refresh is already in progress.");
            }
            else
            {
                isRefreshRequested = true;
                isFullRefreshRequested = fullRefresh;
            }
        }

        private async Task ExecuteScan(CancellationToken cancelToken)
        {
            IScanOperation scanOperation = scanOperationFactory.Invoke();
            await scanOperation.Execute(isFullRefreshRequested, cancelToken);
        }

        private async Task SleepChunksAsync(TimeSpan totalTime, CancellationToken cancelToken)
        {
            isRefreshInProgress = false;
            isRefreshRequested = false;

            TimeSpan totalWaited = TimeSpan.Zero;
            TimeSpan chunkDuration = TimeSpan.FromMilliseconds(500);

            while (totalWaited < totalTime && !isRefreshRequested && !cancelToken.IsCancellationRequested)
            {
                await Task.Delay(chunkDuration, cancelToken).ConfigureAwait(false);
                totalWaited += chunkDuration;
            }

            if (isRefreshRequested)
            {
                logger.LogInformation("Manual refresh was requested, proceeding...");
                isRefreshRequested = false;
            }
        }
    }
}