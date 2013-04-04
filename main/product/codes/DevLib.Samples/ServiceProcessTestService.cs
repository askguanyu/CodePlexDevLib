using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevLib.ServiceProcess;
using System.ServiceProcess;
using System.ComponentModel;

namespace DevLib.Samples
{
    [RunInstaller(true)]
    public class ServiceProcessTestService : WindowsServiceInstaller, IWindowsService
    {
        public ServiceProcessTestService()
        {
            this.WindowsServiceSetupInfo = new WindowsServiceSetup("AAA");
            base.InstallerSetupInfo = this.WindowsServiceSetupInfo;
        }

        public void OnStart(string[] args)
        {
            Console.WriteLine("OnStart");
        }

        public void OnStop()
        {
            Console.WriteLine("OnStop");
        }

        public void OnContinue()
        {
            Console.WriteLine("OnContinue");
        }

        public void OnPause()
        {
            Console.WriteLine("OnPause");
        }

        public void OnShutdown()
        {
            Console.WriteLine("OnShutdown");
        }

        public bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            Console.WriteLine("OnPowerEvent");
            return true;
        }

        public void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Console.WriteLine("OnSessionChange");
        }

        public void OnCustomCommand(int command)
        {
            Console.WriteLine("OnCustomCommand");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }

        public WindowsServiceSetup WindowsServiceSetupInfo
        {
            get;
            set;
        }
    }
}
