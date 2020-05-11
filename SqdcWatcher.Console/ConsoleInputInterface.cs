using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using XFactory.SqdcWatcher.Core.Interfaces;

namespace XFactory.SqdcWatcher.ConsoleApp
{
    [UsedImplicitly]
    public class ConsoleInputInterface
    {
        private readonly ISqdcWatcher watcher;

        private Dictionary<ConsoleKeyInfo, Action> keyBindingsMap;
        public ConsoleInputInterface(ISqdcWatcher watcher, ILogger<ConsoleInputInterface> logger)
        {
            this.watcher = watcher;
            ConfigureKeyBindings();
        }

        private void ConfigureKeyBindings()
        {
            keyBindingsMap = new Dictionary<ConsoleKeyInfo, Action>
            {
                {new ConsoleKeyInfo((char) 0, ConsoleKey.F5, false, false, false), () => watcher.RequestRefresh()},
                {new ConsoleKeyInfo((char) 0, ConsoleKey.F6, false, false, false), () => watcher.RequestRefresh(true)}
            };
        }

        public async Task StartReadingConsoleKeysAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConsoleKeyInfo? key = await TryReadKeyAfter(100, cancellationToken);
                if (key != null)
                {
                    InvokeActionIfBound(key.Value);
                }
            }
        }

        private static async Task<ConsoleKeyInfo?> TryReadKeyAfter(int timeoutMilliseconds, CancellationToken cancellationToken)
        {
            await Task.Delay(timeoutMilliseconds, cancellationToken);
            return Console.KeyAvailable ? Console.ReadKey() : new ConsoleKeyInfo?();
        }

        private void InvokeActionIfBound(in ConsoleKeyInfo key)
        {
            if (keyBindingsMap.TryGetValue(key, out Action action))
            {
                action.Invoke();
            }
        }
    }
}