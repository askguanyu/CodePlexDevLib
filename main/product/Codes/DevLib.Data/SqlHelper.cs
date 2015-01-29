//-----------------------------------------------------------------------
// <copyright file="SqlHelper.cs" company="YuGuan Corporation">
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
    using System.Data.Sql;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Runtime.Serialization;

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, scalable best practices for common uses of SqlClient.
    /// </summary>
    public class SqlHelper
    {
        /// <summary>
        /// Field DbProviderDictionary.
        /// </summary>
        private static readonly Dictionary<DbProvider, string> DbProviderDictionary;

        static SqlHelper()
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
        public SqlHelper(string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

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
            }
            else if (this.ProviderFactory is OdbcFactory)
            {
                this.DiscoverParametersAction = command => OdbcCommandBuilder.DeriveParameters((OdbcCommand)command);
            }
            else if (this.ProviderFactory is OracleClientFactory)
            {
                this.DiscoverParametersAction = command => OracleCommandBuilder.DeriveParameters((OracleCommand)command);
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
        public SqlHelper(string connectionString, DbProvider provider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

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
                    break;
                case DbProvider.ODBC:
                    this.DiscoverParametersAction = command => OdbcCommandBuilder.DeriveParameters((OdbcCommand)command);
                    break;
                case DbProvider.Oracle:
                    this.DiscoverParametersAction = command => OracleCommandBuilder.DeriveParameters((OracleCommand)command);
                    break;
                default:
                    this.DiscoverParametersAction = null;
                    this.GetXmlReaderFunc = null;
                    break;
            }
        }

        public Action<DbCommand> DiscoverParametersAction { get; set; }

        public Converter<DbCommand, XmlReader> GetXmlReaderFunc { get; set; }

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
        /// <returns>A new instance of System.Data.Common.DbParameter.</returns>
        public DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter result = this.ProviderFactory.CreateParameter();

            result.ParameterName = parameterName;
            result.Value = value;

            return result;
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing a stored procedure and optional parameters to be provided.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters.</param>
        /// <returns>A valid SqlCommand object.</returns>
        public DbCommand CreateCommand(CommandType commandType, string commandText)
        {
            return CreateCommand(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing a stored procedure and optional parameters to be provided.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters.</param>
        /// <returns>A valid SqlCommand object.</returns>
        public DbCommand CreateCommand(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;

            if (commandParameters != null)
            {
                AttachParameters(dbCommand, commandParameters);
            }

            return dbCommand;
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing a stored procedure and optional parameters to be provided.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters.</param>
        /// <returns>A valid SqlCommand object.</returns>
        public DbCommand CreateCommand(string commandText)
        {
            return CreateCommand(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Simplify the creation of a Sql command object by allowing a stored procedure and optional parameters to be provided.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters.</param>
        /// <returns>A valid SqlCommand object.</returns>
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

                    AttachParameters(dbCommand, commandParameters);
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
        /// Begin transaction.
        /// </summary>
        public DbTransaction BeginTransaction()
        {
            return this.OpenConnection().BeginTransaction();
        }

        /// <summary>
        /// Commit transaction.
        /// </summary>
        public void CommitTransaction(DbTransaction transaction)
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();

                this.CloseConnection(transaction.Connection);
            }
        }

        /// <summary>
        /// Rollback transaction.
        /// </summary>
        public void RollbackTransaction(DbTransaction transaction)
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();

                this.CloseConnection(transaction.Connection);
            }
        }

        /// <summary>
        /// Calls function on the repository.
        /// </summary>
        /// <typeparam name="TResult">The type of return value.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="submitChanges">true to submit changes of the repository; otherwise, false.</param>
        /// <returns>Result of the function.</returns>
        public TResult FuncOnDb<TResult>(Converter<DbConnection, DbCommand> prepareCommandFunc, Converter<DbCommand, TResult> executeCommandFunc, bool disposeCommand = true)
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
                throw;
            }
            finally
            {
                if (disposeCommand)
                {
                    this.DisposeCommand(dbCommand);
                }

                this.CloseConnection(dbConnection);
            }
        }

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
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<int>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, string commandText)
        {
            return ExecuteNonQuery(transaction, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuerySpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<int>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            return ExecuteScalar(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalarSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public object ExecuteScalarSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public object ExecuteScalarSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<object>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(DbTransaction transaction, string commandText)
        {
            return ExecuteScalar(transaction, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalarSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public object ExecuteScalarSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public object ExecuteScalarSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<object>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command => command.ExecuteScalar());
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(CommandType commandType, string commandText)
        {
            return ExecuteDataset(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(string commandText)
        {
            return ExecuteDataset(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDatasetSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DataSet ExecuteDatasetSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DataSet ExecuteDatasetSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DataSet>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, string commandText)
        {
            return ExecuteDataset(transaction, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public DataSet ExecuteDatasetSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DataSet ExecuteDatasetSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DataSet ExecuteDatasetSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DataSet>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        DataSet result = new DataSet();

                        dbDataAdapter.Fill(result);

                        return result;
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return ExecuteReader(commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(string commandText)
        {
            return ExecuteReader(CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///   SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            return this.FuncOnDb<DbDataReader>(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, string commandText)
        {
            return ExecuteReader(transaction, CommandType.Text, commandText, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public DbDataReader ExecuteReaderSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DbDataReader ExecuteReaderSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public DbDataReader ExecuteReaderSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            return this.FuncOnDbTransaction<DbDataReader>(
                transaction,
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    DbDataReader dbDataReader = command.ExecuteReader(CommandBehavior.CloseConnection);

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
                }, false);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(string commandText)
        {
            return ExecuteXmlReader(CommandType.Text, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSp(string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public XmlReader ExecuteXmlReaderSpDataRowParams(string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public XmlReader ExecuteXmlReaderSpObjectParams(string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDb<XmlReader>(
                    connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(transaction, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, string commandText)
        {
            return ExecuteXmlReader(transaction, CommandType.Text, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public XmlReader ExecuteXmlReaderSp(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public XmlReader ExecuteXmlReaderSpDataRowParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public XmlReader ExecuteXmlReaderSpObjectParams(DbTransaction transaction, string spName, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            if (this.GetXmlReaderFunc != null)
            {
                return this.FuncOnDbTransaction<XmlReader>(
                    transaction,
                    connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                    command => this.GetXmlReaderFunc(command));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(commandType, commandText, dataSet, tableNames, (DbParameter[])null);
        }

        /// <summary>
        /// Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            this.ActionOnDb(
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public void FillDataset(string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(CommandType.Text, commandText, dataSet, tableNames, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSp(string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDb(
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSpDataRowParams(string spName, DataSet dataSet, string[] tableNames, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDb(
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSpObjectParams(string spName, DataSet dataSet, string[] tableNames, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDb(
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, (DbParameter[])null);
        }

        /// <summary>
        /// Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.CheckStringNullOrWhiteSpace(commandText, "commandText");

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommand(null, connection, commandType, commandText, commandParameters),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public void FillDataset(DbTransaction transaction, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, CommandType.Text, commandText, dataSet, tableNames, (DbParameter[])null);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSp(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommandSp(null, connection, spName, parameterValues),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSpDataRowParams(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, DataRow dataRow)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommandSpDataRowParams(null, connection, spName, dataRow),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public void FillDatasetSpObjectParams(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, object data)
        {
            this.CheckStringNullOrWhiteSpace(spName, "spName");

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommandSpObjectParams(null, connection, spName, data),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
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

                        dbDataAdapter.Fill(dataSet);
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet);
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public void UpdateDataset(string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet)
        {
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
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();
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
        /// <remarks>
        /// e.g.:
        ///  UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public void UpdateDataset(string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet, string tableName)
        {
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
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();
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
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public void UpdateDataset(string selectCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            this.ActionOnDb(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataSet);

                            dataSet.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public void UpdateDataset(string selectCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            this.ActionOnDb(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataSet, tableName);

                            dataSet.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  UpdateDataTable(insertCommand, deleteCommand, updateCommand, dataTable);
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public void UpdateDataTable(string insertCommandText, string deleteCommandText, string updateCommandText, DataTable dataTable)
        {
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
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();
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
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public void UpdateDataTable(string selectCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            this.ActionOnDb(
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataTable);

                            dataTable.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet);
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public void UpdateDataset(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet)
        {
            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();
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
        /// <remarks>
        /// e.g.:
        ///  UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public void UpdateDataset(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataSet dataSet, string tableName)
        {
            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();
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
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public void UpdateDataset(DbTransaction transaction, string selectCommandText, DataSet dataSet)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataSet);

                            dataSet.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public void UpdateDataset(DbTransaction transaction, string selectCommandText, DataSet dataSet, string tableName)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataSet, tableName);

                            dataSet.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  UpdateDataTable(insertCommand, deleteCommand, updateCommand, dataTable);
        /// </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source.</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source.</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public void UpdateDataTable(DbTransaction transaction, string insertCommandText, string deleteCommandText, string updateCommandText, DataTable dataTable)
        {
            DbCommand insertCommand = null;
            DbCommand deleteCommand = null;
            DbCommand updateCommand = null;

            try
            {
                DbConnection dbConnection = dbConnection = transaction.Connection;

                insertCommand = this.PrepareCommand(null, dbConnection, insertCommandText);
                deleteCommand = this.PrepareCommand(null, dbConnection, deleteCommandText);
                updateCommand = this.PrepareCommand(null, dbConnection, updateCommandText);

                using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                {
                    dbDataAdapter.UpdateCommand = updateCommand;
                    dbDataAdapter.InsertCommand = insertCommand;
                    dbDataAdapter.DeleteCommand = deleteCommand;

                    dbDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();
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
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public void UpdateDataTable(DbTransaction transaction, string selectCommandText, DataTable dataTable)
        {
            this.CheckStringNullOrWhiteSpace(selectCommandText, "selectCommandText");

            if (dataTable == null)
            {
                throw new ArgumentNullException("dataTable");
            }

            this.ActionOnDbTransaction(
                transaction,
                connection => this.PrepareCommand(null, connection, selectCommandText),
                command =>
                {
                    using (DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter())
                    {
                        dbDataAdapter.SelectCommand = command;

                        using (DbCommandBuilder dbCommandBuilder = this.ProviderFactory.CreateCommandBuilder())
                        {
                            dbCommandBuilder.DataAdapter = dbDataAdapter;

                            dbDataAdapter.Update(dataTable);

                            dataTable.AcceptChanges();
                        }
                    }
                });
        }

        /// <summary>
        /// Convert DbDataReader result to generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="dbDataReader">DbDataReader instance.</param>
        /// <returns>A generic list containing the resultset.</returns>
        public  static List<T> ToList<T>(DbDataReader dbDataReader)
        {
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
                        if (name.Equals(propertyInfo.Name))
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
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        /// This method will assign a value of DbNull to any parameter with a direction of InputOutput and a value of null.
        /// This behavior will prevent default values from being used, but this will be the less common case than an intended pure output parameter (derived as InputOutput) where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added.</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command.</param>
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
        /// This method assigns dataRow column values to an array of SqlParameters.
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        private void AssignParameterValues(IList<DbParameter> commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || commandParameters.Count < 1 || dataRow == null)
            {
                return;
            }

            int i = 0;

            foreach (DbParameter commandParameter in commandParameters)
            {
                if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
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
        /// This method assigns an array of values to an array of SqlParameters.
        /// </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values.</param>
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
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters to the provided command.
        /// </summary>
        /// <param name="command">The SqlCommand to be prepared.</param>
        /// <param name="connection">A valid SqlConnection, on which to execute this command.</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required.</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwise is false.</param>
        private DbCommand PrepareCommand(DbTransaction transaction, DbConnection connection, CommandType commandType, string commandText, IList<DbParameter> commandParameters)
        {
            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.Connection = connection;
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }

                dbCommand.Transaction = transaction;
            }

            if (commandParameters != null && commandParameters.Count > 0)
            {
                AttachParameters(dbCommand, commandParameters);
            }

            return dbCommand;
        }

        private DbCommand PrepareCommand(DbTransaction transaction, DbConnection connection, string commandText)
        {
            DbCommand dbCommand = this.ProviderFactory.CreateCommand();

            dbCommand.Connection = connection;
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = CommandType.Text;

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }

                dbCommand.Transaction = transaction;
            }

            return dbCommand;
        }

        private DbCommand PrepareCommandSp(DbTransaction transaction, DbConnection connection, string spName, IList<object> parameterValues)
        {
            DbCommand dbCommand = null;

            if (parameterValues != null && parameterValues.Count > 0)
            {
                DbParameter[] commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, this.DiscoverParametersAction, spName);

                AssignParameterValues(commandParameters, parameterValues);

                dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, null);
            }

            return dbCommand;
        }

        private DbCommand PrepareCommandSpDataRowParams(DbTransaction transaction, DbConnection connection, string spName, DataRow dataRow)
        {
            DbCommand dbCommand = null;

            DbParameter[] commandParameters = null;

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                commandParameters = DbHelperParameterCache.GetSpParameterSet(connection, this.DiscoverParametersAction, spName);
                AssignParameterValues(commandParameters, dataRow);
            }

            dbCommand = this.PrepareCommand(transaction, connection, CommandType.StoredProcedure, spName, commandParameters);

            return dbCommand;
        }

        private DbCommand PrepareCommandSpObjectParams(DbTransaction transaction, DbConnection connection, string spName, object data)
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
        /// Open database connection.
        /// </summary>
        private DbConnection OpenConnection()
        {
            DbConnection result = this.ProviderFactory.CreateConnection();

            result.ConnectionString = this.ConnectionString;

            if (result.State != ConnectionState.Open)
            {
                result.Open();
            }

            return result;
        }

        /// <summary>
        /// Close database connection.
        /// </summary>
        private void CloseConnection(DbConnection dbConnection)
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        private void DisposeCommand(DbCommand dbCommand)
        {
            if (dbCommand != null)
            {
                dbCommand.Parameters.Clear();
                dbCommand.Dispose();
            }
        }

        /// <summary>
        /// Method CheckStringNullOrWhiteSpace.
        /// </summary>
        private void CheckStringNullOrWhiteSpace(string value, string paramName)
        {
            bool isNullOrWhiteSpace = true;

            if (value == null)
            {
                isNullOrWhiteSpace = true;
            }
            else
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        isNullOrWhiteSpace = false;
                        break;
                    }
                }
            }

            if (isNullOrWhiteSpace)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
