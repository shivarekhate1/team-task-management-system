using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Comments;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
public class CommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Add a comment to a task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest request)
    {
        var comment = await _commentService.AddCommentAsync(request, CurrentUserId, CurrentUserRole);
        return CreatedAtAction(nameof(GetCommentsByTaskId), new { taskId = request.TaskId }, comment);
    }

    /// <summary>
    /// Get all comments for a specific task.
    /// </summary>
    [HttpGet("task/{taskId:guid}")]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentsByTaskId(Guid taskId)
    {
        var comments = await _commentService.GetCommentsByTaskIdAsync(taskId, CurrentUserId, CurrentUserRole);
        return Ok(comments);
    }

    /// <summary>
    /// Delete a comment (Author or Admin).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        await _commentService.DeleteCommentAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }
}
