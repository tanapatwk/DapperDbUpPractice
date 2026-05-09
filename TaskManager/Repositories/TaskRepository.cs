using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Models;

namespace TaskManager.Repositories;

public class TaskRepository(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public void Add(string title)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Execute(
            "INSERT INTO Tasks (Title, CreatedAt) " +
            "VALUES (@Title, @CreatedAt);",
            new { Title = title, CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
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
        return conn.QueryFirstOrDefault<TaskItem>("SELECT * FROM Tasks WHERE Id = @Id", new { Id = id });
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
    }
}