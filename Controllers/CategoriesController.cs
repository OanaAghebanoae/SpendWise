using Microsoft.AspNetCore.Mvc;
using SpendWise.DTOs;
using SpendWise.Services;

namespace SpendWise.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(CategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Get(CancellationToken cancellationToken) => Ok(await categories.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.CreateAsync(request, cancellationToken);
        return Created($"/api/categories/{category.Id}", category);
    }
}
