namespace TaskManagement.Application.DTOs.Teams;

public record CreateTeamRequest(
    string Name,
    string? Description,
    Guid? ManagerId
);

public record UpdateTeamRequest(
    string Name,
    string? Description,
    Guid? ManagerId
);

public record AddTeamMemberRequest(
    Guid UserId
);

public record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName,
    int MemberCount,
    int TaskCount,
    DateTime CreatedAt
);

public record TeamDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ManagerId,
    string? ManagerName,
    DateTime CreatedAt,
    List<Auth.UserDto> Members,
    List<Tasks.TaskDto> Tasks
);
