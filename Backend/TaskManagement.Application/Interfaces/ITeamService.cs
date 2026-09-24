using TaskManagement.Application.DTOs.Teams;

namespace TaskManagement.Application.Interfaces;

public interface ITeamService
{
    Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, Guid currentUserId);
    Task<TeamDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid currentUserId, string currentUserRole);
    Task<List<TeamDto>> GetTeamsAsync(Guid currentUserId, string currentUserRole);
    Task<TeamDetailDto> GetTeamByIdAsync(Guid teamId, Guid currentUserId, string currentUserRole);
    Task AddMemberAsync(Guid teamId, AddTeamMemberRequest request, Guid currentUserId, string currentUserRole);
    Task RemoveMemberAsync(Guid teamId, Guid userId, Guid currentUserId, string currentUserRole);
    Task DeleteTeamAsync(Guid teamId, Guid currentUserId, string currentUserRole);
}
