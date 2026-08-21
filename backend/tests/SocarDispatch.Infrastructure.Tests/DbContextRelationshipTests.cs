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
        var team = new Team { TeamName = "Fire Müdahale" };

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

    // 3. CASCADE DELETE TEST (IncidentMedia records are deleted when Incident is deleted)
    [Fact]
    public async Task DeleteIncident_ShouldCascadeDelete_IncidentMedia()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var reporter = new User { FirstName = "Mehmet", LastName = "Saha", Email = "m.saha@socar.com", Phone = "+905001119988", PasswordHash = "p", Department = "D", RoleType = RoleType.Employee };
        context.Users.Add(reporter);
        await context.SaveChangesAsync();

        var incident = new Incident
        {
            ReporterId = reporter.Id,
            Category = "Fire",
            EmergencyCode = "Kırmızı Kod",
            Latitude = 40.99m,
            Longitude = 29.02m,
            Location = new NetTopologySuite.Geometries.Point(29.02, 40.99) { SRID = 4326 },
            MediaAttachments = new List<IncidentMedia>
            {
                new IncidentMedia { MediaUrl = "http://minio/fire1.jpg", MediaType = MediaType.Photo },
                new IncidentMedia { MediaUrl = "http://minio/fire2.mp4", MediaType = MediaType.Video }
            }
        };


        context.Incidents.Add(incident);
        await context.SaveChangesAsync();

        // Act (Delete Incident)
        context.Incidents.Remove(incident);
        await context.SaveChangesAsync();

        // Assert
        var mediaExists = await context.IncidentMedia.AnyAsync(m => m.IncidentId == incident.Id);
        mediaExists.Should().BeFalse(); // Cascade Delete verified
    }
}