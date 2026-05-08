namespace FinCore.SharedKernel.Results;

public sealed class UseCaseResult<T>
{
    public T? Value { get; private init; }
    public ResultCategory Category { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public bool IsSuccess => Category == ResultCategory.Success;

    private UseCaseResult() { }

    public static UseCaseResult<T> Success(T value) =>
        new() { Value = value, Category = ResultCategory.Success };

    public static UseCaseResult<T> NotFound(string message) =>
        new() { Category = ResultCategory.NotFound, Errors = [message] };

    public static UseCaseResult<T> ValidationFailure(IEnumerable<string> errors) =>
        new() { Category = ResultCategory.Validation, Errors = errors.ToList().AsReadOnly() };

    public static UseCaseResult<T> Unauthorized(string message) =>
        new() { Category = ResultCategory.Unauthorized, Errors = [message] };

    public static UseCaseResult<T> InternalError(string message) =>
        new() { Category = ResultCategory.InternalError, Errors = [message] };
}
