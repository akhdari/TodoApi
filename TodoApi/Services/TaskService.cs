using TodoApi.Models;
using TodoApi.Services.Db;


namespace TodoApi.Services
{
    public class TaskService
    {
        private readonly TaskDbService _taskDb;

        public TaskService(TaskDbService taskDb)
        {
            _taskDb = taskDb;
        }

        public async Task<List<TaskItem>> GetUserTasksAsync(string userId)
        {
            var all = await _taskDb.GetAllAsync();
            return all.Where(t => t.UserId == userId).ToList();
        }

        public async Task<TaskItem?> GetByIdAsync(int id, string userId)
        {
            var task = await _taskDb.GetByIdAsync(id);
            return (task != null && task.UserId == userId) ? task : null;
        }

        public async Task<TaskItem> CreateAsync(TaskItem task, string userId)
        {
            task.UserId = userId;
            return await _taskDb.CreateAsync(task);
        }

        public async Task<bool> UpdateAsync(int id, TaskItem updatedTask, string userId)
        {
            var existing = await _taskDb.GetByIdAsync(id);
            if (existing == null || existing.UserId != userId)
                return false;

            existing.Title = updatedTask.Title;
            existing.IsCompleted = updatedTask.IsCompleted;

            return await _taskDb.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var task = await _taskDb.GetByIdAsync(id);
            if (task == null || task.UserId != userId)
                return false;

            return await _taskDb.DeleteAsync(id);
        }
    }
}
