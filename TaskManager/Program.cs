using System.Reflection;
using DbUp;
using TaskManager.Models;
using TaskManager.Repositories;

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
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Migration ล้มเหลว {result.Error}");
    Console.ResetColor();
    return;
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Success!");
    Console.ResetColor();
}

var task = new TaskRepository(connectionString);

Console.WriteLine("\n======== Add task =========");
task.Add("TASK 1");
task.Add("TASK 2");
task.Add("TASK 3");
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Add task success!");
Console.ResetColor();


PrintAllTasks();

Console.WriteLine("\n======== Mark Done =========");
task.MarkDone(1);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Mark Done success!");
Console.ResetColor();

Console.WriteLine("\n======== GetById  =========");
var taskDone = task.GetById(1);
if (taskDone != null)
    PrintTask(taskDone);

Console.WriteLine("\n======== Delete Task =========");
task.Delete(1);
PrintAllTasks();

Console.WriteLine("\n======== Clear All Task =========");
task.ClearAll();
PrintAllTasks();


void PrintTask(TaskItem taskItem)
{
    Console.WriteLine($"[{taskItem.Id}] {taskItem.Title} " +
                      $"Status:{taskItem.IsDone} ({taskItem.CreatedAt})");
}

void PrintAllTasks()
{
    var tasks = task.GetAll();
    if (!tasks.Any())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No tasks found!");
        Console.ResetColor();
        return;
    }
    Console.WriteLine("Task list:");
    foreach (var taskItem in tasks)
        PrintTask(taskItem);
}



