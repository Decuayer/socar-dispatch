using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Application.Features.Teams.Commands.AddTeamMember;
using SocarDispatch.Application.Features.Teams.Commands.CreateTeam;
using SocarDispatch.Application.Features.Teams.Commands.RemoveTeamMember;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeam;
using SocarDispatch.Application.Features.Teams.Queries.GetTeamById;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;

namespace SocarDispatch.Application.Tests;

public class TeamCrudTests
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
    public async Task CreateTeam_WhenValid_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var leader = new User
        {
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Email = "ahmet@socar.com",
            Phone = "+905551111111",
            PasswordHash = "hash",
            Department = "Arama Kurtarma",
            RoleType = RoleType.Team
        };
        context.Users.Add(leader);
        await context.SaveChangesAsync();

        var handler = new CreateTeamCommandHandler(context);
        var command = new CreateTeamCommand("A Blok İSG Ekibi", leader.Id, leader.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.TeamName.Should().Be("A Blok İSG Ekibi");
        result.Data.LeaderId.Should().Be(leader.Id);
        result.Data.Members.Should().ContainSingle(m => m.UserId == leader.Id);
    }

    [Fact]
    public async Task CreateTeam_WhenNameDuplicate_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Teams.Add(new Team { TeamName = "Mevcut Ekip", Status = TeamStatus.Idle });
        await context.SaveChangesAsync();

        var handler = new CreateTeamCommandHandler(context);
        var command = new CreateTeamCommand("mevcut ekip", null, Guid.NewGuid());

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateTeam_WhenLeaderIsNotTeamRole_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var employee = new User
        {
            FirstName = "Mehmet",
            LastName = "Demir",
            Email = "mehmet@socar.com",
            Phone = "+905552222222",
            PasswordHash = "hash",
            Department = "Üretim",
            RoleType = RoleType.Employee // NOT Team!
        };
        context.Users.Add(employee);
        await context.SaveChangesAsync();

        var handler = new CreateTeamCommandHandler(context);
        var command = new CreateTeamCommand("Yeni Ekip", employee.Id, employee.Id);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Designated team leader must have RoleType 'Team'*");
    }

    [Fact]
    public async Task GetTeamById_WhenTeamExists_ShouldReturnDetails()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var team = new Team { TeamName = "Yangın Müdahale Ekibi", Status = TeamStatus.Idle };
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new GetTeamByIdQueryHandler(context);
        var query = new GetTeamByIdQuery(team.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.TeamName.Should().Be("Yangın Müdahale Ekibi");
    }

    [Fact]
    public async Task AddTeamMember_WhenUserAlreadyInAnotherTeam_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var user = new User
        {
            FirstName = "Can",
            LastName = "Kaya",
            Email = "can@socar.com",
            Phone = "+905553333333",
            PasswordHash = "hash",
            Department = "İSG",
            RoleType = RoleType.Team
        };
        context.Users.Add(user);

        var team1 = new Team { TeamName = "1. Ekip", Status = TeamStatus.Idle };
        var team2 = new Team { TeamName = "2. Ekip", Status = TeamStatus.Idle };
        team1.Members.Add(new TeamMember { UserId = user.Id });
        context.Teams.AddRange(team1, team2);

        var operatorUser = new User { RoleType = RoleType.Operator, Email = "op@socar.com" };
        context.Users.Add(operatorUser);
        await context.SaveChangesAsync();

        var handler = new AddTeamMemberCommandHandler(context);
        var command = new AddTeamMemberCommand(team2.Id, operatorUser.Id, user.Id);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already an active member*");
    }

    [Fact]
    public async Task RemoveTeamMember_WhenMemberIsLeader_ShouldThrowDomainException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var leader = new User
        {
            FirstName = "Serkan",
            LastName = "Kaya",
            Email = "serkan@socar.com",
            Phone = "+905554444444",
            PasswordHash = "hash",
            Department = "İSG",
            RoleType = RoleType.Team
        };
        context.Users.Add(leader);

        var team = new Team { TeamName = "Liderli Ekip", LeaderId = leader.Id, Status = TeamStatus.Idle };
        team.Members.Add(new TeamMember { UserId = leader.Id });
        context.Teams.Add(team);

        var operatorUser = new User { RoleType = RoleType.Operator, Email = "op2@socar.com" };
        context.Users.Add(operatorUser);
        await context.SaveChangesAsync();

        var handler = new RemoveTeamMemberCommandHandler(context);
        var command = new RemoveTeamMemberCommand(team.Id, operatorUser.Id, leader.Id);

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*The team leader cannot be removed from the team directly*");
    }
}
