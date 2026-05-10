using Dapper;
using Microsoft.Data.Sqlite;
using TaskManager.Repositories;

namespace TaskManager.Tests;

public class TaskRepositoryTest : IDisposable
{
    private const string ConnStr = "Data Source=testdb;Mode=Memory;Cache=Shared";
    private readonly SqliteConnection _keepAlive;

    public TaskRepositoryTest()
    {
        _keepAlive = new SqliteConnection(ConnStr);
        _keepAlive.Open();
        _keepAlive.Execute(@"
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
            );
        ");
    }
    
    public void Dispose()
    {
       
        _keepAlive.Execute("DELETE FROM Tasks");
        _keepAlive.Execute("DELETE FROM Categories"); 
        _keepAlive.Execute("DELETE FROM sqlite_sequence WHERE name = 'Tasks'");
        _keepAlive.Execute("DELETE FROM sqlite_sequence WHERE name = 'Categories'");
        _keepAlive.Close();
    }

    [Fact]
    public void GetAllWithCategory_ReturnsTaskWithCategory()
    {
        _keepAlive.Execute("INSERT INTO Categories(Name) VALUES ('Sleep')");
        _keepAlive.Execute(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                        VALUES ('Test Task', '2026-01-01', 1)");

        var repo = new TaskRepository(ConnStr);
        var result = repo.GetAllWithCategory().ToList();

        Assert.Single(result);
        Assert.Equal("Test Task", result[0].Title);
        Assert.Equal("Sleep", result[0].Category?.Name);
    }

    [Fact]
    public void Add_ValidTask_ShouldBeRetrievable()
    {
        _keepAlive.Execute("INSERT INTO Categories(Name) VALUES ('Work')");
        var repo = new TaskRepository(ConnStr);
        repo.Add("Task A", 1);
        var result = repo.GetAllWithCategory().ToList();

        Assert.Single(result);
        Assert.Equal("Task A", result[0].Title);
        Assert.Equal("Work", result[0].Category?.Name);
    }

    [Fact]
    public void MarkTaskAsDone_ValidTask_ShouldBeMarkedAsDone()
    {
        _keepAlive.Execute("INSERT INTO Categories(Name) VALUES ('Testing')");
        _keepAlive.Execute(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                                 VALUES ('Test Task', '2026-01-01', 1)");
        var repo = new TaskRepository(ConnStr);
        var before = repo.GetAllWithCategory().ToList();
        repo.MarkDone(before[0].Id);
        var result = repo.GetAllWithCategory().ToList();
        
        Assert.Single(result);
        Assert.True(result[0].IsDone);
    }

    [Fact]
    public void Delete_ValidTask_ShouldBeDeleted()
    {
        _keepAlive.Execute("INSERT INTO Categories(Name) VALUES ('Testing')");
        _keepAlive.Execute(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                                VALUES ('Test Task', '2026-01-01', 1)");
        var repo = new TaskRepository(ConnStr);
        var before = repo.GetAllWithCategory().ToList();
        repo.Delete(before[0].Id);
        var result = repo.GetAllWithCategory().ToList();
        
        Assert.Empty(result);
    }
}