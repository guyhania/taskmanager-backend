using Xunit;
using TaskManagerApi.Controllers;
using TaskManagerApi.Models;
using TaskManagerApi.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;

public class TasksControllerTests
{
    [Fact]
    public async Task Post_ValidTask_ReturnsCreatedResult()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>())).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "This is a test",
            DueDate = DateTime.Now.AddDays(1),
            Priority = "High",
            FullName = "Test User",
            Telephone = "123456789",
            Email = "test@example.com"
        };

        // Act
        var result = await controller.Create(task);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }
    [Fact]
    public async Task Post_InvalidTask_ReturnsBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        var invalidTask = new TaskItem
        {
            Title = "", // invalid
            Description = "desc",
            DueDate = DateTime.Now.AddDays(1),
            Priority = "High",
            FullName = "Guy",
            Telephone = "0501234567",
            Email = "not-an-email" // invalid
        };

        controller.ModelState.AddModelError("Title", "Required");
        controller.ModelState.AddModelError("Email", "Invalid");

        // Act
        var result = await controller.Create(invalidTask);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    [Fact]
    public async Task GetById_TaskNotFound_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((TaskItem?)null);

        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        // Act
        var result = await controller.GetById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }
    [Fact]
    public async Task Put_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "Old Title",
            Description = "desc",
            DueDate = DateTime.Now.AddDays(1),
            Priority = "High",
            FullName = "Guy",
            Telephone = "0501234567",
            Email = "guy@example.com",
            IsReminderSent = false
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingTask);
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        var updatedTask = new TaskItem
        {
            Id = 1,
            Title = "New Title",
            Description = "updated",
            DueDate = DateTime.Now.AddDays(2),
            Priority = "Medium",
            FullName = "Guy",
            Telephone = "0501234567",
            Email = "guy@example.com",
            IsReminderSent = false
        };

        // Act
        var result = await controller.Update(1, updatedTask);

        // Assert
        (result.Result).Should().BeOfType<OkObjectResult>();
    }
    [Fact]
    public async Task Delete_ExistingTask_ReturnsNoContent()
    {
        // Arrange
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "To delete",
            DueDate = DateTime.Now.AddDays(1),
            FullName = "Guy",
            Telephone = "0501234567",
            Email = "guy@example.com"
        };

        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingTask);
        mockRepo.Setup(r => r.Delete(existingTask));
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        // Act
        var result = await controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
    [Fact]
    public async Task Delete_NonExistingTask_ReturnsNotFound()
    {
        // Arrange
        var mockRepo = new Mock<ITaskRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        var logger = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(mockRepo.Object, logger.Object);

        // Act
        var result = await controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }


}
