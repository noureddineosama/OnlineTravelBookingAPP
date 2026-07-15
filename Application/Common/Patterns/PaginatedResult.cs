using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Patterns
{
    public class PaginatedResult<T> 
    {
        public bool IsSuccess { get; set; } = true;
        public string? message { get; set; }
        public List<T> Data { get; set; }
        public PaginationMetadata pagination { get; set; }
        public int? TotalCount { get; set; }
    }
}
