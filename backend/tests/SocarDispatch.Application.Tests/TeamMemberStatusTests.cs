using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamMemberStatus;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using SocarDispatch.Domain.Events;
using SocarDispatch.Domain.Exceptions;
using SocarDispatch.Infrastructure.Persistence;
using Xunit;

namespace SocarDispatch.Application.Tests;

public class TeamMemberStatusTests
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
    public async Task UpdateTeamMemberStatus_WhenTeamRoleUpdatesOwnStatus_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var user = new User
        {
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Email = "ahmet@socar.com",
            Phone = "+905551112233",
            PasswordHash = "hash",
            Department = "Arama Kurtarma",
            RoleType = RoleType.Team
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var team = new Team { TeamName = "Alpha Ekibi", Status = TeamStatus.Idle };
        var member = new TeamMember
        {
            TeamId = team.Id,
            UserId = user.Id,
            MemberStatus = TeamMemberStatus.Available,
            JoinedAt = DateTime.UtcNow
        };
        team.Members.Add(member);
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamMemberStatusCommandHandler(context, publisherMock.Object);
        var command = new UpdateTeamMemberStatusCommand(team.Id, user.Id, user.Id, "EnRoute");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.MemberStatus.Should().Be("EnRoute");
        result.Data.StatusUpdatedAt.Should().NotBeNull();

        publisherMock.Verify(p => p.Publish(
            It.Is<TeamMemberStatusChangedEvent>(e =>
                e.TeamId == team.Id &&
                e.UserId == user.Id &&
                e.NewStatus == TeamMemberStatus.EnRoute
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateTeamMemberStatus_WhenTeamRoleUpdatesAnotherMembersStatus_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var teamUser1 = new User
        {
            FirstName = "Mehmet",
            LastName = "Demir",
            Email = "mehmet@socar.com",
            Phone = "+905552223344",
            PasswordHash = "hash",
            Department = "İtfaiye",
            RoleType = RoleType.Team
        };
        var teamUser2 = new User
        {
            FirstName = "Ali",
            LastName = "Kaya",
            Email = "ali@socar.com",
            Phone = "+905553334455",
            PasswordHash = "hash",
            Department = "İtfaiye",
            RoleType = RoleType.Team
        };
        context.Users.AddRange(teamUser1, teamUser2);
        await context.SaveChangesAsync();

        var team = new Team { TeamName = "Bravo Ekibi", Status = TeamStatus.Idle };
        team.Members.Add(new TeamMember { TeamId = team.Id, UserId = teamUser1.Id });
        team.Members.Add(new TeamMember { TeamId = team.Id, UserId = teamUser2.Id });
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamMemberStatusCommandHandler(context, publisherMock.Object);
        // teamUser1, teamUser2'nin durumunu değiştirmeye çalışıyor!
        var command = new UpdateTeamMemberStatusCommand(team.Id, teamUser2.Id, teamUser1.Id, "OnScene");

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Yalnızca kendi durumunu güncelleyebilirsin.");
    }

    [Fact]
    public async Task UpdateTeamMemberStatus_WhenOperatorRoleUpdatesAnyMembersStatus_ShouldSucceed()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var operatorUser = new User
        {
            FirstName = "Operator",
            LastName = "Zeynep",
            Email = "zeynep@socar.com",
            Phone = "+905554445566",
            PasswordHash = "hash",
            Department = "Merkez Kontrol",
            RoleType = RoleType.Operator
        };
        var teamUser = new User
        {
            FirstName = "Veli",
            LastName = "Şahin",
            Email = "veli@socar.com",
            Phone = "+905555556677",
            PasswordHash = "hash",
            Department = "Arama Kurtarma",
            RoleType = RoleType.Team
        };
        context.Users.AddRange(operatorUser, teamUser);
        await context.SaveChangesAsync();

        var team = new Team { TeamName = "Charlie Ekibi", Status = TeamStatus.Idle };
        team.Members.Add(new TeamMember { TeamId = team.Id, UserId = teamUser.Id, MemberStatus = TeamMemberStatus.Available });
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamMemberStatusCommandHandler(context, publisherMock.Object);
        var command = new UpdateTeamMemberStatusCommand(team.Id, teamUser.Id, operatorUser.Id, "Unavailable");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Data.MemberStatus.Should().Be("Unavailable");
    }

    [Fact]
    public void UpdateTeamMemberStatusCommandValidator_WhenStatusIsInvalid_ShouldFailValidation()
    {
        // Arrange
        var validator = new UpdateTeamMemberStatusCommandValidator();
        var command = new UpdateTeamMemberStatusCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "InvalidStatusValue");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Status");
    }

    [Fact]
    public async Task UpdateTeamMemberStatus_WhenMemberNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var publisherMock = new Mock<IPublisher>();

        var operatorUser = new User
        {
            FirstName = "Operator",
            LastName = "Test",
            Email = "op@socar.com",
            Phone = "+905559998877",
            PasswordHash = "hash",
            Department = "Merkez",
            RoleType = RoleType.Operator
        };
        context.Users.Add(operatorUser);
        await context.SaveChangesAsync();

        var handler = new UpdateTeamMemberStatusCommandHandler(context, publisherMock.Object);
        var command = new UpdateTeamMemberStatusCommand(Guid.NewGuid(), Guid.NewGuid(), operatorUser.Id, "EnRoute");

        // Act & Assert
        var act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
