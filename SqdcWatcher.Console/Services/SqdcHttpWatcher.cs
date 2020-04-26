#region

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqdcWatcher.DataAccess.EntityFramework;
using SqdcWatcher.Utils;

#endregion

namespace SqdcWatcher.Services
{
    public enum WatcherState
    {
        Idle = 0,
        Running,
        Stopped
    }

    public class SqdcHttpWatcher : ISqdcWatcher
    {
        private readonly ILogger<SqdcHttpWatcher> logger;
        private readonly TimeSpan loopInterval = TimeSpan.FromMinutes(6);
        private readonly SqdcProductsFetcher productsFetcher;
        private readonly ProductsPersister productsPersister;
        private readonly Func<IScanOperation> scanOperationFactory;
        private readonly SlackPostWebHookClient slackPostClient;
        private readonly ISqdcDataAccess sqdcDataAccess;
        private bool isFullRefreshRequested;
        private bool isRefreshInProgress;

        private bool isRefreshRequested;

        public SqdcHttpWatcher(
            ILogger<SqdcHttpWatcher> logger,
            SqdcProductsFetcher productsFetcher,
            ProductsPersister productsPersister,
            ISqdcDataAccess sqdcDataAccess,
            SlackPostWebHookClient slackPostClient,
            Func<IScanOperation> scanOperationFactory)
        {
            this.logger = logger;
            this.productsFetcher = productsFetcher;
            this.productsPersister = productsPersister;
            this.sqdcDataAccess = sqdcDataAccess;
            this.slackPostClient = slackPostClient;
            this.scanOperationFactory = scanOperationFactory;
        }

        public WatcherState State { get; private set; }

        public void Start(CancellationToken cancellationToken)
        {
            if (State == WatcherState.Running)
            {
                throw new WatcherStartException("cannot start watcher, it is already running.");
            }

            State = WatcherState.Running;
            Task.Run(async () =>
            {
                try
                {
                    await Loop(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    logger.LogError(e, e.Message);
                }
                finally
                {
                    State = WatcherState.Stopped;
                }
            }, CancellationToken.None);
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
                catch (Exception e)
                {
                    logger.LogError(e, "Error while executing scan within the watcher loop");
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