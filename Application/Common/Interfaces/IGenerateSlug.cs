using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IGenerateSlug
    {
        string generateSlug(hotel hotel, CancellationToken cancellationToken = default);
    }
}
