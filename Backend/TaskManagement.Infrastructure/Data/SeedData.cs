using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (await dbContext.Users.AnyAsync()) return;

        // 1. Create Users
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "admin@taskmanagement.com",
            FullName = "System Admin",
            PasswordHash = passwordHasher.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "manager@taskmanagement.com",
            FullName = "Engineering Manager",
            PasswordHash = passwordHasher.HashPassword("Manager@123"),
            Role = UserRole.Manager,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userShiva = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Email = "shiva@taskmanagement.com",
            FullName = "Shiva Developer",
            PasswordHash = passwordHasher.HashPassword("User@123"),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userJane = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Email = "jane@taskmanagement.com",
            FullName = "Jane Frontend Dev",
            PasswordHash = passwordHasher.HashPassword("User@123"),
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Users.AddRangeAsync(admin, manager, userShiva, userJane);

        // 2. Create Teams
        var engTeam = new Team
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Name = "Core Engineering Team",
            Description = "Responsible for Backend APIs and Core Platform Architecture",
            ManagerId = manager.Id,
            CreatedAt = DateTime.UtcNow
        };

        var productTeam = new Team
        {
            Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
            Name = "Product & UI Team",
            Description = "Focuses on React UI components and UX polish",
            ManagerId = manager.Id,
            CreatedAt = DateTime.UtcNow
        };

        await dbContext.Teams.AddRangeAsync(engTeam, productTeam);

        // 3. Add Members
        var tm1 = new TeamMember { Id = Guid.NewGuid(), TeamId = engTeam.Id, UserId = userShiva.Id, JoinedAt = DateTime.UtcNow };
        var tm2 = new TeamMember { Id = Guid.NewGuid(), TeamId = engTeam.Id, UserId = userJane.Id, JoinedAt = DateTime.UtcNow };
        var tm3 = new TeamMember { Id = Guid.NewGuid(), TeamId = productTeam.Id, UserId = userJane.Id, JoinedAt = DateTime.UtcNow };

        await dbContext.TeamMembers.AddRangeAsync(tm1, tm2, tm3);

        // 4. Create Sample Tasks
        var task1 = new TaskItem
        {
            Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            Title = "Create REST APIs with JWT Auth",
            Description = "Implement ASP.NET Core Web API with JWT bearer tokens and role authorization.",
            Priority = TaskPriority.High,
            Status = TaskItemStatus.Done,
            Deadline = DateTime.UtcNow.AddDays(2),
            TeamId = engTeam.Id,
            AssignedToId = userShiva.Id,
            CreatedById = manager.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var task2 = new TaskItem
        {
            Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            Title = "Build Dynamic Dashboard visual metrics",
            Description = "Create visual stat widgets and task status charts in React.",
            Priority = TaskPriority.Urgent,
            Status = TaskItemStatus.InProgress,
            Deadline = DateTime.UtcNow.AddDays(4),
            TeamId = engTeam.Id,
            AssignedToId = userShiva.Id,
            CreatedById = manager.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var task3 = new TaskItem
        {
            Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
            Title = "Implement Notifications API & Drawer",
            Description = "Send event notifications on task assignment and status update.",
            Priority = TaskPriority.Medium,
            Status = TaskItemStatus.ToDo,
            Deadline = DateTime.UtcNow.AddDays(7),
            TeamId = productTeam.Id,
            AssignedToId = userJane.Id,
            CreatedById = manager.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await dbContext.Tasks.AddRangeAsync(task1, task2, task3);

        // 5. Create Comments
        var comment1 = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = task1.Id,
            UserId = userShiva.Id,
            CommentText = "JWT Auth API implemented with clean architecture, custom claims, and Swagger Bearer support!",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var comment2 = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = task2.Id,
            UserId = manager.Id,
            CommentText = "Please make sure stats are loaded dynamically from the backend rather than mock data.",
            CreatedAt = DateTime.UtcNow.AddHours(-5)
        };

        await dbContext.Comments.AddRangeAsync(comment1, comment2);

        // 6. Create Notifications
        var notif1 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userShiva.Id,
            Message = "You have been assigned a new task: Build Dynamic Dashboard visual metrics",
            Type = NotificationType.TaskAssigned,
            IsRead = false,
            TaskId = task2.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var notif2 = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userJane.Id,
            Message = "You have been assigned a new task: Implement Notifications API & Drawer",
            Type = NotificationType.TaskAssigned,
            IsRead = false,
            TaskId = task3.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await dbContext.Notifications.AddRangeAsync(notif1, notif2);

        await dbContext.SaveChangesAsync();
    }
}
