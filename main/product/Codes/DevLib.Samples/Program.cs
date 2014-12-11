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
    using System.Data;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.ServiceModel;
    using System.ServiceModel.Routing;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using DevLib.AddIn;
    using DevLib.Compression;
    using DevLib.Configuration;
    using DevLib.Csv;
    using DevLib.DaemonProcess;
    using DevLib.DesignPatterns;
    using DevLib.Diagnostics;
    using DevLib.DirectoryServices;
    using DevLib.Dynamic;
    using DevLib.Expressions;
    using DevLib.ExtensionMethods;
    using DevLib.Input;
    using DevLib.IO.Ports;
    using DevLib.Ioc;
    using DevLib.Logging;
    using DevLib.Main;
    using DevLib.Net;
    using DevLib.Net.Ftp;
    using DevLib.Net.Sockets;
    using DevLib.Parameters;
    using DevLib.Reflection;
    using DevLib.Remoting;
    using DevLib.Serialization;
    using DevLib.ServiceModel;
    using DevLib.ServiceModel.Extensions;
    using DevLib.ServiceProcess;
    using DevLib.TerminalServices;
    using DevLib.Timers;
    using DevLib.Utilities;
    using DevLib.Web.Services;
    using DevLib.WinForms;
    using DevLib.Xml;

    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            InternalLogger.Log("Begin InternalLogger");
            LogManager.Open().Log("Begin LogManager");

            Benchmark.Run(delegate
            {
                PrintStartInfo();

                var result = Benchmark.Run(i =>
                {
                    //TestCodeSnippets();
                });

                Benchmark.Run(i =>
                {
                    //TestCsv();
                });

                Benchmark.Run(i =>
                {
                    //TestDynamic();
                });

                Benchmark.Run(i =>
                {
                    //TestReflection();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibAddIn();
                });

                Benchmark.Run(i =>
                {
                    //TestCompression();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibDaemonProcess();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibDesignPatterns();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibDiagnostics();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibExtensionMethods();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibIoc();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibLogging();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibNet();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibUtilities();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibServiceModel();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibServiceProcess(args);
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibTerminalServices();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibTimer();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibConfiguration();
                });

                Benchmark.Run(delegate
                {
                    //TestDevLibWinForms();
                    new ThreadStart(() => { TestDevLibWinForms(); }).BeginInvoke((asyncResult) => { Console.WriteLine("WinForm exit..."); }, null);
                });

                PrintExitInfo();
            }, 1, "DevLib.Samples");

            InternalLogger.Log("End");
        }

        private static void TestCsv()
        {
            //CsvDocument csv = new CsvDocument();
            //csv.Load(@"c:\test.csv", true, ';', '~');
            //DataTable dataTable = csv.Table;
            //csv.Table.Rows.RemoveAt(1);
            //csv.Table.Columns.Add("NewColumn");
            //csv.Table.Columns.Add("b");
            //csv.Table.Columns.Add("c");
            //csv[0,0] = "hello";
            //string a = csv[0,0];
            //List<string> headers = csv.ColumnNames;
            //int rowCount = csv.RowCount;
            //int columnCount = csv.ColumnCount;
            //csv[0, 0] = "hello";
            //csv.Save(@"c:\new.csv", true, false, true, false, ',', '"', Environment.NewLine);

            //string cellAtRow0Column1 = csv[0, 1];
            //string cellAtRow0ColumnNameA = csv[0, "A"];
            //DataRow row2 = csv[0];
            //DataColumn columnA = csv["A"];


        }

        private static void TestDynamic()
        {
            var json = DynamicJson.Parse(@"{""name"":""json"", ""age"":23, ""nest"":{ ""foobar"":true } }");

            string p1 = json.name; // "json" - dynamic(string)
            int p2 = json.age; // 23 - dynamic(double)
            bool p3 = json.nest.foobar; // true - dynamic(bool)
            bool p4 = json["nest"]["foobar"]; // can access string indexer
            bool p5 = json[2][0]; // can access int indexer

            var p6 = json.Has("name");// true
            var p7 = json.Has("address"); // false
            var p8 = (Dictionary<DynamicJson, DynamicJson>)json;
            json.Arr = new string[] { "ABC", "DEF" }; // Add Array
            json.Obj1 = new { }; // Add Object
            json.address = new { postcode = "abc120", street = "def" }; // Add and Init

            json.Remove("age");
            json.Arr.Remove(0);

            json.Obj1 = 5000; // use 5000 to replace new { }

            string jsonString = json.ToString();


            // DynamicJson - (IsArray)
            var arrayJson = DynamicJson.Parse(@"[1,10,200,300]");
            arrayJson[9] = 600;
            foreach (int item in arrayJson)
            {
                Console.WriteLine(item); // 1, 10, 200, 300, 600
            }

            // DynamicJson - (IsObject)
            var objectJson = DynamicJson.Parse(@"{""foo"":""json"",""bar"":100}");
            foreach (KeyValuePair<string, dynamic> item in objectJson)
            {
                Console.WriteLine(item.Key + ":" + (string)item.Value); // foo:json, bar:100
            }


            var arrayJson2 = DynamicJson.Parse(@"[1,10,200,300]");

            var array1 = (string[])json; // string[] {"1", "10", "200", "300"}
            var list1 = (List<int>)arrayJson2; // List<int> {1, 10, 200, 300}

            var objectJson2 = DynamicJson.Parse(@"{""foo"":""json"",""bar"":100}");
            var foobar1 = (FooBar)objectJson2; // Deserialize to FooBar

            var objectJson3 = DynamicJson.LoadFrom(new FooBar { foo = "你好", bar = 10 });
            string jsonString1 = objectJson3.ToString(); // Serialize to json string

            // with linq
            var objectJsonList = DynamicJson.Parse(@"[{""bar"":50},{""bar"":100}]");
            var barSum = ((FooBar[])objectJsonList).Select(fb => fb.bar).Sum(); // 150
            var dynamicWithLinq = ((IEnumerable<dynamic>)objectJsonList).Select(d => (int)d.bar).ToList();


            var dynamicXml = DynamicXml.Parse("<FooBar name=\"foobar1\" ><foo>xml</foo><bar size=\"456\">123<aaa>xml2</aaa></bar></FooBar>");
            dynamicXml.foo[0] = 1;
            var b1 = dynamicXml.bar[0].aaa;

            string x1 = dynamicXml.foo; // element "xml" - dynamic(string)
            var x2 = dynamicXml.bar; // element 123 - dynamic(int)
            string x4 = dynamicXml["name"]; // attribute "foobar1"
            string x5 = dynamicXml[0, 0]; // first element "xml"
            int x6 = dynamicXml[0, 1]["size"]; // second element's attribute 456

            var x7 = dynamicXml.HasElement("foo");// element true
            var x8 = dynamicXml.HasElement("foooo"); // element false
            var x9 = dynamicXml.HasAttribute("name");// attribute true
            var x10 = dynamicXml.HasAttribute("size"); // attribute false
            var x11 = dynamicXml.bar.HasAttribute("size"); // attribute true

            dynamicXml.Date = DateTime.Now; // add new element
            dynamicXml["age"] = 10; // add new attribute
            dynamicXml[9, 2] = "hello"; // add new element, element name is value's Type
            var atts = dynamicXml.Attributes(); // get all attributes
            var elms = dynamicXml.Elements(); // get all elements
            //dynamicXml.RemoveElement("bar"); //remove element
            //dynamicXml.RemoveElement(0); //remove first element
            //dynamicXml.RemoveAttribute("name"); //remove attribute
            //dynamicXml.RemoveAttribute(0); //remove first attribute

            dynamicXml.Date = 2001; // update element with another type

            var dynamicXml1 = DynamicXml.Parse("<FooBar name=\"foobar1\" ><foo>xml</foo><bar size=\"456\">123</bar></FooBar>");
            foreach (KeyValuePair<string, dynamic> item in dynamicXml1)
            {
                Console.WriteLine(item.Key + ":" + (string)item.Value); //name:foobar1, foo:xml, bar:123
            }

            var dynamicXml2 = DynamicXml.Parse("<FooBar><foo>xml</foo><bar>123</bar></FooBar>");
            var foobar2 = (FooBar)dynamicXml2;

            var dynamicXml4 = DynamicXml.LoadFrom(new List<FooBar> { new FooBar { foo = "foo1", bar = 10 }, new FooBar { foo = "foo2", bar = 20 } });
            var barSum1 = ((FooBar[])dynamicXml4).Select(fb => fb.bar).Sum(); //30
            var fooList = ((IEnumerable<dynamic>)dynamicXml4).Select(p => (string)p.foo).ToList();


            Console.WriteLine("done");

        }

        private static void TestDevLibTimer()
        {
            var idleTimer = new IdleTimer(3000, true, false);
            idleTimer.IdleOccurred += new EventHandler(idleTimer_IdleOccurred);
        }

        static void idleTimer_IdleOccurred(object sender, EventArgs e)
        {
            Console.WriteLine("idle");
        }

        private static void TestDevLibLogging()
        {
            var logger = LogManager.Open("d:\\Work\\temp\\1.log");

            Benchmark.Run(index =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    logger.Log(i, "The quick brown fox jumps over the lazy dog.");
                }
            });

            //Logger logger = LogManager.Open();
            //logger.Log("This is a log message.");

            //Random random = new Random();

            //LogManager.Open(@"e:\1\test.log", new LoggerSetup { RollingFileCountLimit = 5, RollingFileSizeMBLimit = 1 });
            //int i = 0;
            //while (true)
            //{
            //    i++;
            //    LogManager.Open(@"e:\1\test.log").Log(LogLevel.DBUG, i, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow);
            //    LogManager.Open(@"e:\1\test.log").Log(LogLevel.DBUG, i, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow);
            //    LogManager.Open(@"e:\1\test.log").Log(LogLevel.DBUG, i, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow);
            //    LogManager.Open(@"e:\1\test.log").Log(LogLevel.DBUG, i, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow);
            //    LogManager.Open(@"e:\1\test.log").Log(LogLevel.DBUG, i, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow, DateTime.Now, DateTime.UtcNow);

            //    //Thread.Sleep(random.Next(5, 100));
            //}

            //new LogConfig().WriteXml("a.xml",true);

            //LogManager.OpenConfig("a.xml");

            //var a = LogManager.DefaultLogFile;

            //for (int i = 0; i < 10; i++)
            //{
            //    new Thread(new ThreadStart(() => { LogManager.Open(@"C:\\AAA.log").Log(); })).Start();
            //}

            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.DBUG, Thread.CurrentThread.ManagedThreadId);
            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.INFO, Thread.CurrentThread.ManagedThreadId);
            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.EXCP, Thread.CurrentThread.ManagedThreadId);
            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.WARN, Thread.CurrentThread.ManagedThreadId);
            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.ERRO, Thread.CurrentThread.ManagedThreadId);
            //LogManager.Open(@"C:\\AAA.log").Log(LogLevel.FAIL, Thread.CurrentThread.ManagedThreadId);

            //Console.ReadLine();

            //var a = new Uri(Path.GetFullPath(@"C:\a b\b c\c d e\1 3 4.5")).AbsolutePath;

            //LogManager.Open(@"c:\a\b\c\d.log").Log(LogLevel.DBUG, "hello", new Exception());



            //LogManager.DefaultLoggerSetup.RollingByDate = true;

            //while (true)
            //{
            //    LogManager.Open(@"C:\\AAA.log").Log(LogLevel.DBUG, Process.GetCurrentProcess().Id);

            //    Thread.Sleep(random.Next(5, 100));
            //}

            //for (int i = 0; i < 100; i++)
            //{
            //    LogManager.Open(@"C:\\AAA.log").Log(LogLevel.DBUG, i.ToString(), DateTime.Now, Environment.UserName);
            //}

            //Task.Factory.StartNew(() => 
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        LogManager.Open().Log(LogLevel.DBUG, i.ToString());
            //    }


            //    Thread.Sleep(10000);

            //    LogManager.Open().Log(LogLevel.DBUG, "b");
            //});
        }

        private static void TestDevLibIoc()
        {
            PrintMethodName("Test DevLib.Ioc");

            IocContainer container = new IocContainer(true);

            container.Register<Person>(new Person("a", "b", 1));
            Console.WriteLine(container.Resolve<Person>());


        }

        private static void TestDevLibDaemonProcess()
        {
            PrintMethodName("Test DevLib.DaemonProcess");

            //var a = DaemonProcess.NativeAPI.NativeMethodsHelper.GetCommandLine(Process.GetCurrentProcess().Id);


            //DaemonProcessManager.StartProtect(Guid.Parse("8C0CD469-3C7B-4F7A-80D1-2987456877AA"), 1, 1, ProcessMode.Service, "AMD External Events Utility");

            //DaemonProcessManager.StartSelfProtect(Guid.Parse("8C0CD469-3C7B-4F7A-80D1-2987456877AA"), ProcessMode.Process, 0, "a", "b c ", "d", "e", " f g ");

            //DaemonProcessHelper.GetCommandLineArguments("\"C:\\addin_DataReceived.exe\" a b c d e \"f g\"");
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

            TestClass a = Singleton<TestClass>.Instance;

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
            var tempp = LogConfigManager.GetFileFullPath(@"$TMP\a\Logs\b.log");
            var tempp1 = LogConfigManager.GetFileFullPath(@"$TMP$\a\Logs\b.log");
            var tempp2 = LogConfigManager.GetFileFullPath(@"%TMP\a\Logs\b.log");
            var tempp3 = LogConfigManager.GetFileFullPath(@"%TMP%\a\Logs\b.log");

            string evnv = Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Process);
            string evnv1 = Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.User);
            string evnv2 = Environment.GetEnvironmentVariable("windir", EnvironmentVariableTarget.Machine);

            var en = @"%tmp%\a\b\c".Split(Path.DirectorySeparatorChar);

            for (int i = 0; i < en.Length; i++)
            {
                if (en[i].StartsWith("%"))
                {
                    en[i] = Environment.GetEnvironmentVariable(en[i].Trim('%'));
                }
            }

            string dts = DateTime.Now.ToString((string)null);

            string path = "LDAP://contoso.local";

            LdapEntry ldapEntry = new LdapEntry(path, "aaa", "bbb!");

            //var ldapuser = ldapEntry.Authenticate("aaa","ccc");

            var ldapusers = ldapEntry.GetUser("aaa");


            var result = ldapEntry.Authenticate("ccc", "ddd");

            ////Uri baseAddressUri = new Uri("http://localhost:888/opc");

            ////string rp = Path.Combine(baseAddressUri.AbsolutePath, "def").Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            ////var path = new Uri(baseAddressUri, rp);

            ////Uri serviceContractUri = new Uri(path);

            //WebServiceClientProxyFactory wsf = new WebServiceClientProxyFactory("http://wsf.cdyne.com/WeatherWS/Weather.asmx");
            ////var wsc = wsf.GetProxy();
            ////var mds = wsc.Methods;
            ////var rv = wsc.CallMethod("GetCityForecastByZIP", "33133");

            ////WebServiceClientProxy.CompileAssembly("d:\\ws.xml", "d:\\ws1.dll");

            ////WebServiceClientProxy proxy = new WebServiceClientProxy("http://localhost:888/opc");

            ////var w1 = WcfClientProxy<IWcfTest>.GetClientBaseInstance("http://localhost:888/test");
            ////var w2 = WcfClientProxy<IWcfTest>.GetPerSessionUnthrowableInstance("http://localhost:888/test");



            //DynamicClientProxyFactory cf = new DynamicClientProxyFactory("http://wsf.cdyne.com/WeatherWS/Weather.asmx", "d:\\1\\1.dll");

            //var cf1 = DynamicClientProxyFactory.Load("d:\\1\\1.dll");


            //var epr = cf.Endpoints;

            ////var dp = cf.Foo();

            //using (var dp = cf1.GetClientBaseProxy("http://wsf.cdyne.com/WeatherWS/Weather.asmx"))
            //{
            //    //DynamicClientObject obj = new DynamicClientObject(cf.Types.First(i => i.Name == "AgentInfoDTO"));
            //    //obj.CallConstructor();
            //    //obj.SetProperty("Alias", "a1");
            //    //obj.SetProperty("UID", "UID1");
            //    //obj.SetProperty("HostName", "h1");
            //    //obj.SetProperty("SessionIP", "127.0.0.1");
            //    //obj.SetProperty("Port", 888);


            //    var retval = dp.CallMethod("GetCityForecastByZIP", "33133");
            //}

            //RemotingObject<FooBar>.Register("a");
            //RemotingObject<Person>.Register("a");


            RemotingObject<FooBar>.Register();
            FooBar fb1 = RemotingObject<FooBar>.GetObject();
            fb1.foo = "fb1";
            fb1.bar = 111;

            RemotingObject<FooBar>.Register("fb2");
            FooBar fb2 = RemotingObject<FooBar>.GetObject("fb2");
            fb2.foo = "fb2";
            fb2.bar = 222;

            FooBar fb3 = RemotingObject<FooBar>.GetObject();
            string foo3 = fb3.foo; // fb1
            int bar3 = fb3.bar; // 111

            FooBar fb4 = RemotingObject<FooBar>.GetObject("fb2");
            string foo4 = fb4.foo; //fb2
            int bar4 = fb4.bar; // 222

            RemotingObject<FooBar>.Register();
            FooBar fb5 = RemotingObject<FooBar>.GetObject();
            string foo5 = fb5.foo; // fb1
            int bar5 = fb5.bar; //111

            FooBar fb = ArgumentParser.ParseTo<FooBar>(" /bar:123 --ok -foo=aaa");


            string expression1 = PropertyEvaluator.ExtractPropertyName<Company>(p => p.Bosses[3].Home.Street); // will return Bosses[3].Home.Street

            string expression2 = PropertyEvaluator.ExtractPropertyName<Company, int>(p => p.Bosses[3].Home.PostCode); //will return Bosses[3].Home.PostCode

            var aaa = "abcdeABCDe".ReplaceAny('W', false, 'a', 'E');

            Benchmark.Run(i =>
            {
                var csv = new CsvDocument();
                csv.Load(@"d:\work\temp\2.csv", false);
            });

            Benchmark.Run(i =>
            {
                var csv = new CsvDocument();
                csv.Load(@"d:\work\temp\2.csv", false);
            });

            Benchmark.Run(i =>
            {
                var csv = new CsvDocument();
                csv.Load(@"d:\work\temp\2.csv", false);
            });

            //Keyboard.Press(Key.Ctrl);
            //Keyboard.Press(Key.Alt);
            //Keyboard.Type(Key.Delete);
            //Keyboard.Release(Key.Alt);
            //Keyboard.Release(Key.Ctrl);

            //MemorySnapshot s1 = MemorySnapshot.FromProcess(Process.GetCurrentProcess().Id);
            //s1.ToFile(@"s1.xml");

            ////Thread.Sleep(3000);

            //MemorySnapshot s2 = MemorySnapshot.FromProcess(Process.GetCurrentProcess().Id);
            //s1.ToFile(@"s2.xml");

            //s2.CompareTo(s1).ToFile("sDiff.xml");

            //Console.WriteLine("done");
            Console.ReadLine();

            //Keyboard.Type(Key.LWin);
            //Keyboard.Type("notepad");
            //Keyboard.Type(Key.Enter);
            //Keyboard.Press(Key.LeftShift);
            //Keyboard.Type("h");
            //Keyboard.Release(Key.LeftShift);
            //Keyboard.Type("ello DevLib.Diagnostics.Input");
            //Keyboard.Type(Key.Enter);
            //Keyboard.Type("Hello DevLib.Diagnostics.Input", 10);
            //Keyboard.Type(Key.Enter);
            //Keyboard.Type("Bye bye.");

            Console.ReadLine();

            Person pa = new Person();
            pa.Error += (s, ev) =>
            {
                var eee = ev;
            };
            pa.DoTryf();

            Console.ReadLine();

            SpellingOptions sa = new SpellingOptions();
            SpellingOptions sb = new SpellingOptions();
            SpellingOptions sc = new SpellingOptions();

            sa.SpellCheckCAPS = false;
            sa.SpellCheckWhileTyping = true;
            sa.SuggestCorrections = false;

            sb.SpellCheckCAPS = false;
            sb.SpellCheckWhileTyping = true;
            sb.SuggestCorrections = false;

            sc.SpellCheckCAPS = false;
            sc.SpellCheckWhileTyping = false;
            sc.SuggestCorrections = true;

            sa.GetHashCode().ConsoleOutput();
            sb.GetHashCode().ConsoleOutput();
            sc.GetHashCode().ConsoleOutput();

            Console.ReadLine();

            InternalLogger.Log(1);
            InternalLogger.Log(2);
            InternalLogger.Log(3);
            InternalLogger.Log(4);

            Console.ReadLine();

            PrintMethodName("Test CodeSnippets");
            RetryAction.Execute(i =>
            {
                throw new Exception();
            }, null, 9, 250);

            Console.ReadLine();

            for (int i = 0; i < 100000; i++)
            {
                LogManager.Open("AAA.log", "LoggingConfig.xml").Log(LogLevel.ERRO, i, DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            }

            Console.ReadLine();

            object aPerson = new Person();
            aPerson.InvokeMethod("ShowName");
            aPerson.InvokeMethodGeneric("ShowName", null, typeof(string));


            DateTime a = new DateTime();
            a.IsWeekend().ConsoleOutput();

            var b1 = new Dictionary<int, string>(1000000);
            ReaderWriterLockSlim rwl1 = new ReaderWriterLockSlim();
            Benchmark.Initialize();
            Benchmark.Run(delegate
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

            Benchmark.Run(delegate
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
            Benchmark.Initialize();
            Benchmark.Run(delegate
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

            Benchmark.Run(delegate
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
            Benchmark.Initialize();
            Benchmark.Run(delegate
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

            Benchmark.Run(delegate
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
            Benchmark.Initialize();
            Benchmark.Run(delegate
            {
                Parallel.For(0, 1000000, (loop) =>
                {
                    int i = loop;
                    c.Add(i, DateTime.Now.ToString());
                });
            }, 1, "DictW");

            Benchmark.Run(delegate
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
            Benchmark.Initialize();
            Benchmark.Run(delegate
            {
                Parallel.For(0, 1000000, (loop) =>
                {
                    int i = loop;
                    e.TryAdd(i, DateTime.Now.ToString());
                });
            }, 1, "ConcurrentDictionaryW");

            Benchmark.Run(delegate
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
            //aclass.ToXml().ToByteArray(Encoding.UTF8).Compress().Decompress().ToEncodingString(Encoding.UTF8).FromXml<TestEventClass>().MyName.ConsoleOutput();

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

            Benchmark.Initialize();

            int times = 1000 * 10;

            TestClass testClass = new TestClass { Name = "Bill" };
            object[] parameters = new object[] { 1, 2 };
            MethodInfo methodInfo = typeof(TestClass).GetMethod("TestAdd");

            Benchmark.Run(delegate { }, 1, string.Empty, delegate { });

            Benchmark.Run(delegate { testClass.TestAdd(1, 2); }, times);

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

            var aa = "abcd".Remove(null, true);

            #region SerializationExtensions

            Person person = new Person("foo", "好的", 1);
            person.WriteXml("1.xml", true);
            person.SerializeXml().DeserializeXml(new Type[] { typeof(Person) });
            Console.ReadLine();
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
            string b = a.RemoveAny(false, c);
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
            inputString.ToMD5String().ConsoleOutput().MD5VerifyToOriginal(inputString).ConsoleOutput();

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

            AsyncSocketUdpServer s1 = new AsyncSocketUdpServer(999);
            AsyncSocketUdpServer s2 = new AsyncSocketUdpServer(999);
            AsyncSocketUdpServer s3 = new AsyncSocketUdpServer(999);

            s1.DataReceived += (s, e) => { Console.WriteLine("s1 " + e.DataTransferred.ToEncodingString()); };
            s2.DataReceived += (s, e) => { Console.WriteLine("s2 " + e.DataTransferred.ToEncodingString()); };
            s3.DataReceived += (s, e) => { Console.WriteLine("s3 " + e.DataTransferred.ToEncodingString()); };

            s1.Start(true);
            s2.Start(true);
            s3.Start(true);



            AsyncSocketUdpClient c1 = new AsyncSocketUdpClient("127.0.0.1", 999);
            c1.Start();
            for (int i = 0; i < 9; i++)
            {
                c1.Send(i.ToString().ToByteArray());
                Thread.Sleep(100);
            }
            s1.Stop();
            c1.Stop();
            c1.Start();
            for (int i = 9; i < 19; i++)
            {
                c1.Send(i.ToString().ToByteArray());
                Thread.Sleep(100);
            }
            s2.Stop();
            for (int i = 99; i < 109; i++)
            {
                c1.Send(i.ToString().ToByteArray());
                Thread.Sleep(100);
            }
            Console.ReadLine();


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
            //client.Send("hello5  你好 end", Encoding.UTF8);

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
            Application.Run(new DemoForm());
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

            //new WcfServiceHost(typeof(WcfTest), "DevLib.Samples.exe.config", "http://127.0.0.1:6000/WcfTest", true);

            var testsrv = new WcfServiceHost(typeof(WcfTest), new BasicHttpBinding(), "http://127.0.0.1:6000/WcfTest", false);
            //testsrv.Receiving += new EventHandler<WcfServiceHostEventArgs>(calcsvr_Receiving);
            //testsrv.Replying += new EventHandler<WcfServiceHostEventArgs>(calcsvr_Replying);
            testsrv.SetDataContractResolverAction = i => i.AddGenericDataContractResolver();
            //Console.ReadLine();
            testsrv.Open();
            //new WcfServiceHost(typeof(WcfTest), typeof(BasicHttpBinding), "http://127.0.0.1:6000/WcfTest", true);

            var client = WcfClientChannelFactory<IWcfTest>.CreateChannel(typeof(BasicHttpBinding), "http://127.0.0.1:6000/WcfTest", false);

            WcfClientType.SaveGeneratedAssemblyFile = true;

            var client1 = WcfClientProxy<IWcfTest>.GetPerCallUnthrowableInstance(typeof(BasicHttpBinding), "http://127.0.0.1:6000/WcfTest");

            client1.SetDataContractResolver(i => i.AddGenericDataContractResolver());

            //client2.SetClientCredentialsAction = (c) => { c.UserName.UserName = "a"; c.UserName.Password = "b"; };

            client1.AddAnimal(new Dog());

            client1.Foo("");

            //clinet2.ClientCredentials
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

        static void calcsvr_Replying(object sender, WcfServiceHostEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        static void calcsvr_Receiving(object sender, WcfServiceHostEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void TestDevLibConfiguration()
        {
            PrintMethodName("Test DevLib.Settings");


            IniEntry ini = IniManager.Open();

            ini["section1", "hello"] = 123;
            ini["section1", "hello1"] = 123;
            ini["section1", "hello2"] = 123;

            ini["section2", "hello3"] = 123;
            ini["section2", "hello4"] = 123;

            ini["section3", "hello5"] = 123;
            ini["section3", "hello6"] = 123;
            ini.Save();



            var form = new WinFormConfigEditor();
            form.AddPlugin((filename) => { return ConfigManager.Open(filename).GetValue<TestConfig>("keyA"); }, (filename, obj) => { ConfigManager.Open(filename).SetValue("keyA", obj); ConfigManager.Open(filename).Save(); });
            form.AddPlugin((filename) => { return ConfigManager.Open(filename).GetValue<TestConfig>("keyA"); }, (filename, obj) => { ConfigManager.Open(filename).SetValue("keyA", obj); ConfigManager.Open(filename).Save(); }, "haha");

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
            config.SetValue("hello2", Guid.NewGuid());
            config["g1"] = Guid.NewGuid();
            config.Save();
            config.Values.ForEach(p => p.ToString().ConsoleOutput());



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
            setting1["key4"] = Guid.NewGuid();
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
        public event EventHandler<ErrorEventArgs> Error;

        public string DoTryf()
        {
            Error.RaiseEvent(this);

            try
            {
                try
                {
                    return "done";
                }
                finally
                {
                    Console.WriteLine("inside finally");
                }
            }
            finally
            {
                Console.WriteLine("outside finally");
            }
        }


        [DataMember]
        public SpellingOptions Foo { get; set; }

        public Person()
        {

        }

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public int ID { get; set; }

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

        public override string ToString()
        {
            return string.Format("FirstName= {0} LastName= {1} Id= {2}", this.FirstName, this.LastName, this.ID);
        }

    }

    public class FooBar : MarshalByRefObject
    {
        [Parameter(Required = true)]
        public string foo { get; set; }

        [Parameter("b", "bar", "barr", Required = false, DefaultValue = 123)]
        public int bar { get; set; }

        [Parameter("o", "ok", "okk", DefaultValue = true, Required = true)]
        public bool IsOk { get; set; }

        public bool IsOk2 { get; set; }
    }

    public class Company
    {
        public Boss[] Bosses { get; set; }
    }

    public class Boss
    {
        public string Name { get; set; }
        public Address Home { get; set; }
    }

    public class Address
    {
        public int PostCode { get; set; }
        public string Street { get; set; }
    }

    [TypeConverterAttribute(typeof(ExpandableObjectConverter<SpellingOptions>))]
    [DataContract]
    [Serializable]
    public class SpellingOptions
    {
        public SpellingOptions()
        {
            this.SpellCheckCAPS = false;
            this.SpellCheckWhileTyping = true;
            this.SuggestCorrections = true;
        }

        [DataMember]
        public bool SpellCheckWhileTyping
        { get; set; }

        [DataMember]
        public bool SpellCheckCAPS
        { get; set; }

        [DataMember]
        public bool SuggestCorrections
        { get; set; }

        public override int GetHashCode()
        {
            return string.Format("{0}{1}{2}", this.SpellCheckCAPS, this.SpellCheckWhileTyping, this.SuggestCorrections).GetHashCode();
        }


    }

    public class MyDatabaseQueue<T> : IProducerConsumerQueue<T>
    {
        public MyDatabaseQueue()
        {
            // open connection to database.
        }

        public void Enqueue(T item)
        {
            // insert item to databse.
        }

        public long Enqueue(IEnumerable<T> items)
        {
            // insert items to databse, and return the number of items be inserted.
            return items.LongCount();
        }

        public T Dequeue()
        {
            // query one item and remove from database.
            throw new NotImplementedException();
        }

        public T Peek()
        {
            // query one item and keep it from database.
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            // query item is in database or not.
            throw new NotImplementedException();
        }

        public long Count()
        {
            // query total rows in database.
            throw new NotImplementedException();
        }

        public void Enqueue(object item)
        {
            // insert item to databse.
        }

        public long Enqueue(IEnumerable items)
        {
            // insert items to databse, and return the number of items be inserted.
            throw new NotImplementedException();
        }

        object IProducerConsumerQueue.Dequeue()
        {
            // query one item and remove from database.
            throw new NotImplementedException();
        }

        object IProducerConsumerQueue.Peek()
        {
            // query one item and keep it from database.
            throw new NotImplementedException();
        }

        public bool Contains(object item)
        {
            // query item is in database or not.
            throw new NotImplementedException();
        }

        public void Clear()
        {
            // remove all rows in database.
            throw new NotImplementedException();
        }
    }


}
