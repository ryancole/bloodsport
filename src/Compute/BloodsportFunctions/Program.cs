using Bloodsport.Data.Sql;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BloodsportFunctions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        builder
            .Services
            .AddDbContextFactory<SqlDbContext>(options => options.UseSqlServer(builder.Configuration["SqlConnectionString"]));

        builder.Build().Run();
    }
}
