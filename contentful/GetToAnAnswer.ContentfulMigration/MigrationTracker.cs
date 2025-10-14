using Contentful.Core.Models;
using GetToAnAnswer.ContentfulMigration;

namespace CareLeavers.ContentfulMigration;

public class MigrationTracker : IContent
{
    public string Name { get; set; } = string.Empty;
    
    public List<Migration> Migrations { get; set; } = [];
    
    public SystemProperties? Sys { get; set; }
}