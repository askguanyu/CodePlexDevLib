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
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using DevLib.Diagnostics;
    using DevLib.ExtensionMethods;
    using DevLib.Net;
    using DevLib.Net.AsyncSocket;
    using DevLib.WinForms;

    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //TestCodeSnippet();

            new Action(() => TestDevLibDiagnostics()).CodeTime(1);

            //TestDevLibExtensionMethods();

            //TestDevLibNet();

            //new ThreadStart(() => { TestDevLibWinForms(); }).BeginInvoke((asyncResult) => { Console.WriteLine("WinForm exit..."); }, null);



            // Exit infomation
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void TestCodeSnippet()
        {
            Dns.GetHostAddresses("localhost").ForEach(p => p.ConsoleWriteLine());
            NetworkInterface.GetAllNetworkInterfaces().ForEach(p => p.Id.ConsoleWriteLine());
        }

        private static void TestDevLibDiagnostics()
        {
            ConcurrentDictionary<int, string> safeDict = new ConcurrentDictionary<int, string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<int> list = new List<int>();
            ConcurrentBag<string> safeBag = new ConcurrentBag<string>();

            CodeTimer.Initialize();

            int times = 1000 * 200;

            CodeTimer.Time(1, "No action", () => { });

            new Action(() => { }).CodeTime(times);

            CodeTimer.Time(times, "ConcurrentDictionary1", () =>
            {
                safeDict.AddOrUpdate(1, "hello", (key, oldValue) => oldValue);
            }, null);

            CodeTimer.Time(times, "Dictionary1", () =>
            {
                dict.Update(1, "hello");
            });

            CodeTimer.Time(times, "ConcurrentDictionary2", () =>
            {
                safeDict.AddOrUpdate(2, "hello", (key, oldValue) => oldValue);
            });

            CodeTimer.Time(times, "Dictionary2", () =>
            {
                dict.Update(2, "hello");
            });

            CodeTimer.Time(times, "ConcurrentBag1", () =>
            {
                safeBag.Add("hello");
            });
        }

        private static void TestDevLibExtensionMethods()
        {
            #region Array

            #endregion

            #region Byte

            #endregion

            #region Dictionary

            #endregion

            #region EventHandler
            //TestEventClass testEventClassObject = new TestEventClass() { MyName = "testEventClassObject" };
            //testEventClassObject.OnTestMe += new EventHandler<EventArgs>(testEventClassObject_OnTestMe);
            //testEventClassObject.TestMe();
            #endregion

            #region IO
            //"hello".CreateTextFile(@".\out\hello.txt").ConsoleWriteLine().ReadTextFile().ConsoleWriteLine();
            //DateTime.Now.CreateBinaryFile(@".\out\list.bin").ConsoleWriteLine().ReadTextFile().ConsoleWriteLine();
            //@".\out\list.bin".ReadBinaryFile<DateTime>().ConsoleWriteLine();
            #endregion

            #region Object

            #endregion

            #region String

            #endregion

        }

        private static void TestDevLibNet()
        {
            AsyncSocketServer svr = new AsyncSocketServer(9999);
            svr.DataReceived += new EventHandler<AsyncSocketUserTokenEventArgs>(svr_DataReceived);
            svr.Start();

            AsyncSocketClient client = new AsyncSocketClient();
            client.DataSent += new EventHandler<AsyncSocketUserTokenEventArgs>(client_DataSent);
            client.Connect("127.0.0.1", 9999);
            client.Send("hello1  你好 end", Encoding.UTF8);
            client.Send("hello2  你好 end", Encoding.UTF32);
            client.Send("hello3  你好 end", Encoding.BigEndianUnicode);
            client.Send("hello4  你好 end", Encoding.ASCII);
            client.Send("hello5  你好 end", Encoding.Unicode);
        }

        private static void TestDevLibWinForms()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WinFormRibbon());
        }

        static void client_DataSent(object sender, AsyncSocketUserTokenEventArgs e)
        {
            e.TransferredRawData.ToHexString().ConsoleWriteLine();
            Console.WriteLine();
        }

        static void svr_DataReceived(object sender, AsyncSocketUserTokenEventArgs e)
        {
            e.TransferredRawData.ToEncodingString(Encoding.Unicode).ConsoleWriteLine();
            Console.WriteLine();
        }

        static void testEventClassObject_OnTestMe(object sender, EventArgs e)
        {
            (sender as TestEventClass).MyName.ConsoleWriteLine();
        }
    }

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
