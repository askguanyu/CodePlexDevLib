//-----------------------------------------------------------------------
// <copyright file="NetworkResource.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net
{
    using System;
    using DevLib.Net.NativeAPI;

    /// <summary>
    /// Network resource.
    /// </summary>
    public static class NetworkResource
    {
        /// <summary>
        /// Connects to the remote UNC.
        /// </summary>
        /// <param name="remoteUNC">The remote UNC path.</param>
        /// <returns>NetworkResource result.</returns>
        public static NetworkResourceResult ConnectRemote(string remoteUNC)
        {
            return ConnectRemote(remoteUNC, string.Empty, string.Empty, false);
        }

        /// <summary>
        /// Connects to the remote UNC.
        /// </summary>
        /// <param name="remoteUNC">The remote UNC path.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>NetworkResource result.</returns>
        public static NetworkResourceResult ConnectRemote(string remoteUNC, string username, string password)
        {
            return ConnectRemote(remoteUNC, username, password, false);
        }

        /// <summary>
        /// Connects to the remote UNC.
        /// </summary>
        /// <param name="remoteUNC">The remote UNC path.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="promptUser">true to prompt user; otherwise, false.</param>
        /// <returns>NetworkResource result.</returns>
        public static NetworkResourceResult ConnectRemote(string remoteUNC, string username, string password, bool promptUser)
        {
            NetworkResourceResult result = new NetworkResourceResult();

            int ret = -1;

            try
            {
                NETRESOURCE netResource = new NETRESOURCE
                {
                    dwType = NativeMethods.RESOURCETYPE_DISK,
                    lpRemoteName = remoteUNC
                };

                if (promptUser)
                {
                    ret = NativeMethods.WNetUseConnection(IntPtr.Zero, netResource, string.Empty, string.Empty, NativeMethods.CONNECT_INTERACTIVE | NativeMethods.CONNECT_PROMPT, null, null, null);
                }
                else
                {
                    ret = NativeMethods.WNetUseConnection(IntPtr.Zero, netResource, password, username, 0, null, null, null);
                }

                if (ret == NativeMethods.NO_ERROR)
                {
                    result.Succeeded = true;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = NativeMethods.GetErrorString(ret);
                }
            }
            catch (Exception e)
            {
                result.Succeeded = false;
                result.Message = e.ToString();
            }

            result.ErrorCode = ret;

            return result;
        }

        /// <summary>
        /// Disconnects the remote.
        /// </summary>
        /// <param name="remoteUNC">The remote UNC path.</param>
        /// <returns>NetworkResource result.</returns>
        public static NetworkResourceResult DisconnectRemote(string remoteUNC)
        {
            NetworkResourceResult result = new NetworkResourceResult();

            int ret = -1;

            try
            {
                ret = NativeMethods.WNetCancelConnection2(remoteUNC, NativeMethods.CONNECT_UPDATE_PROFILE, false);

                if (ret == NativeMethods.NO_ERROR)
                {
                    result.Succeeded = true;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = NativeMethods.GetErrorString(ret);
                }
            }
            catch (Exception e)
            {
                result.Succeeded = false;
                result.Message = e.ToString();
            }

            result.ErrorCode = ret;

            return result;
        }
    }
}
