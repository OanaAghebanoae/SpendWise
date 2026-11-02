using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpendWise.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SpendWiseDbContext>
{
    public SpendWiseDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SpendWiseDbContext>()
            .UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SpendWise;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=True")
            .Options;

        return new SpendWiseDbContext(options);
    }
}
