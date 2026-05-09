using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Models;

namespace TaskManager.Repositories;

public class TaskRepository(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public void Add(string title, int categoryId)
    {
        using var conn = new SqliteConnection(_connectionString);
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

    public IEnumerable<TaskItem> GetAll()
    {
        using var conn = new SqliteConnection(_connectionString);
        return conn.Query<TaskItem>("SELECT * FROM Tasks").ToList();
    }

    public TaskItem? GetById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
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
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute(
            "UPDATE Tasks SET IsDone = 1 WHERE Id = @Id",new { Id = id });
    }

    public void Delete(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute(
            "DELETE FROM Tasks WHERE Id = @Id", new { Id = id });
    }
    
    public void ClearAll()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute("DELETE FROM Tasks");
        conn.Execute("DELETE FROM sqlite_sequence WHERE name = 'Tasks'");
        
        conn.Execute("DELETE FROM Categories");
        conn.Execute("DELETE FROM sqlite_sequence WHERE name = 'Categories'");
    }
    
    public void AddCategory(string categoryName)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute(
            "INSERT INTO Categories (Name) VALUES (@Name);",new { Name = categoryName }
        );
    }
    
    public IEnumerable<TaskItem> GetAllWithCategory()
    {
        using var conn = new SqliteConnection(_connectionString);
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

    public IEnumerable<Category> GetAllCategories()
    {
        using var conn = new SqliteConnection(_connectionString);
        return conn.Query<Category>("SELECT * FROM Categories").ToList();
    }
    
}