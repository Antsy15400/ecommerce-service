using Ecommerce.Infrastructure.EmailSender;

namespace Ecommerce.Infrastructure.UnitTests.Services;

public class EmailServiceTest
{
    [Fact]
    public void ShouldImplementIEmailService()
    {
        typeof(SendiblueService).Should().BeAssignableTo<IEmailSender>();
    }
}