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
    public async Task GetAllWithCategory_ReturnsTaskWithCategory()
    {
        _keepAlive.Execute("INSERT INTO Categories(Name) VALUES ('Sleep')");
        _keepAlive.Execute(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                        VALUES ('Test Task', '2026-01-01', 1)");

        var repo = new TaskRepository(ConnStr);
        var result = await repo.GetAllWithCategoryAsync();
        
        var resultList = result.ToList();

        Assert.Single(resultList);
        Assert.Equal("Test Task", resultList[0].Title);
        Assert.Equal("Sleep", resultList[0].Category?.Name);
    }

    [Fact]
    public async Task Add_ValidTask_ShouldBeRetrievable()
    {
        await _keepAlive.ExecuteAsync("INSERT INTO Categories(Name) VALUES ('Work')");
        var repo = new TaskRepository(ConnStr);
        await repo.AddAsync("Task A", 1);
        var result = await repo.GetAllWithCategoryAsync();
        
        var resultList = result.ToList();

        Assert.Single(resultList);
        Assert.Equal("Task A", resultList[0].Title);
        Assert.Equal("Work", resultList[0].Category?.Name);
    }

    [Fact]
    public async Task MarkTaskAsDone_ValidTask_ShouldBeMarkedAsDone()
    {
        await _keepAlive.ExecuteAsync("INSERT INTO Categories(Name) VALUES ('Testing')");
        await _keepAlive.ExecuteAsync(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                                 VALUES ('Test Task', '2026-01-01', 1)");
        var repo = new TaskRepository(ConnStr);
        var before = await repo.GetAllWithCategoryAsync();
        var beforeList = before.ToList();
        await repo.MarkDoneAsync(beforeList[0].Id);
        var result = await repo.GetAllWithCategoryAsync();
        var resultList = result.ToList();
        
        Assert.Single(resultList);
        Assert.True(resultList[0].IsDone);
    }

    [Fact]
    public async Task Delete_ValidTask_ShouldBeDeleted()
    {
        await _keepAlive.ExecuteAsync("INSERT INTO Categories(Name) VALUES ('Testing')");
        await _keepAlive.ExecuteAsync(@"INSERT INTO Tasks(Title, CreatedAt, CategoryId)
                                VALUES ('Test Task', '2026-01-01', 1)");
        var repo = new TaskRepository(ConnStr);
        var before = await repo.GetAllWithCategoryAsync();
        var beforeList = before.ToList();
        await repo.DeleteAsync(beforeList[0].Id);
        var result = await repo.GetAllWithCategoryAsync();
        
        Assert.Empty(result.ToList());
    }
}