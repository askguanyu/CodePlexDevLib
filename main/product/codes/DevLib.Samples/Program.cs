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
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Linq;
    using DevLib.Diagnostics;
    using DevLib.ExtensionMethods;
    using DevLib.Main;
    using DevLib.Net;
    using DevLib.Net.AsyncSocket;
    using DevLib.Utilities;
    using DevLib.WinForms;
    using System.Diagnostics;
    using System.Dynamic;

    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            PrintStartInfo();


            TestCodeSnippet();

            //new Action(() => TestDevLibDiagnostics()).CodeTime(1);

            //TestDevLibExtensionMethods();

            //TestDevLibNet();

            //TestDevLibUtilities();

            //new ThreadStart(() => { TestDevLibWinForms(); }).BeginInvoke((asyncResult) => { Console.WriteLine("WinForm exit..."); }, null);



            PrintExitInfo();
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

        private static void TestCodeSnippet()
        {
            PrintMethodName("TestCodeSnippet");

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

            //Trace.Listeners.Add(new ConsoleTraceListener());
            //Trace.Listeners.Add(new TextWriterTraceListener("trace.log"));
            //Trace.AutoFlush = false;
            //Trace.WriteLine("Entering Main");
            //Trace.TraceError("hello error");
            //Trace.TraceError("hello error");
            //Console.WriteLine("Hello World.");
            //Trace.WriteLine("Exiting Main");
            //Trace.Unindent();

            TraceSource ts = new TraceSource("TraceTest");
            SourceSwitch sourceSwitch = new SourceSwitch("SourceSwitch", "Verbose");
            ts.Switch = sourceSwitch;
            int idxConsole = ts.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
            ts.Listeners[idxConsole].Name = "console";

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
            ts.TraceEvent(TraceEventType.Start, 9, "Will not appear until the switch is changed.");
            ts.Switch.Level = SourceLevels.ActivityTracing | SourceLevels.Critical;
            ts.TraceEvent(TraceEventType.Suspend, 10, "Switch includes ActivityTracing, this should appear");
            ts.TraceEvent(TraceEventType.Critical, 11, "Switch includes Critical, this should appear");
            
            ts.Flush();
            ts.Close();
        }

        private static void TestDevLibDiagnostics()
        {
            PrintMethodName("Test Dev.Lib.Diagnostics");

            ConcurrentDictionary<int, string> safeDict = new ConcurrentDictionary<int, string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<int> list = new List<int>();
            ConcurrentBag<string> safeBag = new ConcurrentBag<string>();

            //CodeTimer.Initialize();

            //int times = 1000 * 200;

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

            #region Array
            TestEventClass[] sourceArray
                = new TestEventClass[]
            {
                new TestEventClass(){ MyName="a"},
                new TestEventClass(){ MyName="b"},
                new TestEventClass(){ MyName="c"},
            };

            TestEventClass[] appendArray = new TestEventClass[]
            {
                new TestEventClass(){ MyName="d"},
                new TestEventClass(){ MyName="e"},
                new TestEventClass(){ MyName="f"},
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

            sourceArray[1].MyName = "change1";
            appendArray[1].MyName = "change2";
            sourceArray.ForEach((p) => { p.MyName.ConsoleOutput(); });

            sourceValueTypeArray.AddRangeTo(ref appendValueTypeArray);
            appendValueTypeArray.ForEach((p) => { p.ConsoleOutput(); });
            "".ConsoleOutput();
            sourceValueTypeArray[1] = 3;
            appendValueTypeArray.FindArray(sourceValueTypeArray).ConsoleOutput();

            "End: ArrayExtensions".ConsoleOutput();
            #endregion

            #region Byte
            //byte[] bytes = new byte[] { 1, 2, 3, 4, 5,6,7,8,9,10 };
            ////var obj = bytes.ToObject<int>();

            //TestEventClass aobject = new TestEventClass() { MyName = "object to []" };
            //aobject.ToByteArray().ToObject<TestEventClass>().MyName.ConsoleWriteLine();
            //int i = 123;
            //i.ToByteArray().ToObject().ConsoleWriteLine();

            //"compress".ConsoleWriteLine();
            //sourceArray.ToByteArray().Length.ConsoleWriteLine();
            //sourceArray.ToByteArray().Compress().Length.ConsoleWriteLine();
            //sourceArray.ToByteArray().Compress().Decompress().Length.ConsoleWriteLine();
            #endregion

            #region Collection

            #endregion

            #region EventHandler
            //TestEventClass testEventClassObject = new TestEventClass() { MyName = "testEventClassObject" };
            //testEventClassObject.OnTestMe += new EventHandler<EventArgs>(testEventClassObject_OnTestMe);
            //testEventClassObject.TestMe();
            #endregion

            #region IO
            //"hello".CreateTextFile(@".\out\hello.txt").GetFullPath().OpenContainingFolder();
            //DateTime.Now.CreateBinaryFile(@".\out\list.bin").ConsoleWriteLine().ReadTextFile().ConsoleWriteLine();
            //@".\out\list.bin".ReadBinaryFile<DateTime>().ConsoleWriteLine();
            #endregion

            #region Object
            //sourceArray.ToJson().FromJson<TestEventClass[]>().ForEach((p) => { p.MyName.ConsoleWriteLine(); });
            //sourceArray.ToXml(Encoding.UTF8).ConsoleWriteLine();

            sourceArray.ForEach((p) => { p.CopyPropertiesFrom(appendArray[1]); });
            sourceArray.ForEach((p) => { p.MyName.ConsoleOutput(); });
            "End: Object".ConsoleOutput();
            #endregion

            #region String

            #endregion

        }

        private static void TestDevLibNet()
        {
            PrintMethodName("Test Dev.Lib.Net");

            #region AsyncSocket
            //AsyncSocketServer svr = new AsyncSocketServer(9999);
            //svr.DataReceived += new EventHandler<AsyncSocketUserTokenEventArgs>(svr_DataReceived);
            //svr.Start();

            //AsyncSocketClient client = new AsyncSocketClient();
            //client.DataSent += new EventHandler<AsyncSocketUserTokenEventArgs>(client_DataSent);
            //client.Connect("127.0.0.1", 9999);
            //client.Send("hello1  你好 end", Encoding.UTF8);
            //client.Send("hello2  你好 end", Encoding.UTF32);
            //client.Send("hello3  你好 end", Encoding.BigEndianUnicode);
            //client.Send("hello4  你好 end", Encoding.ASCII);
            //client.Send("hello5  你好 end", Encoding.Unicode);
            #endregion

            NetUtilities.GetLocalIPArray().ForEach((p) => { p.ConsoleOutput(); });
            NetUtilities.GetRandomPortNumber().ConsoleOutput();

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

        private static void PrintMethodName(string name)
        {
            ConsoleColor originalForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("{0} is running...", name);
            Console.ForegroundColor = originalForeColor;
        }

        private static void client_DataSent(object sender, AsyncSocketUserTokenEventArgs e)
        {
            e.TransferredRawData.ToHexString().ConsoleOutput();
            Console.WriteLine();
        }

        private static void svr_DataReceived(object sender, AsyncSocketUserTokenEventArgs e)
        {
            e.TransferredRawData.ToEncodingString(Encoding.Unicode).ConsoleOutput();
            Console.WriteLine();
        }

        private static void testEventClassObject_OnTestMe(object sender, EventArgs e)
        {
            (sender as TestEventClass).MyName.ConsoleOutput();
        }
    }

    [Serializable]
    public class TestEventClass
    {
        public event EventHandler<EventArgs> OnTestMe;

        public string MyName
        {
            get;
            set;
        }

        public void TestMe()
        {
            Console.WriteLine("TestEventClass.TestMe() done!");
            OnTestMe.RaiseEvent(this, new EventArgs());
        }
    }
}
