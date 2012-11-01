//-----------------------------------------------------------------------
// <copyright file="AddInActivatorProcess.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a process for AddInDomain and handles things such as attach/detach events and restarting the process.
    /// </summary>
    internal class AddInActivatorProcess : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        private const string ConfigFileStringFormat = @"DevLib.AddIn.{0}.cfg";

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
        private bool _killRequested;

        /// <summary>
        ///
        /// </summary>
        private AddInActivatorClient _addInActivatorClient;

        /// <summary>
        ///
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="addInDomainSetup"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInActivatorProcess(string friendlyName, AddInDomainSetup addInDomainSetup)
        {
            this._friendlyName = friendlyName;
            this._addInDomainSetup = addInDomainSetup;
            this._assemblyFile = AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly(friendlyName, addInDomainSetup);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                //CreateNoWindow = true,
                FileName = this._assemblyFile,
                UseShellExecute = false,
                ErrorDialog = false,
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
        /// A proxy to the remote AddInActivator to use to create remote object instances.
        /// </summary>
        public AddInActivator AddInActivatorClient
        {
            get { return _addInActivatorClient != null ? _addInActivatorClient.AddInActivator : null; }
        }

        /// <summary>
        /// Starts the remote process which will host an AddInActivator.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Start()
        {
            this.CheckDisposed();
            this.DisposeClient();
            this._killRequested = false;
            bool isCreated;
            string guid = Guid.NewGuid().ToString();

            using (EventWaitHandle serverStartedHandle = new EventWaitHandle(false, EventResetMode.ManualReset, string.Format(AddInActivatorHost.AddInDomainEventNameStringFormat, guid), out isCreated))
            {
                if (!isCreated)
                {
                    throw new Exception("Event handle already existed for remote process.");
                }

                string addInDomainAssemblyPath = Path.GetFullPath(new Uri(typeof(AddInActivatorProcess).Assembly.CodeBase).AbsolutePath);

                AddInDomainSetup.WriteSetupFile(this._addInDomainSetup, this._addInDomainSetupFile);

                // args[0] = process domain assembly path
                // args[1] = guid
                // args[2] = process id
                // args[3] = ProcessDomainSetup file

                this._process.StartInfo.Arguments = string.Format("\"{0}\" {1} {2} \"{3}\"", addInDomainAssemblyPath, guid, Process.GetCurrentProcess().Id, this._addInDomainSetupFile);
                bool isStarted = this._process.Start();

                if (!isStarted)
                {
                    throw new Exception(string.Format("Failed to start process from: {0}", this._process.StartInfo.FileName));
                }

                if (!serverStartedHandle.WaitOne(_addInDomainSetup.ProcessStartTimeout))
                {
                    throw new Exception("Waiting for remote process to start timeout.");
                }

                this._process.PriorityClass = this._addInDomainSetup.ProcessPriority;

                this._addInActivatorClient = new AddInActivatorClient(guid, this._addInDomainSetup);

                this.RaiseEvent(Attached);
            }
        }

        /// <summary>
        /// Kills the remote process.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Kill()
        {
            this.CheckDisposed();

            this._killRequested = true;

            try
            {
                if (this._process != null && !this._process.HasExited)
                {
                    this._process.Kill();
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
                        throw new AddInDeleteOnUnloadException(string.Format("Failed to delete AddIn Domain '{0}' assembly", this._friendlyName), e);
                    }

                    try
                    {
                        File.Delete(this._addInDomainSetupFile);
                    }
                    catch (Exception e)
                    {
                        throw new AddInDeleteOnUnloadException(string.Format("Failed to delete AddIn Domain '{0}' configuration file", this._friendlyName), e);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
            DisposeClient();
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
        private void OnProcessExited(object sender, EventArgs e)
        {
            this.RaiseEvent(Detached);

            if (this._addInDomainSetup.RestartOnProcessExit && !this._killRequested && this._addInActivatorClient != null)
            {
                try
                {
                    this._process.Refresh();
                }
                catch
                {
                }
            }

            this._killRequested = false;
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
                    Thread.Sleep(100);
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
            if (this._disposed)
            {
                throw new ObjectDisposedException("AddInActivatorProcess");
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
