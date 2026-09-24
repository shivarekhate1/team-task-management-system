using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Comments;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _dbContext;

    public CommentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentDto> AddCommentAsync(CreateCommentRequest request, Guid currentUserId, string currentUserRole)
    {
        if (string.IsNullOrWhiteSpace(request.CommentText))
        {
            throw new ValidationException("Comment text cannot be empty.");
        }

        var task = await _dbContext.Tasks.FindAsync(request.TaskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{request.TaskId}' not found.");
        }

        EnsureCanAccessTask(task, currentUserId, currentUserRole);

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            UserId = currentUserId,
            CommentText = request.CommentText.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Comments.AddAsync(comment);
        await _dbContext.SaveChangesAsync();

        var user = await _dbContext.Users.FindAsync(currentUserId);

        return new CommentDto(
            comment.Id, comment.TaskId, comment.UserId, user?.FullName ?? "Unknown",
            comment.CommentText, comment.CreatedAt, comment.UpdatedAt
        );
    }

    public async Task<List<CommentDto>> GetCommentsByTaskIdAsync(Guid taskId, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanAccessTask(task, currentUserId, currentUserRole);

        var comments = await _dbContext.Comments
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(c => new CommentDto(
            c.Id, c.TaskId, c.UserId, c.User.FullName, c.CommentText, c.CreatedAt, c.UpdatedAt
        )).ToList();
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid currentUserId, string currentUserRole)
    {
        var comment = await _dbContext.Comments.FindAsync(commentId);
        if (comment == null)
        {
            throw new NotFoundException($"Comment with ID '{commentId}' not found.");
        }

        if (currentUserRole != UserRole.Admin.ToString() && comment.UserId != currentUserId)
        {
            throw new ForbiddenException("You do not have permission to delete this comment.");
        }

        _dbContext.Comments.Remove(comment);
        await _dbContext.SaveChangesAsync();
    }

    private void EnsureCanAccessTask(TaskItem task, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.Admin.ToString()) return;
        if (task.CreatedById == currentUserId || task.AssignedToId == currentUserId) return;

        if (task.TeamId.HasValue)
        {
            var isMember = _dbContext.TeamMembers.Any(tm => tm.TeamId == task.TeamId.Value && tm.UserId == currentUserId);
            var isManager = _dbContext.Teams.Any(t => t.Id == task.TeamId.Value && t.ManagerId == currentUserId);

            if (isMember || isManager) return;
        }

        throw new ForbiddenException("You are not authorized to view or comment on this task.");
    }
}
