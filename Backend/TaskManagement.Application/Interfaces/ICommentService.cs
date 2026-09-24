using TaskManagement.Application.DTOs.Comments;

namespace TaskManagement.Application.Interfaces;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(CreateCommentRequest request, Guid currentUserId, string currentUserRole);
    Task<List<CommentDto>> GetCommentsByTaskIdAsync(Guid taskId, Guid currentUserId, string currentUserRole);
    Task DeleteCommentAsync(Guid commentId, Guid currentUserId, string currentUserRole);
}
