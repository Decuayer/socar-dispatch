using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.CreateEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.DeleteEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.Commands.UpdateEmergencyCode;
using SocarDispatch.Application.Features.EmergencyCodes.Queries.GetEmergencyCodes;
using SocarDispatch.Application.Features.Incidents.Commands.CreateIncident;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;
using MediatR;
using Moq;

namespace SocarDispatch.Application.Tests;

public class EmergencyCodeTests
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
    public async Task CreateIncident_WithNonExistentOrInactiveEmergencyCode_ShouldThrowDomainException()
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
        await context.SaveChangesAsync();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateIncidentCommandHandler(context, publisherMock.Object);
        var command = new CreateIncidentCommand(
            ReporterId: reporter.Id,
            Category: "Fire",
            EmergencyCode: "GeçersizKod",
            Description: "A Blok Fire",
            MediaAttachments: new(),
            Latitude: 40.0m,
            Longitude: 29.0m
        );

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*emergency code*");
    }

    [Fact]
    public async Task GetEmergencyCodes_ShouldReturnOnlyActiveCodes_OrderedBySeverity()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        // Seed verilerini temizleyip testi bağımsız hale getirelim
        context.EmergencyCodes.RemoveRange(context.EmergencyCodes);
        await context.SaveChangesAsync();

        var activeCode1 = new EmergencyCodeDefinition
        {
            Code = "Green",
            ColorHex = "#34C759",
            SeverityLevel = 3,
            IsActive = true
        };
        var activeCode2 = new EmergencyCodeDefinition
        {
            Code = "Red",
            ColorHex = "#FF3B30",
            SeverityLevel = 1,
            IsActive = true
        };
        var inactiveCode = new EmergencyCodeDefinition
        {
            Code = "Blue",
            ColorHex = "#0000FF",
            SeverityLevel = 2,
            IsActive = false
        };

        context.EmergencyCodes.AddRange(activeCode1, activeCode2, inactiveCode);
        await context.SaveChangesAsync();

        var handler = new GetEmergencyCodesQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetEmergencyCodesQuery(), CancellationToken.None);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data[0].Code.Should().Be("Red");   // Severity 1
        result.Data[1].Code.Should().Be("Green"); // Severity 3
        result.Data.Should().NotContain(c => c.Code == "Blue");
    }

    [Fact]
    public async Task DeleteEmergencyCode_ShouldPerformSoftDelete()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var code = new EmergencyCodeDefinition
        {
            Code = "Orange",
            ColorHex = "#FF9500",
            SeverityLevel = 2,
            IsActive = true
        };
        context.EmergencyCodes.Add(code);
        await context.SaveChangesAsync();

        var handler = new DeleteEmergencyCodeCommandHandler(context);

        // Act
        var result = await handler.Handle(new DeleteEmergencyCodeCommand(code.Id), CancellationToken.None);

        // Assert
        result.Data.Should().BeTrue();
        var entityInDb = await context.EmergencyCodes.FindAsync(code.Id);
        entityInDb.Should().NotBeNull();
        entityInDb!.IsActive.Should().BeFalse();
    }
}
