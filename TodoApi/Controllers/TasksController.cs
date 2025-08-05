using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services; 
using System.Security.Claims;

namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")] 
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    // Get user ID from JWT claims
    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User ID not found");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetUserTasksAsync(userId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var userId = GetUserId();
        var task = await _taskService.GetByIdAsync(id, userId);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] TaskItem task)
    {
        var userId = GetUserId();
        var created = await _taskService.CreateAsync(task, userId);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] TaskItem task)
    {
        var userId = GetUserId();
        var success = await _taskService.UpdateAsync(id, task, userId);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var success = await _taskService.DeleteAsync(id, userId);
        return success ? Ok() : NotFound();
    }
}
