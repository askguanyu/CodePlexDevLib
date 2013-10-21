//-----------------------------------------------------------------------
// <copyright file="SerialPortState.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.IO.Ports
{
    /// <summary>
    /// Enum SerialPortState.
    /// </summary>
    public enum SerialPortState
    {
        /// <summary>
        /// Represents Init.
        /// </summary>
        Init = 0,

        /// <summary>
        /// Represents Operate OK.
        /// </summary>
        OperateOK,

        /// <summary>
        /// Represents Not Found Port.
        /// </summary>
        NotFoundPort,

        /// <summary>
        /// Represents Not Exist Port.
        /// </summary>
        NotExistPort,

        /// <summary>
        /// Represents Serial Port Null.
        /// </summary>
        SerialPortNull,

        /// <summary>
        /// Represents Open Exception.
        /// </summary>
        OpenException,

        /// <summary>
        /// Represents Send Exception.
        /// </summary>
        SendException,

        /// <summary>
        /// Represents Send Data Empty.
        /// </summary>
        SendDataEmpty,

        /// <summary>
        /// Represents Read Timeout.
        /// </summary>
        ReadTimeout,

        /// <summary>
        /// Represents Read Exception.
        /// </summary>
        ReadException
    }
}
