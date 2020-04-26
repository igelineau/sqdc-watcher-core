#region

using System;

#endregion

namespace XFactory.SqdcWatcher.ConsoleApp.Utils
{
    public static class ConditionalDisposeWrapper
    {
        public static DisposeIfNullWrapper<T> Create<T>(T value, Func<T> defaultValueFactory) where T : IDisposable
        {
            return new DisposeIfNullWrapper<T>(value, defaultValueFactory);
        }
    }

    /// <summary>
    ///     Wrapper around a potentially null IDisposable object.  A factory method is used to generate a default value if the object is null.
    ///     The underlying object will only be disposed if the default was used.
    /// </summary>
    public class DisposeIfNullWrapper<T> : IDisposable where T : IDisposable
    {
        public DisposeIfNullWrapper(T value, Func<T> defaultValueFactory)
        {
            WillBeDisposed = value == null;
            Object = value ?? defaultValueFactory.Invoke();
        }

        public T Object { get; }
        private bool WillBeDisposed { get; }

        public void Dispose()
        {
            if (WillBeDisposed)
            {
                Object.Dispose();
            }
        }
    }
}