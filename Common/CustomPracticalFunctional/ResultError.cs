using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.CustomPracticalFunctional
{
    public class ResultError
    {
        public string Message { get; }
        public Exception? Exception { get; }
        public string? Code { get; }

        public bool HasException => Exception is not null;

        public ResultError(string message, Exception? exception = null, string? code = null)
        {
            Message = message;
            Exception = exception;
            Code = code;
        }

        public override string ToString() =>
            Code is null ? Message : $"{Code}: {Message}";
    }
}