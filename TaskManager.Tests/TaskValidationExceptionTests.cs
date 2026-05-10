using TaskManager.Exceptions;
using TaskManager.Repositories;

namespace TaskManager.Tests;

public class TaskValidationExceptionTests
{
    private const string Connstr = "Data source=:memory:";

    [Fact]
    public async Task CreateCategory_EmptyName_ThrowsValidationException()
    {
        var repo = new CategoryRepository(Connstr);
        await Assert.ThrowsAsync<TaskValidationException>(
            () => repo.CreateCategoryWithTaskAsync("", ["Task A"])
        );
    }
    
    [Fact]
    public async Task CreateTask_EmptyName_ThrowsValidationException()
    {
        var repo = new CategoryRepository(Connstr);
        await Assert.ThrowsAsync<TaskValidationException>(
            () => repo.CreateCategoryWithTaskAsync("WORK", [""])
        );
    }
}