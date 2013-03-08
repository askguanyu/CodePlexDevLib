//-----------------------------------------------------------------------
// <copyright file="FtpSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.Net;

    /// <summary>
    /// Class FtpSetup.
    /// </summary>
    [Serializable]
    public class FtpSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpSetup" /> class.
        /// </summary>
        public FtpSetup()
        {
            this.EnableSSL = false;
            this.KeepAlive = true;
            this.Proxy = null;
            this.ReadWriteTimeout = 300000;
            this.UseBinary = true;
            this.UsePassive = true;
            this.FtpCredential = new NetworkCredential();
        }

        /// <summary>
        /// Gets the credentials used to communicate with the FTP server.
        /// </summary>
        public NetworkCredential FtpCredential
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets ftp host name.
        /// </summary>
        public string HostName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ftp username.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.FtpCredential != null ? this.FtpCredential.UserName : string.Empty;
            }

            set
            {
                if (this.FtpCredential == null)
                {
                    this.FtpCredential = new NetworkCredential();
                }

                this.FtpCredential.UserName = value;
            }
        }

        /// <summary>
        /// Gets or sets ftp password.
        /// </summary>
        public string Password
        {
            get
            {
                return this.FtpCredential != null ? this.FtpCredential.Password : string.Empty;
            }

            set
            {
                if (this.FtpCredential == null)
                {
                    this.FtpCredential = new NetworkCredential();
                }

                this.FtpCredential.Password = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an SSL connection should be used.
        /// </summary>
        public bool EnableSSL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the control connection to the FTP server is closed after the request completes.
        /// </summary>
        public bool KeepAlive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the proxy used to communicate with the FTP server.
        /// </summary>
        public IWebProxy Proxy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a time-out when reading from or writing to a stream, in milliseconds.
        /// </summary>
        public int ReadWriteTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use binary for file transfers.
        /// </summary>
        public bool UseBinary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether use passive behavior of a client application's data transfer process.
        /// </summary>
        public bool UsePassive
        {
            get;
            set;
        }
    }
}
