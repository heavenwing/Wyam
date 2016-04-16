using System;
using System.IO;
using System.Threading;
using Wyam.Common.IO;

namespace Wyam.Core.IO.Local
{
    public class LocalFileProvider : IFileProvider
    {
        public IFile GetFile(FilePath path)
        {
            return new LocalFile(path);
        }

        public IDirectory GetDirectory(DirectoryPath path)
        {
            return new LocalDirectory(path);
        }

        // *** Retry logic (used by LocalFile and LocalDirectory)

        private static readonly TimeSpan InitialInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan IntervalDelta = TimeSpan.FromMilliseconds(100);

        private const int RetryCount = 3;

        internal static T Retry<T>(Func<T> func)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    TimeSpan? interval = ShouldRetry(retryCount, ex);
                    if (!interval.HasValue)
                    {
                        throw;
                    }
                    Thread.Sleep(interval.Value);
                }
                retryCount++;
            }
        }

        internal static void Retry(Action action)
        {
            Retry<object>(() =>
            {
                action();
                return null;
            });
        }

        private static TimeSpan? ShouldRetry(int retryCount, Exception exception) =>
            (exception is IOException || exception is UnauthorizedAccessException) && retryCount < RetryCount
                ? (TimeSpan?)InitialInterval.Add(TimeSpan.FromMilliseconds(IntervalDelta.TotalMilliseconds * retryCount)) : null;
    }
}