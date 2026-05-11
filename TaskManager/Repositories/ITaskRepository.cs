using TaskManager.Models;

namespace TaskManager.Repositories;

public interface ITaskRepository
{
     Task AddAsync(string title, int categoryId);
     Task<TaskItem?> GetByIdAsync(int id);
     Task MarkDoneAsync(int id);
     Task DeleteAsync(int id);
     Task ClearAllAsync();
     Task<IEnumerable<TaskItem>> GetAllWithCategoryAsync();
}