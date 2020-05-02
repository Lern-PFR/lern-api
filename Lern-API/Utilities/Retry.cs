﻿using System;
using System.Collections.Generic;
using System.Threading;
using NullGuard;

namespace Lern_API.Utilities
{
    public static class Retry
    {
        public static void Do(Action action, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, maxAttemptCount);
        }

        [return: AllowNull]
        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int maxAttemptCount = 3)
        {
            if (maxAttemptCount < 1)
                throw new ArgumentException(nameof(maxAttemptCount));

            var exceptions = new List<Exception>();

            for (var attempted = 0; attempted < maxAttemptCount; attempted++)
            {
                try
                {
                    if (attempted > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }

                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
