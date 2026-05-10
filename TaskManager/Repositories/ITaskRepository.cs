using TaskManager.Models;

namespace TaskManager.Repositories;

public interface ITaskRepository
{
     void Add(string title, int categoryId);
     TaskItem? GetById(int id);
     void MarkDone(int id);
     void Delete(int id);
     void ClearAll();
     IEnumerable<TaskItem> GetAllWithCategory();
}