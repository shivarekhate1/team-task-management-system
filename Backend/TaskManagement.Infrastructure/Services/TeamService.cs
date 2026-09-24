using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Auth;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.DTOs.Teams;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly ApplicationDbContext _dbContext;

    public TeamService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, Guid currentUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Team name cannot be empty.");
        }

        if (request.ManagerId.HasValue)
        {
            var managerExists = await _dbContext.Users.AnyAsync(u => u.Id == request.ManagerId.Value);
            if (!managerExists)
            {
                throw new NotFoundException($"Manager with ID '{request.ManagerId}' not found.");
            }
        }

        var team = new Team
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ManagerId = request.ManagerId ?? currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Teams.AddAsync(team);

        // Add manager as a team member automatically if not already
        if (team.ManagerId.HasValue)
        {
            var member = new TeamMember
            {
                Id = Guid.NewGuid(),
                TeamId = team.Id,
                UserId = team.ManagerId.Value,
                JoinedAt = DateTime.UtcNow
            };
            await _dbContext.TeamMembers.AddAsync(member);
        }

        await _dbContext.SaveChangesAsync();

        return await GetTeamDtoByIdAsync(team.Id);
    }

    public async Task<TeamDto> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid currentUserId, string currentUserRole)
    {
        var team = await _dbContext.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID '{teamId}' not found.");
        }

        EnsureAdminOrTeamManager(team, currentUserId, currentUserRole);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Team name cannot be empty.");
        }

        team.Name = request.Name.Trim();
        team.Description = request.Description?.Trim();

        if (request.ManagerId.HasValue && request.ManagerId != team.ManagerId)
        {
            var managerExists = await _dbContext.Users.AnyAsync(u => u.Id == request.ManagerId.Value);
            if (!managerExists)
            {
                throw new NotFoundException($"Manager with ID '{request.ManagerId}' not found.");
            }

            team.ManagerId = request.ManagerId;

            // Ensure new manager is in TeamMembers
            var isMember = await _dbContext.TeamMembers.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == request.ManagerId.Value);
            if (!isMember)
            {
                await _dbContext.TeamMembers.AddAsync(new TeamMember
                {
                    Id = Guid.NewGuid(),
                    TeamId = teamId,
                    UserId = request.ManagerId.Value,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        team.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return await GetTeamDtoByIdAsync(team.Id);
    }

    public async Task<List<TeamDto>> GetTeamsAsync(Guid currentUserId, string currentUserRole)
    {
        IQueryable<Team> query = _dbContext.Teams.Include(t => t.Manager).Include(t => t.Members).Include(t => t.Tasks);

        if (currentUserRole == UserRole.Admin.ToString())
        {
            // Admins see all teams
        }
        else if (currentUserRole == UserRole.Manager.ToString())
        {
            // Managers see teams where they are Manager OR a Member
            query = query.Where(t => t.ManagerId == currentUserId || t.Members.Any(m => m.UserId == currentUserId));
        }
        else
        {
            // Users see teams they belong to
            query = query.Where(t => t.Members.Any(m => m.UserId == currentUserId));
        }

        var teams = await query.ToListAsync();

        return teams.Select(t => new TeamDto(
            t.Id,
            t.Name,
            t.Description,
            t.ManagerId,
            t.Manager?.FullName,
            t.Members.Count,
            t.Tasks.Count,
            t.CreatedAt
        )).ToList();
    }

    public async Task<TeamDetailDto> GetTeamByIdAsync(Guid teamId, Guid currentUserId, string currentUserRole)
    {
        var team = await _dbContext.Teams
            .Include(t => t.Manager)
            .Include(t => t.Members).ThenInclude(m => m.User)
            .Include(t => t.Tasks).ThenInclude(task => task.AssignedTo)
            .Include(t => t.Tasks).ThenInclude(task => task.CreatedBy)
            .Include(t => t.Tasks).ThenInclude(task => task.Comments)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            throw new NotFoundException($"Team with ID '{teamId}' not found.");
        }

        // Authorization check
        if (currentUserRole != UserRole.Admin.ToString())
        {
            var isAssociated = team.ManagerId == currentUserId || team.Members.Any(m => m.UserId == currentUserId);
            if (!isAssociated)
            {
                throw new ForbiddenException("You are not authorized to view this team's details.");
            }
        }

        var members = team.Members.Select(m => new UserDto(
            m.User.Id, m.User.FullName, m.User.Email, m.User.Role, m.User.IsActive, m.User.CreatedAt
        )).ToList();

        var tasks = team.Tasks.Select(t => new TaskDto(
            t.Id, t.Title, t.Description, t.Priority, t.Status, t.Deadline,
            t.TeamId, team.Name, t.AssignedToId, t.AssignedTo?.FullName,
            t.CreatedById, t.CreatedBy?.FullName ?? "Unknown",
            t.CreatedAt, t.UpdatedAt, t.Comments.Count
        )).ToList();

        return new TeamDetailDto(
            team.Id, team.Name, team.Description, team.ManagerId, team.Manager?.FullName,
            team.CreatedAt, members, tasks
        );
    }

    public async Task AddMemberAsync(Guid teamId, AddTeamMemberRequest request, Guid currentUserId, string currentUserRole)
    {
        var team = await _dbContext.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == teamId);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID '{teamId}' not found.");
        }

        EnsureAdminOrTeamManager(team, currentUserId, currentUserRole);

        var user = await _dbContext.Users.FindAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{request.UserId}' not found.");
        }

        if (team.Members.Any(m => m.UserId == request.UserId))
        {
            throw new ValidationException("User is already a member of this team.");
        }

        var member = new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };

        await _dbContext.TeamMembers.AddAsync(member);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, Guid currentUserId, string currentUserRole)
    {
        var team = await _dbContext.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == teamId);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID '{teamId}' not found.");
        }

        EnsureAdminOrTeamManager(team, currentUserId, currentUserRole);

        var member = team.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            throw new NotFoundException("User is not a member of this team.");
        }

        _dbContext.TeamMembers.Remove(member);

        // If removed member is manager, clear manager field
        if (team.ManagerId == userId)
        {
            team.ManagerId = null;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteTeamAsync(Guid teamId, Guid currentUserId, string currentUserRole)
    {
        var team = await _dbContext.Teams.FindAsync(teamId);
        if (team == null)
        {
            throw new NotFoundException($"Team with ID '{teamId}' not found.");
        }

        EnsureAdminOrTeamManager(team, currentUserId, currentUserRole);

        _dbContext.Teams.Remove(team);
        await _dbContext.SaveChangesAsync();
    }

    private static void EnsureAdminOrTeamManager(Team team, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole != UserRole.Admin.ToString() && team.ManagerId != currentUserId)
        {
            throw new ForbiddenException("Only an Admin or the Team Manager can perform this action.");
        }
    }

    private async Task<TeamDto> GetTeamDtoByIdAsync(Guid teamId)
    {
        var t = await _dbContext.Teams
            .Include(team => team.Manager)
            .Include(team => team.Members)
            .Include(team => team.Tasks)
            .FirstAsync(team => team.Id == teamId);

        return new TeamDto(
            t.Id, t.Name, t.Description, t.ManagerId, t.Manager?.FullName,
            t.Members.Count, t.Tasks.Count, t.CreatedAt
        );
    }
}
