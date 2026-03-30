using NUnit.Framework;

namespace TaxManager.IntegrationTests;

public abstract class TestBase
{
    public TestingWebApplicationFactory<Program> _webApplicationFactory;

    [SetUp]
    public void Setup()
    {
        _webApplicationFactory = new TestingWebApplicationFactory<Program>();
    }

    [TearDown]
    public void TearDown()
    {
        _webApplicationFactory.Dispose();
    }
}
