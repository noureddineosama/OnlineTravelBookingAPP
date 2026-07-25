using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface ICacheInvalidationService
    {
        CancellationToken GetToken(long userId, CancellationToken cancellationToken);
        void Invalidate (long userId, CancellationToken cancellationToken);
    }
}
