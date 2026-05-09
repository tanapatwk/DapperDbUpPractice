namespace TaskManager.Models;

public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; } 
    public bool IsDone { get; set; } = false;
    public string CreatedAt { get; set; } = string.Empty;
}