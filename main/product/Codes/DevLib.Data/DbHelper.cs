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
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// The DbHelper class is intended to encapsulate common uses for generic database access.
    /// </summary>
    public class DbHelper : IDisposable
    {
        /// <summary>
        /// Field DbProviderDictionary.
        /// </summary>
        private static readonly Dictionary<DbProvider, string> DbProviderDictionary;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

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
            this.ConnectionString = connectionString;
            this.ProviderFactory = DbProviderFactories.GetFactory(providerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbHelper" /> class.
        /// </summary>
        /// <param name="connectionString">The string used to open the database connection.</param>
        /// <param name="provider">DbProvider enum value.</param>
        public DbHelper(string connectionString, DbProvider provider)
        {
            this.ConnectionString = connectionString;
            this.ProviderFactory = DbProviderFactories.GetFactory(DbProviderDictionary[provider]);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DbHelper" /> class.
        /// </summary>
        ~DbHelper()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// DbProvider enum.
        /// </summary>
        public enum DbProvider
        {
            /// <summary>
            /// Represents SQL Server.
            /// </summary>
            SqlServer,

            /// <summary>
            /// Represents SQL Server CE.
            /// </summary>
            SqlServerCE,

            /// <summary>
            /// Represents My SQL.
            /// </summary>
            MySQL,

            /// <summary>
            /// Represents SQLite.
            /// </summary>
            SQLite,

            /// <summary>
            /// Represents Ole DB.
            /// </summary>
            OleDB,

            /// <summary>
            /// Represents ODBC.
            /// </summary>
            ODBC,

            /// <summary>
            /// Represents Oracle.
            /// </summary>
            Oracle,

            /// <summary>
            /// Represents PostgreSQL.
            /// </summary>
            PostgreSQL,

            /// <summary>
            /// Represents DB2.
            /// </summary>
            DB2,
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
        /// Gets the DbConnection used to open the connection.
        /// </summary>
        public DbConnection Connection
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
        /// Gets the DbTransaction.
        /// </summary>
        public DbTransaction Transaction
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
        /// Open database connection.
        /// </summary>
        public void OpenConnection()
        {
            this.CheckDisposed();

            if (this.Connection == null)
            {
                this.Connection = this.ProviderFactory.CreateConnection();
                this.Connection.ConnectionString = this.ConnectionString;
            }

            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
        }

        /// <summary>
        /// Close database connection.
        /// </summary>
        public void CloseConnection()
        {
            this.CheckDisposed();

            if (this.Transaction == null)
            {
                if (this.Connection != null)
                {
                    this.Connection.Dispose();
                    this.Connection = null;
                }
            }
        }

        /// <summary>
        /// Begin transaction.
        /// </summary>
        public void BeginTransaction()
        {
            this.CheckDisposed();

            this.OpenConnection();

            if (this.Transaction == null)
            {
                this.Transaction = this.Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Commit transaction.
        /// </summary>
        public void CommitTransaction()
        {
            this.CheckDisposed();

            if (this.Transaction != null)
            {
                this.Transaction.Commit();
                this.Transaction.Dispose();
                this.Transaction = null;
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Rollback transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            this.CheckDisposed();

            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
                this.Transaction.Dispose();
                this.Transaction = null;
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Execute a sql command.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataSet ExecuteDataSet(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckDisposed();

            try
            {
                this.OpenConnection();
                DbCommand dbCommand = this.CreateCommand(commandType, commandText, commandParameters);
                DbDataAdapter dbDataAdapter = this.ProviderFactory.CreateDataAdapter();
                dbDataAdapter.SelectCommand = dbCommand;
                DataSet dataSet = new DataSet();
                dbDataAdapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Execute a sql command.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckDisposed();

            try
            {
                this.OpenConnection();
                DbCommand dbCommand = this.CreateCommand(commandType, commandText, commandParameters);
                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Execute a sql command.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckDisposed();

            try
            {
                this.OpenConnection();
                DbCommand dbCommand = this.CreateCommand(commandType, commandText, commandParameters);
                return dbCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Execute a sql command.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A generic list containing the resultset generated by the command.</returns>
        public IList<T> ExecuteReaderList<T>(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            this.CheckDisposed();

            try
            {
                this.OpenConnection();
                DbCommand dbCommand = this.CreateCommand(commandType, commandText, commandParameters);
                DbDataReader dbDataReader = dbCommand.ExecuteReader();
                IList<T> result = this.ToList<T>(dbDataReader);
                return result;
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Execute a sql command.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public T ExecuteReaderObject<T>(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteReaderList<T>(commandType, commandText, commandParameters)[0];
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DbHelper" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DbHelper" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="DbHelper" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                if (this.Transaction != null)
                {
                    this.Transaction.Dispose();
                    this.Transaction = null;
                }

                if (this.Connection != null)
                {
                    this.Connection.Dispose();
                    this.Connection = null;
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Create sql command.
        /// </summary>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>DbCommand instance.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        private DbCommand CreateCommand(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            DbCommand dbCommand = this.Connection.CreateCommand();
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;
            dbCommand.Parameters.AddRange(commandParameters);

            if (this.Transaction != null)
            {
                dbCommand.Transaction = this.Transaction;
            }

            return dbCommand;
        }

        /// <summary>
        /// Convert DbDataReader result to generic list.
        /// </summary>
        /// <typeparam name="T">The type of the element of the list.</typeparam>
        /// <param name="dbDataReader">DbDataReader instance.</param>
        /// <returns>A generic list containing the resultset.</returns>
        private IList<T> ToList<T>(DbDataReader dbDataReader)
        {
            Type type = typeof(T);
            IList<T> result = null;

            if (type.IsValueType || type == typeof(string))
            {
                result = this.CreateValue<T>(dbDataReader, type);
            }
            else
            {
                result = this.CreateObject<T>(dbDataReader, type);
            }

            dbDataReader.Dispose();

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
        private IList<T> CreateObject<T>(DbDataReader dbDataReader, Type type)
        {
            IList<T> result = new List<T>();

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
        private IList<T> CreateValue<T>(DbDataReader dbDataReader, Type type)
        {
            IList<T> result = new List<T>();

            while (dbDataReader.Read())
            {
                T item = (T)Convert.ChangeType(dbDataReader[0], type, null);

                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Data.DbHelper");
            }
        }
    }
}
