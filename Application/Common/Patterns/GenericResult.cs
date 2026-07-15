using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Patterns
{
    public class GenericResult<T> 
    {
        public bool IsSuccess { get; set; }
        public string message { get; set; }
        public T? Data { get; set; }
    }
}
