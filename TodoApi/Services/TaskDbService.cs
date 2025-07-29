using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using TodoApi.Data;

namespace TodoApi.Services
{
    public class TaskDbService
    {
        private readonly AppDbContext _context;

        public TaskDbService(AppDbContext context)
        {
            _context = context;
        }

        // Get all tasks
        public async Task<List<TaskItem>> GetAllAsync() // the returbn of an async method is Task<T>
        {
            return await _context.TaskItems.ToListAsync();
        }

        // Get task by ID
        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            return await _context.TaskItems.FindAsync(id);
        }

        // Create a new task
        public async Task<TaskItem> CreateAsync(TaskItem task)
        {
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        // Update a task
        public async Task<bool> UpdateAsync(int id, TaskItem updatedTask)
        {
            var existing = await _context.TaskItems.FindAsync(id);
            if (existing == null) return false;

            existing.Title = updatedTask.Title;
            existing.IsCompleted = updatedTask.IsCompleted;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete a task
        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.TaskItems.FindAsync(id);
            if (task == null) return false;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get user by email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Create a user
        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
    }
}
