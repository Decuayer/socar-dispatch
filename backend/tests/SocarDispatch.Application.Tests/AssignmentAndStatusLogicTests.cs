using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Features.Assignments.Commands.CreateAssignment;
using SocarDispatch.Application.Features.Incidents.Commands.ChangeIncidentStatus;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;
using MediatR;
using Moq;
using SocarDispatch.Domain.Events;

namespace SocarDispatch.Application.Tests;

public class AssignmentAndStatusLogicTests
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

    // 1. ASSIGNMENT BUSINESS RULE TEST (Incident -> Assigned, Team -> Forwarded)

    [Fact]
    public async Task CreateAssignment_ShouldLinkEntities_AndTransitionStatusesCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var operatorUser = new User
        {
            FirstName = "Dispatch",
            LastName = "Operatörü",
            Email = "operator@socar.com",
            Phone = "+905559998877",
            PasswordHash = "hash",
            Department = "Dispatch Center",
            RoleType = RoleType.Operator
        };

        var reporter = new User
        {
            FirstName = "Saha",
            LastName = "Çalışanı",
            Email = "worker@socar.com",
            Phone = "+905551112233",
            PasswordHash = "hash",
            Department = "Saha",
            RoleType = RoleType.Employee
        };

        context.Users.AddRange(operatorUser, reporter);
        await context.SaveChangesAsync();

        var team = new Team
        {
            TeamName = "A Blok İSG Ekibi",
            Status = "Idle"
        };
        context.Teams.Add(team);

        var incident = new Incident
        {
            ReporterId = reporter.Id,
            Category = "Yangın",
            EmergencyCode = "Kırmızı Kod",
            Latitude = 40.99m,
            Longitude = 29.02m,
            Location = new NetTopologySuite.Geometries.Point(29.02, 40.99) { SRID = 4326 },
            Status = "Open"
        };
        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateAssignmentCommandHandler(context, publisherMock.Object);
        var command = new CreateAssignmentCommand(incident.Id, team.Id, operatorUser.Id);

        // Act (Operator Team Dispatches to the Incident)
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        result.Data.TeamName.Should().Be("A Blok İSG Ekibi");

        // Veritabanı Durum Kontrolleri
        var updatedIncident = await context.Incidents.FindAsync(incident.Id);
        updatedIncident!.Status.Should().Be("Assigned"); // Event became 'Assigned'

        var updatedTeam = await context.Teams.FindAsync(team.Id);
        updatedTeam!.Status.Should().Be("Forwarded"); // Team status changed to 'Redirected'

        var assignmentExists = await context.Assignments.AnyAsync(a => a.IncidentId == incident.Id && a.TeamId == team.Id);
        assignmentExists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAssignment_WithNonExistingIncident_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();
        var handler = new CreateAssignmentCommandHandler(context, publisherMock.Object);
        var command = new CreateAssignmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // 2. Team Status Update Test (Idle -> OnScene)
    [Fact]
    public async Task UpdateTeamStatus_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var operatorUser = new User
        {
            FirstName = "Operator",
            LastName = "User",
            Email = "opstatus@socar.com",
            Phone = "+905559990011",
            PasswordHash = "hash",
            Department = "Dispatch",
            RoleType = RoleType.Operator
        };
        context.Users.Add(operatorUser);
        var team = new Team { TeamName = "Kurtarma Ekibi", Status = "Forwarded" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();
        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(team.Id, operatorUser.Id, "OnScene");
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        result.Data.Status.Should().Be("OnScene");
        var updatedTeam = await context.Teams.FindAsync(team.Id);
        updatedTeam!.Status.Should().Be("OnScene");
    }

    // 3. INCIDENT STATUS UPDATE TEST (Assigned -> Resolved)    
    [Fact]
    public async Task ChangeIncidentStatus_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var reporter = new User { FirstName = "Ali", LastName = "Veli", Email = "ali@socar.com", Phone = "+905000000000", PasswordHash = "p", Department = "D", RoleType = RoleType.Employee };
        context.Users.Add(reporter);
        await context.SaveChangesAsync();

        var incident = new Incident 
        { 
            ReporterId = reporter.Id, 
            Category = "Yaralanma", 
            EmergencyCode = "Sarı Kod", 
            Latitude = 40.0m, 
            Longitude = 29.0m, 
            Location = new NetTopologySuite.Geometries.Point(29.0, 40.0) { SRID = 4326 },
            Status = "Assigned" 
        };        
        
        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        var handler = new ChangeIncidentStatusCommandHandler(context);
        var command = new ChangeIncidentStatusCommand(incident.Id, "Resolved");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Data.Status.Should().Be("Resolved");
        var updatedIncident = await context.Incidents.FindAsync(incident.Id);
        updatedIncident!.Status.Should().Be("Resolved");
    }

    [Fact]
    public async Task CreateAssignment_WhenTeamAlreadyHasActiveAssignment_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var operatorUser = new User { FirstName = "Op", LastName = "User", Email = "op@socar.com", Phone = "+905000000001", PasswordHash = "p", Department = "D", RoleType = RoleType.Operator };
        var reporter = new User { FirstName = "Rep", LastName = "User", Email = "rep@socar.com", Phone = "+905000000002", PasswordHash = "p", Department = "D", RoleType = RoleType.Employee };
        context.Users.AddRange(operatorUser, reporter);

        var team = new Team { TeamName = "Alpha Team", Status = "Forwarded" };
        context.Teams.Add(team);

        var activeIncident = new Incident { ReporterId = reporter.Id, Category = "Gas", EmergencyCode = "Yellow", Latitude = 40, Longitude = 29, Location = new NetTopologySuite.Geometries.Point(29.0, 40.0) { SRID = 4326 }, Status = "Assigned" };
        var newIncident = new Incident { ReporterId = reporter.Id, Category = "Fire", EmergencyCode = "Red", Latitude = 40, Longitude = 29, Location = new NetTopologySuite.Geometries.Point(29.0, 40.0) { SRID = 4326 }, Status = "Open" };
        context.Incidents.AddRange(activeIncident, newIncident);
        await context.SaveChangesAsync();

        context.Assignments.Add(new Assignment { IncidentId = activeIncident.Id, TeamId = team.Id, OperatorId = operatorUser.Id, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var handler = new CreateAssignmentCommandHandler(context, publisherMock.Object);
        var command = new CreateAssignmentCommand(newIncident.Id, team.Id, operatorUser.Id);

        // Act & Assert (Aktif ataması olan ekibe tekrar atama yapılamamalı)
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("The selected team is already assigned to an active incident.");
    }

    [Fact]
    public async Task CreateAssignment_WhenPreviousAssignmentIsResolved_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var operatorUser = new User { FirstName = "Op", LastName = "User", Email = "op2@socar.com", Phone = "+905000000003", PasswordHash = "p", Department = "D", RoleType = RoleType.Operator };
        var reporter = new User { FirstName = "Rep", LastName = "User", Email = "rep2@socar.com", Phone = "+905000000004", PasswordHash = "p", Department = "D", RoleType = RoleType.Employee };
        context.Users.AddRange(operatorUser, reporter);

        var team = new Team { TeamName = "Beta Team", Status = "Idle" };
        context.Teams.Add(team);

        // Önceki olay çözülmüş (Resolved)
        var resolvedIncident = new Incident { ReporterId = reporter.Id, Category = "Gas", EmergencyCode = "Yellow", Latitude = 40, Longitude = 29, Location = new NetTopologySuite.Geometries.Point(29.0, 40.0) { SRID = 4326 }, Status = "Resolved" };
        var newIncident = new Incident { ReporterId = reporter.Id, Category = "Fire", EmergencyCode = "Red", Latitude = 40, Longitude = 29, Location = new NetTopologySuite.Geometries.Point(29.0, 40.0) { SRID = 4326 }, Status = "Open" };
        context.Incidents.AddRange(resolvedIncident, newIncident);
        await context.SaveChangesAsync();

        context.Assignments.Add(new Assignment { IncidentId = resolvedIncident.Id, TeamId = team.Id, OperatorId = operatorUser.Id, AssignedAt = DateTime.UtcNow.AddHours(-1) });
        await context.SaveChangesAsync();

        var handler = new CreateAssignmentCommandHandler(context, publisherMock.Object);
        var command = new CreateAssignmentCommand(newIncident.Id, team.Id, operatorUser.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert (Çözülmüş olayın ataması engellememeli)
        result.Should().NotBeNull();
        result.Data.Should().NotBeNull();
        publisherMock.Verify(p => p.Publish(It.IsAny<AssignmentCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

}
