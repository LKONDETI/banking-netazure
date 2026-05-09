using FinCore.SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Web.Controllers.Base;

/// <summary>
/// Base controller that provides a uniform <see cref="ToActionResult{T}"/> helper
/// converting a <see cref="UseCaseResult{T}"/> into the appropriate HTTP response.
/// </summary>
[ApiController]
public abstract class FinCoreApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(UseCaseResult<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is not null
                ? onSuccess(result.Value!)
                : Ok(result.Value);
        }

        return result.Category switch
        {
            ResultCategory.NotFound => NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = string.Join("; ", result.Errors),
                Status = StatusCodes.Status404NotFound
            }),
            ResultCategory.Validation => UnprocessableEntity(new ValidationProblemDetails
            {
                Title = "Validation Failed",
                Detail = string.Join("; ", result.Errors),
                Status = StatusCodes.Status422UnprocessableEntity,
                Errors = { ["Filter"] = result.Errors.ToArray() }
            }),
            ResultCategory.Unauthorized => Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = string.Join("; ", result.Errors),
                Status = StatusCodes.Status401Unauthorized
            }),
            ResultCategory.Conflict => Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = string.Join("; ", result.Errors),
                Status = StatusCodes.Status409Conflict
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError
            })
        };
    }
}
