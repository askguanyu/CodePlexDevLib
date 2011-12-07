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
    using System.Windows.Forms;
    using DevLib.Diagnostics;
    using DevLib.ExtensionMethods;
    using DevLib.Net;
    using DevLib.Net.AsyncSocket;
    using DevLib.WinForms;
    using System.Net;
    using System.Text;

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region WinForm
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            #endregion


            string a = "123";
            a.ConsoleWriteLine().ConsoleWriteLine();
            Console.WriteLine(a.ConvertTo<int>());


            ConcurrentDictionary<int, string> safeDict = new ConcurrentDictionary<int, string>();
            Dictionary<int, string> dict = new Dictionary<int, string>();

            //CodeTimer.Initialize();

            int times = 100 * 1000;

            //CodeTimer.Time("ConcurrentDictionary1", times, () =>
            //{
            //    safeDict.AddOrUpdate(1, "hello", (key, oldValue) => oldValue);
            //});

            //CodeTimer.Time("Dictionary1", times, () => 
            //{ 
            //    dict.Update(1, "hello"); 
            //});

            //CodeTimer.Time("ConcurrentDictionary2", times, () =>
            //{
            //    safeDict.AddOrUpdate(2, "hello", (key, oldValue) => oldValue);
            //});

            //CodeTimer.Time("Dictionary2", times, () =>
            //{
            //    dict.Update(2, "hello");
            //});


            //CodeTimer.Restore();



            AsyncSocketServer svr = new AsyncSocketServer(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999));
            svr.DataReceived += new EventHandler<AsyncSocketUserTokenEventArgs>(svr_DataReceived);
            svr.Start();

            AsyncSocketClient client = new AsyncSocketClient();
            client.SendOnce("127.0.0.1", 9999, "hello", Encoding.UTF8);

            while (true)
            {

            }


            Console.ReadKey();
        }

        static void svr_DataReceived(object sender, AsyncSocketUserTokenEventArgs e)
        {
            e.ReceivedRawData.ToHexString().ConsoleWriteLine();
        }
    }
}
