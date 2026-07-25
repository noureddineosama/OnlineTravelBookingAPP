using Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class CachService<T> : ICachService<T>
    {
        private readonly IMemoryCache cach;
        private readonly ICacheInvalidationService cacheInvalidation;

        public CachService(IMemoryCache cach, ICacheInvalidationService  cacheInvalidation)
        {
            this.cach = cach;
            this.cacheInvalidation = cacheInvalidation;
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

        public Task SetUserIdScopedAsync(string key,long userId ,T data, CancellationToken cancellationToken = default)
        {
            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(
                new CancellationChangeToken(
                cacheInvalidation.GetToken(userId, cancellationToken)));

           cach.Set(key,
                 data, options);
            return Task.CompletedTask;
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            cach.Remove(key);
        }
    }
}
