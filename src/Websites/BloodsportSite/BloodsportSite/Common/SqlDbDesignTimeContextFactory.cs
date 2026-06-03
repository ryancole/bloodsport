using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Bloodsport.Data.Sql;

namespace BloodsportSite.Common
{
    public class SqlDbDesignTimeContextFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        private readonly IConfiguration m_configuration;

        public SqlDbDesignTimeContextFactory()
        {
            m_configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .AddUserSecrets<Program>()
                .Build();
        }

        #region Methods

        public SqlDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlDbContext>();

            optionsBuilder.UseSqlServer(m_configuration["ConnectionStrings:Bloodsport"], ConfigureSqlServer);

            return new SqlDbContext(optionsBuilder.Options);
        }

        private void ConfigureSqlServer(SqlServerDbContextOptionsBuilder options)
        {
            var contextTypeInfo = typeof(SqlDbContext).GetTypeInfo();
            var migrationAssemblyName = contextTypeInfo.Assembly.GetName();

            options.MigrationsAssembly(migrationAssemblyName.Name);
        }

        #endregion
    }
}
