using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace TasksAppAPI.Infrastructure;

public class AppPathsProvider : IAppPathsProvider
{
    public AppPathsProvider(IWebHostEnvironment environment)
    {
        ContentRootPath = environment.ContentRootPath;
    }

    public string ContentRootPath { get; }
}
