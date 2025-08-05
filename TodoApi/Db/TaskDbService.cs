using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services.Db
{
    public class TaskDbService
    {
        private readonly AppDbContext _context;

        public TaskDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskItem>> GetAllAsync()
        {
            return await _context.TaskItems.ToListAsync();
        }

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.TaskItems.FindAsync(id);
        }

        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<bool> UpdateAsync(TaskItem updatedTask)
        {
            _context.TaskItems.Update(updatedTask);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return false;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
