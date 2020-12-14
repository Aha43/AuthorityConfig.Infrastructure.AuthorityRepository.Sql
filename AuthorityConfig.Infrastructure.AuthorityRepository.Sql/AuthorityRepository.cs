﻿using AuthorityConfig.Infrastructure.AuthorityRepository.Sql.Config;
using AuthorityConfig.Specification.Repository;
using AuthorityConfig.Specification.Repository.Dao;
using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public async Task<IEnumerable<string>> GetAuthorityNames(CancellationToken cancellation)
        {
            try
            {
                var procedure = "authconfig.pr__get_authorities";
                using (var con = _connectionProvider.GetSqlConnection())
                {
                    return await con.QueryAsync<string>(procedure, commandType: CommandType.StoredProcedure);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<AuthorityDao> GetConfigurationAsync(string authority, CancellationToken cancellationToken)
        {
            try
            {
                var procedure = "authconfig.pr__get_configuration";
                using (var con = _connectionProvider.GetSqlConnection())
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("authority", authority, DbType.String);

                    return await con.QueryFirstOrDefaultAsync<AuthorityDao>(procedure, parameters, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SetConfigurationAsync(AuthorityDao config, CancellationToken cancellationToken)
        {
            try
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }

}