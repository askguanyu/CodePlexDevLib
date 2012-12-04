//-----------------------------------------------------------------------
// <copyright file="AddInActivatorProcess.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Represents a process for AddInDomain and handles things such as attach/detach events and restarting the process.
    /// </summary>
    internal class AddInActivatorProcess : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private const string ConfigFileStringFormat = @"{0}.exe.cfg";

        /// <summary>
        ///
        /// </summary>
        private readonly AddInDomainSetup _addInDomainSetup;

        /// <summary>
        ///
        /// </summary>
        private readonly string _assemblyFile;

        /// <summary>
        ///
        /// </summary>
        private readonly Process _process;

        /// <summary>
        ///
        /// </summary>
        private readonly string _friendlyName;

        /// <summary>
        ///
        /// </summary>
        private readonly string _addInDomainSetupFile;

        /// <summary>
        ///
        /// </summary>
        private AddInActivatorProcessInfo _addInActivatorProcessInfo;

        /// <summary>
        ///
        /// </summary>
        private AddInActivatorClient _addInActivatorClient;

        /// <summary>
        ///
        /// </summary>
        private bool _isDisposing;

        /// <summary>
        ///
        /// </summary>
        private bool _redirectOutput;

        /// <summary>
        ///
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="redirectOutput"></param>
        /// <param name="addInDomainSetup"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInActivatorProcess(string friendlyName, bool redirectOutput, AddInDomainSetup addInDomainSetup)
        {
            this._friendlyName = friendlyName;
            this._redirectOutput = redirectOutput;
            this._addInDomainSetup = addInDomainSetup;

            this._assemblyFile = AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly(friendlyName, addInDomainSetup);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                FileName = this._assemblyFile,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = this._addInDomainSetup.WorkingDirectory,
            };

            if (_addInDomainSetup.EnvironmentVariables != null)
            {
                foreach (KeyValuePair<string, string> item in this._addInDomainSetup.EnvironmentVariables)
                {
                    processStartInfo.EnvironmentVariables[item.Key] = item.Value;
                }
            }

            this._process = new Process
            {
                StartInfo = processStartInfo
            };

            this._addInDomainSetupFile = Path.Combine(addInDomainSetup.ExeFileDirectory, string.Format(ConfigFileStringFormat, friendlyName));

            this._process.OutputDataReceived += this.OnProcessDataReceived;
            this._process.ErrorDataReceived += this.OnProcessDataReceived;
            this._process.Exited += this.OnProcessExited;
            this._process.EnableRaisingEvents = true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancelEvent"></param>
        /// <returns></returns>
        private delegate void DeleteAssemblyFileDelegate(ManualResetEvent cancelEvent);

        /// <summary>
        ///
        /// </summary>
        public event EventHandler Attached;

        /// <summary>
        ///
        /// </summary>
        public event EventHandler Detached;

        /// <summary>
        ///
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// A proxy to the remote AddInActivator to use to create remote object instances.
        /// </summary>
        public AddInActivator AddInActivatorClient
        {
            get { return this._addInActivatorClient != null ? this._addInActivatorClient.AddInActivator : null; }
        }

        /// <summary>
        /// Gets a value indicating whether the process is started and running.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets AddInActivatorProcessInfo.
        /// </summary>
        public AddInActivatorProcessInfo ProcessInfo
        {
            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                if (this._addInActivatorProcessInfo == null)
                {
                    this._addInActivatorProcessInfo = new AddInActivatorProcessInfo();
                }

                if (this._process != null)
                {
                    this._process.Refresh();

                    try
                    {
                        this._addInActivatorProcessInfo.BasePriority = this._process.BasePriority;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.BasePriority = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ExitCode = this._process.ExitCode;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ExitCode = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ExitTime = this._process.ExitTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ExitTime = DateTime.MinValue;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.HasExited = this._process.HasExited;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.HasExited = false;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.MachineName = this._process.MachineName;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.MachineName = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.MainWindowTitle = this._process.MainWindowTitle;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.MainWindowTitle = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.NonpagedSystemMemorySize64 = this._process.NonpagedSystemMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.NonpagedSystemMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PagedMemorySize64 = this._process.PagedMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PagedMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PagedSystemMemorySize64 = this._process.PagedSystemMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PagedSystemMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakPagedMemorySize64 = this._process.PeakPagedMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakPagedMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakVirtualMemorySize64 = this._process.PeakVirtualMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakVirtualMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakWorkingSet64 = this._process.PeakWorkingSet64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakWorkingSet64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PID = this._process.Id;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PID = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PrivateMemorySize64 = this._process.PrivateMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PrivateMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PrivilegedProcessorTime = this._process.PrivilegedProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PrivilegedProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ProcessName = this._process.ProcessName;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ProcessName = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.SessionId = this._process.SessionId;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.SessionId = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.StartTime = this._process.StartTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.StartTime = DateTime.MinValue;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.TotalProcessorTime = this._process.TotalProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.TotalProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.UserProcessorTime = this._process.UserProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.UserProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.VirtualMemorySize64 = this._process.VirtualMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.VirtualMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.WorkingSet64 = this._process.WorkingSet64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.WorkingSet64 = -1;
                    }
                }

                return this._addInActivatorProcessInfo;
            }
        }

        /// <summary>
        /// Starts the remote process which will host an AddInActivator.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Start()
        {
            this.CheckDisposed();
            this.DisposeClient();

            string guid = Guid.NewGuid().ToString();
            bool isCreated;

            using (EventWaitHandle serverStartedHandle = new EventWaitHandle(false, EventResetMode.ManualReset, string.Format(AddInActivatorHost.AddInDomainEventNameStringFormat, guid), out isCreated))
            {
                if (!isCreated)
                {
                    throw new Exception(AddInConstants.EventHandleAlreadyExistedException);
                }

                string addInDomainAssemblyPath = typeof(AddInActivatorProcess).Assembly.Location;

                AddInDomainSetup.WriteSetupFile(this._addInDomainSetup, this._addInDomainSetupFile);

                // args[0] = AddInDomain assembly path
                // args[1] = GUID
                // args[2] = PID
                // args[3] = AddInDomainSetup file
                // args[4] = Redirect output or not

                this._process.StartInfo.Arguments = string.Format("\"{0}\" {1} {2} \"{3}\" {4}", addInDomainAssemblyPath, guid, Process.GetCurrentProcess().Id, this._addInDomainSetupFile, this._redirectOutput);
                this.IsRunning = this._process.Start();

                if (!this.IsRunning)
                {
                    Debug.WriteLine(string.Format(AddInConstants.ProcessStartExceptionStringFormat, this._process.StartInfo.FileName));
                    throw new Exception(string.Format(AddInConstants.ProcessStartExceptionStringFormat, this._process.StartInfo.FileName));
                }

                if (!serverStartedHandle.WaitOne(_addInDomainSetup.ProcessStartTimeout))
                {
                    Debug.WriteLine(AddInConstants.ProcessStartTimeoutException);
                    throw new Exception(AddInConstants.ProcessStartTimeoutException);
                }

                this._process.BeginOutputReadLine();
                this._process.BeginErrorReadLine();
                this._process.PriorityClass = this._addInDomainSetup.ProcessPriority;
                this._addInActivatorClient = new AddInActivatorClient(guid, this._addInDomainSetup);
                this.RaiseEvent(Attached);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (this._isDisposing)
            {
                return;
            }

            this._isDisposing = true;

            this.DisposeClient();
            this.Kill();

            this.IsRunning = false;
            this.RaiseEvent(Detached);
        }

        /// <summary>
        /// Kills the remote process.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void Kill()
        {
            try
            {
                if (this._process != null && !this._process.HasExited)
                {
                    this._process.Kill();
                    this._process.WaitForExit(1000);
                }
            }
            catch
            {
            }

            if (this._addInDomainSetup.DeleteOnUnload)
            {
                DeleteAssemblyFileDelegate deleteAssemblyFileDelegate = DeleteAssemblyFile;

                using (ManualResetEvent cancelEvent = new ManualResetEvent(false))
                {
                    IAsyncResult result = deleteAssemblyFileDelegate.BeginInvoke(cancelEvent, null, null);

                    if (!result.AsyncWaitHandle.WaitOne(_addInDomainSetup.FileDeleteTimeout))
                    {
                        cancelEvent.Set();
                    }

                    try
                    {
                        deleteAssemblyFileDelegate.EndInvoke(result);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(AddInConstants.DeleteFileExceptionStringFormat, this._friendlyName));
                        throw new AddInDeleteOnUnloadException(string.Format(AddInConstants.DeleteFileExceptionStringFormat, this._friendlyName), e);
                    }

                    try
                    {
                        File.Delete(this._addInDomainSetupFile);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(string.Format(AddInConstants.DeleteFileExceptionStringFormat, this._addInDomainSetupFile));
                        throw new AddInDeleteOnUnloadException(string.Format(AddInConstants.DeleteFileExceptionStringFormat, this._addInDomainSetupFile), e);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="eventHandler"></param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(null, null);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RaiseDataReceivedEvent(DataReceivedEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety
            DataReceivedEventHandler temp = Interlocked.CompareExchange(ref DataReceived, null, null);

            if (temp != null)
            {
                temp(null, e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                string output = string.Format(AddInConstants.ProcessOuputStringFormat, this._assemblyFile, e.Data);

                Debug.WriteLine(output);

                this.RaiseDataReceivedEvent(e);

                if (this._redirectOutput)
                {
                    Console.WriteLine(output);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnProcessExited(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cancelEvent"></param>
        private void DeleteAssemblyFile(ManualResetEvent cancelEvent)
        {
            bool isDeleted = false;
            bool isCanceled = false;

            Exception lastException = null;

            do
            {
                try
                {
                    File.Delete(this._assemblyFile);
                    isDeleted = true;
                }
                catch (Exception e)
                {
                    lastException = e;
                }

                isCanceled = cancelEvent.WaitOne(0);
            } while (!isDeleted && !isCanceled);

            if (!isDeleted && lastException != null)
            {
                throw lastException;
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void CheckDisposed()
        {
            if (this._isDisposing)
            {
                throw new ObjectDisposedException("DevLib.AddIn.AddInActivatorProcess");
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void DisposeClient()
        {
            if (this._addInActivatorClient != null)
            {
                this._addInActivatorClient.Dispose();
                this._addInActivatorClient = null;
            }
        }
    }
}
