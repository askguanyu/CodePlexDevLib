//-----------------------------------------------------------------------
// <copyright file="TerminalServicesSession.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.TerminalServices
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Principal;
    using DevLib.TerminalServices.NativeAPI;

    /// <summary>
    /// A session on a terminal server.
    /// </summary>
    public class TerminalServicesSession
    {
        /// <summary>
        /// Field _applicationName.
        /// </summary>
        private readonly LazyLoadedProperty<string> _applicationName;

        /// <summary>
        /// Field _clientBuildNumber.
        /// </summary>
        private readonly LazyLoadedProperty<int> _clientBuildNumber;

        /// <summary>
        /// Field _clientDirectory.
        /// </summary>
        private readonly LazyLoadedProperty<string> _clientDirectory;

        /// <summary>
        /// Field _clientDisplay.
        /// </summary>
        private readonly LazyLoadedProperty<ClientDisplay> _clientDisplay;

        /// <summary>
        /// Field _clientHardwareId.
        /// </summary>
        private readonly LazyLoadedProperty<int> _clientHardwareId;

        /// <summary>
        /// Field _clientIPAddress.
        /// </summary>
        private readonly LazyLoadedProperty<IPAddress> _clientIPAddress;

        /// <summary>
        /// Field _clientName.
        /// </summary>
        private readonly LazyLoadedProperty<string> _clientName;

        /// <summary>
        /// Field _clientProductId.
        /// </summary>
        private readonly LazyLoadedProperty<short> _clientProductId;

        /// <summary>
        /// Field _clientProtocolType.
        /// </summary>
        private readonly LazyLoadedProperty<ClientProtocolType> _clientProtocolType;

        /// <summary>
        /// Field _connectTime.
        /// </summary>
        private readonly GroupLazyLoadedProperty<DateTime?> _connectTime;

        /// <summary>
        /// Field _connectState.
        /// </summary>
        private readonly GroupLazyLoadedProperty<ConnectState> _connectState;

        /// <summary>
        /// Field _currentTime.
        /// </summary>
        private readonly GroupLazyLoadedProperty<DateTime?> _currentTime;

        /// <summary>
        /// Field _disconnectTime.
        /// </summary>
        private readonly GroupLazyLoadedProperty<DateTime?> _disconnectTime;

        /// <summary>
        /// Field _domainName.
        /// </summary>
        private readonly GroupLazyLoadedProperty<string> _domainName;

        /// <summary>
        /// Field _incomingStatistics.
        /// </summary>
        private readonly GroupLazyLoadedProperty<ProtocolStatistics> _incomingStatistics;

        /// <summary>
        /// Field _initialProgram.
        /// </summary>
        private readonly LazyLoadedProperty<string> _initialProgram;

        /// <summary>
        /// Field _lastInputTime.
        /// </summary>
        private readonly GroupLazyLoadedProperty<DateTime?> _lastInputTime;

        /// <summary>
        /// Field _loginTime.
        /// </summary>
        private readonly GroupLazyLoadedProperty<DateTime?> _loginTime;

        /// <summary>
        /// Field _outgoingStatistics.
        /// </summary>
        private readonly GroupLazyLoadedProperty<ProtocolStatistics> _outgoingStatistics;

        /// <summary>
        /// Field _remoteEndPoint.
        /// </summary>
        private readonly LazyLoadedProperty<EndPoint> _remoteEndPoint;

        /// <summary>
        /// Field _server.
        /// </summary>
        private readonly TerminalServer _server;

        /// <summary>
        /// Field _sessionId.
        /// </summary>
        private readonly int _sessionId;

        /// <summary>
        /// Field _userName.
        /// </summary>
        private readonly GroupLazyLoadedProperty<string> _userName;

        /// <summary>
        /// Field _windowStationName.
        /// </summary>
        private readonly GroupLazyLoadedProperty<string> _windowStationName;

        /// <summary>
        /// Field _workingDirectory.
        /// </summary>
        private readonly LazyLoadedProperty<string> _workingDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalServicesSession" /> class.
        /// </summary>
        /// <param name="server">TerminalServer instance.</param>
        /// <param name="sessionId">Session Id.</param>
        internal TerminalServicesSession(TerminalServer server, int sessionId)
        {
            this._server = server;
            this._sessionId = sessionId;

            this._clientBuildNumber = new LazyLoadedProperty<int>(this.GetClientBuildNumber);
            this._clientIPAddress = new LazyLoadedProperty<IPAddress>(this.GetClientIPAddress);
            this._remoteEndPoint = new LazyLoadedProperty<EndPoint>(this.GetRemoteEndPoint);
            this._clientDisplay = new LazyLoadedProperty<ClientDisplay>(this.GetClientDisplay);
            this._clientDirectory = new LazyLoadedProperty<string>(this.GetClientDirectory);
            this._workingDirectory = new LazyLoadedProperty<string>(this.GetWorkingDirectory);
            this._initialProgram = new LazyLoadedProperty<string>(this.GetInitialProgram);
            this._applicationName = new LazyLoadedProperty<string>(this.GetApplicationName);
            this._clientHardwareId = new LazyLoadedProperty<int>(this.GetClientHardwareId);
            this._clientProductId = new LazyLoadedProperty<short>(this.GetClientProductId);
            this._clientProtocolType = new LazyLoadedProperty<ClientProtocolType>(this.GetClientProtocolType);
            this._clientName = new LazyLoadedProperty<string>(this.GetClientName);

            var loader = IsVistaSp1OrHigher ? (GroupPropertyLoader)this.LoadWtsInfoProperties : this.LoadWinStationInformationProperties;
            this._windowStationName = new GroupLazyLoadedProperty<string>(loader);
            this._connectState = new GroupLazyLoadedProperty<ConnectState>(loader);
            this._connectTime = new GroupLazyLoadedProperty<DateTime?>(loader);
            this._currentTime = new GroupLazyLoadedProperty<DateTime?>(loader);
            this._disconnectTime = new GroupLazyLoadedProperty<DateTime?>(loader);
            this._lastInputTime = new GroupLazyLoadedProperty<DateTime?>(loader);
            this._loginTime = new GroupLazyLoadedProperty<DateTime?>(loader);
            this._userName = new GroupLazyLoadedProperty<string>(loader);
            this._domainName = new GroupLazyLoadedProperty<string>(loader);
            this._incomingStatistics = new GroupLazyLoadedProperty<ProtocolStatistics>(loader);
            this._outgoingStatistics = new GroupLazyLoadedProperty<ProtocolStatistics>(loader);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminalServicesSession" /> class.
        /// </summary>
        /// <param name="server">TerminalServer instance.</param>
        /// <param name="sessionInfo">WTS_SESSION_INFO instance.</param>
        internal TerminalServicesSession(TerminalServer server, WTS_SESSION_INFO sessionInfo)
            : this(server, sessionInfo.SessionID)
        {
            this._windowStationName.Value = sessionInfo.WinStationName;
            this._connectState.Value = sessionInfo.ConnectState;
        }

        /// <summary>
        /// Gets incoming protocol statistics for the session.
        /// </summary>
        public ProtocolStatistics IncomingStatistics
        {
            get
            {
                return this._incomingStatistics.Value;
            }
        }

        /// <summary>
        /// Gets outgoing protocol statistics for the session.
        /// </summary>
        public ProtocolStatistics OutgoingStatistics
        {
            get
            {
                return this._outgoingStatistics.Value;
            }
        }

        /// <summary>
        /// Gets the name of the published application that this session is running.
        /// </summary>
        /// <remarks>
        /// This property may throw an exception for the console session (where <see cref="ClientProtocolType"/> is ClientProtocolType.Console ).
        /// </remarks>
        public string ApplicationName
        {
            get
            {
                return this._applicationName.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this session is running on the local terminal server.
        /// </summary>
        public bool IsLocal
        {
            get
            {
                return this._server.IsLocal;
            }
        }

        /// <summary>
        /// Gets the remote endpoint (IP address and port) of the client connected to the session.
        /// </summary>
        /// <remarks>
        /// This property currently supports only IPv4 addresses, and will be <c>null</c> if no client is connected to the session.
        /// </remarks>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this._remoteEndPoint.Value;
            }
        }

        /// <summary>
        /// Gets the initial program run when the session started.
        /// </summary>
        /// <remarks>
        /// This property may throw an exception for the console session (where <see cref="ClientProtocolType"/> is ClientProtocolType.Console ).
        /// </remarks>
        public string InitialProgram
        {
            get
            {
                return this._initialProgram.Value;
            }
        }

        /// <summary>
        /// Gets the working directory used when launching the initial program.
        /// </summary>
        /// <remarks>
        /// This property may throw an exception for the console session (where <see cref="ClientProtocolType"/> is ClientProtocolType.Console ).
        /// </remarks>
        public string WorkingDirectory
        {
            get
            {
                return this._workingDirectory.Value;
            }
        }

        /// <summary>
        /// Gets the protocol that the client is using to connect to the session.
        /// </summary>
        public ClientProtocolType ClientProtocolType
        {
            get
            {
                return this._clientProtocolType.Value;
            }
        }

        /// <summary>
        /// Gets the client-specific product identifier.
        /// </summary>
        /// <remarks>
        /// This value is typically <c>1</c> for the standard RDP client.
        /// </remarks>
        public short ClientProductId
        {
            get
            {
                return this._clientProductId.Value;
            }
        }

        /// <summary>
        /// Gets the client-specific hardware identifier.
        /// </summary>
        /// <remarks>
        /// This value is typically <c>0</c>.
        /// </remarks>
        public int ClientHardwareId
        {
            get
            {
                return this._clientHardwareId.Value;
            }
        }

        /// <summary>
        /// Gets the directory on the client computer in which the client software is installed.
        /// </summary>
        /// <remarks>
        /// This is typically the full path to the RDP ActiveX control DLL on the client machine; e.g. <c>C:\WINDOWS\SYSTEM32\mstscax.dll</c>.
        /// </remarks>
        public string ClientDirectory
        {
            get
            {
                return this._clientDirectory.Value;
            }
        }

        /// <summary>
        /// Gets the information about a client's display.
        /// </summary>
        public ClientDisplay ClientDisplay
        {
            get
            {
                return this._clientDisplay.Value;
            }
        }

        /// <summary>
        /// Gets the build number of the client.
        /// </summary>
        /// <remarks>
        /// Note that this does not include the major version, minor version, or revision number. It is only the build number.
        /// For example, the full file version of the RDP 6 client on Windows XP is 6.0.6001.18000, so this property will return 6001 for this client.
        /// May be zero, e.g. for a listening session.
        /// </remarks>
        public int ClientBuildNumber
        {
            get
            {
                return this._clientBuildNumber.Value;
            }
        }

        /// <summary>
        /// Gets the terminal server on which this session is located.
        /// </summary>
        public TerminalServer Server
        {
            get
            {
                return this._server;
            }
        }

        /// <summary>
        /// Gets the IP address reported by the client.
        /// </summary>
        /// <remarks>
        /// Note that this is not guaranteed to be the client's actual, remote IP address.
        /// If the client is behind a router with NAT, for example, the IP address reported will be the client's internal IP address on its LAN.
        /// </remarks>
        public IPAddress ClientIPAddress
        {
            get
            {
                return this._clientIPAddress.Value;
            }
        }

        /// <summary>
        /// Gets the name of the session's window station.
        /// </summary>
        public string WindowStationName
        {
            get
            {
                return this._windowStationName.Value;
            }
        }

        /// <summary>
        /// Gets the domain of the user account that last connected to the session.
        /// </summary>
        public string DomainName
        {
            get
            {
                return this._domainName.Value;
            }
        }

        /// <summary>
        /// Gets the user account that last connected to the session.
        /// </summary>
        public NTAccount UserAccount
        {
            get
            {
                return string.IsNullOrEmpty(this.UserName) ? null : new NTAccount(this.DomainName, this.UserName);
            }
        }

        /// <summary>
        /// Gets the name of the machine last connected to this session.
        /// </summary>
        public string ClientName
        {
            get
            {
                return this._clientName.Value;
            }
        }

        /// <summary>
        /// Gets the connection state of the session.
        /// </summary>
        public ConnectState ConnectState
        {
            get
            {
                return this._connectState.Value;
            }
        }

        /// <summary>
        /// Gets the time at which the user connected to this session.
        /// </summary>
        public DateTime? ConnectTime
        {
            get
            {
                return this._connectTime.Value;
            }
        }

        /// <summary>
        /// Gets the current time in the session.
        /// </summary>
        public DateTime? CurrentTime
        {
            get
            {
                return this._currentTime.Value;
            }
        }

        /// <summary>
        /// Gets the time at which the user disconnected from this session.
        /// </summary>
        public DateTime? DisconnectTime
        {
            get
            {
                return this._disconnectTime.Value;
            }
        }

        /// <summary>
        /// Gets the time at which this session last received input, mouse movements, key presses, etc.
        /// </summary>
        public DateTime? LastInputTime
        {
            get
            {
                return this._lastInputTime.Value;
            }
        }

        /// <summary>
        /// Gets the time at which the user logged into this session for the first time.
        /// </summary>
        public DateTime? LoginTime
        {
            get
            {
                return this._loginTime.Value;
            }
        }

        /// <summary>
        /// Gets length of time that the session has been idle.
        /// </summary>
        public TimeSpan IdleTime
        {
            get
            {
                if (this.ConnectState == ConnectState.Disconnected)
                {
                    if (this.CurrentTime != null && this.DisconnectTime != null)
                    {
                        return this.CurrentTime.Value - this.DisconnectTime.Value;
                    }
                }
                else
                {
                    if (this.CurrentTime != null && this.LastInputTime != null)
                    {
                        return this.CurrentTime.Value - this.LastInputTime.Value;
                    }
                }

                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Gets the Id of the session.
        /// </summary>
        public int SessionId
        {
            get
            {
                return this._sessionId;
            }
        }

        /// <summary>
        /// Gets the name of the user account that last connected to the session.
        /// </summary>
        public string UserName
        {
            get
            {
                return this._userName.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether
        /// </summary>
        private static bool IsVistaSp1OrHigher
        {
            get
            {
                return Environment.OSVersion.Version >= new Version(6, 0, 6001);
            }
        }

        /// <summary>
        /// Logs the session off, disconnecting any user that might be attached.
        /// </summary>
        /// <param name="synchronous">true to waits until the session is fully logged off before returning from the method. false to returns immediately, even though the session may not be completely logged off yet.</param>
        public void Logoff(bool synchronous = false)
        {
            NativeMethodsHelper.LogoffSession(this._server.Handle, this._sessionId, synchronous);
        }

        /// <summary>
        /// Disconnects any attached user from the session.
        /// </summary>
        /// <param name="synchronous">true to waits until the session is fully disconnected before returning from the method. false to returns immediately, even though the session may not be completely disconnected yet.</param>
        public void Disconnect(bool synchronous = false)
        {
            NativeMethodsHelper.DisconnectSession(this._server.Handle, this._sessionId, synchronous);
        }

        /// <summary>
        /// Displays a message box in the session and returns the user's response to the message box.
        /// </summary>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The caption of the message box.</param>
        /// <param name="synchronous">true to wait for and return the user's response to the message box. Otherwise, return immediately.</param>
        /// <param name="timeoutSeconds">The amount of time to wait for a response from the user before closing the message box. The system will wait forever if this is set to 0.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="icon">The icon to display in the message box.</param>
        /// <param name="defaultButton">The button that should be selected by default in the message box.</param>
        /// <param name="options">Options for the message box.</param>
        /// <returns>
        /// The user's response to the message box. If <paramref name="synchronous" /> is <c>false</c>, the method will always return <see cref="RemoteMessageBoxResult.Asynchronous" />.
        /// If the timeout expired before the user responded to the message box, the result will be <see cref="RemoteMessageBoxResult.Timeout" />.
        /// </returns>
        public RemoteMessageBoxResult MessageBox(
            string text,
            string caption = null,
            bool synchronous = false,
            int timeoutSeconds = 0,
            RemoteMessageBoxButtons buttons = RemoteMessageBoxButtons.Ok,
            RemoteMessageBoxIcon icon = RemoteMessageBoxIcon.None,
            RemoteMessageBoxDefaultButton defaultButton = RemoteMessageBoxDefaultButton.Button1,
            RemoteMessageBoxOptions options = RemoteMessageBoxOptions.None & RemoteMessageBoxOptions.TopMost)
        {
            timeoutSeconds = timeoutSeconds < 0 ? 0 : timeoutSeconds;

            var style = (int)buttons | (int)icon | (int)defaultButton | (int)options;

            var result = NativeMethodsHelper.SendMessage(
                this._server.Handle,
                this._sessionId,
                caption,
                text,
                style,
                timeoutSeconds,
                synchronous);

            return result == 0 ? RemoteMessageBoxResult.Timeout : result;
        }

        /// <summary>
        /// Retrieves a list of processes running in this session.
        /// </summary>
        /// <returns>A list of processes.</returns>
        public List<TerminalServicesProcess> GetProcesses()
        {
            var allProcesses = this._server.GetProcesses();

            var results = new List<TerminalServicesProcess>();

            foreach (TerminalServicesProcess process in allProcesses)
            {
                if (process.SessionId == this._sessionId)
                {
                    results.Add(process);
                }
            }

            return results;
        }

        /// <summary>
        /// Starts remote control of the session.
        /// </summary>
        /// <param name="hotkey">The key to press to stop remote control of the session.</param>
        /// <param name="hotkeyModifiers">The modifiers for the key to press to stop remote control.</param>
        public void StartRemoteControl(ConsoleKey hotkey, RemoteControlHotkeyModifiers hotkeyModifiers)
        {
            if (IsVistaSp1OrHigher)
            {
                NativeMethodsHelper.StartRemoteControl(this._server.Handle, this._sessionId, hotkey, hotkeyModifiers);
            }
            else
            {
                NativeMethodsHelper.LegacyStartRemoteControl(this._server.Handle, this._sessionId, hotkey, hotkeyModifiers);
            }
        }

        /// <summary>
        /// Stops remote control of the session. The session must be running on the local server.
        /// </summary>
        public void StopRemoteControl()
        {
            if (!this.IsLocal)
            {
                throw new InvalidOperationException(
                    "Cannot stop remote control on sessions that are running on remote servers");
            }

            if (IsVistaSp1OrHigher)
            {
                NativeMethodsHelper.StopRemoteControl(this._sessionId);
            }
            else
            {
                NativeMethodsHelper.LegacyStopRemoteControl(this._server.Handle, this._sessionId, true);
            }
        }

        /// <summary>
        /// Connects this session to an existing session. Both sessions must be running on the local server.
        /// </summary>
        /// <param name="target">The session to which to connect.</param>
        /// <param name="password">The password of the user logged on to the target session. If the user logged on to the target session is the same as the user logged on to this session, this parameter can be an empty string.</param>
        /// <param name="synchronous">true to waits until the operation has completed before returning from the method. false to returns immediately, even though the operation may not be complete yet.</param>
        public void Connect(TerminalServicesSession target, string password, bool synchronous)
        {
            if (!this.IsLocal)
            {
                throw new InvalidOperationException("Cannot connect sessions that are running on remote servers");
            }

            if (IsVistaSp1OrHigher)
            {
                NativeMethodsHelper.Connect(this._sessionId, target.SessionId, password, synchronous);
            }
            else
            {
                NativeMethodsHelper.LegacyConnect(this._server.Handle, this._sessionId, target.SessionId, password, synchronous);
            }
        }

        /// <summary>
        /// Method LoadWinStationInformationProperties.
        /// </summary>
        private void LoadWinStationInformationProperties()
        {
            var winStationInfo = NativeMethodsHelper.GetWinStationInformation(this._server.Handle, this._sessionId);
            this._windowStationName.Value = winStationInfo.WinStationName;
            this._connectState.Value = winStationInfo.ConnectState;
            this._connectTime.Value = NativeMethodsHelper.FileTimeToDateTime(winStationInfo.ConnectTime);
            this._currentTime.Value = NativeMethodsHelper.FileTimeToDateTime(winStationInfo.CurrentTime);
            this._disconnectTime.Value = NativeMethodsHelper.FileTimeToDateTime(winStationInfo.DisconnectTime);
            this._lastInputTime.Value = NativeMethodsHelper.FileTimeToDateTime(winStationInfo.LastInputTime);
            this._loginTime.Value = NativeMethodsHelper.FileTimeToDateTime(winStationInfo.LoginTime);
            this._userName.Value = winStationInfo.UserName;
            this._domainName.Value = winStationInfo.Domain;
            this._incomingStatistics.Value = new ProtocolStatistics(winStationInfo.ProtocolStatus.Input);
            this._outgoingStatistics.Value = new ProtocolStatistics(winStationInfo.ProtocolStatus.Output);
        }

        /// <summary>
        /// Method LoadWtsInfoProperties.
        /// </summary>
        private void LoadWtsInfoProperties()
        {
            var info = NativeMethodsHelper.QuerySessionInformationForStruct<WTSINFO>(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSSessionInfo);
            this._connectState.Value = info.ConnectState;
            this._incomingStatistics.Value = new ProtocolStatistics(info.IncomingBytes, info.IncomingFrames, info.IncomingCompressedBytes);
            this._outgoingStatistics.Value = new ProtocolStatistics(info.OutgoingBytes, info.OutgoingFrames, info.OutgoingCompressedBytes);
            this._windowStationName.Value = info.WinStationName;
            this._domainName.Value = info.Domain;
            this._userName.Value = info.UserName;
            this._connectTime.Value = NativeMethodsHelper.FileTimeToDateTime(info.ConnectTime);
            this._disconnectTime.Value = NativeMethodsHelper.FileTimeToDateTime(info.DisconnectTime);
            this._lastInputTime.Value = NativeMethodsHelper.FileTimeToDateTime(info.LastInputTime);
            this._loginTime.Value = NativeMethodsHelper.FileTimeToDateTime(info.LogonTime);
            this._currentTime.Value = NativeMethodsHelper.FileTimeToDateTime(info.CurrentTime);
        }

        /// <summary>
        /// Method GetClientName.
        /// </summary>
        /// <returns>Client name.</returns>
        private string GetClientName()
        {
            return NativeMethodsHelper.QuerySessionInformationForString(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientName);
        }

        /// <summary>
        /// Method GetApplicationName.
        /// </summary>
        /// <returns>Application name.</returns>
        private string GetApplicationName()
        {
            return NativeMethodsHelper.QuerySessionInformationForString(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSApplicationName);
        }

        /// <summary>
        /// Method GetRemoteEndPoint.
        /// </summary>
        /// <returns>Remote EndPoint.</returns>
        private EndPoint GetRemoteEndPoint()
        {
            return NativeMethodsHelper.QuerySessionInformationForEndPoint(this._server.Handle, this._sessionId);
        }

        /// <summary>
        /// Method GetInitialProgram.
        /// </summary>
        /// <returns>Initial program name.</returns>
        private string GetInitialProgram()
        {
            return NativeMethodsHelper.QuerySessionInformationForString(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSInitialProgram);
        }

        /// <summary>
        /// Method GetWorkingDirectory.
        /// </summary>
        /// <returns>Working directory.</returns>
        private string GetWorkingDirectory()
        {
            return NativeMethodsHelper.QuerySessionInformationForString(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSWorkingDirectory);
        }

        /// <summary>
        /// Method GetClientProtocolType.
        /// </summary>
        /// <returns>Client ProtocolType.</returns>
        private ClientProtocolType GetClientProtocolType()
        {
            return (ClientProtocolType)NativeMethodsHelper.QuerySessionInformationForShort(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientProtocolType);
        }

        /// <summary>
        /// Method GetClientProductId.
        /// </summary>
        /// <returns>Client ProductId.</returns>
        private short GetClientProductId()
        {
            return NativeMethodsHelper.QuerySessionInformationForShort(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientProductId);
        }

        /// <summary>
        /// Method GetClientHardwareId.
        /// </summary>
        /// <returns>Client HardwareId.</returns>
        private int GetClientHardwareId()
        {
            return NativeMethodsHelper.QuerySessionInformationForInt(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientHardwareId);
        }

        /// <summary>
        /// Method GetClientDirectory.
        /// </summary>
        /// <returns>Client directory.</returns>
        private string GetClientDirectory()
        {
            return NativeMethodsHelper.QuerySessionInformationForString(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientDirectory);
        }

        /// <summary>
        /// Method GetClientDisplay.
        /// </summary>
        /// <returns>Client display.</returns>
        private ClientDisplay GetClientDisplay()
        {
            var clientDisplay = NativeMethodsHelper.QuerySessionInformationForStruct<WTS_CLIENT_DISPLAY>(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientDisplay);

            return new ClientDisplay(clientDisplay);
        }

        /// <summary>
        /// Method GetClientIPAddress.
        /// </summary>
        /// <returns>Client IPAddress.</returns>
        private IPAddress GetClientIPAddress()
        {
            var clientAddress = NativeMethodsHelper.QuerySessionInformationForStruct<WTS_CLIENT_ADDRESS>(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientAddress);

            return NativeMethodsHelper.ExtractIPAddress(clientAddress.AddressFamily, clientAddress.Address);
        }

        /// <summary>
        /// Method GetClientBuildNumber.
        /// </summary>
        /// <returns>Client BuildNumber.</returns>
        private int GetClientBuildNumber()
        {
            return NativeMethodsHelper.QuerySessionInformationForInt(this._server.Handle, this._sessionId, WTS_INFO_CLASS.WTSClientBuildNumber);
        }
    }
}
