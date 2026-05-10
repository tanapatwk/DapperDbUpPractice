using TaskManager.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Data Source=../TaskManager/bin/Debug/net10.0/taskmanager.db";

builder.Services.AddSingleton<ITaskRepository>(_ => new TaskRepository(connectionString));
builder.Services.AddSingleton<ICategoryRepository>(_ => new CategoryRepository(connectionString));

var app = builder.Build();

app.MapGet("/tasks", (ITaskRepository repo) =>
{
    var task = repo.GetAllWithCategory();
    return Results.Ok(task);
});

app.MapPost("/tasks", (ITaskRepository repo, CreateTaskRequest request) =>
{
    repo.Add(request.Title, request.CategoryId);
    return Results.Created("/tasks", null);
});

app.MapPut("/tasks/{id}/done", (ITaskRepository repo, int id) =>
{
    repo.MarkDone(id);
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", (ITaskRepository repo, int id) =>
{
    repo.Delete(id);
    return Results.NoContent();
});

// ===== Category Enpoints ======
app.MapGet("/categories", (ICategoryRepository repo) =>
{
    var categories = repo.GetAllCategories();
    return Results.Ok(categories);
});

app.MapPost("/categories", (ICategoryRepository repo, CreateCategoryRequest request) =>
{
    repo.AddCategory(request.Name);
    return Results.Created("/categories", null);
});

app.Run();

record CreateTaskRequest(string Title, int CategoryId); 
record CreateCategoryRequest(string Name);