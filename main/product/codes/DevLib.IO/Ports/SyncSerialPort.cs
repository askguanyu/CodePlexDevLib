//-----------------------------------------------------------------------
// <copyright file="SyncSerialPort.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.IO.Ports
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Threading;

    /// <summary>
    /// Represents a sync serial port resource.
    /// </summary>
    public class SyncSerialPort : MarshalByRefObject, IDisposable
    {
        /// <summary>
        /// Field _syncRoot.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _isSendingSync.
        /// </summary>
        private bool _isSendingSync = false;

        /// <summary>
        /// Field _serialPort.
        /// </summary>
        private SerialPort _serialPort = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncSerialPort" /> class.
        /// </summary>
        /// <param name="portName">The port to use (for example, COM1).</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="partity">One of the System.IO.Ports.SerialPort.Parity values.</param>
        /// <param name="dataBits">The data bits value.</param>
        /// <param name="stopBits">One of the System.IO.Ports.SerialPort.StopBits values.</param>
        public SyncSerialPort(string portName = "COM1", int baudRate = 9600, Parity partity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (string.IsNullOrEmpty(portName))
            {
                throw new ArgumentNullException("portName");
            }
            else
            {
                try
                {
                    this._serialPort = new SerialPort(portName, baudRate, partity, dataBits, stopBits);
                    this._serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
                    this._serialPort.WriteTimeout = SerialPort.InfiniteTimeout;
                    this._serialPort.DataReceived += this.SerialPortDataReceived;
                    this._serialPort.ErrorReceived += this.SerialPortErrorReceived;
                    this._serialPort.PinChanged += this.SerialPortPinChanged;
                    this.CurrentPort = portName;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SyncSerialPort" /> class.
        /// </summary>
        ~SyncSerialPort()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Represents the method that will handle the data received event of this SerialPort.
        /// </summary>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Represents the method that handles the error event of this SerialPort.
        /// </summary>
        public event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Represents the method that will handle the serial pin changed event of this SerialPort.
        /// </summary>
        public event EventHandler<SerialPinChangedEventArgs> PinChanged;

        /// <summary>
        /// Gets an array of serial port names for the current computer.
        /// </summary>
        public static string[] PortNames
        {
            get
            {
                return SerialPort.GetPortNames();
            }
        }

        /// <summary>
        /// Gets current using serial port name.
        /// </summary>
        public string CurrentPort
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the open or closed status of SyncSerialPort.
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this._serialPort.IsOpen;
            }
        }

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Open()
        {
            this.CheckDisposed();

            if (this._serialPort != null)
            {
                try
                {
                    if (!this._serialPort.IsOpen)
                    {
                        this._serialPort.Open();
                    }

                    return true;
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the port connection.
        /// </summary>
        /// <returns>true if succeeded; otherwise, false.</returns>
        public bool Close()
        {
            this.CheckDisposed();

            if (this._serialPort != null)
            {
                if (this._serialPort.IsOpen)
                {
                    this._serialPort.Close();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sync send a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="sendData">The byte array that contains the data to write to the port.</param>
        /// <param name="receivedData">The byte array to write the received data.</param>
        /// <param name="timeout">The number of milliseconds before a time-out occurs when a read operation does not finish.</param>
        /// <param name="waitTimeout">Whether read receive data after wait for timeout to expire or read on data received.</param>
        /// <returns>The number of bytes read.</returns>
        public int SendSync(byte[] sendData, out byte[] receivedData, int timeout = 1000, bool waitTimeout = false)
        {
            this.CheckDisposed();

            if (sendData == null || sendData.Length == 0)
            {
                throw new ArgumentNullException("sendData");
            }

            if (this._serialPort == null || !this.Open())
            {
                throw new IOException("The specified port could not be found or opened.");
            }

            if (timeout < 1)
            {
                timeout = 1;
            }

            lock (this._syncRoot)
            {
                int bytesRead = -1;

                try
                {
                    this._isSendingSync = true;

                    this._serialPort.DiscardInBuffer();

                    this._serialPort.DiscardOutBuffer();

                    this._serialPort.Write(sendData, 0, sendData.Length);

                    if (waitTimeout)
                    {
                        Thread.Sleep(timeout);
                    }
                    else
                    {
                        int timeoutCount = 0;

                        while (timeoutCount <= timeout)
                        {
                            if (this._serialPort.BytesToRead <= 0)
                            {
                                Thread.Sleep(1);
                            }

                            timeoutCount++;
                        }
                    }

                    Thread.Sleep(1);

                    if (this._serialPort.BytesToRead > 0)
                    {
                        byte[] result = new byte[this._serialPort.BytesToRead];

                        bytesRead = this._serialPort.Read(result, 0, result.Length);

                        receivedData = result;
                    }
                    else
                    {
                        receivedData = new byte[0];
                    }
                }
                catch (Exception e)
                {
                    ExceptionHandler.Log(e);

                    receivedData = new byte[0];
                }
                finally
                {
                    this._isSendingSync = false;
                }

                return bytesRead;
            }
        }

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="data">The byte array that contains the data to write to the port.</param>
        public void Send(byte[] data)
        {
            this.CheckDisposed();

            if (this._serialPort == null || !this.Open())
            {
                throw new IOException("The specified port could not be found or opened.");
            }

            this._serialPort.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="data">The byte array that contains the data to write to the port.</param>
        /// <param name="offset">The zero-based byte offset in the data parameter at which to begin copying bytes to the port.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void Send(byte[] data, int offset, int count)
        {
            this.CheckDisposed();

            if (this._serialPort == null || !this.Open())
            {
                throw new IOException("The specified port could not be found or opened.");
            }

            this._serialPort.Write(data, offset, count);
        }

        /// <summary>
        /// Reads all bytes from the serial port input buffer.
        /// </summary>
        /// <param name="timeout">The number of milliseconds before a time-out occurs when a read operation does not finish.</param>
        /// <param name="waitTimeout">Whether read receive data after wait for timeout to expire or read on data received.</param>
        /// <returns>The byte array of the receive buffer.</returns>
        public byte[] Read(int timeout = 1000, bool waitTimeout = false)
        {
            this.CheckDisposed();

            byte[] result = new byte[0];

            if (!this._isSendingSync)
            {
                if (this._serialPort == null || !this.Open())
                {
                    throw new IOException("The specified port could not be found or opened.");
                }

                if (timeout < 1)
                {
                    timeout = 1;
                }

                if (waitTimeout)
                {
                    Thread.Sleep(timeout);
                }
                else
                {
                    int timeoutCount = 0;

                    while (timeoutCount <= timeout)
                    {
                        if (this._serialPort.BytesToRead <= 0)
                        {
                            Thread.Sleep(1);
                        }

                        timeoutCount++;
                    }
                }

                Thread.Sleep(1);

                if (this._serialPort.BytesToRead > 0)
                {
                    result = new byte[this._serialPort.BytesToRead];
                }
            }

            return result;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="SyncSerialPort" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="SyncSerialPort" /> class.
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

                if (this._serialPort != null)
                {
                    this._serialPort.Dispose();
                    this._serialPort = null;
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
        /// Method SerialPortDataReceived.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of SerialDataReceivedEventArgs.</param>
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!this._isSendingSync)
            {
                this.RaiseEvent<SerialDataReceivedEventArgs>(this.DataReceived, this, e);
            }
        }

        /// <summary>
        /// Method SerialPortErrorReceived.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of SerialErrorReceivedEventArgs.</param>
        private void SerialPortErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            this.RaiseEvent<SerialErrorReceivedEventArgs>(this.ErrorReceived, this, e);
        }

        /// <summary>
        /// Method SerialPortPinChanged
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of SerialPinChangedEventArgs.</param>
        private void SerialPortPinChanged(object sender, SerialPinChangedEventArgs e)
        {
            this.RaiseEvent<SerialPinChangedEventArgs>(this.PinChanged, this, e);
        }

        /// <summary>
        /// Thread safety raise event.
        /// </summary>
        /// <typeparam name="T">The type of the event data generated by the event.</typeparam>
        /// <param name="source">Source EventHandler{T}.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        private void RaiseEvent<T>(EventHandler<T> source, object sender, T e) where T : EventArgs
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<T> safeHandler = Interlocked.CompareExchange(ref source, null, null);

            if (safeHandler != null)
            {
                safeHandler(sender, e);
            }
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.IO.Ports.SyncSerialPort");
            }
        }
    }
}
