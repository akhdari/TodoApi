using Microsoft.AspNetCore.Authorization; // Used to secure endpoints via JWT or other authentication schemes
using Microsoft.AspNetCore.Mvc; // Base for controller functionality and HTTP responses
using TodoApi.Models; // TaskItem model
using TodoApi.Services; // TaskDbService for database operations

namespace TodoApi.Controllers;

// Indicates that this is a Web API controller (enables auto model validation, better error responses, etc.)
[ApiController]

// The route is automatically set to "tasks" since the class is named TasksController
[Route("[controller]")]

// Requires authentication (e.g., JWT token) to access any endpoint in this controller
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskDbService _db;

    // Constructor injects the database service (used for CRUD operations)
    public TasksController(TaskDbService db)
    {
        _db = db;
    }

    // GET /tasks
    // Returns a list of all tasks
    [HttpGet]
    public IActionResult Get() => Ok(_db.GetAll()); // ActionResult is a return type used in ASP.NET Core controllers to represent HTTP responses.

    // GET /tasks/{id}
    // Returns a task by its ID, or 404 if not found
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var task = _db.GetById(id);
        return task == null ? NotFound() : Ok(task);
    }

    // POST /tasks
    // Creates a new task from the request body
    [HttpPost]
    public IActionResult Post([FromBody] TaskItem task) => Ok(_db.Create(task));

    // PUT /tasks/{id}
    // Updates a task by ID with new data from the request body
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] TaskItem task) =>
        _db.Update(id, task) ? Ok() : NotFound();

    // DELETE /tasks/{id}
    // Deletes a task by ID
    [HttpDelete("{id}")]
    public IActionResult Delete(int id) =>
        _db.Delete(id) ? Ok() : NotFound();
}
