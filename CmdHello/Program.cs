using System;
using Terminal.Gui;
using System;
using Mono.Terminal;
using CmdHello.Windows;
namespace CmdHello
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Init();
            var mainWin = new MainWindow();
            var top = Application.Top;
            top.Add(mainWin);
            Application.Run();
        }
    }
}
