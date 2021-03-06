using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using XFactory.SqdcWatcher.Core.Interfaces;
using XFactory.SqdcWatcher.Core.Services;

namespace SqdcWatcher.Core.Tests.Services
{
    public class SqdcHttpWatcherTest
    {
        private Mock<IHostApplicationLifetime> applicationLifetimeMock;
        private ILogger<MarketsWatcher> logger;
        private Mock<IEnumerable<Func<IScanOperation>>> scanOperationFactoryMock;
        private MarketsWatcher watcher;

        [SetUp]
        public void SetUp()
        {
            logger = new Mock<ILogger<MarketsWatcher>>(MockBehavior.Loose).Object;
            scanOperationFactoryMock = new Mock<IEnumerable<Func<IScanOperation>>>();
            applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            watcher = new MarketsWatcher(
                logger,
                scanOperationFactoryMock.Object,
                applicationLifetimeMock.Object);
        }

        [Test]
        public void NotStarted_IsIdle()
        {
            Assert.AreEqual(WatcherState.Idle, watcher.State);
        }

        [Test]
        public void CanStart()
        {
            watcher.Start(CancellationToken.None);
            Assert.AreEqual(WatcherState.Running, watcher.State);
        }

        [Test]
        public void CanBeCancelled()
        {
            var cts = new CancellationTokenSource();
            Task watcherTask = watcher.Start(cts.Token);

            cts.Cancel();
            Task.WaitAny(new[] {watcherTask}, 50);

            Assert.AreEqual(watcher.State, WatcherState.Stopped);
        }
    }
}