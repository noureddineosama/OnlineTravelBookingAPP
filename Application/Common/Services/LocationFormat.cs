using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Services
{
    public static class LocationFormat
    {
       public static string FormatLocation(location? loc)
        {
            if (loc == null) return string.Empty;
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(loc.city)) parts.Add(loc.city);
            if (!string.IsNullOrEmpty(loc.country)) parts.Add(loc.country);
            return string.Join(", ", parts);
        }
    }
}
