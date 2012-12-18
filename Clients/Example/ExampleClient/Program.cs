using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IPS.Controller;
using System.Net;
using System.IO;
using IPS.SharedObjects;

namespace ExampleClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            
        }

       
    }
}
