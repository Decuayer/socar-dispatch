using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using SocarDispatch.Application.Features.Media.Commands.UploadMedia;
using Xunit;

namespace SocarDispatch.Application.Tests;

public class UploadMediaCommandValidatorTests
{
    private readonly UploadMediaCommandValidator _validator = new();

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("video/mp4")]
    public void Should_Pass_When_ContentType_Is_Allowed(string contentType)
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        var command = new UploadMediaCommand(fileMock.Object, "incident");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.File);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("text/plain")]
    [InlineData("application/x-msdownload")]
    public void Should_Fail_When_ContentType_Is_Not_Allowed(string contentType)
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        var command = new UploadMediaCommand(fileMock.Object, "incident");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File)
              .WithErrorMessage("Unsupported file type. Only JPEG, PNG, and MP4 formats are accepted.");
    }

    [Fact]
    public void Should_Fail_When_FileSize_Exceeds_50MB()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(50 * 1024 * 1024 + 1); // 50MB + 1 byte
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var command = new UploadMediaCommand(fileMock.Object, "incident");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File)
              .WithErrorMessage("The file size exceeds the 50MB limit.");
    }
}
