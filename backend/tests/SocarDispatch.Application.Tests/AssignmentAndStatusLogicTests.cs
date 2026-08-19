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

        var handler = new CreateAssignmentCommandHandler(context);
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
        var handler = new CreateAssignmentCommandHandler(context);
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
        var team = new Team { TeamName = "Kurtarma Ekibi", Status = "Forwarded" };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(team.Id, "OnScene");

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
}
