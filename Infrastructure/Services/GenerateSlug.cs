using Application.Common.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class GenerateSlug : IGenerateSlug
    {
        public string generateSlug(hotel hotel, CancellationToken cancellationToken = default)
        {
            return hotel.name
                .Trim()
                .ToLower()
                .Replace(" ", "-");
        }
    }
}
