using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Models;

namespace TaskManager.Repositories;

public class TaskRepository :ITaskRepository
{
    private readonly string? _connectionString;
    private readonly SqliteConnection? _connection;

    public TaskRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public TaskRepository(SqliteConnection connection)
    {
        _connection = connection;
    }

    private SqliteConnection GetConnection()
    {
        return _connection ?? new SqliteConnection(_connectionString);
    }

    public void Add(string title, int categoryId)
    {
        using var conn = GetConnection();
        conn.Execute(
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

    public TaskItem? GetById(int id)
    {
        using var conn = GetConnection();
        return conn.Query<TaskItem, Category, TaskItem>(
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
        ).FirstOrDefault();
    }
    
    public void MarkDone(int id)
    {
        using var conn = GetConnection();
        conn.Execute(
            "UPDATE Tasks SET IsDone = 1 WHERE Id = @Id",new { Id = id });
    }

    public void Delete(int id)
    {
        using var conn = GetConnection();
        conn.Execute(
            "DELETE FROM Tasks WHERE Id = @Id", new { Id = id });
    }
    
    public void ClearAll()
    {
        using var conn = GetConnection();
        conn.Execute("DELETE FROM Tasks");
        conn.Execute("DELETE FROM sqlite_sequence WHERE name = 'Tasks'");
        
        conn.Execute("DELETE FROM Categories");
        conn.Execute("DELETE FROM sqlite_sequence WHERE name = 'Categories'");
    }
    
    public IEnumerable<TaskItem> GetAllWithCategory()
    {
        using var conn = GetConnection();
        return conn.Query<TaskItem, Category, TaskItem>(
            @"SELECT t.*, c.Id, c.Name
                FROM Tasks t
                LEFT JOIN categories c ON c.Id = t.CategoryId",
            map: (task, category) =>
            {
                task.Category = category;
                return task;
            },
            splitOn: "Id"
        ).ToList();
    }
}