using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TotalWarPackReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread MainFormThread = new Thread(() => {
                Application.Run(new MainForm());
            });
            MainFormThread.SetApartmentState(ApartmentState.STA);
            MainFormThread.Start();
            Console.WriteLine("MainForm loaded.");
        }
    }
}
