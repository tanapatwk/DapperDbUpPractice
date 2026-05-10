using System.Reflection;
using DbUp;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TaskManager.Exceptions;
using TaskManager.Helpers;
using TaskManager.Models;
using TaskManager.Repositories;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/taskmanager.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var dbPath = "taskmanager.db";
var connectionString = $"Data Source={dbPath}";

//==== Migration =====
var upgrader = DeployChanges.To
    .SqliteDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
    .LogToConsole()
    .Build();
    
var result = upgrader.PerformUpgrade();
if (!result.Successful)
{
    Log.Error("{Failed} Migration ล้มเหลว {Error}", Emoji.Failed, result.Error);
    return;
}
else
{
    Log.Information("{Success} Migration สำเร็จ", Emoji.Success);
}

var services = new ServiceCollection();

services.AddSingleton<ITaskRepository>(_ => new TaskRepository(connectionString));
services.AddSingleton<ICategoryRepository>(_ => new CategoryRepository(connectionString));

var provider = services.BuildServiceProvider();


var task = provider.GetRequiredService<ITaskRepository>();
var categoryRepo = provider.GetRequiredService<ICategoryRepository>();

Console.WriteLine("\n======== Add Categories =========");
categoryRepo.AddCategory("Work");
categoryRepo.AddCategory("Study");
categoryRepo.AddCategory("Personal");
Log.Information("{Success} Add Categories success!", Emoji.Success);

Console.WriteLine("\n======== Add task =========");
task.Add("TASK 1", 1);
task.Add("TASK 2", 2);
task.Add("TASK 3", 2);
Log.Information("{Success} Add task success!", Emoji.Success);


PrintAllTasks();

Console.WriteLine("\n======== Mark Done =========");
task.MarkDone(1);
Log.Information("{Success} Mark Done!", Emoji.Success);

Console.WriteLine("\n======== GetById  =========");
var taskDone = task.GetById(1);
if (taskDone != null)
    PrintTask(taskDone);

Console.WriteLine("\n======== Delete Task =========");
task.Delete(1);
PrintAllTasks();



Console.WriteLine("\n======== Get All with Category Task =========");
var taskWithCategory = task.GetAllWithCategory();
foreach (var taskItem in taskWithCategory)
{
    PrintTask(taskItem);
}

// ------------ Test Category Repository ---------------
Console.WriteLine("\n==== Transaction: Success Case =====");
try
{
    await categoryRepo.CreateCategoryWithTaskAsync("WORK", ["Task A", "Task B"]);
    Console.WriteLine($"{Emoji.Success} Success Case!");
}
catch (TaskValidationException tex)
{
    Log.Error("{Failed} An error occured: {Error}", Emoji.Failed, tex.Message);
}
catch (Exception e)
{
    Log.Error("{Failed} An error occured: {Error}", Emoji.Failed, e);
}

// ------------ Test Category Repository ---------------

Console.WriteLine("\n==== Transaction: Rollback Case =====");
try
{
    await categoryRepo.CreateCategoryWithTaskAsync("Study", ["Task C", null!, "Task D"]);
    Log.Information("{Success} Success Case!", Emoji.Success);
}
catch (TaskValidationException tex)
{
    Log.Error("{Failed} An update db error: {Error}", Emoji.Failed, tex.Message);
}
catch (Exception e)
{
    Log.Error("{Failed} An error occured: {Error}", Emoji.Failed, e);
}

Console.WriteLine("====== Task List =========");
PrintAllTasks();

task.ClearAll();


void PrintTask(TaskItem taskItem)
{
    Console.WriteLine($"[{taskItem.Id}] {taskItem.Title} " +
                      $"Status:{taskItem.IsDone} " +
                      $"Category: {taskItem.Category?.Name?? $"{Emoji.Failed} No category"} ({taskItem.CreatedAt})");
}

void PrintAllTasks()
{
    var tasks = task.GetAllWithCategory();
    if (!tasks.Any())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{Emoji.Warning} No tasks found!");
        Console.ResetColor();
        return;
    }
    Console.WriteLine("Task list:");
    foreach (var taskItem in tasks)
        PrintTask(taskItem);
}



