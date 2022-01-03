using EasyDesk.CleanArchitecture.Application.Environment;
using Microsoft.AspNetCore.Hosting;

namespace EasyDesk.CleanArchitecture.Infrastructure.Environment;

public class EnvironmentInfo : IEnvironmentInfo
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public EnvironmentInfo(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public string EnvironmentName => _webHostEnvironment.EnvironmentName;
}
