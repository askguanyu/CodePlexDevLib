//-----------------------------------------------------------------------
// <copyright file="DbHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.Odbc;
    using System.Data.OleDb;
    using System.Data.OracleClient;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;

    /// <summary>
    /// Represents a database that commands can be run against.
    /// </summary>
    public class DbHelper
    {
        /// <summary>
        /// Field DbProviderDictionary.
        /// </summary>
        private static readonly Dictionary<DbProvider, string> DbProviderDictionary;

        /// <summary>
        /// Initializes static members of the <see cref="DbHelper" /> class.
        /// </summary>
        static DbHelper()
        {
            DbProviderDictionary = new Dictionary<DbProvider, string>(9);

            DbProviderDictionary.Add(DbProvider.SqlServer, "System.Data.SqlClient");
            DbProviderDictionary.Add(DbProvider.SqlServerCE, "System.Data.SqlServerCe");
            DbProviderDictionary.Add(DbProvider.MySQL, "MySql.Data.MySqlClient");
            DbProviderDictionary.Add(DbProvider.SQLite, "System.Data.SQLite");
            DbProviderDictionary.Add(DbProvider.OleDB, "System.Data.OleDb");
            DbProviderDictionary.Add(DbProvider.ODBC, "System.Data.Odbc");
            DbProviderDictionary.Add(DbProvider.Oracle, "System.Data.OracleClient");
            DbProviderDictionary.Add(DbProvider.PostgreSQL, "Devart.Data.PostgreSql");
            DbProviderDictionary.Add(DbProvider.DB2, "IBM.Data.DB2");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbHelper" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the database connection.</param>
        /// <param name="providerName">Invariant name of a database provider.</param>
        public DbHelper(string connectionString, string providerName)
        {
            this.CheckStringNullOrWhiteSpace(connectionString, "connectionString");
            this.CheckStringNullOrWhiteSpace(providerName, "providerName");

            this.ConnectionString = connectionString;
            this.ProviderFactory = DbProviderFactories.GetFactory(providerName);

            if (this.ProviderFactory is SqlClientFactory)
            {
                this.DiscoverParametersAction = command => SqlCommandBuilder.DeriveParameters((SqlCommand)command);
                this.GetXmlReaderFunc = command => ((SqlCommand)command).ExecuteXmlReader();
            }
            else if (this.ProviderFactory is OleDbFactory)
            {
                this.DiscoverParametersAction = command => OleDbCommandBuilder.DeriveParameters((OleDbCommand)command);
                this.GetXmlReaderFunc = null;
            }
            else if (this.ProviderFactory is OdbcFactory)
            {
                this.DiscoverParametersAction = command => OdbcCommandBuilder.DeriveParameters((OdbcCommand)command);
                this.GetXmlReaderFunc = null;
            }
            else if (this.ProviderFactory is OracleClientFactory)
            {
                this.DiscoverParametersAction = command => OracleCommandBuilder.DeriveParameters((OracleCommand)command);
                this.GetXmlReaderFunc = null;
            }
            else
            {
                this.DiscoverParametersAction = null;
                this.GetXmlReaderFunc = null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbHelper" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the database connection.</param>
        /// <param name="provider">DbProvider enum value.</param>
        public DbHelper(string connectionString, DbProvider provider)
        {
            this.CheckStringNullOrWhiteSpace(connectionString, "connectionString");

            this.ConnectionString = connectionString;
            this.ProviderFactory = DbProviderFactories.GetFactory(DbProviderDictionary[provider]);

            switch (provider)
            {
                case DbProvider.SqlServer:
                    this.DiscoverParametersAction = command => SqlCommandBuilder.DeriveParameters((SqlCommand)command);
                    this.GetXmlReaderFunc = command => ((SqlCommand)command).ExecuteXmlReader();
                    break;
                case DbProvider.OleDB:
                    this.DiscoverParametersAction = command => OleDbCommandBuilder.DeriveParameters((OleDbCommand)command);
                    this.GetXmlReaderFunc = null;
                    break;
                case DbProvider.ODBC:
                    this.DiscoverParametersAction = command => OdbcCommandBuilder.DeriveParameters((OdbcCommand)command);
                    this.GetXmlReaderFunc = null;
                    break;
                case DbProvider.Oracle:
                    this.DiscoverParametersAction = command => OracleCommandBuilder.DeriveParameters((OracleCommand)command);
                    this.GetXmlReaderFunc = null;
                    break;
                default:
                    this.DiscoverParametersAction = null;
                    this.GetXmlReaderFunc = null;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the discover parameters action.
        /// </summary>
        public Action<DbCommand> DiscoverParametersAction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ExecuteXmlReader function.
        /// </summary>
        public Converter<DbCommand, XmlReader> GetXmlReaderFunc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the string used to open the database connection.
        /// </summary>
        public string ConnectionString
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the DbProviderFactory.
        /// </summary>
        public DbProviderFactory ProviderFactory
        {
            get;
            private set;
        }

        /// <summary>
        /// Convert DbDataReader result to generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="dbDataReader">DbDataReader instance.</param>
        /// <returns>A generic list containing the resultset.</returns>
        public static List<T> ToList<T>(DbDataReader dbDataReader)
        {
            if (dbDataReader == null)
            {
                throw new ArgumentNullException("dbDataReader");
            }

            if (!dbDataReader.HasRows)
            {
                return null;
            }

            Type type = typeof(T);

            List<T> result = null;

            if (type.IsValueType || type == typeof(string))
            {
                result = CreateValue<T>(dbDataReader, type);
            }
            else
            {
                result = CreateObject<T>(dbDataReader, type);
            }

            return result;
        }

        /// <summary>
        /// Open database connection.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>DbConnection instance.</returns>
        public DbConnection OpenConnection(bool throwOnError = true)
        {
            DbConnection result = null;

            try
            {
                result = this.ProviderFactory.CreateConnection();

                result.ConnectionString = this.ConnectionString;

                if (result.State != ConnectionState.Open)
                {
                    result.Open();
                }

                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return result != null ? result : null;
                }
            }
        }

        /// <summary>
        /// Close database connection.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Reviewed.")]
        public void CloseConnection(DbConnection dbConnection, bool throwOnError = false)
        {
            if (dbConnection != null)
            {
                try
                {
                    dbConnection.Close();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
                finally
                {
                    dbConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        /// <returns>An object representing the new transaction.</returns>
        public DbTransaction BeginTransaction(bool throwOnError = true)
        {
            try
            {
                return this.OpenConnection(throwOnError).BeginTransaction();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <param name="transaction">The transaction to commit.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        public void CommitTransaction(DbTransaction transaction, bool throwOnError = true)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Commit();
                    transaction.Dispose();
                    this.CloseConnection(transaction.Connection, false);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <param name="transaction">The transaction to roll back.</param>
        /// <param name="throwOnError">true to throw any exception that occurs.-or- false to ignore any exception that occurs.</param>
        public void RollbackTransaction(DbTransaction transaction, bool throwOnError = false)
        {
            if (transaction != null)
            {
                try
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    this.CloseConnection(transaction.Connection, false);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);

                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Prepares the DbCommand object.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction, or null.</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter to be associated with the command or 'null' if no parameters are required.</param>
        /// <returns>A new instance of DbCommand.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public DbCommand PrepareCommand(DbTransaction transaction, DbConnection connection, CommandType commandType, string commandText, IList<DbParameter> commandParameters)
        {
            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.Connection = connection;
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or committed, please provide an open transaction.", "transaction");
                }

                dbCommand.Transaction = transaction;
            }

            if (commandParameters != null && commandParameters.Count > 0)
            {
                this.AttachParameters(dbCommand, commandParameters);
            }

            return dbCommand;
        }

        /// <summary>
        /// Prepares the DbCommand object.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction, or null.</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A new instance of DbCommand.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public DbCommand PrepareCommand(DbTransaction transaction, DbConnection connection, string commandText)
        {
            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.Connection = connection;
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = CommandType.Text;

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or committed, please provide an open transaction.", "transaction");
                }

                dbCommand.Transaction = transaction;
            }

            return dbCommand;
        }

        /// <summary>
        /// Prepares the DbCommand object.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction, or null.</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned.</param>
        /// <returns>A new instance of DbCommand.</returns>
        public DbCommand PrepareCommandSp(DbTransaction transaction, DbConnection connection, string spName, IList<object> parameterValues)
        {
            DbCommand dbCommand = null;

            if (parameterValues != null && parameterValues.Count > 0)
            {
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, this.DiscoverParametersAction, spName);

                this.AssignParameterValues(commandParameters, parameterValues);

                dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, null);
            }

            return dbCommand;
        }

        /// <summary>
        /// Prepares the DbCommand object.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction, or null.</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A new instance of DbCommand.</returns>
        public DbCommand PrepareCommandSpDataRowParams(DbTransaction transaction, DbConnection connection, string spName, DataRow dataRow)
        {
            DbCommand dbCommand = null;

            DbParameter[] commandParameters = null;

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, this.DiscoverParametersAction, spName);

                this.AssignParameterValues(commandParameters, dataRow);
            }

            dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, commandParameters);

            return dbCommand;
        }

        /// <summary>
        /// Prepares the DbCommand object.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction, or null.</param>
        /// <param name="connection">A valid DbConnection, on which to execute this command.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>A new instance of DbCommand.</returns>
        public DbCommand PrepareCommandSpObjectParams(DbTransaction transaction, DbConnection connection, string spName, object data)
        {
            DbCommand dbCommand = null;

            List<DbParameter> commandParameters = new List<DbParameter>();

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                if (property.CanRead)
                {
                    commandParameters.Add(this.CreateParameter(property.Name, property.GetValue(data, null)));
                }
            }

            dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, commandParameters);

            return dbCommand;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbCommand class.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A new instance of System.Data.Common.DbCommand.</returns>
        public DbCommand CreateCommand(CommandType commandType, string commandText)
        {
            return this.CreateCommand(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbCommand class.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A new instance of System.Data.Common.DbCommand.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public DbCommand CreateCommand(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;

            if (commandParameters != null)
            {
                this.AttachParameters(dbCommand, commandParameters);
            }

            return dbCommand;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbCommand class.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <returns>A new instance of System.Data.Common.DbCommand.</returns>
        public DbCommand CreateCommand(string commandText)
        {
            return this.CreateCommand(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbCommand class.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters.</param>
        /// <returns>A new instance of System.Data.Common.DbCommand.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public DbCommand CreateCommandSp(string spName, params string[] sourceColumns)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            DbConnection dbConnection = null;

            try
            {
                dbConnection = this.OpenConnection();

                DbCommand dbCommand = dbConnection.CreateCommand();

                dbCommand.CommandText = spName;
                dbCommand.CommandType = CommandType.StoredProcedure;

                if (sourceColumns != null && sourceColumns.Length > 0)
                {
                    DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(dbConnection, this.DiscoverParametersAction, spName);

                    for (int i = 0; i < sourceColumns.Length; i++)
                    {
                        commandParameters[i].SourceColumn = sourceColumns[i];
                    }

                    this.AttachParameters(dbCommand, commandParameters);
                }

                return dbCommand;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.CloseConnection(dbConnection);
            }
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbParameter class.
        /// </summary>
        /// <returns>A new instance of System.Data.Common.DbParameter.</returns>
        public DbParameter CreateParameter()
        {
            return this.ProviderFactory.CreateParameter();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the System.Data.Common.DbParameter class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value for the parameter.</param>
        /// <returns>A new instance of System.Data.Common.DbParameter.</returns>
        public DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter result = this.ProviderFactory.CreateParameter();

            result.ParameterName = parameterName;
            result.Value = value;

            return result;
        }

        /// <summary>
        /// Returns a list of the provider's class that implements the System.Data.Common.DbParameter class.
        /// </summary>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A list of System.Data.Common.DbParameter.</returns>
        public List<DbParameter> CreateObjectParams(object data, string parameterNameFormat = "@{0}")
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            List<DbParameter> result = new List<DbParameter>();

            foreach (PropertyInfo property in data.GetType().GetProperties())
            {
                if (property.CanRead)
                {
                    DbParameter dbParameter = this.ProviderFactory.CreateParameter();

                    dbParameter.ParameterName = string.Format(parameterNameFormat, property.Name);
                    dbParameter.Value = property.GetValue(data, null);

                    result.Add(dbParameter);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a list of the provider's class that implements the System.Data.Common.DbParameter class.
        /// </summary>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A list of System.Data.Common.DbParameter.</returns>
        public List<DbParameter> CreateDataRowParams(DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            List<DbParameter> result = new List<DbParameter>();

            foreach (DataColumn item in dataRow.Table.Columns)
            {
                DbParameter dbParameter = this.ProviderFactory.CreateParameter();

                dbParameter.ParameterName = string.Format(parameterNameFormat, item.ColumnName);
                dbParameter.Value = dataRow[item];

                result.Add(dbParameter);
            }

            return result;
        }

        /// <summary>
        /// Disposes the command.
        /// </summary>
        /// <param name="dbCommand">The database command.</param>
        public void DisposeCommand(DbCommand dbCommand)
        {
            if (dbCommand != null)
            {
                try
                {
                    dbCommand.Parameters.Clear();
                    dbCommand.Dispose();
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
        }

        /// <summary>
        /// Calls function on the database.
        /// </summary>
        /// <typeparam name="TResult">The type of return value.</typeparam>
        /// <param name="prepareCommandFunc">The prepare command function.</param>
        /// <param name="executeCommandFunc">The execute command function.</param>
        /// <param name="disposeCommand">true to dispose command when finished; otherwise, false.</param>
        /// <param name="disposeConnection">true to dispose connection when finished; otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        public TResult FuncOnDb<TResult>(Converter<DbConnection, DbCommand> prepareCommandFunc, Converter<DbCommand, TResult> executeCommandFunc, bool disposeCommand = true, bool disposeConnection = true)
        {
            DbConnection dbConnection = null;
            DbCommand dbCommand = null;

            try
            {
                dbConnection = this.OpenConnection();

                dbCommand = prepareCommandFunc(dbConnection);

                return executeCommandFunc(dbCommand);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                this.DisposeCommand(dbCommand);
                this.CloseConnection(dbConnection);

                throw;
            }
            finally
            {
                if (disposeCommand)
                {
                    this.DisposeCommand(dbCommand);
                }

                if (disposeConnection)
                {
                    this.CloseConnection(dbConnection);
                }
            }
        }

        /// <summary>
        /// Calls function on the database with transaction.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="prepareCommandFunc">The prepare command function.</param>
        /// <param name="executeCommandFunc">The execute command function.</param>
        /// <param name="disposeCommand">true to dispose command when finished, otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        public TResult FuncOnDbTransaction<TResult>(DbTransaction transaction, Converter<DbConnection, DbCommand> prepareCommandFunc, Converter<DbCommand, TResult> executeCommandFunc, bool disposeCommand = true)
        {
            DbCommand dbCommand = null;

            try
            {
                dbCommand = prepareCommandFunc(transaction.Connection);

                return executeCommandFunc(dbCommand);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);

                this.DisposeCommand(dbCommand);

                throw;
            }
            finally
            {
                if (disposeCommand)
                {
                    this.DisposeCommand(dbCommand);
                }
            }
        }

        /// <summary>
        /// Calls action on the database.
        /// </summary>
        /// <param name="prepareCommandFunc">The prepare command function.</param>
        /// <param name="executeCommandAction">The execute command action.</param>
        public void ActionOnDb(Converter<DbConnection, DbCommand> prepareCommandFunc, Action<DbCommand> executeCommandAction)
        {
            DbConnection dbConnection = null;
            DbCommand dbCommand = null;

            try
            {
                dbConnection = this.OpenConnection();

                dbCommand = prepareCommandFunc(dbConnection);

                executeCommandAction(dbCommand);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(dbCommand);
                this.CloseConnection(dbConnection);
            }
        }

        /// <summary>
        /// Calls action on the database with transaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="prepareCommandFunc">The prepare command function.</param>
        /// <param name="executeCommandAction">The execute command action.</param>
        public void ActionOnDbTransaction(DbTransaction transaction, Converter<DbConnection, DbCommand> prepareCommandFunc, Action<DbCommand> executeCommandAction)
        {
            DbCommand dbCommand = null;

            try
            {
                dbCommand = prepareCommandFunc(transaction.Connection);

                executeCommandAction(dbCommand);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(dbCommand);
            }
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryText(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteNonQuery(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryTextDataRowParams(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteNonQuery(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryTextObjectParams(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteNonQuery(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes a stored procedure against a connection object.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a stored procedure against a connection object.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a stored procedure against a connection object.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a SQL statement against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a SQL statement against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryText(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteNonQuery(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes a SQL statement against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryTextDataRowParams(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteNonQuery(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes a SQL statement against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQueryTextObjectParams(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteNonQuery(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes a stored procedure against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a stored procedure against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes a stored procedure against the specified DbTransaction.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuerySpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarText(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteScalar(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarTextDataRowParams(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteScalar(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarTextObjectParams(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteScalar(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public object ExecuteScalarSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public object ExecuteScalarSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarText(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteScalar(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarTextDataRowParams(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteScalar(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarTextObjectParams(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteScalar(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The first column of the first row in the result set.</returns>
        public object ExecuteScalarSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public object ExecuteScalarSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public object ExecuteScalarSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSet(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetText(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteDataSet(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetTextDataRowParams(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteDataSet(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetTextObjectParams(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteDataSet(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query and returns a DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetText(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteDataSet(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetTextDataRowParams(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteDataSet(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataSetTextObjectParams(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteDataSet(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSetSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText, CommandBehavior commandBehavior, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderText(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteReader(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderTextDataRowParams(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteReader(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderTextObjectParams(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteReader(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(string spName, CommandBehavior commandBehavior, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>The number of rows affected.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(string spName, DataRow dataRow, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>The number of rows affected.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(string spName, object data, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query and returns a DbDataReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false,
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, CommandBehavior commandBehavior, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderText(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteReader(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderTextDataRowParams(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteReader(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderTextObjectParams(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteReader(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(DbTransaction transaction, string spName, CommandBehavior commandBehavior, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(DbTransaction transaction, string spName, object data, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(commandBehavior);

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a DbDataReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader();

                    bool canClear = true;

                    foreach (DbParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                        {
                            canClear = false;
                            break;
                        }
                    }

                    if (canClear)
                    {
                        this.DisposeCommand(command);
                    }

                    return dbDataReader;
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteList<T>(CommandType commandType, string commandText, CommandBehavior commandBehavior, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader(commandBehavior))
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteList<T>(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListText<T>(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteList<T>(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListTextDataRowParams<T>(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteList<T>(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListTextObjectParams<T>(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteList<T>(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListSp<T>(string spName, CommandBehavior commandBehavior, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader(commandBehavior))
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListSp<T>(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>The number of rows affected.</returns>
        public List<T> ExecuteListSpDataRowParams<T>(string spName, DataRow dataRow, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader(commandBehavior))
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public List<T> ExecuteListSpDataRowParams<T>(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <param name="commandBehavior">One of the System.Data.CommandBehavior values.</param>
        /// <returns>The number of rows affected.</returns>
        public List<T> ExecuteListSpObjectParams<T>(string spName, object data, CommandBehavior commandBehavior)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader(commandBehavior))
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows affected.</returns>
        public List<T> ExecuteListSpObjectParams<T>(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<List<T>>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteList<T>(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<List<T>>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListText<T>(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteList<T>(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListTextDataRowParams<T>(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteList<T>(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command.</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListTextObjectParams<T>(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteList<T>(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListSp<T>(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<List<T>>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListSpDataRowParams<T>(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<List<T>>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns a generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>A DbDataReader containing the resultset generated by the command.</returns>
        public List<T> ExecuteListSpObjectParams<T>(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<List<T>>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command =>
                {
                    using (DbDataReader dbDataReader = command.ExecuteReader())
                    {
                        bool canClear = true;

                        foreach (DbParameter commandParameter in command.Parameters)
                        {
                            if (commandParameter.Direction != ParameterDirection.Input)
                            {
                                canClear = false;
                                break;
                            }
                        }

                        if (canClear)
                        {
                            this.DisposeCommand(command);
                        }

                        return ToList<T>(dbDataReader);
                    }
                },
                false);
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                    command => this.GetXmlReaderFunc(command),
                    false,
                    false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderText(string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteXmlReader(CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderTextDataRowParams(string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteXmlReader(CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderTextObjectParams(string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteXmlReader(CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                    command => this.GetXmlReaderFunc(command),
                    false,
                    false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                    command => this.GetXmlReaderFunc(command),
                    false,
                    false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query and returns an XmlReader.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                    command => this.GetXmlReaderFunc(command),
                    false,
                    false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderText(DbTransaction transaction, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteXmlReader(transaction, CommandType.Text, commandText, commandParameters);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderTextDataRowParams(DbTransaction transaction, string commandText, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteXmlReader(transaction, CommandType.Text, commandText, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The T-SQL command using "FOR XML AUTO".</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderTextObjectParams(DbTransaction transaction, string commandText, object data, string parameterNameFormat = "@{0}")
        {
            return this.ExecuteXmlReader(transaction, CommandType.Text, commandText, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and returns an XmlReader.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetText(string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            return this.FillDataSet(CommandType.Text, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetTextDataRowParams(string commandText, DataSet dataSet, string[] tableNames, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.FillDataSet(CommandType.Text, commandText, dataSet, tableNames, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetTextObjectParams(string commandText, DataSet dataSet, string[] tableNames, object data, string parameterNameFormat = "@{0}")
        {
            return this.FillDataSet(CommandType.Text, commandText, dataSet, tableNames, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSp(string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSpDataRowParams(string spName, DataSet dataSet, string[] tableNames, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSpObjectParams(string spName, DataSet dataSet, string[] tableNames, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSet(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of DbParameter used to execute the command.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetText(DbTransaction transaction, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            return this.FillDataSet(transaction, CommandType.Text, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="dataRow">The DataRow used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetTextDataRowParams(DbTransaction transaction, string commandText, DataSet dataSet, string[] tableNames, DataRow dataRow, string parameterNameFormat = "@{0}")
        {
            return this.FillDataSet(transaction, CommandType.Text, commandText, dataSet, tableNames, this.CreateDataRowParams(dataRow, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="data">The object used to hold the parameter values.</param>
        /// <param name="parameterNameFormat">A composite format string for parameter name.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetTextObjectParams(DbTransaction transaction, string commandText, DataSet dataSet, string[] tableNames, object data, string parameterNameFormat = "@{0}")
        {
            return this.FillDataSet(transaction, CommandType.Text, commandText, dataSet, tableNames, this.CreateObjectParams(data, parameterNameFormat).ToArray());
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSp(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSp(transaction, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSpDataRowParams(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (dataRow == null)
            {
                throw new ArgumentNullException("dataRow");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(transaction, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the query against the provided DbTransaction and adds or refreshes rows in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="data">The object used to hold the stored procedure's parameter values.</param>
        /// <returns>The number of rows successfully added to or refreshed in the DataSet. This does not include rows affected by statements that do not return rows.</returns>
        public int FillDataSetSpObjectParams(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(transaction, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        if (tableNames != null && tableNames.Length > 0)
                        {
                            string tableName = "Table";

                            for (int i = 0; i < tableNames.Length; i++)
                            {
                                if (string.IsNullOrEmpty(tableNames[i]))
                                {
                                    throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                                }

                                dbDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                                tableName += (i + 1).ToString();
                            }
                        }

                        return dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            DbConnection dbConnection = null;

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                dbConnection = this.OpenConnection();

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);

                this.CloseConnection(dbConnection);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            DbConnection dbConnection = null;

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                dbConnection = this.OpenConnection();

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);

                this.CloseConnection(dbConnection);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(string selectCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataSet);

                            dataSet.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(string selectCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataSet, tableName);

                            dataSet.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(transaction, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(transaction, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(transaction, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(transaction, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(transaction, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(transaction, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(DbTransaction transaction, string selectCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataSet);

                            dataSet.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataSet(DbTransaction transaction, string selectCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataSet, tableName);

                            dataSet.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataTable(string insertCommandText, string deleteCommandText, string updateCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            DbConnection dbConnection = null;

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                dbConnection = this.OpenConnection();

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);

                this.CloseConnection(dbConnection);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataTable(string selectCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataTable);

                            dataTable.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="insertCommandText">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommandText">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommandText">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataTable(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(insertCommandText, "insertCommandText");
            this.CheckStringNullOrWhiteSpace(deleteCommandText, "deleteCommandText");
            this.CheckStringNullOrWhiteSpace(updateCommandText, "updateCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(transaction, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(transaction, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(transaction, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateBatchSize = 0;

                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    int result = dbDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();

                    return result;
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.DisposeCommand(insertCommand);
                this.DisposeCommand(deleteCommand);
                this.DisposeCommand(updateCommand);
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="transaction">A valid DbTransaction.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        /// <returns>The number of rows successfully updated from the DataSet.</returns>
        public int UpdateDataTable(DbTransaction transaction, string selectCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(transaction, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.UpdateBatchSize = 0;

                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            int result = dbDataAdapter.Update(dataTable);

                            dataTable.AcceptChanges();

                            return result;
                        }
                    }
                });
        }

        /// <summary>
        /// Convert DbDataReader result to generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="dbDataReader">DbDataReader instance.</param>
        /// <param name="type">The element type.</param>
        /// <returns>A generic list containing the resultset.</returns>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private static List<T> CreateObject<T>(DbDataReader dbDataReader, Type type)
        {
            List<T> result = new List<T>();

            PropertyInfo[] properties = type.GetProperties();

            while (dbDataReader.Read())
            {
                T item = default(T);

                try
                {
                    item = Activator.CreateInstance<T>();
                }
                catch
                {
                    item = (T)FormatterServices.GetUninitializedObject(type);
                }

                for (int i = 0; i < dbDataReader.FieldCount; i++)
                {
                    string name = dbDataReader.GetName(i);

                    foreach (PropertyInfo propertyInfo in properties)
                    {
                        if (name.Equals(propertyInfo.Name) && dbDataReader[propertyInfo.Name] != null && dbDataReader[propertyInfo.Name].GetType() != typeof(DBNull))
                        {
                            propertyInfo.SetValue(item, Convert.ChangeType(dbDataReader[propertyInfo.Name], propertyInfo.PropertyType), null);
                            break;
                        }
                    }
                }

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Convert DbDataReader result to generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="dbDataReader">DbDataReader instance.</param>
        /// <param name="type">The element type.</param>
        /// <returns>A generic list containing the resultset.</returns>
        private static List<T> CreateValue<T>(DbDataReader dbDataReader, Type type)
        {
            List<T> result = new List<T>();

            while (dbDataReader.Read())
            {
                T item = (T)Convert.ChangeType(dbDataReader[0], type, null);

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Attaches array of DbParameter to a DbCommand.
        /// This method will assign a value of DbNull to any parameter with a direction of InputOutput and a value of null.
        /// This behavior will prevent default values from being used, but this will be the less common case than an intended pure output parameter (derived as InputOutput) where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added.</param>
        /// <param name="commandParameters">An array of DbParameter to be added to command.</param>
        private void AttachParameters(DbCommand command, IList<DbParameter> commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandParameters != null && commandParameters.Count > 0)
            {
                foreach (DbParameter commandParameter in commandParameters)
                {
                    if (commandParameter != null)
                    {
                        if ((commandParameter.Direction == ParameterDirection.InputOutput || commandParameter.Direction == ParameterDirection.Input) && (commandParameter.Value == null))
                        {
                            commandParameter.Value = DBNull.Value;
                        }

                        command.Parameters.Add(commandParameter);
                    }
                }
            }
        }

        /// <summary>
        /// Assigns dataRow column values to an array of DbParameter.
        /// </summary>
        /// <param name="commandParameters">Array of DbParameter to be assigned values.</param>
        /// <param name="dataRow">The DataRow used to hold the stored procedure's parameter values.</param>
        private void AssignParameterValues(IList<DbParameter> commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || commandParameters.Count < 1 || dataRow == null)
            {
                return;
            }

            int i = 0;

            foreach (DbParameter commandParameter in commandParameters)
            {
                if (string.IsNullOrEmpty(commandParameter.ParameterName))
                {
                    throw new Exception(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", i, commandParameter.ParameterName));
                }

                if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                {
                    commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                }

                i++;
            }
        }

        /// <summary>
        /// Assigns an array of values to an array of DbParameter.
        /// </summary>
        /// <param name="commandParameters">Array of DbParameter to be assigned values.</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned.</param>
        private void AssignParameterValues(IList<DbParameter> commandParameters, IList<object> parameterValues)
        {
            if (commandParameters == null || commandParameters.Count < 1 || parameterValues == null || parameterValues.Count < 1)
            {
                return;
            }

            if (commandParameters.Count != parameterValues.Count)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            for (int i = 0; i < commandParameters.Count; i++)
            {
                if (parameterValues[i] is IDbDataParameter)
                {
                    IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];

                    if (paramInstance.Value == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = paramInstance.Value;
                    }
                }
                else if (parameterValues[i] == null)
                {
                    commandParameters[i].Value = DBNull.Value;
                }
                else
                {
                    commandParameters[i].Value = parameterValues[i];
                }
            }
        }

        /// <summary>
        /// Method CheckStringNullOrWhiteSpace.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        private void CheckStringNullOrWhiteSpace(string value, string paramName)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return;
                    }
                }
            }

            throw new ArgumentNullException(paramName);
        }
    }
}
