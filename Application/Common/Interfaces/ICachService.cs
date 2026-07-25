using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICachService<T>
    {
      Task<T?> GetAsync(string key, CancellationToken cancellationToken);
      Task SetAsync(string key, T data, CancellationToken cancellationToken = default);
      Task SetUserIdScopedAsync(string key, long userId, T data, CancellationToken cancellationToken = default);
      Task RemoveAsync(string key, CancellationToken cancellationToken);
    }
}
