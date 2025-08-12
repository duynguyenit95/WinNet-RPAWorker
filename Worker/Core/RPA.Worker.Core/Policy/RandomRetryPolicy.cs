using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace RPA.Worker.Core
{
    public class RandomRetryPolicy : IRetryPolicy
    {
        private readonly Random _random = new Random();
        private readonly int _min;
        private readonly int _max;
        public RandomRetryPolicy(int min, int max)
        {
            _min = min;
            _max = max;
        }
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            // wait between 0 and 60 seconds before the next reconnect attempt.
            return TimeSpan.FromSeconds(_random.Next(_min, _max));
        }
    }
}
