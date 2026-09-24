using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Exceptions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Xunit;

namespace TaskManagement.Tests;

public class TaskServiceTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly NotificationService _notificationService;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _notificationService = new NotificationService(_dbContext);
        _taskService = new TaskService(_dbContext, _notificationService);
    }

    [Fact]
    public async Task CreateTaskAsync_ValidRequest_ShouldCreateTaskAndSendNotification()
    {
        // Arrange
        var creator = new User { Id = Guid.NewGuid(), Email = "creator@test.com", FullName = "Creator User", Role = UserRole.Manager };
        var assignee = new User { Id = Guid.NewGuid(), Email = "assignee@test.com", FullName = "Assignee User", Role = UserRole.User };
        await _dbContext.Users.AddRangeAsync(creator, assignee);
        await _dbContext.SaveChangesAsync();

        var request = new CreateTaskRequest("New Feature Task", "Implement feature X", TaskPriority.High, DateTime.UtcNow.AddDays(3), null, assignee.Id);

        // Act
        var result = await _taskService.CreateTaskAsync(request, creator.Id, UserRole.Manager.ToString());

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Feature Task");
        result.Status.Should().Be(TaskItemStatus.ToDo);
        result.AssignedToId.Should().Be(assignee.Id);

        // Verify notification created for assignee
        var notifications = await _notificationService.GetUserNotificationsAsync(assignee.Id);
        notifications.Should().HaveCount(1);
        notifications.First().Message.Should().Contain("New Feature Task");
    }

    [Fact]
    public async Task CreateTaskAsync_EmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateTaskRequest("", "Description", TaskPriority.Medium, DateTime.UtcNow.AddDays(1), null, null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _taskService.CreateTaskAsync(request, Guid.NewGuid(), UserRole.Admin.ToString()));
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_StatusChange_ShouldNotifyCreator()
    {
        // Arrange
        var creator = new User { Id = Guid.NewGuid(), Email = "creator2@test.com", FullName = "Task Creator", Role = UserRole.Manager };
        var worker = new User { Id = Guid.NewGuid(), Email = "worker@test.com", FullName = "Worker User", Role = UserRole.User };
        await _dbContext.Users.AddRangeAsync(creator, worker);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Status Update Task",
            Status = TaskItemStatus.ToDo,
            CreatedById = creator.Id,
            AssignedToId = worker.Id,
            Deadline = DateTime.UtcNow.AddDays(5)
        };
        await _dbContext.Tasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();

        // Act - Worker changes status to InProgress
        var updated = await _taskService.UpdateTaskStatusAsync(task.Id, TaskItemStatus.InProgress, worker.Id, UserRole.User.ToString());

        // Assert
        updated.Status.Should().Be(TaskItemStatus.InProgress);

        var creatorNotifications = await _notificationService.GetUserNotificationsAsync(creator.Id);
        creatorNotifications.Should().HaveCount(1);
        creatorNotifications.First().Message.Should().Contain("In Progress");
    }

    [Fact]
    public async Task GetTasksAsync_UserRoleScoping_ShouldOnlyReturnAuthorizedTasks()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid(), Email = "u1@test.com", FullName = "User 1", Role = UserRole.User };
        var user2 = new User { Id = Guid.NewGuid(), Email = "u2@test.com", FullName = "User 2", Role = UserRole.User };
        await _dbContext.Users.AddRangeAsync(user1, user2);

        var t1 = new TaskItem { Id = Guid.NewGuid(), Title = "User 1 Task", CreatedById = user1.Id, AssignedToId = user1.Id, Deadline = DateTime.UtcNow.AddDays(1) };
        var t2 = new TaskItem { Id = Guid.NewGuid(), Title = "User 2 Task", CreatedById = user2.Id, AssignedToId = user2.Id, Deadline = DateTime.UtcNow.AddDays(1) };
        await _dbContext.Tasks.AddRangeAsync(t1, t2);
        await _dbContext.SaveChangesAsync();

        // Act
        var user1Tasks = await _taskService.GetTasksAsync(new TaskFilterParams(), user1.Id, UserRole.User.ToString());

        // Assert
        user1Tasks.Should().HaveCount(1);
        user1Tasks.First().Title.Should().Be("User 1 Task");
    }
}
