using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskDbService _db;

    public TasksController(TaskDbService db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Get() => Ok(_db.GetAll());

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var task = _db.GetById(id);
        return task == null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public IActionResult Post([FromBody] TaskItem task) => Ok(_db.Create(task));

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] TaskItem task) =>
        _db.Update(id, task) ? Ok() : NotFound();

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) =>
        _db.Delete(id) ? Ok() : NotFound();
}
