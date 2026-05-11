using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Weather.Infrastructure.Tests.TestSupport;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Tests";

    public string ApplicationName { get; set; } = "Weather.Infrastructure.Tests";

    public string ContentRootPath { get; set; } = string.Empty;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    public static TestHostEnvironment ForContentRoot(string contentRootPath)
    {
        return new TestHostEnvironment
        {
            ContentRootPath = contentRootPath
        };
    }
}
