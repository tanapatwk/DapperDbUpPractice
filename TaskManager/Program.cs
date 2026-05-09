using System.Reflection;
using DbUp;

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

