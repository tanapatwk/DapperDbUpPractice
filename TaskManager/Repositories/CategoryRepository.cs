using Dapper;
using Microsoft.Data.Sqlite;

namespace TaskManager.Repositories;

public class CategoryRepository(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public async Task CreateCategoryWithTaskAsync(string categoryName, List<string> taskTitles)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var tx = conn.BeginTransaction();
        try
        {

            int categoryId = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO Categories(Name) VALUES (@Name);
                        SELECT last_insert_rowid();",
                new { Name = categoryName },
                transaction: tx
            );

            foreach (var taskTitle in taskTitles)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO Tasks (Title, CreatedAt, CategoryId) 
                    VALUES (@Title, @CreatedAt, @CategoryId);",
                    new
                    {
                        Title = taskTitle,
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        CategoryId = categoryId
                    },
                    transaction: tx);
            }
            
            await tx.CommitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await tx.RollbackAsync();
            throw;
        }
    }
}