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
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using DevLib.Diagnostics;
    using DevLib.ExtensionMethods;
    using DevLib.Net;
    using DevLib.Net.AsyncSocket;
    using DevLib.WinForms;
    using System.Net.NetworkInformation;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //new ThreadStart(() => { TestDevLibWinForms(); }).BeginInvoke((asyncResult) => { Console.WriteLine("WinForm exit..."); }, null);

            //TestDevLibWinForms();

            //TestDevLibDiagnostics();

            //TestDevLibNet();

            //TestDevLibExtensionMethods();

            //TestSnippet();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void TestSnippet()
        {
            Dns.GetHostAddresses("localhost").ForEach(p => p.ConsoleWriteLine());
            NetworkInterface.GetAllNetworkInterfaces().ForEach(p => p.Id.ConsoleWriteLine());
        }

        private static void TestDevLibWinForms()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WinFormRibbon());
        }

        private static void TestDevLibExtensionMethods()
        {
            string a = "123";
            a.ConsoleWriteLine().ConsoleWriteLine();
            Console.WriteLine(a.ConvertTo<int>());

            "123".CreateBinaryFile(@".\out\123.bin");

            "你好".CreateTextFile(@".\out\456.txt");
            @".\out\out.txt".ReadTextFile().ConsoleWriteLine();

            DateTime date = DateTime.Now;
            date.CreateBinaryFile(@".\out\date.bin");
            @".\out\out.bin".ReadBinaryFile<DateTime>().ConsoleWriteLine();

        }

        private static void TestDevLibDiagnostics()
        {
            ConcurrentDictionary<int, string> safeDict = new ConcurrentDictionary<int, string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            List<int> list = new List<int>();
            ConcurrentBag<string> safeBag = new ConcurrentBag<string>();

            CodeTimer.Initialize();

            int times = 1000 * 500;

            CodeTimer.Time("No action", times, () => { });

            CodeTimer.Time("ConcurrentDictionary1", times, () =>
            {
                safeDict.AddOrUpdate(1, "hello", (key, oldValue) => oldValue);
            });

            CodeTimer.Time("Dictionary1", times, () =>
            {
                dict.Update(1, "hello");
            });

            CodeTimer.Time("ConcurrentDictionary2", times, () =>
            {
                safeDict.AddOrUpdate(2, "hello", (key, oldValue) => oldValue);
            });

            CodeTimer.Time("Dictionary2", times, () =>
            {
                dict.Update(2, "hello");
            });

            CodeTimer.Time("ConcurrentBag1", times, () =>
            {
                safeBag.Add("hello");
            });

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
    }
}
