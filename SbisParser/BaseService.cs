using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SbisParser.Interfaces;
using SbisParser.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;

namespace SbisParser
{
    public class BaseService : IBaseService
    {
        private readonly ILogger<BaseService> _logger;
        private readonly DataBaseSettings _option;
        private SqlConnectionStringBuilder _connectionStringBuilder = new();
        private SqlConnection _connection;

        public BaseService(ILogger<BaseService> logger, IOptions<DataBaseSettings> options)
        {
            _logger = logger;
            _option = options.Value;
            init();
        }

        private void init()
        {
            _connectionStringBuilder.DataSource = _option.DataSource.ReturnTempString();
            _connectionStringBuilder.UserID = _option.UserID.ReturnTempString();
            _connectionStringBuilder.Password = _option.Password.ReturnTempString();
            _connectionStringBuilder.InitialCatalog = _option.InitialCatalog.ReturnTempString();
            _connectionStringBuilder.ConnectTimeout = _option.ConnectTimeout;
            _connectionStringBuilder.ApplicationName = _option.ApplicationName.ReturnTempString();
            _connectionStringBuilder.MultipleActiveResultSets = _option.MultipleActiveResultSets;
            _connection = new(_connectionStringBuilder.ConnectionString);
        }

        public async Task<bool> WriteDataToBase(bool IsCreateTable, DataTable Data)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    using (SqlBulkCopy blk = new(_connection))
                    {

                        await _connection.OpenAsync();
                        if (IsCreateTable)
                        {
                            string sqlCommand = $"TRUNCATE TABLE {Data.TableName}";
                            SqlCommand command = new(sqlCommand, _connection);
                            int req = await command.ExecuteNonQueryAsync();
                            _logger.LogInformation($"table {Data.TableName} is clean: {req}");
                        }

                        //else
                        //{
                        //    string sqlCommand = $"TRUNCATE TABLE {Data.TableName}";
                        //    SqlCommand command = new(sqlCommand, _connection);
                        //    int req = await command.ExecuteNonQueryAsync();
                        //    _logger.LogInformation($"table {Data.TableName} is clean: {req}");
                        //}

                        blk.DestinationTableName = Data.TableName;
                        blk.BulkCopyTimeout = 0;
                        await blk.WriteToServerAsync(Data);
                        _logger.LogInformation($"Written {Data.Rows.Count} positions in the table {Data.TableName}");
                        await _connection.CloseAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }


    }
}
