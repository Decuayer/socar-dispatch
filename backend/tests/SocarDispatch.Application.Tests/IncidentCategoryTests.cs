using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Features.IncidentCategories.Commands.CreateIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.Commands.DeleteIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.Commands.UpdateIncidentCategory;
using SocarDispatch.Application.Features.IncidentCategories.Queries.GetIncidentCategories;
using SocarDispatch.Application.Features.Incidents.Commands.CreateIncident;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;
using MediatR;
using Moq;

namespace SocarDispatch.Application.Tests;

public class IncidentCategoryTests
{
    private static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CreateIncident_WithNonExistentOrInactiveCategory_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var reporter = new User
        {
            FirstName = "Ali",
            LastName = "Veli",
            Email = "ali@socar.com",
            Phone = "+905001112233",
            PasswordHash = "hash",
            Department = "Saha"
        };
        context.Users.Add(reporter);

        // EmergencyCode ekleyelim
        context.EmergencyCodes.Add(new EmergencyCodeDefinition { Code = "Red", ColorHex = "#FF0000", SeverityLevel = 1, IsActive = true });
        await context.SaveChangesAsync();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateIncidentCommandHandler(context, publisherMock.Object);
        var command = new CreateIncidentCommand(
            ReporterId: reporter.Id,
            Category: "GeçersizKategori",
            EmergencyCode: "Red",
            Description: "A Blok yangın",
            MediaAttachments: new(),
            Latitude: 40.0m,
            Longitude: 29.0m
        );

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*incident category*");
    }

    [Fact]
    public async Task GetIncidentCategories_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Seed verilerini temizleyelim
        context.IncidentCategories.RemoveRange(context.IncidentCategories);
        await context.SaveChangesAsync();

        var activeCat = new IncidentCategory { Code = "Fire", Name = "Fire Emergency", IsActive = true };
        var inactiveCat = new IncidentCategory { Code = "OldCategory", Name = "Deprecated", IsActive = false };

        context.IncidentCategories.AddRange(activeCat, inactiveCat);
        await context.SaveChangesAsync();

        var handler = new GetIncidentCategoriesQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetIncidentCategoriesQuery(), CancellationToken.None);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data[0].Code.Should().Be("Fire");
    }

    [Fact]
    public async Task CreateIncidentCategory_WithDuplicateCode_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.IncidentCategories.Add(new IncidentCategory { Code = "Fire", Name = "Fire Emergency" });
        await context.SaveChangesAsync();

        var handler = new CreateIncidentCategoryCommandHandler(context);
        var command = new CreateIncidentCategoryCommand("Fire", "Fire Emergency Duplicate", "Desc");

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task DeleteIncidentCategory_ShouldPerformSoftDelete()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var category = new IncidentCategory { Code = "Medical", Name = "Medical Emergency", IsActive = true };
        context.IncidentCategories.Add(category);
        await context.SaveChangesAsync();

        var handler = new DeleteIncidentCategoryCommandHandler(context);

        // Act
        var result = await handler.Handle(new DeleteIncidentCategoryCommand(category.Id), CancellationToken.None);

        // Assert
        result.Data.Should().BeTrue();
        var entityInDb = await context.IncidentCategories.FindAsync(category.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.IsActive.Should().BeFalse();
    }
}
