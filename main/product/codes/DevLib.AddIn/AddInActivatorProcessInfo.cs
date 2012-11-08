//-----------------------------------------------------------------------
// <copyright file="AddInActivatorProcessInfo.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    /// AddInActivator process infomation.
    /// </summary>
    public class AddInActivatorProcessInfo
    {
        /// <summary>
        ///
        /// </summary>
        public AddInActivatorProcessInfo()
        {
            this.MachineName = string.Empty;
            this.MainWindowTitle = string.Empty;
            this.ProcessName = string.Empty;
            this.BasePriority = -1;
            this.ExitCode = -1;
            this.Id = -1;
            this.NonpagedSystemMemorySize64 = -1;
            this.PagedMemorySize64 = -1;
            this.PagedSystemMemorySize64 = -1;
            this.PeakPagedMemorySize64 = -1;
            this.PeakVirtualMemorySize64 = -1;
            this.PeakWorkingSet64 = -1;
            this.PrivateMemorySize64 = -1;
            this.SessionId = -1;
            this.VirtualMemorySize64 = -1;
            this.WorkingSet64 = -1;
        }

        /// <summary>
        /// Gets the base priority of the associated process.
        /// </summary>
        public int BasePriority
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the value that the associated process specified when it terminated.
        /// </summary>
        public int ExitCode
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the time that the associated process exited.
        /// </summary>
        public DateTime ExitTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating whether the associated process has been terminated.
        /// </summary>
        public bool HasExited
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the unique identifier for the associated process.
        /// </summary>
        public int Id
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the name of the computer the associated process is running on.
        /// </summary>
        public string MachineName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the caption of the main window of the process.
        /// </summary>
        public string MainWindowTitle
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of nonpaged system memory allocated for the associated process.
        /// </summary>
        public long NonpagedSystemMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of paged memory allocated for the associated process.
        /// </summary>
        public long PagedMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of pageable system memory allocated for the associated process.
        /// </summary>
        public long PagedSystemMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the maximum amount of memory in the virtual memory paging file used by the associated process.
        /// </summary>
        public long PeakPagedMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the maximum amount of virtual memory used by the associated process.
        /// </summary>
        public long PeakVirtualMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the maximum amount of physical memory used by the associated process.
        /// </summary>
        public long PeakWorkingSet64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of private memory allocated for the associated process.
        /// </summary>
        public long PrivateMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the privileged processor time for this process.
        /// </summary>
        public TimeSpan PrivilegedProcessorTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the name of the process.
        /// </summary>
        public string ProcessName
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the Terminal Services session identifier for the associated process.
        /// </summary>
        public int SessionId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the time that the associated process was started.
        /// </summary>
        public DateTime StartTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the total processor time for this process.
        /// </summary>
        public TimeSpan TotalProcessorTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the user processor time for this process.
        /// </summary>
        public TimeSpan UserProcessorTime
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of the virtual memory allocated for the associated process.
        /// </summary>
        public long VirtualMemorySize64
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the amount of physical memory allocated for the associated process.
        /// </summary>
        public long WorkingSet64
        {
            get;
            internal set;
        }
    }
}
