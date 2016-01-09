//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Web.Hosting.WebHost20.NativeAPI
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Class NativeMethods.
    /// </summary>
    internal static class NativeMethods
    {
        public const int SEC_E_OK = 0;
        public const int SEC_I_COMPLETE_AND_CONTINUE = 590612;
        public const int SEC_I_COMPLETE_NEEDED = 590611;
        public const int SEC_I_CONTINUE_NEEDED = 590610;
        public const int SECPKG_CRED_INBOUND = 1;
        public const int SECBUFFER_VERSION = 0;
        public const int SECBUFFER_EMPTY = 0;
        public const int SECBUFFER_DATA = 1;
        public const int SECBUFFER_TOKEN = 2;
        public const int SECURITY_NETWORK_DREP = 0;
        public const int ISC_REQ_DELEGATE = 1;
        public const int ISC_REQ_MUTUAL_AUTH = 2;
        public const int ISC_REQ_REPLAY_DETECT = 4;
        public const int ISC_REQ_SEQUENCE_DETECT = 8;
        public const int ISC_REQ_CONFIDENTIALITY = 16;
        public const int ISC_REQ_USE_SESSION_KEY = 32;
        public const int ISC_REQ_PROMPT_FOR_CREDS = 64;
        public const int ISC_REQ_USE_SUPPLIED_CREDS = 128;
        public const int ISC_REQ_ALLOCATE_MEMORY = 256;
        public const int ISC_REQ_STANDARD_FLAGS = 20;

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern bool ImpersonateSelf(int level);

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int RevertToSelf();

        [DllImport("KERNEL32.DLL", SetLastError = true)]
        public static extern IntPtr GetCurrentThread();

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int AcquireCredentialsHandle(string pszPrincipal, string pszPackage, uint fCredentialUse, IntPtr pvLogonID, IntPtr pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, ref SecHandle phCredential, ref long ptsExpiry);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int FreeCredentialsHandle(ref SecHandle phCredential);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int AcceptSecurityContext(ref SecHandle phCredential, IntPtr phContext, ref SecBufferDesc pInput, uint fContextReq, uint targetDataRep, ref SecHandle phNewContext, ref SecBufferDesc pOutput, ref uint pfContextAttr, ref long ptsTimeStamp);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int DeleteSecurityContext(ref SecHandle phContext);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int QuerySecurityContextToken(ref SecHandle phContext, ref IntPtr phToken);

        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode)]
        public static extern int CloseHandle(IntPtr phToken);
    }
}
