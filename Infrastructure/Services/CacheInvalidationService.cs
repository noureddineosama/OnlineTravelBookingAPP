using Application.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public sealed class CacheInvalidationService : ICacheInvalidationService
    {

        //. key --> userId, TokenSorce --> as a value
        private readonly ConcurrentDictionary<long, CancellationTokenSource> _tokens = new();

        public CancellationToken GetToken(long userId, CancellationToken cancellationToken)
        {
            return _tokens.GetOrAdd(userId, _ => new CancellationTokenSource()).Token;
        }

        public void Invalidate(long userId, CancellationToken cancellationToken)
        {
            if (_tokens.TryRemove(userId, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
    }
}
