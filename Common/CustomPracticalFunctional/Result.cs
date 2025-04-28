namespace Common.CustomPracticalFunctional
{
    public class Result<TValue>
    {
        public TValue? Value { get; }
        public ResultError? Error { get; }
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        private Result(TValue value)
        {
            Value = value;
            IsSuccess = true;
            Error = null;
        }
        private Result(ResultError error)
        {
            Value = default;
            IsSuccess = false;
            Error = error;
        }
#pragma warning disable CA1000 // Do not declare static members on generic types
        public static Result<TValue> Success(TValue value) => new(value);
        public static Result<TValue> Failure(ResultError error) => new(error);
#pragma warning restore CA1000 // Do not declare static members on generic types

#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(ResultError error) => new(error);
#pragma warning restore CA2225 // Operator overloads have named alternates
    }

}