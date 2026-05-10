using TaskManager.Models;

namespace TaskManager.Repositories;

public interface ICategoryRepository
{ 
     Task CreateCategoryWithTaskAsync(string categoryName, List<string> taskTitles);
     IEnumerable<Category> GetAllCategories();
     void AddCategory(string categoryName);
}