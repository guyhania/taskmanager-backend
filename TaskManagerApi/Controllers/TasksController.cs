using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Models;
using TaskManagerApi.Repositories;

namespace TaskManagerApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<TasksController> _logger;


    public TasksController(ITaskRepository repository, ILogger<TasksController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll() =>
        Ok(await _repository.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _repository.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _repository.AddAsync(task);
        await _repository.SaveChangesAsync();
        
        _logger.LogInformation("Creating new task: {Title}", task.Title);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TaskItem>> Update(int id, TaskItem task)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Task {Id} not found", id);
            return NotFound();
        }

        // Update fields
        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.DueDate = task.DueDate;
        existing.Priority = task.Priority;
        existing.FullName = task.FullName;
        existing.Telephone = task.Telephone;
        existing.Email = task.Email;
        existing.IsReminderSent = task.IsReminderSent;

        _repository.Update(existing);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Updated task {Id}", id);

        return Ok(existing);
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task is null) return NotFound();

        _repository.Delete(task);
        await _repository.SaveChangesAsync();

        _logger.LogWarning("Deleting task {Id}", id);

        return NoContent();
    }
}
