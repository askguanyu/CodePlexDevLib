//-----------------------------------------------------------------------
// <copyright file="SqlHelperParameterCache.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters,
    /// and the ability to discover parameters for stored procedures at run-time.
    /// </summary>
    public sealed class SqlHelperParameterCache
    {
        /// <summary>
        /// Field ParamCache.
        /// </summary>
        private static readonly Hashtable ParamCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// Prevents a default instance of the <see cref="SqlHelperParameterCache" /> class from being created.
        /// </summary>
        private SqlHelperParameterCache()
        {
        }

        /// <summary>
        /// Add parameter array to the cache.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters to be cached.</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
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
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An array of SqlParameters.</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
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

            SqlParameter[] cachedParameters = ParamCache[hashKey] as SqlParameter[];

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
        /// Retrieves the set of SqlParameters appropriate for the stored procedure.
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <returns>An array of SqlParameters.</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return GetSpParameterSet(connectionString, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure.
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of SqlParameters.</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                return GetSpParameterSetInternal(sqlConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure.
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <returns>An array of SqlParameters.</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return GetSpParameterSet(connection, spName, false);
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure.
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of SqlParameters.</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            using (SqlConnection clonedSqlConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                return GetSpParameterSetInternal(clonedSqlConnection, spName, includeReturnValueParameter);
            }
        }

        /// <summary>
        /// Resolve at run time the appropriate set of SqlParameters for a stored procedure.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter.</param>
        /// <returns>The parameter array discovered.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            SqlCommand sqlCommand = new SqlCommand(spName, connection);

            sqlCommand.CommandType = CommandType.StoredProcedure;

            connection.Open();

            SqlCommandBuilder.DeriveParameters(sqlCommand);

            connection.Close();

            if (!includeReturnValueParameter)
            {
                sqlCommand.Parameters.RemoveAt(0);
            }

            SqlParameter[] discoveredParameters = new SqlParameter[sqlCommand.Parameters.Count];

            sqlCommand.Parameters.CopyTo(discoveredParameters, 0);

            foreach (SqlParameter discoveredParameter in discoveredParameters)
            {
                discoveredParameter.Value = DBNull.Value;
            }

            return discoveredParameters;
        }

        /// <summary>
        /// Deep copy of cached SqlParameter array.
        /// </summary>
        /// <param name="originalParameters">Original parameters.</param>
        /// <returns>A deep copy of original parameters.</returns>
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

            for (int i = 0; i < originalParameters.Length; i++)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        /// <summary>
        /// Retrieves the set of SqlParameters appropriate for the stored procedure.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results.</param>
        /// <returns>An array of SqlParameters.</returns>
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
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

            SqlParameter[] cachedParameters = null;

            cachedParameters = ParamCache[hashKey] as SqlParameter[];

            if (cachedParameters == null)
            {
                SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);

                ParamCache[hashKey] = spParameters;

                cachedParameters = spParameters;
            }

            return CloneParameters(cachedParameters);
        }
    }
}
