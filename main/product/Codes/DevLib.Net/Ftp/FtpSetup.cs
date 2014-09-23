//-----------------------------------------------------------------------
// <copyright file="FtpSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.Ftp
{
    using System;
    using System.Net;
    using System.Xml.Serialization;

    /// <summary>
    /// Class FtpSetup.
    /// </summary>
    [Serializable]
    public class FtpSetup
    {
        /// <summary>
        /// Field _ftpCredential.
        /// </summary>
        [NonSerialized]
        private NetworkCredential _ftpCredential;

        /// <summary>
        /// Field _hostName.
        /// </summary>
        private string _hostName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpSetup" /> class.
        /// </summary>
        public FtpSetup()
        {
            this.UseAnonymous = false;
            this.UserName = "anonymous";
            this.Password = "anonymous@anonymous.com";
            this.EnableSSL = false;
            this.KeepAlive = true;
            this.Proxy = null;
            this.ReadWriteTimeoutMilliseconds = 300000;
            this.UseBinary = true;
            this.UsePassive = true;
        }

        /// <summary>
        /// Gets the credentials used to communicate with the FTP server.
        /// </summary>
        [XmlIgnore]
        public NetworkCredential FtpCredential
        {
            get
            {
                if (this._ftpCredential == null)
                {
                    this._ftpCredential = new NetworkCredential(this.UserName ?? string.Empty, this.Password ?? string.Empty);
                }

                this._ftpCredential.UserName = this.UseAnonymous ? "anonymous" : this.UserName ?? string.Empty;
                this._ftpCredential.Password = this.Password ?? string.Empty;

                return this._ftpCredential;
            }
        }

        /// <summary>
        /// Gets or sets ftp host name.
        /// </summary>
        public string HostName
        {
            get
            {
                return this._hostName;
            }

            set
            {
                if (value.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
                {
                    this._hostName = value;
                }
                else
                {
                    this._hostName = string.Format("ftp://{0}", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether using anonymous logon.
        /// </summary>
        public bool UseAnonymous
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ftp username.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ftp password.
        /// </summary>
        public string Password
        {
            get;
            set;
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
        [XmlIgnore]
        public IWebProxy Proxy
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a time-out when reading from or writing to a stream, in milliseconds.
        /// </summary>
        public int ReadWriteTimeoutMilliseconds
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

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="FtpSetup" />.</returns>
        public override int GetHashCode()
        {
            if (object.ReferenceEquals(this, null))
            {
                return 0;
            }

            return this.UseAnonymous ?
                (this.HostName ?? string.Empty).ToLowerInvariant().GetHashCode() :
                (this.HostName ?? string.Empty).ToLowerInvariant().GetHashCode() ^ (this.UserName ?? string.Empty).GetHashCode() ^ (this.Password ?? string.Empty).GetHashCode();
        }
    }
}
