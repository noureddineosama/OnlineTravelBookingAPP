using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Patterns
{
    public class Result
    { 
        public static async Task<GenericResult<T>> SuccessAsync<T>(T data, string message = "")
        {
            return await Task.FromResult(new GenericResult<T>
            {
                IsSuccess = true,
                message = message,
                Data = data
            });
        }

        public static async Task<GenericResult<T>> FailureAsync<T>(string message)
        {
            return await Task.FromResult(new GenericResult<T>
            {
                IsSuccess = false,
                message = message,
                Data = default
            });
        }
    }
}
