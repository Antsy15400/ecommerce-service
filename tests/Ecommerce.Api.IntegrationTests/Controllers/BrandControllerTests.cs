using Ecommerce.APi.Dtos.Brand;
using Ecommerce.Core.Entities;

namespace Ecommerce.Api.IntegrationTests.Controllers;

[Collection("BaseIntegrationTestCollection")]
public class BrandController
{
    readonly BaseIntegrationTest _baseIntegrationTest;

    readonly string endPointPath = "api/brand/";
    
    public BrandController(BaseIntegrationTest baseIntegrationTest)
    {
        _baseIntegrationTest = baseIntegrationTest;
    }

    [Fact]
    public async Task GetAllBrands_ShouldReturnAListOfBrands()
    {
        // Act
        var response = await _baseIntegrationTest.DefaultUserHttpClient.GetAsync(endPointPath + "getall");
    
        var responseReaded = await response.Content.ReadAsStringAsync();
        
        List<Brand> listOfBrands = JsonConvert.DeserializeObject<List<Brand>>(responseReaded);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        listOfBrands.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBrandById_ShouldReturnBadRequest_WhenInvalidIdIsSending()
    {
        // Arrange
        int invalidId = 0;

        // Act
        var response = await _baseIntegrationTest.DefaultUserHttpClient.GetAsync(endPointPath + $"getbyid/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBrandById_ShouldReturnNotFound_WhenUnExistingBrandIdIsSending()
    {
        // Arrange
        int randomId = 100_000_000;

        // Act
        var response = await _baseIntegrationTest.DefaultUserHttpClient.GetAsync(endPointPath + $"getbyid/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBrandById_ShouldReturnOkWithASingleBrand_WhenValidIdIsSending()
    {
        // Arrange
        using var db = _baseIntegrationTest.EcommerceProgram.CreateApplicationDbContext();

        var dbBrand = db.Brands.First();

        // Act
        var response = await _baseIntegrationTest.DefaultUserHttpClient.GetAsync(endPointPath + $"getbyid/{dbBrand.Id}");
        
        var responseReaded = await response.Content.ReadAsStringAsync();
        
        var requestBrand = JsonConvert.DeserializeObject<Ecommerce.Core.Entities.Brand>(responseReaded);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        requestBrand.Should().BeEquivalentTo(dbBrand);
    }

    [Fact]
    public async Task CreateBrand_ShouldRedirectToRoute_WhenValidBrandIsSending()
    {
        // Arrange
        CreateBrandRequest validDto = new("test", true);

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.PostAsJsonAsync(endPointPath + "create", validDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Found);
    }

    [Fact]
    public async Task EditBrand_ShouldReturnBadRequest_WhenInvalidIdIsSending()
    {
        // Arrange
        int invalidId = 0;

        EditBrandRequest dto = new(default!, default);

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.PutAsJsonAsync(endPointPath + $"edit/{invalidId}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EditBrand_ShouldReturnNotFound_WhenUnExistingBrandIdIsSending()
    {
        // Arrange
        int randomId = 100_000_000;

        EditBrandRequest dto = new("", true);

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.PutAsJsonAsync(endPointPath + $"edit/{randomId}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EditBrand_ShouldReturnNoContent_WhenValidIdIsSending()
    {
        // Arrange
        using var db = _baseIntegrationTest.EcommerceProgram.CreateApplicationDbContext();
        
        Brand dbBrand = db.Brands.First();

        EditBrandRequest brandDto = new("test", true);

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.PutAsJsonAsync(endPointPath + $"edit/{dbBrand.Id}", brandDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteBrand_ShouldReturnBadRequest_WhenInvalidIdIsSending()
    {
        // Arrange
        int invalidId = 0;

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.DeleteAsync(endPointPath + $"delete/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteBrand_ShouldReturnNotFound_WhenUnExistingBrandIdIsSending()
    {
        // Arrange
        int randomId = 100_000_000;

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.DeleteAsync(endPointPath + $"delete/{randomId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBrand_ShouldReturnNoContent_WhenValidBrandIdIsSending()
    {
        // Arrange
        using var db = _baseIntegrationTest.EcommerceProgram.CreateApplicationDbContext();
        
        var dbBrand = db.Brands.OrderBy(b => b).Last();

        // Act
        var response = await _baseIntegrationTest.AdminUserHttpClient.DeleteAsync(endPointPath + $"delete/{dbBrand.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}