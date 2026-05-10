using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Repositories;

namespace TaskManager.Tests;

public class TaskRepositoryTest
{
    private const string ConnStr = "Data Source=:memory:";
    
    private SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(ConnStr);
        conn.Open();
        conn.Execute(@"
            CREATE TABLE Categories(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            );
            CREATE TABLE Tasks(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                IsDone INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT,
                CategoryId INTEGER,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            )
        ");
        return conn;
    }

    [Fact]
    public void GetAllWithCategory_ReturnsTaskWithCategory()
    {
        using var conn = CreateConnection();
        
        conn.Execute("INSERT INTO Categories(Name) VALUES ('Work')");
        conn.Execute(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                        VALUES ('Test Task', '2026-01-01', 1)");

        var repo = new TaskRepository(conn);
        var result = repo.GetAllWithCategory().ToList();
        
        Assert.Single(result);
        Assert.Equal("Test Task", result[0].Title);
        Assert.Equal("Work", result[0].Category?.Name);
    }
}