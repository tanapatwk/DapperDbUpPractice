using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Exceptions;
using TaskManager.Models;

namespace TaskManager.Repositories;

public class CategoryRepository(string connectionString) : ICategoryRepository
{
    private readonly string _connectionString = connectionString;

    public async Task CreateCategoryWithTaskAsync(string categoryName, List<string> taskTitles)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new TaskValidationException("Category ต้องไม่เป็น null หรือว่างเปล่า");

        foreach (var taskTitle in taskTitles)
        {
            if (string.IsNullOrWhiteSpace(taskTitle))
                throw new TaskValidationException("Task name ต้องไม่เป็น null หรือว่างเปล่า");
        }
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
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    
    public IEnumerable<Category> GetAllCategories()
    {
        using var conn = new SqliteConnection(_connectionString);
        return conn.Query<Category>("SELECT * FROM Categories").ToList();
    }

    public void AddCategory(string categoryName)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute(
            "INSERT INTO Categories (Name) VALUES (@Name);",new { Name = categoryName }
        );
    }

}

