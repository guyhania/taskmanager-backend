using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Models;

namespace TaskManagerApi.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    public TaskRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TaskItem>> GetAllAsync() =>
        await _context.Tasks.ToListAsync();

    public async Task<TaskItem?> GetByIdAsync(int id) =>
        await _context.Tasks.FindAsync(id);

    public async Task AddAsync(TaskItem task) =>
        await _context.Tasks.AddAsync(task);

    public void Update(TaskItem task) =>
        _context.Tasks.Update(task);

    public void Delete(TaskItem task) =>
        _context.Tasks.Remove(task);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}
