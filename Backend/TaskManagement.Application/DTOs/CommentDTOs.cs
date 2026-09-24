namespace TaskManagement.Application.DTOs.Comments;

public record CreateCommentRequest(
    Guid TaskId,
    string CommentText
);

public record CommentDto(
    Guid Id,
    Guid TaskId,
    Guid UserId,
    string UserName,
    string CommentText,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
