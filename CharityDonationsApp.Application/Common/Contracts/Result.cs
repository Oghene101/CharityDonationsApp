namespace CharityDonationsApp.Application.Common.Contracts;

public class Result
{
    protected Result(bool isSuccess, Error[] errors)
    {
        switch (isSuccess)
        {
            case true when errors.Length != 0:
                throw new InvalidOperationException("cannot be successful with error");
            case false when errors.Length == 0:
                throw new InvalidOperationException("cannot be unsuccessful without error");
        }

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public readonly bool IsSuccess;
    public readonly Error[] Errors;

    public static Result Success() => new(true, Error.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, [error]);
    public static Result Failure(Error[] errors) => new(false, errors);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, [error]);
    public static Result<TValue> Failure<TValue>(Error[] errors) => new(default, false, errors);

    public static implicit operator Result(Error error) => Failure(error);
    public static implicit operator Result(Error[] errors) => Failure(errors);

    public void Deconstruct(out bool isSuccess, out Error[] errors)
    {
        isSuccess = IsSuccess;
        errors = Errors;
    }
}

public class Result<TValue>(TValue value, bool isSuccess, Error[] errors) : Result(isSuccess, errors)

{
    private readonly TValue _value = isSuccess switch
    {
        true when value is null => throw new InvalidOperationException("Successful result must have a value"),
        false when value is not null => throw new InvalidOperationException("Failed result must not have a value"),
        _ => value
    };

    public TValue Value =>
        IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of a failed result");

    public static implicit operator Result<TValue>(TValue value) => Success(value);

    public static implicit operator TValue(Result<TValue> result)
    {
        if (result.IsSuccess) return result.Value;

        throw new InvalidOperationException("Cannot convert a failed result to a value.");
    }

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    public static implicit operator Result<TValue>(Error[] errors) => Failure<TValue>(errors);

    public void Deconstruct(out bool isSuccess, out TValue value, out Error[] errors)
    {
        isSuccess = IsSuccess;
        value = _value;
        errors = Errors;
    }
}