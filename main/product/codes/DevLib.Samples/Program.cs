//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Samples
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using DevLib.AddIn;
    using DevLib.Compression;
    using DevLib.Configuration;
    using DevLib.DesignPatterns;
    using DevLib.Diagnostics;
    using DevLib.ExtensionMethods;
    using DevLib.Main;
    using DevLib.Net;
    using DevLib.Net.AsyncSocket;
    using DevLib.Net.Ftp;
    using DevLib.ServiceModel;
    using DevLib.ServiceProcess;
    using DevLib.TerminalServices;
    using DevLib.Utilities;
    using DevLib.WinForms;
    using System.ServiceModel.Routing;

    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            CodeTimer.Time(delegate
            {
                PrintStartInfo();

                var result = CodeTimer.Time(() =>
                {
                    //TestCodeSnippets();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibAddIn();
                });

                CodeTimer.Time(() =>
                {
                    //TestCompression();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibDesignPatterns();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibDiagnostics();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibExtensionMethods();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibNet();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibUtilities();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibServiceModel();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibServiceProcess(args);
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibTerminalServices();
                });

                CodeTimer.Time(delegate
                {
                    //TestDevLibConfiguration();
                });

                CodeTimer.Time(delegate
                {
                    new ThreadStart(() => { TestDevLibWinForms(); }).BeginInvoke((asyncResult) => { Console.WriteLine("WinForm exit..."); }, null);
                });

                PrintExitInfo();
            }, 1, "DevLib.Samples");
        }

        private static void TestDevLibTerminalServices()
        {
            PrintMethodName("Test DevLib.TerminalServices");

            //string domainname = string.Empty;

            //SelectQuery query = new SelectQuery("Win32_ComputerSystem");

            //using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            //{
            //    foreach (ManagementObject mo in searcher.Get())
            //    {
            //        if ((bool)mo["partofdomain"])
            //        {
            //            domainname = mo["domain"].ToString();
            //            break;
            //        }
            //    }
            //}

            Console.WriteLine(SystemInformation.UserDomainName);

            foreach (var item in TerminalServicesManager.GetServers(SystemInformation.UserDomainName))
            {
                item.Open();
                Console.WriteLine(item.ServerName);
                item.Dispose();
            }



            //DevLib.TerminalServices.RemoteServerHandle
            foreach (var item in TerminalServicesManager.GetLocalServer().GetSessions())
            {
                Console.WriteLine(@"/--------\");
                try
                {
                    Console.WriteLine(item.MessageBox("Hello", "123456789", false, 5, RemoteMessageBoxButtons.YesNoCancel, RemoteMessageBoxIcon.Exclamation));
                }
                catch
                {
                    //Console.WriteLine(e);
                }
                Console.WriteLine(item.SessionId);
                Console.WriteLine(item.UserName);
                Console.WriteLine(item.WindowStationName);
                Console.WriteLine(item.ConnectState);
                Console.WriteLine(@"\--------/");
            }
        }

        private static void TestCompression()
        {
            PrintMethodName("Test DevLib.Compression");

            ZipFile.CreateFromDirectory("E:\\A", "E:\\1.zip", false, true);

            Console.ReadLine();

            ZipFile.OpenRead("E:\\1.zip").ExtractToDirectory("E:\\B", true);
            //var zip = ZipFile.OpenRead("c:\\22.zip");
            //foreach (ZipArchiveEntry item in zip.Entries)
            //{
            //    Console.WriteLine(item.Name);
            //}

            //zip.ExtractToDirectory("c:\\3\\4", true);

            //ZipFile.CreateFromDirectory("c:\\1", "c:\\1.zip", true, true);
            //ZipFile.CreateFromDirectory("c:\\1", "c:\\2.zip", true, false);
            //ZipFile.CreateFromDirectory("c:\\1", "c:\\3.zip", false, true);
            //ZipFile.CreateFromDirectory("c:\\1", "c:\\4.zip", false, false);
            //var zip1 = ZipFile.Open("c:\\4.zip", ZipArchiveMode.Update);
            //zip1.CreateEntryFromFile("c:\\22.zip", "22.zip");
            //zip1.Dispose();
        }

        private static void TestDevLibServiceProcess(string[] args)
        {
            PrintMethodName("Test DevLib.ServiceProcess");

            ServiceProcessTestService testService = new ServiceProcessTestService();

            WindowsServiceBase.Run(testService, args);
        }

        private static void PrintStartInfo()
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Start DevLib.Samples ...");
            Console.ForegroundColor = originalForeColor;
            Console.WriteLine();
        }

        private static void PrintExitInfo()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void TestDevLibDesignPatterns()
        {
            PrintMethodName("Test DevLib.DesignPatterns");


            var a = Singleton<TestClass>.Instance;
            a.Age = 30;
            a.Name = "Same one";

            Parallel.For(0, 5, (i) =>
            {
                int age = i;
                Singleton<TestClass>.Instance.Age = age;
                Singleton<TestClass>.Instance.Age.ConsoleOutput();
                Singleton<TestClass>.Instance.Name.ConsoleOutput();
            });

            Singleton<TestClass>.Instance.Age.ConsoleOutput();

        }

        private static void TestDevLibAddIn()
        {
            PrintMethodName("Test DevLib.AddIn");

            var addin = new AddInDomain();
            addin.CreateInstance<DateTime>();

            //var info = new AddInActivatorProcessInfo();

            //var addin = new AddInDomain("DevLib.AddIn.Sample");
            //addin.Loaded += new EventHandler(addin_Started);
            //addin.Reloaded += new EventHandler(addin_Restarted);
            //addin.Unloaded += new EventHandler(addin_Stopped);
            //addin.DataReceived += new DataReceivedEventHandler(addin_DataReceived);

            //addin.CreateInstance<WcfServiceHost>(@"E:\Temp\WcfCalc.dll", @"E:\Temp\WcfCalc.dll.config");

            //using (AddInDomain domain = new AddInDomain("DevLib.AddIn.Sample1", false))
            //{
            //    var remoteObj = domain.CreateInstance<TestClass>();
            //    string a = remoteObj.ShowAndExit();
            //    Console.WriteLine(a);
            //}

            //AddInDomain domain = new AddInDomain("DevLib.AddIn.Sample1", true, new AddInDomainSetup { Platform = PlatformTargetEnum.AnyCPU });
            //AddInDomain domain3 = new AddInDomain("DevLib.AddIn.Sample3", true, new AddInDomainSetup { Platform = PlatformTargetEnum.AnyCPU });
            //var remoteObj = domain.CreateInstance<AsyncSocketServer>();
            //var remoteObj3 = domain3.CreateInstance<AsyncSocketServer>();
            //remoteObj.Start(2000);
            //remoteObj3.Start(2500);
            //remoteObj.DataReceived += remoteObj_DataReceived;
            //domain.DataReceived += domain_DataReceived;

            //remoteObj3.DataReceived += remoteObj_DataReceived;
            //domain3.DataReceived += domain_DataReceived;

            //Console.WriteLine("next");
            //domain.ProcessInfo.PrivateWorkingSetMemorySize.ConsoleOutput();
            //Console.ReadKey();
            //domain.AddInDomainSetupInfo.DllDirectory.ConsoleOutput();

            ////Task.Factory.StartNew(() =>
            ////{
            ////    Parallel.For(0, 2000, (loop) =>
            ////    {

            ////        AsyncSocketClient client1 = new AsyncSocketClient("127.0.0.1", 9999);
            ////        client1.Connect();
            ////        //client1.SendOnce("127.0.0.1", 9999, loop.ToString(), Encoding.ASCII);
            ////        client1.Send(loop.ToString(), Encoding.Default);
            ////    });
            ////});

            //Console.WriteLine("next");
            //Console.ReadKey();

            //AddInDomain domain1 = new AddInDomain("DevLib.AddIn.Sample2");
            //var remoteObj1 = domain1.CreateInstance<AsyncSocketClient>();
            //remoteObj1.Connect("127.0.0.1", 2500);

            //for (int i = 0; i < 20000; i++)
            //{
            //    new AsyncSocketClient("127.0.0.1", 2500).Connect().Send(i.ToString(), Encoding.Default);

            //    //remoteObj1.Send(DateTime.Now.ToString()+"  ", Encoding.Default);
            //}

            //Console.WriteLine("next1");
            //Console.ReadKey();

            //Task.Factory.StartNew(() =>
            //{
            //    Parallel.For(0, 20000, (loop) =>
            //    {
            //        new AsyncSocketClient("127.0.0.1", 999).Connect().Send(DateTime.Now.ToString(), Encoding.Default);
            //    });
            //});
            //remoteObj1.SendOnce("127.0.0.1", 9999, "!!!!!!!!!!!!!!!!hello555555555555", Encoding.Default);


            //Console.ReadKey();
            //domain1.AddInDomainSetupInfo.DllDirectory.ConsoleOutput();

            //domain.Dispose();
            //domain1.Dispose();

            //addin.CreateInstance<TestClass>().TestAdd(1,2).ConsoleOutput();

            //addin.AddInObject.ConsoleOutput();
            //addin.ProcessInfo.RetrieveProperties().ConsoleOutput();

            //addin.AddInObject.GetType().AssemblyQualifiedName.ConsoleOutput();

            //addin.Dispose();
            //addin.Dispose();
            //addin.Dispose();
            //addin.Reload();


            //var form = addin.CreateInstance<WinFormRibbon>();
            //form.ShowDialog();

            //wcf = addin.CreateInstance<WcfServiceHost>(new object[] { @"E:\Temp\WcfCalc.dll", @"E:\Temp\WcfCalc.dll.config" });
            //wcf.Initialize(@"E:\Temp\WcfCalc.dll", @"E:\Temp\WcfCalc.dll.config");
            //wcf.Open();

            //addin.Dispose();

            //WcfServiceHost wcf = n  WcfServiceHost();

            //ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, "WcfCalc.dll.config") }, ConfigurationUserLevel.None);
            ////WcfIsolatedServiceHost wcf = new WcfIsolatedServiceHost();
            //wcf.Initialize(Path.Combine(Environment.CurrentDirectory, "WcfCalc.dll"));
            //wcf.Open();

        }

        static void domain_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        static void remoteObj_DataReceived(object sender, AsyncSocketSessionEventArgs e)
        {

        }

        static void addin_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        static void addin_Started(object sender, EventArgs e)
        {


            Console.WriteLine("!!!!!!!!addin_Started    ");
            //e.ProcessInfo.RetrieveProperties().ForEach((k, v) => { Console.WriteLine(string.Format("{0} = {1}", k, v)); });
            Debug.WriteLine("!!!!!!!!addin_Started    ");
        }

        static void addin_Stopped(object sender, EventArgs e)
        {
            Console.WriteLine("!!!!!!!!addin_Stopped    ");
            //e.ProcessInfo.RetrieveProperties().ForEach((k, v) => { Console.WriteLine(string.Format("{0} = {1}", k, v)); });
            Debug.WriteLine("!!!!!!!!addin_Stopped    ");
        }

        static void addin_Restarted(object sender, EventArgs e)
        {
            Console.WriteLine("!!!!!!!!addin_Restarted    ");
            //e.ProcessInfo.RetrieveProperties().ForEach((k, v) => { Console.WriteLine(string.Format("{0} = {1}", k, v)); });
            Debug.WriteLine("!!!!!!!!addin_Restarted    ");
            var temp = (sender as AddInDomain).AddInObject as WcfServiceHost;
            if (temp != null)
            {
                temp.Open();
            }
        }

        private static void TestCodeSnippets()
        {
            PrintMethodName("Test CodeSnippets");

            object aPerson = new Person();
            aPerson.InvokeMethod("ShowName");
            aPerson.InvokeMethodGeneric("ShowName", null, typeof(string));


            DateTime a = new DateTime();
            a.IsWeekend().ConsoleOutput();

            var b1 = new Dictionary<int, string>(1000000);
            ReaderWriterLockSlim rwl1 = new ReaderWriterLockSlim();
            CodeTimer.Initialize();
            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        int i = loop;
                        rwl1.EnterWriteLock();
                        try
                        {
                            b1[i] = DateTime.Now.ToString();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            rwl1.ExitWriteLock();
                        }
                    });
                }
            }, 2, "ReaderWriterLockSlimDictW");

            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        string output;
                        int i = loop;
                        rwl1.EnterReadLock();
                        try
                        {
                            b1.TryGetValue(2, out output);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            rwl1.ExitReadLock();
                        }
                    });
                }
            }, 2, "ReaderWriterLockSlimDictR");

            var b = new Dictionary<int, string>(1000000);
            ReaderWriterLock rwl = new ReaderWriterLock();
            CodeTimer.Initialize();
            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        int i = loop;
                        rwl.AcquireWriterLock(Timeout.Infinite);
                        try
                        {
                            b[i] = DateTime.Now.ToString();
                        }
                        catch
                        {
                        }
                        finally
                        {
                            rwl.ReleaseWriterLock();
                        }
                    });
                }
            }, 2, "ReaderWriterLockDictW");

            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        string output;
                        int i = loop;
                        rwl.AcquireReaderLock(Timeout.Infinite);
                        try
                        {
                            b.TryGetValue(2, out output);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            rwl.ReleaseReaderLock();
                        }
                    });
                }
            }, 2, "ReaderWriterLockDictR");

            var d = new Dictionary<int, string>(1000000);
            CodeTimer.Initialize();
            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        int i = loop;
                        //c[i] = DateTime.Now.ToString();
                        lock (((ICollection)d).SyncRoot)
                        {
                            try
                            {
                                d[i] = DateTime.Now.ToString();
                            }
                            catch
                            {
                            }
                        }
                    });
                }
            }, 2, "LockDictW");

            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        int i = loop;
                        string output;
                        lock (((ICollection)d).SyncRoot)
                        {
                            try
                            {
                                d.TryGetValue(2, out output);
                            }
                            catch
                            {
                            }
                        }
                    });
                }
            }, 2, "LockDictR");





            var c = new Dictionary<int, string>(1000000);
            CodeTimer.Initialize();
            CodeTimer.Time(delegate
            {
                Parallel.For(0, 1000000, (loop) =>
                {
                    int i = loop;
                    c.Add(i, DateTime.Now.ToString());
                });
            }, 1, "DictW");

            CodeTimer.Time(delegate
            {
                for (int loop = 0; loop < 1000000; loop++)
                {
                    Task.Factory.StartNew(() =>
                    {
                        int i = loop;
                        string output;

                        try
                        {
                            c.TryGetValue(2, out output);
                        }
                        catch
                        {
                        }
                    });
                }
            }, 1, "DictR");

            var e = new ConcurrentDictionary<int, string>(10000, 1000000);
            CodeTimer.Initialize();
            CodeTimer.Time(delegate
            {
                Parallel.For(0, 1000000, (loop) =>
                {
                    int i = loop;
                    e.TryAdd(i, DateTime.Now.ToString());
                });
            }, 1, "ConcurrentDictionaryW");

            CodeTimer.Time(delegate
            {
                Parallel.For(0, 1000000, (loop) =>
                {
                    int i = loop;
                    string output;
                    e.TryGetValue(2, out output);
                });
            }, 1, "ConcurrentDictionaryR");


            Console.WriteLine("done!");
            Console.ReadKey();



            //AddInProcess addin = new AddInProcess(Platform.AnyCpu);
            //Activator.CreateInstance<>

            //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, "test.config") }, ConfigurationUserLevel.None);
            //Configuration config1 = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap() { ExeConfigFilename = Path.Combine(Environment.CurrentDirectory, Guid.NewGuid().ToString()) }, ConfigurationUserLevel.None);
            //config.Sections.Add(.AppSettings.Settings.Add("key1", "value1");
            //config.AppSettings.Settings.Add("key2", "value2");
            //config.AppSettings.Settings.Add("key3", "value3");
            //config.AppSettings.Settings["key3"].Value = "value3";
            //config.Save(ConfigurationSaveMode.Minimal, true);
            //config.SaveAs("test1.config", ConfigurationSaveMode.Minimal, true);

            //Properties.Settings.Default



            //Dns.GetHostAddresses("localhost").ForEach(p => p.ConsoleWriteLine());
            //NetworkInterface.GetAllNetworkInterfaces().ForEach(p => p.Id.ConsoleWriteLine());
            //"hello".ConsoleOutput();
            //"Hello".ConsoleOutput(" MD5 of {0} is ", false).ToMD5().ConsoleOutput();
            //TestEventClass obj = null;
            //"Hello".ConsoleOutput(obj);
            //"hello".ConsoleOutput("  ");

            //int[] a = null;
            //a.IsEmpty().ConsoleOutput();


            //AssemblyAccessor.AssemblyVersion().ConsoleOutput();
            //AssemblyAccessor.AssemblyDescription().ConsoleOutput();

            //Dictionary<int, string> dict = new Dictionary<int, string>();

            //for (int i = 0; i < 10; i++)
            //{
            //    dict.Add(i, i.ToString() + "Hello");
            //}

            //byte[] bys = new byte[] { 1, 2, 3, 4, 5 };
            //bys.ToHexString();
            //string.Empty.ToMD5().ConsoleOutput();
            //string.Empty.ToHexByteArray().ForEach((p) => { p.ConsoleOutput(); });
            //"monday".IsItemInEnum<DayOfWeek>().ConsoleOutput();
            //"asd".ToEnum<DayOfWeek>().ConsoleOutput();
            //decimal? de = null;
            //de.HasValue.ConsoleOutput("has value {0}");
            //long? lo = (long?)de;
            //lo.ConsoleOutput();

            //TestEventClass testclass = new TestEventClass() { MyName = "a" };
            //var a = XDocument.Parse(testclass.ToXml(Encoding.UTF8));
            //XmlDocument b = new XmlDocument();
            //XmlNode node = b.CreateElement("Hello");
            //"AppendChildNodeTo".AppendChildNodeTo(node);
            //node.CreateChildNode("CreateChildNode");
            //b.AppendChild(node);

            //"hello".Base64Encode().ConsoleOutput().Base64Decode().ConsoleOutput();
            //"Hello".ToByteArray().Compress(CompressionType.Deflate).Decompress(CompressionType.Deflate).ToEncodingString().ConsoleOutput();
            //"DevLib.ExtensionMethods.dll".ReadBinaryFile().ConsoleOutput().Compress().CreateBinaryFile("demo.bin").OpenContainingFolder();
            //Trace.Listeners.Add(new ConsoleTraceListener());
            //Trace.Listeners.Add(new TextWriterTraceListener("trace.log"));
            //Trace.AutoFlush = false;
            //Trace.WriteLine("Entering Main");
            //Trace.TraceError("hello error");
            //Trace.TraceError("hello error");
            //Console.WriteLine("Hello World.");
            //Trace.WriteLine("Exiting Main");
            //Trace.Unindent();

            //TestClass aclass = new TestClass() { MyName = "aaa" };
            //aclass.ToByteArray().Compress().WriteBinaryFile("test.bin").ReadBinaryFile().Decompress().ToObject<TestEventClass>().MyName.ConsoleOutput();
            //aclass.ToXml().ToByteArray(Encoding.Unicode).Compress().Decompress().ToEncodingString(Encoding.Unicode).FromXml<TestEventClass>().MyName.ConsoleOutput();

            //Environment.GetLogicalDrives().ForEach(p => p.ConsoleOutput());
            //Environment.MachineName.ConsoleOutput();
            //Environment.OSVersion.Platform.ConsoleOutput();
            //Environment.WorkingSet.ConsoleOutput();
            //Path.GetDirectoryName(@"""""""").ConsoleOutput();
            //@"""""""".GetFullPath().ConsoleOutput();

            //@"hello".Base64Encode().ConsoleOutput();

            //WMIUtilities.QueryWQL(WMIUtilities.MACADDRESS).ForEach(p => p.ConsoleOutput());


            //TraceSource ts = new TraceSource("TraceTest");
            //SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
            //ts.Switch = sourceSwitch;
            //int idxConsole = ts.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
            //ts.Listeners.Add(new TextWriterTraceListener("test.log"));
            //ts.Listeners[idxConsole].Name = "console";

            //ts.Listeners["console"].TraceOutputOptions |= TraceOptions.Callstack;
            //ts.TraceEvent(TraceEventType.Warning, 1);
            //ts.Listeners["console"].TraceOutputOptions = TraceOptions.DateTime;
            //// Issue file not found message as a warning.
            //ts.TraceEvent(TraceEventType.Warning, 2, "File Test not found");

            //// Issue file not found message as a verbose event using a formatted string.
            //ts.TraceEvent(TraceEventType.Verbose, 3, "File {0} not found.", "test");
            //// Issue file not found message as information.
            //ts.TraceInformation("File {0} not found.", "test");


            //ts.Listeners["console"].TraceOutputOptions |= TraceOptions.LogicalOperationStack;
            //// Issue file not found message as an error event.
            //ts.TraceEvent(TraceEventType.Error, 4, "File {0} not found.", "test");


            //// Test the filter on the ConsoleTraceListener.
            //ts.Listeners["console"].Filter = new SourceFilter("No match");
            //ts.TraceData(TraceEventType.Error, 5,
            //    "SourceFilter should reject this message for the console trace listener.");
            //ts.Listeners["console"].Filter = new SourceFilter("TraceTest");
            //ts.TraceData(TraceEventType.Error, 6,
            //    "SourceFilter should let this message through on the console trace listener.");
            //ts.Listeners["console"].Filter = null;


            //// Use the TraceData method. 
            //ts.TraceData(TraceEventType.Warning, 7, "hello");
            //ts.TraceData(TraceEventType.Warning, 8, new object[] { "Message 1", "Message 2" });


            // Activity tests.

            //ts.TraceEvent(TraceEventType.Start, 9, "Will not appear until the switch is changed.");
            //ts.Switch.Level = SourceLevels.ActivityTracing | SourceLevels.Critical;
            //ts.TraceEvent(TraceEventType.Suspend, 10, "Switch includes ActivityTracing, this should appear");
            //ts.TraceEvent(TraceEventType.Critical, 11, "Switch includes Critical, this should appear");

            //ts.Flush();
            //ts.Close();

            //CodeTimer.Initialize();
        }

        private static void TestDevLibDiagnostics()
        {
            PrintMethodName("Test Dev.Lib.Diagnostics");

            ConcurrentDictionary<int, string> safeDict = new ConcurrentDictionary<int, string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<int> list = new List<int>();
            ConcurrentBag<string> safeBag = new ConcurrentBag<string>();

            CodeTimer.Initialize();

            int times = 1000 * 10;

            TestClass testClass = new TestClass { Name = "Bill" };
            object[] parameters = new object[] { 1, 2 };
            MethodInfo methodInfo = typeof(TestClass).GetMethod("TestAdd");

            CodeTimer.Time(delegate() { testClass.TestAdd(1, 2); }, times);

            //methodInfo.Invoke(testClass, parameters).ConsoleOutput();
            //CodeTimer.Time(new Action(() =>
            //{
            //    methodInfo.Invoke(testClass, parameters);
            //}), times, "Reflection invoke1");

            //testClass.InvokeMethod("TestAdd", parameters).ConsoleOutput();
            //CodeTimer.Time(new Action(() =>
            //{
            //    testClass.InvokeMethod("TestAdd", parameters);
            //}), times, "Reflection invoke3");

            //ReflectionUtilities.DynamicMethodExecute(methodInfo, testClass, parameters).ConsoleOutput();
            //CodeTimer.Time(times, "DynamicMethodExecute", () =>
            //{
            //    ReflectionUtilities.DynamicMethodExecute(methodInfo, testClass, parameters);
            //});

            //ReflectionUtilities.FastInvokeExecute(methodInfo, testClass, parameters).ConsoleOutput();
            //CodeTimer.Time(times, "FastInvokeExecute", () =>
            //{
            //    ReflectionUtilities.FastInvokeExecute(methodInfo, testClass, parameters);
            //});


            //CodeTimer.Time(1, "No action", () => { });

            //new Action(() => { }).CodeTime(times);

            //CodeTimer.Time(times, "ConcurrentDictionary1", () =>
            //{
            //    safeDict.AddOrUpdate(1, "hello", (key, oldValue) => oldValue);
            //}, null);

            //CodeTimer.Time(times, "Dictionary1", () =>
            //{
            //    dict.Update(1, "hello");
            //});

            //CodeTimer.Time(times, "ConcurrentDictionary2", () =>
            //{
            //    safeDict.AddOrUpdate(2, "hello", (key, oldValue) => oldValue);
            //});

            //CodeTimer.Time(times, "Dictionary2", () =>
            //{
            //    dict.Update(2, "hello");
            //});

            //CodeTimer.Time(times, "ConcurrentBag1", () =>
            //{
            //    safeBag.Add("hello");
            //});
        }

        private static void TestDevLibExtensionMethods()
        {
            PrintMethodName("Test Dev.Lib.ExtensionMethods");

            #region SerializationExtensions

            Person person = new Person("foo", "好的", 1);
            //person.SerializeJson().ConsoleOutput().DeserializeJson<Person>();
            //var aperson = person.SerializeJson(Encoding.UTF8).ConsoleOutput().DeserializeJson<Person>(Encoding.UTF8);
            var aperson = person.SerializeXml().DeserializeXml<Person>().LastName.ConsoleOutput();

            Console.ReadKey();

            #endregion

            #region Array
            TestClass[] sourceArray
                = new TestClass[]
            {
                new TestClass(){ Name="a"},
                new TestClass(){ Name="b"},
                new TestClass(){ Name="c"},
            };

            TestClass[] appendArray = new TestClass[]
            {
                new TestClass(){ Name="d"},
                new TestClass(){ Name="e"},
                new TestClass(){ Name="f"},
            };

            int[] sourceValueTypeArray
                = new int[]
            {
                1,
                2,
                3,
            };

            int[] appendValueTypeArray = new int[]
            {
                4,
                5,
                6,
            };

            //sourceArray = null;
            appendArray.AddRangeTo(ref sourceArray, true);

            sourceArray[1].Name = "change1";
            appendArray[1].Name = "change2";
            sourceArray.ForEach((p) => { p.Name.ConsoleOutput(); });

            sourceValueTypeArray.AddRangeTo(ref appendValueTypeArray);
            appendValueTypeArray.ForEach((p) => { p.ConsoleOutput(); });
            "".ConsoleOutput();
            sourceValueTypeArray[1] = 3;
            appendValueTypeArray.FindArray(sourceValueTypeArray).ConsoleOutput();

            "End: ArrayExtensions".ConsoleOutput();
            #endregion

            #region Byte
            byte[] bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            //var obj = bytes.ToObject<int>();

            "compress".ConsoleOutput();
            var input = sourceArray.SerializeBinary();
            input.Length.ConsoleOutput();

            var compress = input.Compress();
            compress.Length.ConsoleOutput();

            var output = compress.Decompress();
            output.Length.ConsoleOutput();
            #endregion

            #region Collection

            #endregion

            #region EventHandler
            //TestEventClass testEventClassObject = new TestEventClass() { MyName = "testEventClassObject" };
            //testEventClassObject.OnTestMe += new EventHandler<EventArgs>(testEventClassObject_OnTestMe);
            //testEventClassObject.TestMe();
            #endregion

            #region IO
            var a = "   C:\\asdasd\\ \" \"   ";
            var c = Path.GetInvalidPathChars();
            string b = a.RemoveAny(c);
            "hello".WriteTextFile(@".\out\hello.txt").GetFullPath().OpenContainingFolder();

            //DateTime.Now.CreateBinaryFile(@".\out\list.bin").ConsoleWriteLine().ReadTextFile().ConsoleWriteLine();
            //@".\out\list.bin".ReadBinaryFile<DateTime>().ConsoleWriteLine();
            #endregion

            #region Object
            //sourceArray.ToJson().FromJson<TestEventClass[]>().ForEach((p) => { p.MyName.ConsoleWriteLine(); });
            //sourceArray.ToXml(Encoding.UTF8).ConsoleWriteLine();

            sourceArray.ForEach((p) => { p.CopyPropertiesFrom(appendArray[1]); });
            sourceArray.ForEach((p) => { p.Name.ConsoleOutput(); });
            "End: Object".ConsoleOutput();
            #endregion

            #region Security
            string inputString = "Hello I am secret.".ConsoleOutput();
            inputString.RSAEncrypt("key").ConsoleOutput().RSADecrypt("key").ConsoleOutput();
            inputString.ToMD5().ConsoleOutput().MD5VerifyToOriginal(inputString).ConsoleOutput();

            #endregion

            #region String

            #endregion

        }

        static AsyncSocketTcpServer staticserver = null;


        private static void TestDevLibNet()
        {
            PrintMethodName("Test Dev.Lib.Net");

            #region AsyncSocket

            //try
            //{
            //    AsyncSocketUdpServer udpserver = new AsyncSocketUdpServer(999);
            //    udpserver.DataReceived += server_DataReceived;
            //    udpserver.Start().IfTrue(() => { Console.WriteLine("udp server started."); });
            //}
            //catch (Exception e)
            //{
            //    e.ConsoleOutput();
            //}

            //Console.ReadKey();

            //AsyncSocketUdpClient.SendTo("127.0.0.1", 999, "Hello udp01".ToByteArray());
            //AsyncSocketUdpClient.SendTo("127.0.0.1", 999, "Hello udp02".ToByteArray());
            //AsyncSocketUdpClient.SendTo("127.0.0.1", 999, "Hello udp03".ToByteArray());
            //AsyncSocketUdpClient.SendTo("127.0.0.1", 999, "Hello udp04".ToByteArray());

            //AsyncSocketUdpClient udpclient = new AsyncSocketUdpClient("127.0.0.1", 999);
            //udpclient.Start();
            //udpclient.Send("Hello udp1".ToByteArray());
            //udpclient.Send("Hello udp2".ToByteArray());
            //udpclient.Send("Hello udp3".ToByteArray());
            //udpclient.Send("Hello udp4".ToByteArray());
            //udpclient.Stop();
            //udpclient.Send("Hello udp5".ToByteArray());
            //Console.WriteLine("udp client sent.");
            //Console.ReadKey();

            AddInDomain tcpdomain = null;

            try
            {
                //throw new Exception();

                tcpdomain = new AddInDomain("AsyncSocketTcpServer");
                staticserver = tcpdomain.CreateInstance<AsyncSocketTcpServer>();
                staticserver.LocalPort = 999;
            }
            catch (Exception e)
            {
                e.ConsoleOutput();

                try
                {
                    staticserver = new AsyncSocketTcpServer(999);
                }
                catch (Exception ee)
                {
                    ee.ConsoleOutput();
                }
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    staticserver.Connected += server_Connected;
                    staticserver.Disconnected += server_Disconnected;
                    staticserver.DataReceived += server_DataReceived;
                    staticserver.DataSent += server_DataSent;
                    staticserver.Start().IfTrue(() => "Started!".ConsoleOutput()).IfFalse(() => "Start failed!".ConsoleOutput());
                    new AsyncSocketTcpServer(999).Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            });

            Console.WriteLine("press to stop server");
            Console.ReadKey();
            try
            {
                staticserver.Stop().IfTrue(() => { Console.WriteLine("server stoped!"); Console.ReadKey(); });
            }
            catch (Exception e)
            {
                e.ConsoleOutput();
            }


            try
            {
                staticserver.Start().IfTrue(() => { Console.WriteLine("server start!"); Console.ReadKey(); });
            }
            catch (Exception e)
            {
                e.ConsoleOutput();
            }

            Console.WriteLine("tcp client.");
            Console.ReadKey();
            AsyncSocketTcpClient tcpclient = new AsyncSocketTcpClient("127.0.0.1", 999);
            tcpclient.DataReceived += tcpclient_DataReceived;
            tcpclient.Start();
            tcpclient.Send("hello1".ToByteArray(), "test token1");
            Console.WriteLine("tcp client over1.");
            Console.ReadKey();
            tcpclient.Send("hello2".ToByteArray(), "test token2");
            Thread.Sleep(50);
            tcpclient.Send("hello3".ToByteArray(), "test token3");
            Console.WriteLine("tcp client over2.");
            Console.ReadKey();

            AsyncSocketTcpClient client1 = new AsyncSocketTcpClient("127.0.0.1", 999);
            client1.Start();
            client1.Send("hello".ToByteArray());

            Console.WriteLine("Over1.");
            Console.ReadKey();
            //client1.Disconnect();

            Console.WriteLine("press to stop server");
            Console.ReadKey();
            try
            {
                staticserver.Stop().IfTrue(() => { Console.WriteLine("server stoped!"); Console.ReadKey(); });
            }
            catch (Exception e)
            {
                e.ConsoleOutput();
            }


            try
            {
                staticserver.Start().IfTrue(() => { Console.WriteLine("server start!"); Console.ReadKey(); });
            }
            catch (Exception e)
            {
                e.ConsoleOutput();
            }

            //AsyncSocketClient client = new AsyncSocketClient("127.0.0.1", 999);
            //client.Connect();
            //client.Send("a");
            //client.Send("b");

            //AsyncSocketClient client1 = new AsyncSocketClient("127.0.0.1", 999);
            //client1.Connect();
            //client1.Send("c");
            //client1.Send("d");

            //client.Send("a");

            string temp = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345 || ";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 1; i++)
            {
                builder.Append(temp);
            }


            //client1.Send(builder.ToString());
            Console.WriteLine("Over2.");
            Console.ReadKey();

            AsyncSocketTcpClient client2 = new AsyncSocketTcpClient("127.0.0.1", 999);
            client2.Start();
            client2.Send(builder.ToString().ToByteArray());

            Console.WriteLine("Over3.");
            Console.ReadKey();
            //client1.Send(new byte[1] { 1 });
            client2.Stop();



            Console.WriteLine("Over4.");
            Console.ReadKey();

            List<AsyncSocketTcpClient> clientList = new List<AsyncSocketTcpClient>();

            for (int loop = 0; loop < 1000; loop++)
            {
                clientList.Add(new AsyncSocketTcpClient("127.0.0.1", 999));
            }

            foreach (var item in clientList)
            {
                item.DataSent += item_DataSent;
                item.DataReceived += tcpclient_DataReceived;
                item.Start();
                //Thread.Sleep(5);
            }

            for (int i = 0; i < clientList.Count; i++)
            {
                int loop = i;
                Task.Factory.StartNew(() =>
                {
                    clientList[loop].Send(loop.ToString().ToByteArray(), loop);
                    //Thread.Sleep(5);
                });
            }

            //for (int i = 0; i < 1000; i++)
            //{
            //    Thread.Sleep(5);
            //    client.Send(i.ToString());
            //    client1.Send(i.ToString() + Environment.NewLine);
            //}


            Console.WriteLine("Press to Stop Server.");
            Console.ReadKey();
            staticserver.Stop();

            Console.WriteLine("Over.");
            Console.ReadKey();
            //AsyncSocketServer svr = new AsyncSocketServer();
            //svr.DataReceived += svr_DataReceived;
            //svr.Start(9999);

            //Task.Factory.StartNew(() =>
            //{
            //    Parallel.For(0, 2000, (loop) =>
            //    {

            //        AsyncSocketClient client1 = new AsyncSocketClient("127.0.0.1", 9999);
            //        client1.Connect();
            //        client1.SendOnce("127.0.0.1", 9999, loop.ToString(), Encoding.ASCII);
            //        client1.Send(loop.ToString(), Encoding.ASCII);
            //    });
            //});

            //Console.ReadKey();

            //AsyncSocketClient client = new AsyncSocketClient();
            //client.DataSent += new EventHandler<AsyncSocketUserTokenEventArgs>(client_DataSent);
            //client.Connect("127.0.0.1", 9999);
            //client.Send("hello1  你好 end", Encoding.UTF8);
            //Thread.Sleep(100);
            //client.Send("hello2  你好 end", Encoding.UTF8);
            //Thread.Sleep(100);
            //client.Send("hello3  你好 end", Encoding.UTF8);
            //Thread.Sleep(100);
            //client.Send("hello2  你好 end", Encoding.UTF32);
            //client.Send("hello3  你好 end", Encoding.BigEndianUnicode);
            //client.Send("hello4  你好 end", Encoding.ASCII);
            //client.Send("hello5  你好 end", Encoding.Unicode);

            //Console.ReadKey();

            //svr.Dispose();
            //client.Dispose();

            if (tcpdomain != null)
            {
                tcpdomain.Dispose();
            }


            #endregion

            //NetUtilities.GetLocalIPArray().ForEach((p) => { p.ConsoleOutput(); });
            //NetUtilities.GetRandomPortNumber().ConsoleOutput();

        }

        static void item_DataSent(object sender, AsyncSocketSessionEventArgs e)
        {
            "client sent: {0} token{1}".FormatWith(e.DataTransferred.ToEncodingString(), e.UserToken).ConsoleOutput();
        }

        static void tcpclient_DataReceived(object sender, AsyncSocketSessionEventArgs e)
        {
            "client received: {0} token{1}".FormatWith(e.DataTransferred.ToEncodingString(), e.UserToken).ConsoleOutput();
        }

        static void server_DataSent(object sender, AsyncSocketSessionEventArgs e)
        {
            //e.DataTransferred.ToEncodingString().ConsoleOutput(" sent!");
        }

        static byte[] senddata = "hello".ToByteArray(Encoding.ASCII);
        static void server_DataReceived(object sender, AsyncSocketSessionEventArgs e)
        {
            staticserver.Send(e.SessionId, senddata);
            //(sender as AsyncSocketTcpServer).Send(e.SessionId, e.DataTransferred);

            //e.DataTransferred.ToEncodingString().ConsoleOutput(" received!");

            //(sender as AsyncSocketTcpServer).Send(e.SessionId, "from server".ToByteArray(Encoding.UTF8));
            //Thread.Sleep(10);
            //(sender as AsyncSocketTcpServer).Send(e.SessionId, e.SessionIPEndPoint.ToString().ToByteArray(Encoding.UTF8));
            //Thread.Sleep(10);
            //try
            //{
            //    (sender as AsyncSocketTcpServer).Send(e.SessionId, e.DataTransferred);
            //}
            //catch (Exception ee)
            //{
            //    ee.ConsoleOutput();
            //}
            //(sender as AsyncSocketServer).GetRemoteIPEndPoint(e.sessionId).Port.ConsoleOutput();
        }

        static void server_Disconnected(object sender, AsyncSocketSessionEventArgs e)
        {
            e.SessionIPEndPoint.ConsoleOutput(" disconnected!");
            (sender as AsyncSocketTcpServer).ConnectedSocketsCount.ConsoleOutput("Disconnected! There are {0} clients connected to the server");
            (sender as AsyncSocketTcpServer).PeakConnectedSocketsCount.ConsoleOutput("Peak: {0} clients connected to the server");
        }

        static void server_Connected(object sender, AsyncSocketSessionEventArgs e)
        {
            e.SessionIPEndPoint.ConsoleOutput(" connected.");
            (sender as AsyncSocketTcpServer).ConnectedSocketsCount.ConsoleOutput("Connected. There are {0} clients connected to the server");
        }

        private static void TestDevLibWinForms()
        {
            PrintMethodName("Test Dev.Lib.WinForms");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WinFormRibbon());
        }

        private static void TestDevLibUtilities()
        {
            PrintMethodName("Test Dev.Lib.Utilities");

            NetUtilities.GetRandomPortNumber().ConsoleOutput();
            StringUtilities.GetRandomAlphabetString(32).ConsoleOutput();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private static void TestDevLibServiceModel()
        {
            PrintMethodName("Test Dev.Lib.ServiceModel");

            //new WcfServiceHost(typeof(RoutingService), "DevLib.Samples.exe.config", null, true);

            new WcfServiceHost(typeof(WcfTest), typeof(BasicHttpBinding), "http://127.0.0.1:6000/WcfTest", true);


            var client = WcfClientChannelFactory<IWcfTest>.CreateChannel(typeof(BasicHttpBinding), "http://127.0.0.1:6000/WcfTest", false);

            string a = string.Empty;
            object b = new object();

            try
            {
                a = client.MyOperation1("a", 1);
                b = client.Foo("aaa");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine(a);
            Console.WriteLine(b);

            Console.ReadLine();

            ////WcfServiceHost host = WcfServiceHost.Create(@"C:\YuGuan\Document\DevLib\DevLib.Samples\bin\Debug\Service1.dll", @"C:\YuGuan\Document\DevLib\DevLib.Samples\bin\Debug\Service1.dll.config");
            ////host.CurrentAppDomain.FriendlyName.ConsoleOutput("AppDomain");

            //var a = WcfServiceType.LoadFile(@"E:\Temp\WcfCalc.dll")[0].GetInterfaces()[0];

            //var wcfclient = WcfClientBase<IWcfTest>.GetReusableFaultUnwrappingInstance();

            ////var client1 = WcfClientBase<IWcfService>.GetInstance("");
            ////var x = new BasicHttpBinding().RetrieveProperties().SerializeBinary();

            //WcfServiceHost host = new WcfServiceHost();
            //host.Initialize(@"E:\Temp\WcfCalc.dll", typeof(BasicHttpBinding), @"http://localhost:888/abcd");

            //host.Opened += (s, e) => (e as WcfServiceHostEventArgs).WcfServiceName.ConsoleOutput("|Opened");
            //host.Closed += (s, e) => (e as WcfServiceHostEventArgs).WcfServiceName.ConsoleOutput("|Closed");
            //host.Reloaded += (s, e) => s.ConsoleOutput();

            //host.Open();
            //Console.WriteLine("first open");



            //Console.ReadKey();

            //host.Close();
            //Console.WriteLine("first close");
            //host.Open();
            //Console.WriteLine("2 open");
            //host.Close();
            //Console.WriteLine("2 close");
            //host.Open();
            //Console.WriteLine("3 open");
            //host.Abort();
            //Console.WriteLine("Abort");
            //host.Open();
            //Console.WriteLine("4 open");
            ////host.Restart();
            //host.GetAppDomain().FriendlyName.ConsoleOutput("|AppDomain");
            ////host.GetStateList().Values.ToList().ForEach(p => p.ConsoleOutput());
            ////var a = host.GetStateList();
            //Console.ReadKey();

            //host.Unload();
            ////host.GetStateList().Values.ToList().ForEach(p => p.ConsoleOutput());
            //host.Unload();
            //host.Unload();
            //host.Reload();
            //host.Reload();
            ////host.GetStateList().Values.ToList().ForEach(p => p.ConsoleOutput());
            //host.Open();
            ////host.GetStateList().Values.ToList().ForEach(p => p.ConsoleOutput());
            //host.GetAppDomain().FriendlyName.ConsoleOutput("|after reload AppDomain");

            //Console.ReadKey();
            //host.Dispose();
        }

        private static void TestDevLibConfiguration()
        {
            PrintMethodName("Test DevLib.Settings");

            var form = new WinFormConfigEditor();
            form.AddPlugin((fileName) => { return ConfigManager.Open(fileName).GetValue<TestConfig>("keyA"); }, (fileName, obj) => { ConfigManager.Open(fileName).SetValue("keyA", obj); ConfigManager.Open(fileName).Save(); });
            form.AddPlugin((fileName) => { return ConfigManager.Open(fileName).GetValue<TestConfig>("keyA"); }, (fileName, obj) => { ConfigManager.Open(fileName).SetValue("keyA", obj); ConfigManager.Open(fileName).Save(); }, "haha");

            //try
            //{
            //    form.OpenConfigFile(@"e:\d.xml");
            //}
            //catch
            //{
            //}
            Application.Run(form);


            Console.ReadKey();

            TestClass me = new TestClass() { Name = "Foo", Age = 29 };

            //Settings settings1 = SettingsManager.Open(Path.Combine(Environment.CurrentDirectory, "test3.xml"));
            //Settings settings2 = SettingsManager.Open(Path.Combine(Environment.CurrentDirectory, "test3.xml"));

            Hashtable a = new Hashtable();
            a.Add("hello", DateTime.Now);

            //settings1.SetValue("time0", a);
            //settings1.SetValue("time", DateTime.Now);
            //settings1.Remove("asdf");
            ////settings1.SetValue("time", DateTime.Now);
            ////settings1.SetValue("time", DateTime.Now);
            ////settings1.SetValue("txt1", "hello1");
            ////settings1.SetValue("color", (ConsoleColor)9);
            //settings1.SetValue("me", me);
            ////settings2.SetValue("time1", DateTime.Now);
            ////settings2.SetValue("time2", DateTime.Now);
            ////settings2.SetValue("time3", DateTime.Now);
            ////settings2.SetValue("txt2", "hello2");
            ////settings2.SetValue("color5", (ConsoleColor)15);
            ////settings2.SetValue("me1", me);
            //settings1.GetValue<DateTime>("time").ConsoleOutput();
            //settings1.GetValue<ConsoleColor>("color").ConsoleOutput();
            ////settings1.GetValue<TestClass>("me").Name.ConsoleOutput();
            ////settings1.GetValue<TestClass>("me").Age.ConsoleOutput();
            //settings1.GetValue<string>("hello2", "defalut").ConsoleOutput();
            //settings1.Save();
            //settings2.Save();
            //Dictionary<string, object> a = new Dictionary<string, object>();
            //IsolatedStorageSettings setting = IsolatedStorageSettings.ApplicationSettings;
            //a["key1"] = 1;
            //a["key2"] = "hello string";
            //a["key3"] = DateTime.Now;
            //a["key4"] = me;

            //a.ForEach((k, v) => { Console.WriteLine(k.ToString() + v.ToString()); });

            List<string> alist = new List<string>() { "a", "b", "c" };
            List<TestClass> blist = new List<TestClass>() { me, me, me };

            Config config = ConfigManager.Open("zzzz.xml");
            config.SetValue("hello", "a");
            config.SetValue("hello", blist);
            config.SetValue("hello", "b");
            config.Save();


            Settings setting = SettingsManager.Open("zzz.xml");
            Settings setting1 = SettingsManager.Open("zzz.xml");

            //setting["key1"] = 1;
            //setting["key2"] = "hello string";
            //setting["key3"] = DateTime.Now;
            //setting["key4"] = me;
            //setting["key5"] = alist;
            //setting["key6"] = blist;
            setting.Save();

            setting1["key3"] = DateTime.Now;
            setting1["key2"] = "hello string123";
            setting1.Save();
            setting.Reload();
            setting.ConfigFile.ConsoleOutput();
            setting.Values.ForEach(p => p.ToString().ConsoleOutput());

        }

        private static void PrintMethodName(string name)
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("{0} is running...", name);
            Console.ForegroundColor = originalForeColor;
        }

        private static void client_DataSent(object sender, AsyncSocketSessionEventArgs e)
        {

            Console.WriteLine();
        }

        private static void svr_DataReceived(object sender, AsyncSocketSessionEventArgs e)
        {

            //svr.Send(e.ConnectionId, e.TransferredRawData);
            Console.WriteLine();
        }

        private static void testEventClassObject_OnTestMe(object sender, EventArgs e)
        {
            (sender as TestClass).Name.ConsoleOutput();
        }
    }

    public class TestConfig
    {
        public TestConfig()
        {
            this.MySpell = new List<SpellingOptions>();
            this.MyString = Guid.NewGuid().ToString();
        }

        public int MyInt { get; set; }
        public string MyString { get; set; }

        [Editor(typeof(PropertyValueChangedCollectionEditor), typeof(UITypeEditor))]
        public List<SpellingOptions> MySpell { get; set; }

        public override string ToString()
        {
            return this.MyString;
        }
    }

    [DataContract()]
    [Serializable]
    public class TestClass
    {
        public event EventHandler<EventArgs> OnTestMe;

        public Person APerson { get; set; }

        public SpellingOptions Spell { get; set; }

        [DataMember()]
        public string Name
        {
            get;
            set;
        }

        [DataMember()]
        public int Age
        {
            get;
            set;
        }

        public void TestMe()
        {
            Console.WriteLine("TestClass.TestMe() done!");
            OnTestMe.RaiseEvent(this, new EventArgs());
        }

        public string ShowAndExit()
        {
            try
            {
                return "TestClass.ShowAndExit";
            }
            finally
            {
                var p = Process.GetCurrentProcess();
            }
        }

        public int TestAdd(int a, int b)
        {
            return a + b;
        }
    }

    [DataContract]
    [Serializable]
    [TypeConverterAttribute(typeof(ExpandableObjectConverter))]
    public class Person
    {
        public string Foo { get; set; }

        public Person()
        {

        }

        [DataMember()]
        public string FirstName;
        [DataMember]
        public string LastName;
        [DataMember()]
        public int ID;

        public Person(string newfName, string newLName, int newID)
        {
            FirstName = newfName;
            LastName = newLName;
            ID = newID;
        }

        public void ShowName()
        {
            Console.WriteLine("Normal Method");
        }

        public void ShowName(string a)
        {
            Console.WriteLine("Normal Method a");
        }

        public void ShowName<T>()
        {
            Console.WriteLine("Generic Method");
        }

        public void ShowName<T>(string a)
        {
            Console.WriteLine("Generic Method a");
        }

        public void ShowName<T0, T1>()
        {
            Console.WriteLine("Generic Method");
        }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter<SpellingOptions>))]
    public class SpellingOptions
    {
        private bool spellCheckWhileTyping = true;
        private bool spellCheckCAPS = false;
        private bool suggestCorrections = true;

        [DefaultValueAttribute(true)]
        public bool SpellCheckWhileTyping
        {
            get { return spellCheckWhileTyping; }
            set { spellCheckWhileTyping = value; }
        }

        [DefaultValueAttribute(false)]
        public bool SpellCheckCAPS
        {
            get { return spellCheckCAPS; }
            set { spellCheckCAPS = value; }
        }
        [DefaultValueAttribute(true)]
        public bool SuggestCorrections
        {
            get { return suggestCorrections; }
            set { suggestCorrections = value; }
        }
    }

}
