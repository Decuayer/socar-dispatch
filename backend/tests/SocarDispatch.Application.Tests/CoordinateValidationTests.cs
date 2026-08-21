using FluentAssertions;
using SocarDispatch.Application.Features.Incidents.Commands.CreateIncident;
using SocarDispatch.Application.Features.Teams.Commands.UpdateTeamLocation;
using Xunit;

namespace SocarDispatch.Application.Tests;

public class CoordinateValidationTests
{
    private readonly CreateIncidentCommandValidator _incidentValidator = new();
    private readonly UpdateTeamLocationCommandValidator _teamLocationValidator = new();

    // 1. POSITIVE TESTS (Valid Coordinates)
    [Fact]
    public void CreateIncidentCommand_WithValidCoordinates_ShouldPassValidation()
    {
        // Arrange (İstanbul/Kocaeli civarı geçerli koordinatlar)
        var command = new CreateIncidentCommand(
            ReporterId: Guid.NewGuid(),
            Category: "Fire",
            EmergencyCode: "Kırmızı Kod",
            Description: "A Blok Fire",
            MediaAttachments: new(),
            Latitude: 40.991234m,
            Longitude: 29.023456m
        );

        // Act
        var result = _incidentValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void UpdateTeamLocationCommand_WithValidCoordinates_ShouldPassValidation()
    {
        // Arrange
        var command = new UpdateTeamLocationCommand(
            TeamId: Guid.NewGuid(),
            Latitude: 41.0082m,
            Longitude: 28.9784m
        );
        // Act
        var result = _teamLocationValidator.Validate(command);
        // Assert
        result.IsValid.Should().BeTrue();
    }

    // 2. NEGATIVE TESTS (Invalid Latitude / Latitude > 90 or < -90)
    [Theory]
    [InlineData(91.0)]
    [InlineData(120.5)]
    [InlineData(-90.1)]
    [InlineData(-180.0)]
    public void CreateIncidentCommand_WithInvalidLatitude_ShouldFailValidation(decimal invalidLatitude)
    {
        // Arrange
        var command = new CreateIncidentCommand(
            ReporterId: Guid.NewGuid(),
            Category: "Fire",
            EmergencyCode: "Sarı Kod",
            Description: "Tesis gaz kokusu",
            MediaAttachments: new(),
            Latitude: invalidLatitude, // Invalid Latitude
            Longitude: 29.0m
        );
        // Act
        var result = _incidentValidator.Validate(command);
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Latitude));
    }

    // 3. NEGATIVE TESTS (Invalid Longitude / Longitude > 180 or < -180)
    [Theory]
    [InlineData(180.1)]
    [InlineData(200.0)]
    [InlineData(-180.1)]
    [InlineData(-250.0)]
    public void UpdateTeamLocationCommand_WithInvalidLongitude_ShouldFailValidation(decimal invalidLongitude)
    {
        // Arrange
        var command = new UpdateTeamLocationCommand(
            TeamId: Guid.NewGuid(),
            Latitude: 40.0m,
            Longitude: invalidLongitude // Invalid Longitude
        );
        // Act
        var result = _teamLocationValidator.Validate(command);
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Longitude));
    }

    [Theory]
    [InlineData("Fire")]
    [InlineData("Medical")]
    [InlineData("Security")]
    [InlineData("Environmental")]
    [InlineData("Chemical")]
    public void CreateIncidentCommand_WithAllowedCategory_ShouldPassValidation(string category)
    {
        var command = new CreateIncidentCommand(
            ReporterId: Guid.NewGuid(),
            Category: category,
            EmergencyCode: "Red",
            Description: "Test incident",
            MediaAttachments: new(),
            Latitude: 40.0m,
            Longitude: 29.0m
        );

        var result = _incidentValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    public void CreateIncidentCommand_WithEmptyCategory_ShouldFailValidation(string invalidCategory)
    {
        var command = new CreateIncidentCommand(
            ReporterId: Guid.NewGuid(),
            Category: invalidCategory,
            EmergencyCode: "Red",
            Description: "Test incident",
            MediaAttachments: new(),
            Latitude: 40.0m,
            Longitude: 29.0m
        );

        var result = _incidentValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Category));
    }


}

