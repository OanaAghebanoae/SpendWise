using Microsoft.EntityFrameworkCore;
using SpendWise.Data;
using SpendWise.DTOs;
using SpendWise.Models;

namespace SpendWise.Services;

public sealed class CategoryService(SpendWiseDbContext db)
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new CategoryDto(x.Id, x.Name, x.Description)).ToListAsync(cancellationToken);

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.");
        if (await db.Categories.AnyAsync(x => x.Name == name, cancellationToken)) throw new InvalidOperationException("A category with this name already exists.");
        var category = new Category { Id = Guid.NewGuid(), Name = name, Description = request.Description?.Trim() };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return new CategoryDto(category.Id, category.Name, category.Description);
    }
}
