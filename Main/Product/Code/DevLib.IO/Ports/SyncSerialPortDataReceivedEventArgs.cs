//-----------------------------------------------------------------------
// <copyright file="SyncSerialPortDataReceivedEventArgs.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.IO.Ports
{
    using System;
    using System.IO.Ports;

    /// <summary>
    /// Provides data for the DataReceived event.
    /// </summary>
    public class SyncSerialPortDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncSerialPortDataReceivedEventArgs" /> class.
        /// </summary>
        /// <param name="eventType">Specifies the type of character that was received on the serial port.</param>
        /// <param name="receivedData">Received bytes.</param>
        public SyncSerialPortDataReceivedEventArgs(SerialData eventType, byte[] receivedData)
        {
            this.EventType = eventType;
            this.ReceivedData = receivedData;
        }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public SerialData EventType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets received data.
        /// </summary>
        public byte[] ReceivedData
        {
            get;
            private set;
        }
    }
}
