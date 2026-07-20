using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class CachService<T> : ICachService<T>
    {
        private readonly IMemoryCache cach;

        public CachService(IMemoryCache cach)
        {
            this.cach = cach;
        }
        public Task<T?> GetAsync(string key, CancellationToken cancellationToken)
        {
            cach.TryGetValue<T>(key, out T? value);
            return Task.FromResult(value);
        }

        public Task SetAsync(string key, T data, CancellationToken cancellationToken = default)
        {
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            cach.Set(key,
                 data, options);
            return Task.CompletedTask;
        }

        public async Task RemoveASync(string key, CancellationToken cancellationToken = default)
        {
            cach.Remove(key);
        }
    }
}
