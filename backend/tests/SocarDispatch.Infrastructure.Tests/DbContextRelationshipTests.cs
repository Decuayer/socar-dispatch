using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SocarDispatch.Domain.Entities;
using SocarDispatch.Domain.Enums;
using Xunit;

namespace SocarDispatch.Infrastructure.Tests;

public class DbContextRelationshipTests
{
    // 1. CASCADE DELETE TEST (Member is deleted when Team is deleted)
    [Fact]
    public async Task DeleteTeam_ShouldCascadeDelete_TeamMembers()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var user = new User { FirstName = "Ali", LastName = "Veli", Email = "ali@socar.com", Phone = "+905001112233", PasswordHash = "p", Department = "D", RoleType = RoleType.Team };
        var team = new Team { TeamName = "Kurtarma Ekibi" };

        context.Users.Add(user);
        context.Teams.Add(team);
        await context.SaveChangesAsync();

        context.TeamMembers.Add(new TeamMember { TeamId = team.Id, UserId = user.Id });
        await context.SaveChangesAsync();

        // Act (Delete Team)
        context.Teams.Remove(team);
        await context.SaveChangesAsync();

        // Assert
        var memberExists = await context.TeamMembers.AnyAsync(tm => tm.TeamId == team.Id);
        memberExists.Should().BeFalse(); // Cascade Delete completed
    }

    // 2. SET NULL TEST (Team.LeaderId becomes null when the leader is deleted)

    [Fact]
    public async Task DeleteLeaderUser_ShouldSetTeamLeaderIdToNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var leader = new User { FirstName = "Lider", LastName = "Kaptan", Email = "lider@socar.com", Phone = "+905009998877", PasswordHash = "p", Department = "D", RoleType = RoleType.Team };
        var team = new Team { TeamName = "Yangın Müdahale" };

        context.Users.Add(leader);
        await context.SaveChangesAsync();
        team.LeaderId = leader.Id;

        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Act (Delete Leader User)
        context.Users.Remove(leader);
        await context.SaveChangesAsync();

        // Assert
        var updatedTeam = await context.Teams.FindAsync(team.Id);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.LeaderId.Should().BeNull(); // SetNull completed
    }

}