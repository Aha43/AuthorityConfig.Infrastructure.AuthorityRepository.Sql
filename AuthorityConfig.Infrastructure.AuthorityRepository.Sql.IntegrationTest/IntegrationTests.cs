using AuthorityConfig.Domain.Param;
using AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Config;
using AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Dao;
using AuthorityConfig.Specification.Repository;
using AuthorityConfig.Specification.Repository.Dao;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AuthorityConfig.Infrastructure.AuthorityRepository.Sql.IntegrationTest
{
    public class IntegrationTests
    {
        [Fact]
        public async Task SetConfigurationShouldNotFailAsync()
        {
            var (repo, jsonSample) = Configure();

            var configParam = new SetConfigParam
            {
                Authority = "TestAuthority",
                Description = "Test data",
                Config = JsonSerializer.Deserialize<ConfigDao>(jsonSample),
                Uri = "https://test-uri"
            };

            await repo.SetConfigurationAsync(configParam, CancellationToken.None);   
        }

        //[Fact]
        public async Task GetConfigurationShouldNotFailAsync()
        {
            var (repo, jsonSample) = Configure();

            var param = new GetAuthorityParam
            {
                Authority = "TestAuthority"
            };

            var result = await repo.GetAuthorityAsync(param, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("TestAuthority", result.Name);
            Assert.Equal("Test data", result.Description);
            //Assert.Equal(jsonSample, result.Json);
            Assert.Equal("https://test-uri", result.Uri);
        }

        private static (IAuthorityRepository repo, string jsonSample) Configure()
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<IntegrationTests>();

            var configuration = builder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.ConfigureSqlAuthorityRepository(configuration);
            var sp = serviceCollection.BuildServiceProvider();

            var json = File.ReadAllText(configuration.GetSection("TestData")["ExampleJsonFilePath"]);

            return (sp.GetService<IAuthorityRepository>(), json);
        }

    }

}
