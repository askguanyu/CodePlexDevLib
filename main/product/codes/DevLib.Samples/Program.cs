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
    using DevLib.ExtensionMethods;
    using DevLib.Net;
    using DevLib.WinForms;

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
            //safeDict.AddOrUpdate


            Console.ReadKey();
        }
    }
}
