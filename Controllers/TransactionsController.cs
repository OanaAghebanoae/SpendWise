using Microsoft.AspNetCore.Mvc;
using SpendWise.DTOs;
using SpendWise.Services;

namespace SpendWise.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController(TransactionService transactions, ImportService imports) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> Get([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] Guid? categoryId, [FromQuery] string? search, CancellationToken cancellationToken)
        => Ok(await transactions.SearchAsync(new TransactionFilter(from, to, categoryId, search), cancellationToken));

    [HttpPatch("{id:guid}/category")]
    public async Task<IActionResult> AssignCategory(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        => await transactions.AssignCategoryAsync(id, request.CategoryId, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("import")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<ImportResultDto>> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest("A non-empty CSV file is required.");
        await using var stream = file.OpenReadStream();
        return Ok(await imports.ImportAsync(file.FileName, stream, cancellationToken));
    }
}
