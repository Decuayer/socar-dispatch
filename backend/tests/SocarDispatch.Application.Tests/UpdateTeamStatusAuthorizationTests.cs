using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;

namespace SocarDispatch.Application.Tests;

public class UpdateTeamStatusAuthorizationTests
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
    public async Task UpdateTeamStatus_WhenRequesterIsTeamMember_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var teamMember = new User
        {
            FirstName = "Ali",
            LastName = "Veli",
            Email = "ali@socar.com",
            Phone = "+905551112233",
            PasswordHash = "hash",
            Department = "Arama Kurtarma",
            RoleType = RoleType.Team
        };
        context.Users.Add(teamMember);
        await context.SaveChangesAsync();

        var team = new Team { TeamName = "A Ekibi", Status = TeamStatus.Idle };
        team.Members.Add(new TeamMember { UserId = teamMember.Id });
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(team.Id, teamMember.Id, "OnScene");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Status.Should().Be("OnScene");
    }

    [Fact]
    public async Task UpdateTeamStatus_WhenRequesterIsTeamLeader_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var leaderUser = new User
        {
            FirstName = "Hasan",
            LastName = "Hüseyin",
            Email = "hasan@socar.com",
            Phone = "+905552223344",
            PasswordHash = "hash",
            Department = "İSG",
            RoleType = RoleType.Team
        };
        context.Users.Add(leaderUser);
        await context.SaveChangesAsync();

        var team = new Team { TeamName = "B Ekibi", LeaderId = leaderUser.Id, Status = TeamStatus.Idle };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(team.Id, leaderUser.Id, "Busy");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Status.Should().Be("Busy");
    }

    [Fact]
    public async Task UpdateTeamStatus_WhenRequesterIsOperator_ShouldSucceedWithoutMembership()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var operatorUser = new User
        {
            FirstName = "Operator",
            LastName = "Zeynep",
            Email = "zeynep@socar.com",
            Phone = "+905553334455",
            PasswordHash = "hash",
            Department = "Merkez",
            RoleType = RoleType.Operator
        };
        context.Users.Add(operatorUser);

        var team = new Team { TeamName = "C Ekibi", Status = TeamStatus.Idle };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(team.Id, operatorUser.Id, "Forwarded");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.Status.Should().Be("Forwarded");
    }

    [Fact]
    public async Task UpdateTeamStatus_WhenRequesterIsDifferentTeamMember_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        var teamUser = new User
        {
            FirstName = "Mustafa",
            LastName = "Can",
            Email = "mustafa@socar.com",
            Phone = "+905554445566",
            PasswordHash = "hash",
            Department = "İtfaiye",
            RoleType = RoleType.Team
        };
        context.Users.Add(teamUser);

        var targetTeam = new Team { TeamName = "Hedef Ekip", Status = TeamStatus.Idle };
        context.Teams.Add(targetTeam);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamStatusCommandHandler(context);
        var command = new UpdateTeamStatusCommand(targetTeam.Id, teamUser.Id, "Busy"); // Ekip üyesi ama bu ekipte değil!

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("You are only authorized to update the status of your assigned team.");
    }
}
