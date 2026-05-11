using Microsoft.AspNetCore.Diagnostics;
using TaskAPI.DTO;
using TaskManager.Exceptions;
using TaskManager.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found");

builder.Services.AddSingleton<ITaskRepository>(_ => new TaskRepository(connectionString));
builder.Services.AddSingleton<ICategoryRepository>(_ => new CategoryRepository(connectionString));

var app = builder.Build();

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        if (exception is TaskValidationException exValidation)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = exValidation.Message
            });
        }
        else if (exception is TaskNotFoundException exNotFound)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = exNotFound.Message
            });
        }
        else
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal Server Error occurred. Please try again later."
            });
        }
    });
});

app.MapGet("/tasks", async (ITaskRepository repo) =>
{
    var tasks = (await repo.GetAllWithCategoryAsync())
        .Select(t => new TaskResponse(
            t.Id,
            t.Title,
            t.IsDone,
            t.CreatedAt ?? "",
            t.Category?.Name
        ));
    return Results.Ok(tasks);
});

app.MapPost("/tasks", async (ITaskRepository repo, CreateTaskRequest request) =>
{
    await repo.AddAsync(request.Title, request.CategoryId);
    return Results.Created("/tasks", null);
});

app.MapPut("/tasks/{id}/done", async (ITaskRepository repo, int id) =>
{
    await repo.MarkDoneAsync(id);
    return Results.NoContent();
});

app.MapDelete("/tasks/{id}", async (ITaskRepository repo, int id) =>
{
    await repo.DeleteAsync(id);
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