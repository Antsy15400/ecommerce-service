using Ecommerce.Infrastructure.Options;
using Ecommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.UnitTests.Services;

public class CloudinaryServiceTest
{
    private readonly CloudinaryOptions cloudinaryOptions = new()
    {
        ApiKey = "ds9f7sdf987sd9f87sdf",
        ApiSecret = "ss9df87s9d8f7s9df87s9df",
        CloudName = "s98f7s9d8f7s"
    };
    private readonly IOptions<CloudinaryOptions> cloudinaryIOptions;
    private readonly Mock<CloudinaryService> cloudinaryServiceMock;

    public CloudinaryServiceTest()
    {
        cloudinaryIOptions = Microsoft.Extensions.Options.Options.Create<CloudinaryOptions>(cloudinaryOptions);
        cloudinaryServiceMock = new Mock<CloudinaryService>(cloudinaryIOptions);
    }

    [Fact]
    public void ShouldImplementICloudinaryService()
    {
        typeof(CloudinaryService).Should().BeAssignableTo<ICloudinaryService>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void DeleteImage_WithInvalidImageName_ShouldThrowInvalidOperationException(string imageName)
    {
        Func<Task> act = () => cloudinaryServiceMock.Object.DeleteImage(imageName);

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetImage_WithInvalidImageName_ShouldThrowInvalidOperationException(string imageName)
    {
        Func<Task> act = () => cloudinaryServiceMock.Object.GetImage(imageName);

        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UploadImage_WithInvalidImageName_ShouldThrowInvalidOperationException(string imageName)
    {
        Func<Task> act = () => cloudinaryServiceMock.Object.UploadImage(It.IsAny<IFormFile>(), imageName);

        act.Should().ThrowAsync<InvalidOperationException>();
    }
}