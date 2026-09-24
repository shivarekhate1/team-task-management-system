using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Get tasks with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] TaskFilterParams filter)
    {
        var tasks = await _taskService.GetTasksAsync(filter, CurrentUserId, CurrentUserRole);
        return Ok(tasks);
    }

    /// <summary>
    /// Get task details by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        var task = await _taskService.GetTaskByIdAsync(id, CurrentUserId, CurrentUserRole);
        return Ok(task);
    }

    /// <summary>
    /// Create a new task (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var task = await _taskService.CreateTaskAsync(request, CurrentUserId, CurrentUserRole);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    /// <summary>
    /// Update task details (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _taskService.UpdateTaskAsync(id, request, CurrentUserId, CurrentUserRole);
        return Ok(task);
    }

    /// <summary>
    /// Update task status (To Do, In Progress, Done).
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request)
    {
        var task = await _taskService.UpdateTaskStatusAsync(id, request.Status, CurrentUserId, CurrentUserRole);
        return Ok(task);
    }

    /// <summary>
    /// Assign task to a user (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:guid}/assign")]
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
    {
        var task = await _taskService.AssignTaskAsync(id, request.AssignedToId, CurrentUserId, CurrentUserRole);
        return Ok(task);
    }

    /// <summary>
    /// Delete a task (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        await _taskService.DeleteTaskAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }
}
