namespace Ecommerce.Api.IntegrationTests.Controllers.Basket;

[Collection("BaseIntegrationTestCollection")]
public class IncreaseProductQuantityTests
{
    BaseIntegrationTest _baseIntegrationTest;
    string endPointPath = "api/basket/increaseproductquantity?productId=";
    string addProductEndPointPath = "api/basket/addproduct?productId=";

    public IncreaseProductQuantityTests(BaseIntegrationTest baseIntegrationTest)
    {
        this._baseIntegrationTest = baseIntegrationTest;
    }

    [Fact]
    public async Task WithNoSpecificProductInBasket_ShouldReturnBadRequest()
    {
        int invalidProductId = 0;
        var response = await _baseIntegrationTest.DefaultUserHttpClient.PostAsync(endPointPath + invalidProductId.ToString(), null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WithSpecificProductInBasket_ShoulReturnOk()
    {
        using var db = _baseIntegrationTest.EcommerceProgram.CreateApplicationDbContext();
        int validProductId = db.Products.Select(p => p.Id).First();
        var _ = await _baseIntegrationTest.DefaultUserHttpClient.PostAsync(addProductEndPointPath + validProductId.ToString(), null);

        var response = await _baseIntegrationTest.DefaultUserHttpClient.PostAsync(endPointPath + validProductId.ToString(), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}