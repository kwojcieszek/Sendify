using Microsoft.EntityFrameworkCore;
using Sendify.Data;

namespace Sendify.DataManager;

public class DataContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private static string? _defaultConnectionstring;
    private static string? _defaultDatabaseName;

    public DbSet<Message> Messages { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Pattern> Patterns { get; set; }
    public DbSet<Token> Tokens { get; set; }

    public static void SetDefaultDatabaseSettings(string defaultConnectionstring, string defaultDatabaseName)
    {
        _defaultDatabaseName = defaultDatabaseName;
        _defaultConnectionstring = defaultConnectionstring;
    }

    public DataContext()
    {
        if (_defaultConnectionstring == null || _defaultDatabaseName == null)
        {
            throw new InvalidOperationException("Default database settings are not set.");
        }

        _connectionString = _defaultConnectionstring;
        _databaseName = _defaultDatabaseName;
    }

    public DataContext(string connectionString, string databaseName)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMongoDB(_connectionString, _databaseName);

        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
}