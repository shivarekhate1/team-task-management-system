using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs.Teams;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

[Authorize]
public class TeamsController : BaseApiController
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    /// <summary>
    /// Get list of teams accessible to the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _teamService.GetTeamsAsync(CurrentUserId, CurrentUserRole);
        return Ok(teams);
    }

    /// <summary>
    /// Get details of a specific team.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamById(Guid id)
    {
        var team = await _teamService.GetTeamByIdAsync(id, CurrentUserId, CurrentUserRole);
        return Ok(team);
    }

    /// <summary>
    /// Create a new team (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var team = await _teamService.CreateTeamAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetTeamById), new { id = team.Id }, team);
    }

    /// <summary>
    /// Update team info (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _teamService.UpdateTeamAsync(id, request, CurrentUserId, CurrentUserRole);
        return Ok(team);
    }

    /// <summary>
    /// Delete a team (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        await _teamService.DeleteTeamAsync(id, CurrentUserId, CurrentUserRole);
        return NoContent();
    }

    /// <summary>
    /// Add a user to a team (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost("{id:guid}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddTeamMemberRequest request)
    {
        await _teamService.AddMemberAsync(id, request, CurrentUserId, CurrentUserRole);
        return Ok(new { message = "User successfully added to team." });
    }

    /// <summary>
    /// Remove a user from a team (Admin & Manager).
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        await _teamService.RemoveMemberAsync(id, userId, CurrentUserId, CurrentUserRole);
        return Ok(new { message = "User successfully removed from team." });
    }
}
