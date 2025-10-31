using Common.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Unit.Tests.Util;

public static class TestUtils
{
    public static GetToAnAnswerDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<GetToAnAnswerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new GetToAnAnswerDbContext(options);
    }
}