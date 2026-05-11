using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Exceptions;
using TaskManager.Models;

namespace TaskManager.Repositories;

public class TaskRepository(string connectionString) : ITaskRepository
{
    private readonly string _connectionString = connectionString;
    
    public async Task AddAsync(string title, int categoryId)
    {
        if(string.IsNullOrWhiteSpace(title))
            throw new TaskValidationException("Title is required");
        
        if(categoryId <= 0)
            throw new TaskValidationException("CategoryId is required");
        
        await using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync(
            "INSERT INTO Tasks (Title, CreatedAt, CategoryId) " +
            "VALUES (@Title, @CreatedAt, @CategoryId);",
            new
            {
                Title = title, 
                CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                CategoryId = categoryId
            }
        );
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        if(id <= 0)
            throw new TaskValidationException("Id is required");
        
        await using var conn = new SqliteConnection(_connectionString);
        var result = await conn.QueryAsync<TaskItem, Category, TaskItem>(
            @"SELECT t.*, c.Id, c.Name " +
            "FROM Tasks t " +
            "LEFT JOIN Categories c ON t.CategoryId = c.Id " +
            "WHERE t.Id = @Id",
            map: (task, category) =>
            {
                task.Category = category;
                return task;
            },
            param: new { Id = id },
            splitOn: "Id"
        );
        return result.FirstOrDefault();
    }
    
    public async Task MarkDoneAsync(int id)
    {
        if(id <= 0)
            throw new TaskValidationException("Id is required");
        
        var task = await GetByIdAsync(id);
        if(task == null)
            throw new TaskNotFoundException("Task not found");
        
        await using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync(
            "UPDATE Tasks SET IsDone = 1 WHERE Id = @Id",new { Id = id });
    }

    public async Task DeleteAsync(int id)
    {
        if(id <= 0)
            throw new TaskValidationException("Id is required");
        
        var task = await GetByIdAsync(id);
        if(task == null)
            throw new TaskNotFoundException("Task not found");
        
        await using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync("DELETE FROM Tasks WHERE Id = @Id", new { Id = id });
    }

    public async Task ClearAllAsync()
    {
        await using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync("DELETE FROM Tasks");
        await conn.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Tasks'");
        
        await conn.ExecuteAsync("DELETE FROM Categories");
        await conn.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Categories'");
    }
    
    public async Task<IEnumerable<TaskItem>> GetAllWithCategoryAsync()
    {
        await using var conn = new SqliteConnection(_connectionString);
        var result = await  conn.QueryAsync<TaskItem, Category, TaskItem>(
            @"SELECT t.*, c.Id, c.Name
                FROM Tasks t
                LEFT JOIN categories c ON c.Id = t.CategoryId",
            map: (task, category) =>
            {
                task.Category = category;
                return task;
            },
            splitOn: "Id"
        );
        return result.ToList();
    }
}