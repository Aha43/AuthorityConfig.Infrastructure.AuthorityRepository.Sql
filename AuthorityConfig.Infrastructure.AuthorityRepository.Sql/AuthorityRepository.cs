using AuthorityConfig.Domain.Model;
using AuthorityConfig.Domain.Param;
using AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Config;
using AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Dao;
using AuthorityConfig.Specification.Repository;
using AuthorityConfig.Specification.Repository.Dao;
using Dapper;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AuthorityConfig.Infrastructure.AuthorityRepository.Sql
{
    public class AuthorityRepository : IAuthorityRepository
    {
        private readonly DbConnectionProvider _connectionProvider;

        public AuthorityRepository(
            DbConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        // make private after new interface in use
        

        public async Task SetConfigurationAsync(AuthorityDao config, CancellationToken cancellationToken)
        {
            var procedure = "authconfig.pr__set_configuration";
            using (var con = _connectionProvider.GetSqlConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("authority", config.Authority, DbType.String);
                parameters.Add("json", config.Json, DbType.String);
                parameters.Add("uri", config.Uri, DbType.String);
                parameters.Add("description", config.Description, DbType.String);

                //await con.OpenAsync(cancellationToken);
                var result = await con.QueryAsync<StatusReturn>(procedure, parameters, commandType: CommandType.StoredProcedure);
            }   
        }

        // NEW API

        public async Task<object> GetConfigurationAsync(AuthorityParam param, CancellationToken cancellationToken)
        {
            return await GetConfigDaoAsync(param.Authority, cancellationToken);
        }

        public async Task SetConfigurationAsync(SetConfigParam param, CancellationToken cancellationToken)
        {
            await SetConfigDaoAsync(param.Authority, cancellationToken, (ConfigDao)param.Config, param.Uri, param.Description);
        }

        public async Task<Authorities> GetAuthoritiesAsync(CancellationToken cancellation)
        {
            var procedure = "authconfig.pr__get_authorities";
            using (var con = _connectionProvider.GetSqlConnection())
            {
                var names = await con.QueryAsync<string>(procedure, commandType: CommandType.StoredProcedure);
                return new Authorities { Names = names };
            }
        }

        public async Task<Client> GetClientAsync(GetClientParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            return config.Clients.Where(e => e.ClientId.Equals(param.ClientId, StringComparison.Ordinal)).FirstOrDefault();
        }

        public async Task SetClientAsync(Client client, SetClientParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            var newClients = config.Clients == null ? new List<Client>() : new List<Client>(config.Clients);
            var oldClient = newClients.Where(e => e.ClientId.Equals(client.ClientId, StringComparison.Ordinal)).FirstOrDefault();
            if (oldClient != null) newClients.Remove(oldClient);
            newClients.Add(client);
            config.Clients = newClients;
            await SetConfigDaoAsync(param.Authority, cancellationToken, config);
        }

        public async Task<IEnumerable<Client>> GetClientsAsync(AuthorityParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            return config.Clients == null ? Array.Empty<Client>() : config.Clients.ToArray();
        }

        public async Task<Authority> GetAuthorityAsync(GetAuthorityParam param, CancellationToken cancellationToken)
        {
            var dao = await GetAuthorityDaoAsync(param.Authority, cancellationToken);
            if (dao == null)
            {
                return null;
            }
            else
            {
                return new Authority
                {
                    Name = dao.Authority,
                    Description = dao.Description,
                    Uri = dao.Uri
                };
            }
        }

        // api sope
        public async Task<IEnumerable<ApiScope>> GetApiScopesAsync(AuthorityParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            return config.Apis;
        }

        public async Task<ApiScope> GetApiScopeAsync(GatApiScopeParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            return config?.Apis.Where(e => e.Name.Equals(param.Name, StringComparison.Ordinal)).FirstOrDefault();
        }

        public async Task SetApiScopeAsync(ApiScope apiScope, SetApiParam param, CancellationToken cancellationToken)
        {
            var config = await GetConfigDaoAsync(param.Authority, cancellationToken);
            var newApis = config.Apis == null ? new List<ApiScope>() : new List<ApiScope>(config.Apis);
            var oldApi = newApis.Where(e => e.Name.Equals(apiScope.Name, StringComparison.Ordinal)).FirstOrDefault();
            if (oldApi != null) newApis.Remove(oldApi);
            newApis.Add(apiScope);
            config.Apis = newApis;
            await SetConfigDaoAsync(param.Authority, cancellationToken, config);
        }

        // Helpers
        private async Task<IEnumerable<string>> GetAuthorityNames(CancellationToken cancellation)
        {
            var procedure = "authconfig.pr__get_authorities";
            using (var con = _connectionProvider.GetSqlConnection())
            {
                return await con.QueryAsync<string>(procedure, commandType: CommandType.StoredProcedure);
            }
        }

        private async Task<AuthorityDao> GetAuthorityDaoAsync(string authority, CancellationToken cancellationToken)
        {
            var procedure = "authconfig.pr__get_configuration";
            using (var con = _connectionProvider.GetSqlConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("authority", authority, DbType.String);

                return await con.QueryFirstOrDefaultAsync<AuthorityDao>(procedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        private async Task<ConfigDao> GetConfigDaoAsync(string authority, CancellationToken cancellationToken)
        {
            var stored = await GetAuthorityDaoAsync(authority, cancellationToken);
            if (stored == null)
            {
                return null;
            }
            var retVal = JsonSerializer.Deserialize<ConfigDao>(stored.Json);
            return retVal;
        }

        private async Task SetConfigDaoAsync(string authority, CancellationToken cancellationToken, ConfigDao configDao = null, string uri = null, string description = null)
        {
            var dao = new AuthorityDao
            {
                Authority = authority,
                Json = (configDao == null) ? null : JsonSerializer.Serialize(configDao),
                Uri = uri,
                Description = description
            };

            await SetConfigurationAsync(dao, cancellationToken);
        }

    }

}
