//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Net.NativeAPI
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const int RESOURCE_CONNECTED = 0x00000001;
        public const int RESOURCE_GLOBALNET = 0x00000002;
        public const int RESOURCE_REMEMBERED = 0x00000003;

        public const int RESOURCETYPE_ANY = 0x00000000;
        public const int RESOURCETYPE_DISK = 0x00000001;
        public const int RESOURCETYPE_PRINT = 0x00000002;

        public const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
        public const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
        public const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
        public const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
        public const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
        public const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;

        public const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
        public const int RESOURCEUSAGE_CONTAINER = 0x00000002;

        public const int CONNECT_INTERACTIVE = 0x00000008;
        public const int CONNECT_PROMPT = 0x00000010;
        public const int CONNECT_REDIRECT = 0x00000080;
        public const int CONNECT_UPDATE_PROFILE = 0x00000001;
        public const int CONNECT_COMMANDLINE = 0x00000800;
        public const int CONNECT_CMD_SAVECRED = 0x00001000;

        public const int CONNECT_LOCALDRIVE = 0x00000100;

        public const int NO_ERROR = 0;

        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_ALREADY_ASSIGNED = 85;
        public const int ERROR_BAD_DEVICE = 1200;
        public const int ERROR_BAD_NET_NAME = 67;
        public const int ERROR_BAD_PROVIDER = 1204;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_EXTENDED_ERROR = 1208;
        public const int ERROR_INVALID_ADDRESS = 487;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_INVALID_PASSWORD = 1216;
        public const int ERROR_MORE_DATA = 234;
        public const int ERROR_NO_MORE_ITEMS = 259;
        public const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        public const int ERROR_NO_NETWORK = 1222;

        public const int ERROR_BAD_PROFILE = 1206;
        public const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        public const int ERROR_DEVICE_IN_USE = 2404;
        public const int ERROR_NOT_CONNECTED = 2250;
        public const int ERROR_OPEN_FILES = 2401;

        public static readonly Dictionary<int, string> ErrorDictionary;

        static NativeMethods()
        {
            ErrorDictionary = new Dictionary<int, string>(20);

            ErrorDictionary.Add(ERROR_ACCESS_DENIED, "Error: Access Denied");
            ErrorDictionary.Add(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned");
            ErrorDictionary.Add(ERROR_BAD_DEVICE, "Error: Bad Device");
            ErrorDictionary.Add(ERROR_BAD_NET_NAME, "Error: Bad Net Name");
            ErrorDictionary.Add(ERROR_BAD_PROVIDER, "Error: Bad Provider");
            ErrorDictionary.Add(ERROR_CANCELLED, "Error: Cancelled");
            ErrorDictionary.Add(ERROR_EXTENDED_ERROR, "Error: Extended Error");
            ErrorDictionary.Add(ERROR_INVALID_ADDRESS, "Error: Invalid Address");
            ErrorDictionary.Add(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter");
            ErrorDictionary.Add(ERROR_INVALID_PASSWORD, "Error: Invalid Password");
            ErrorDictionary.Add(ERROR_MORE_DATA, "Error: More Data");
            ErrorDictionary.Add(ERROR_NO_MORE_ITEMS, "Error: No More Items");
            ErrorDictionary.Add(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path");
            ErrorDictionary.Add(ERROR_NO_NETWORK, "Error: No Network");
            ErrorDictionary.Add(ERROR_BAD_PROFILE, "Error: Bad Profile");
            ErrorDictionary.Add(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile");
            ErrorDictionary.Add(ERROR_DEVICE_IN_USE, "Error: Device In Use");
            ErrorDictionary.Add(ERROR_EXTENDED_ERROR, "Error: Extended Error");
            ErrorDictionary.Add(ERROR_NOT_CONNECTED, "Error: Not Connected");
            ErrorDictionary.Add(ERROR_OPEN_FILES, "Error: Open Files");
        }

        public static string GetErrorString(int errorCode)
        {
            if (ErrorDictionary.ContainsKey(errorCode))
            {
                return ErrorDictionary[errorCode];
            }
            else
            {
                return "Error: Unknown, " + errorCode.ToString();
            }
        }

        [DllImport("Mpr.dll", CharSet = CharSet.Unicode)]
        public static extern int WNetUseConnection(IntPtr hwndOwner, NETRESOURCE lpNetResource, string lpPassword, string lpUserID, int dwFlags, string lpAccessName, string lpBufferSize, string lpResult);

        [DllImport("Mpr.dll", CharSet = CharSet.Unicode)]
        public static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);
    }
}
