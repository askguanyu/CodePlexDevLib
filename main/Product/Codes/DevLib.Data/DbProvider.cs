//-----------------------------------------------------------------------
// <copyright file="DbProvider.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Data
{
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
}
