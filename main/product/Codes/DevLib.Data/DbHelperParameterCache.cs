//-----------------------------------------------------------------------
// <copyright file="DbHelperParameterCache.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides functions to leverage a static cache of procedure parameters, and the ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public static class DbHelperParameterCache
    {
        /// <summary>
        /// Field ParamCache.
        /// </summary>
        private static readonly Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Add parameter array to the cache.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a DbConnection.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of DbParameter to be cached.</param>
        public static void CacheParameterSet(string connectionString, string commandText, params DbParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            string hashKey = connectionString + ":" + commandText;

            ParamCache[hashKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve a parameter array from the cache.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a DbConnection.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An array of DbParameter.</returns>
        public static DbParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }

            string hashKey = connectionString + ":" + commandText;

            DbParameter[] cachedParameters = ParamCache[hashKey] as DbParameter[];

            if (cachedParameters == null)
            {
                return null;
            }
            else
            {
                return CloneParameters(cachedParameters);
            }
        }

        /// <summary>
        /// Retrieves the set of DbParameter appropriate for the stored procedure.
        /// </summary>
        /// <param name="dbProviderFactory">The database provider factory.</param>
        /// <param name="discoverParametersAction">The discover parameters action.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of DbParameter.</returns>
        public static DbParameter[] GetSpParameterSet(DbProviderFactory dbProviderFactory, Action<DbCommand> discoverParametersAction, string spName, bool includeReturnValueParameter = false)
        {
            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException("dbProviderFactory");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            using (DbConnection dbConnection = dbProviderFactory.CreateConnection())
            {
                return GetSpParameterSetInternal(dbConnection, discoverParametersAction, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of DbParameter appropriate for the stored procedure.
        /// </summary>
        /// <param name="connection">A valid DbConnection object.</param>
        /// <param name="discoverParametersAction">The discover parameters action.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of DbParameter.</returns>
        public static DbParameter[] GetSpParameterSet(DbConnection connection, Action<DbCommand> discoverParametersAction, string spName, bool includeReturnValueParameter = false)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            using (DbConnection clonedDbConnection = (DbConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedDbConnection, discoverParametersAction, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Resolve at run time the appropriate set of DbParameter for a stored procedure.
        /// </summary>
        /// <param name="connection">A valid DbConnection object.</param>
        /// <param name="discoverParametersAction">The discover parameters action.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter.</param>
        /// <returns>The parameter array discovered.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        private static DbParameter[] DiscoverSpParameterSet(DbConnection connection, Action<DbCommand> discoverParametersAction, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            DbCommand dbCommand = connection.CreateCommand();

            dbCommand.CommandText = spName;
            dbCommand.CommandType = CommandType.StoredProcedure;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            if (discoverParametersAction != null)
            {
                discoverParametersAction(dbCommand);
            }

            connection.Close();

            if (!includeReturnValueParameter)
            {
                dbCommand.Parameters.RemoveAt(0);
            }

            DbParameter[] discoveredParameters = new DbParameter[dbCommand.Parameters.Count];

            dbCommand.Parameters.CopyTo(discoveredParameters, 0);

            foreach (DbParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }

            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached DbParameter array.
        /// </summary>
        /// <param name="originalParameters">Original parameters.</param>
        /// <returns>A deep copy of original parameters.</returns>
        private static DbParameter[] CloneParameters(DbParameter[] originalParameters)
        {
            DbParameter[] clonedParameters = new DbParameter[originalParameters.Length];

            for (int i = 0; i < originalParameters.Length; i++)
            {
                clonedParameters[i] = (DbParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        /// <summary>
        /// Retrieves the set of DbParameter appropriate for the stored procedure.
        /// </summary>
        /// <param name="connection">A valid DbConnection object.</param>
        /// <param name="discoverParametersAction">The discover parameters action.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of DbParameter.</returns>
        private static DbParameter[] GetSpParameterSetInternal(DbConnection connection, Action<DbCommand> discoverParametersAction, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : string.Empty);

            DbParameter[] cachedParameters = null;

            cachedParameters = ParamCache[hashKey] as DbParameter[];

            if (cachedParameters == null)
            {
                DbParameter[] spParameters = DiscoverSpParameterSet(connection, discoverParametersAction, spName, includeReturnValueParameter);

                ParamCache[hashKey] = spParameters;

                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }
    }
}
