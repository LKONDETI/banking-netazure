using FinCore.Application.Accounts.Queries.ListAccounts;
using FinCore.Core.Enums;
using FinCore.Web.Controllers.Base;
using FinCore.Web.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinCore.Web.Controllers;

/// <summary>
/// Handles account-related API endpoints.
/// </summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public sealed class AccountsController : FinCoreApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns all accounts matching the given AccountTypes bitmask filter.
    /// </summary>
    /// <param name="id">
    /// Numeric bitmask for <see cref="AccountTypes"/> flags.
    /// Default 15 = Deposits | Loans | FromAccounts | ToAccounts.
    /// Pass 0 to get a 422 (None is rejected by FluentValidation).
    /// </param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:int?}")]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAccounts(
        [FromRoute] int id = 15,
        CancellationToken cancellationToken = default)
    {
        var filter = (AccountTypes)id;

        _logger.LogDebug("GET /api/accounts/{Id} → filter={Filter}", id, filter);

        var query = new ListAccountsQuery(filter);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result, listResult =>
        {
            var responses = listResult.Accounts
                .Select(AccountResponse.FromDto)
                .ToList();

            return Ok(responses);
        });
    }
}
