//-----------------------------------------------------------------------
// <copyright file="SqlHelper.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;

    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, scalable best practices for common uses of SqlClient.
    /// </summary>
    public sealed class SqlHelper
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="SqlHelper" /> class from being created.
        /// </summary>
        private SqlHelper()
        {
        }

        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        private enum SqlConnectionOwnership
        {
            /// <summary>
            /// Connection is owned and managed by
            /// </summary>
            Internal,

            /// <summary>
            /// Connection is owned and managed by the caller.
            /// </summary>
            External
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            int result = sqlCommand.ExecuteNonQuery();

            sqlCommand.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQuery(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
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
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(transaction, commandType, commandText, (SqlParameter[])null);
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static int ExecuteNonQuery(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            int result = sqlCommand.ExecuteNonQuery();

            sqlCommand.Parameters.Clear();

            return result;
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
        public static int ExecuteNonQuery(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
            {
                DataSet result = new DataSet();

                sqlDataAdapter.Fill(result);

                sqlCommand.Parameters.Clear();

                if (mustCloseConnection)
                {
                    connection.Close();
                }

                return result;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
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
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteDataset(transaction, commandType, commandText, (SqlParameter[])null);
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static DataSet ExecuteDataset(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
            {
                DataSet result = new DataSet();

                sqlDataAdapter.Fill(result);

                sqlCommand.Parameters.Clear();

                return result;
            }
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
        public static DataSet ExecuteDataset(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteReader(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static SqlDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            SqlConnection sqlConnection = null;

            try
            {
                sqlConnection = new SqlConnection(connectionString);

                sqlConnection.Open();

                return ExecuteReader(sqlConnection, null, commandType, commandText, commandParameters, SqlConnectionOwnership.Internal);
            }
            catch
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }

                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            return ExecuteReader(connection, (SqlTransaction)null, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
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
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteReader(transaction, commandType, commandText, (SqlParameter[])null);
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
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            return ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, SqlConnectionOwnership.External);
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
        public static SqlDataReader ExecuteReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in the connection string.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                return ExecuteScalar(sqlConnection, commandType, commandText, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteScalar(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object ExecuteScalar(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            object result = sqlCommand.ExecuteScalar();

            sqlCommand.Parameters.Clear();

            if (mustCloseConnection)
            {
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalar(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
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
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText)
        {
            return ExecuteScalar(transaction, commandType, commandText, (SqlParameter[])null);
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static object ExecuteScalar(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            object result = sqlCommand.ExecuteScalar();

            sqlCommand.Parameters.Clear();

            return result;
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
        public static object ExecuteScalar(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText)
        {
            return ExecuteXmlReader(connection, commandType, commandText, (SqlParameter[])null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO".</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static XmlReader ExecuteXmlReader(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;

            SqlCommand sqlCommand = new SqlCommand();

            try
            {
                PrepareCommand(sqlCommand, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

                XmlReader result = sqlCommand.ExecuteXmlReader();

                sqlCommand.Parameters.Clear();

                return result;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO".</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public static XmlReader ExecuteXmlReader(SqlConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
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
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText)
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            XmlReader result = sqlCommand.ExecuteXmlReader();

            sqlCommand.Parameters.Clear();

            return result;
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
        public static XmlReader ExecuteXmlReader(SqlTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
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
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                FillDataset(sqlConnection, commandType, commandText, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        public static void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                FillDataset(sqlConnection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in the connection string using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public static void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                FillDataset(sqlConnection, spName, dataSet, tableNames, parameterValues);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        public static void FillDataset(SqlConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the provided parameter values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// e.g.:
        ///  FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </remarks>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure.</param>
        public static void FillDataset(SqlConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }

        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:
        ///  FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="transaction">A valid SqlTransaction.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="dataSet">A dataset which will contain the resultset generated by the command.</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced by a user defined name (probably the actual table name).</param>
        /// <param name="commandParameters">An array of SqlParameters used to execute the command.</param>
        public static void FillDataset(SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
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
        public static void FillDataset(SqlTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if ((parameterValues != null) && (parameterValues.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, parameterValues);

                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
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
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }

            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }

            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                sqlDataAdapter.UpdateCommand = updateCommand;

                sqlDataAdapter.InsertCommand = insertCommand;

                sqlDataAdapter.DeleteCommand = deleteCommand;

                sqlDataAdapter.Update(dataSet);

                dataSet.AcceptChanges();
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
        public static void UpdateDataset(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }

            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }

            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                sqlDataAdapter.UpdateCommand = updateCommand;

                sqlDataAdapter.InsertCommand = insertCommand;

                sqlDataAdapter.DeleteCommand = deleteCommand;

                sqlDataAdapter.Update(dataSet, tableName);

                dataSet.AcceptChanges();
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataset(SqlConnection connection, string selectCommandText, DataSet dataSet)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connection))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataset(SqlConnection connection, string selectCommandText, DataSet dataSet, string tableName)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connection))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataset(string connectionString, string selectCommandText, DataSet dataSet)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connectionString))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataSet);

                    dataSet.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataset(string connectionString, string selectCommandText, DataSet dataSet, string tableName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connectionString))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataSet, tableName);

                    dataSet.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public static void UpdateDataset(SqlConnection connection, SqlCommand selectCommand, DataSet dataSet)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            UpdateDataset(connection, selectCommand.CommandText, dataSet);
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public static void UpdateDataset(SqlConnection connection, SqlCommand selectCommand, DataSet dataSet, string tableName)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            UpdateDataset(connection, selectCommand.CommandText, dataSet, tableName);
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        public static void UpdateDataset(string connectionString, SqlCommand selectCommand, DataSet dataSet)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            UpdateDataset(connectionString, selectCommand.CommandText, dataSet);
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataSet">The DataSet used to update the data source.</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public static void UpdateDataset(string connectionString, SqlCommand selectCommand, DataSet dataSet, string tableName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            UpdateDataset(connectionString, selectCommand.CommandText, dataSet, tableName);
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
        public static void UpdateDataTable(SqlCommand insertCommand, SqlCommand deleteCommand, SqlCommand updateCommand, DataTable dataTable)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }

            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }

            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter())
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                sqlDataAdapter.UpdateCommand = updateCommand;

                sqlDataAdapter.InsertCommand = insertCommand;

                sqlDataAdapter.DeleteCommand = deleteCommand;

                sqlDataAdapter.Update(dataTable);

                dataTable.AcceptChanges();
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataTable(SqlConnection connection, string selectCommandText, DataTable dataTable)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connection))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommandText">A <see cref="T:System.String" /> that is a Transact-SQL SELECT statement or stored procedure to be used by the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static void UpdateDataTable(string connectionString, string selectCommandText, DataTable dataTable)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(selectCommandText))
            {
                throw new ArgumentNullException("selectCommandText");
            }

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommandText, connectionString))
            {
                sqlDataAdapter.ContinueUpdateOnError = true;

                using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter))
                {
                    sqlDataAdapter.Update(dataTable);

                    dataTable.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="connection">A valid SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public static void UpdateDataTable(SqlConnection connection, SqlCommand selectCommand, DataTable dataTable)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            UpdateDataTable(connection, selectCommand.CommandText, dataTable);
        }

        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataTable.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="selectCommand">A <see cref="T:System.Data.SqlClient.SqlCommand" /> that is a Transact-SQL SELECT statement or stored procedure and is set as the <see cref="P:System.Data.SqlClient.SqlDataAdapter.SelectCommand" /> property of the <see cref="T:System.Data.SqlClient.SqlDataAdapter" />.</param>
        /// <param name="dataTable">The DataTable used to update the data source.</param>
        public static void UpdateDataTable(string connectionString, SqlCommand selectCommand, DataTable dataTable)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (selectCommand == null)
            {
                throw new ArgumentNullException("selectCommand");
            }

            UpdateDataTable(connectionString, selectCommand.CommandText, dataTable);
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        public static SqlCommand CreateCommand(SqlConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            SqlCommand result = new SqlCommand(spName, connection);

            result.CommandType = CommandType.StoredProcedure;

            if ((sourceColumns != null) && (sourceColumns.Length > 0))
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    commandParameters[i].SourceColumn = sourceColumns[i];
                }

                AttachParameters(result, commandParameters);
            }

            return result;
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQueryTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command.</returns>
        public static int ExecuteNonQueryTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the dataRow column values as the store procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command.</returns>
        public static DataSet ExecuteDatasetTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command.</returns>
        public static SqlDataReader ExecuteReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalarTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command.</returns>
        public static object ExecuteScalarTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid SqlConnection object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid SqlTransaction object.</param>
        /// <param name="spName">The name of the stored procedure.</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command.</returns>
        public static XmlReader ExecuteXmlReaderTypedParams(SqlTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }

            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }

            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection, spName);

                AssignParameterValues(commandParameters, dataRow);

                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                return ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
        }

        /// <summary>
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        /// This method will assign a value of DbNull to any parameter with a direction of InputOutput and a value of null.
        /// This behavior will prevent default values from being used, but this will be the less common case than an intended pure output parameter (derived as InputOutput) where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added.</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command.</param>
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandParameters != null)
            {
                foreach (SqlParameter commandParameter in commandParameters)
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
        private static void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if ((commandParameters == null) || (dataRow == null))
            {
                return;
            }

            int i = 0;

            foreach (SqlParameter commandParameter in commandParameters)
            {
                if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
                {
                    throw new Exception(string.Format(
                        "Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.",
                        i,
                        commandParameter.ParameterName));
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
        private static void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if ((commandParameters == null) || (parameterValues == null))
            {
                return;
            }

            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }

            for (int i = 0; i < commandParameters.Length; i++)
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
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed.")]
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }

            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            command.Connection = connection;

            command.CommandText = commandText;

            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }

                command.Transaction = transaction;
            }

            command.CommandType = commandType;

            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }

            return;
        }

        /// <summary>
        /// Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid SqlConnection, on which to execute this command.</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'.</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.).</param>
        /// <param name="commandText">The stored procedure name or T-SQL command.</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required.</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by </param>
        /// <returns>SqlDataReader containing the results of the command.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            bool mustCloseConnection = false;

            SqlCommand sqlCommand = new SqlCommand();

            try
            {
                PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

                SqlDataReader dataReader;

                if (connectionOwnership == SqlConnectionOwnership.External)
                {
                    dataReader = sqlCommand.ExecuteReader();
                }
                else
                {
                    dataReader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
                }

                bool canClear = true;

                foreach (SqlParameter commandParameter in sqlCommand.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                    {
                        canClear = false;
                    }
                }

                if (canClear)
                {
                    sqlCommand.Parameters.Clear();
                }

                return dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }

                throw;
            }
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        private static void FillDataset(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params SqlParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }

            SqlCommand sqlCommand = new SqlCommand();

            bool mustCloseConnection = false;

            PrepareCommand(sqlCommand, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
            {
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";

                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        if (string.IsNullOrEmpty(tableNames[i]))
                        {
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        }

                        sqlDataAdapter.TableMappings.Add(tableName, tableNames[i]);

                        tableName += (i + 1).ToString();
                    }
                }

                sqlDataAdapter.Fill(dataSet);

                sqlCommand.Parameters.Clear();
            }

            if (mustCloseConnection)
            {
                connection.Close();
            }
        }
    }
}
