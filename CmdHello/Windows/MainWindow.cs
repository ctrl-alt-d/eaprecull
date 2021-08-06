using Terminal.Gui;
using System;
using Mono.Terminal;

namespace CmdHello.Windows
{
    public class MainWindow: Window
    {
        public MainWindow(): base("Eap recull")
        {
			X = 0;
			Y = 1;
			Width = Dim.Fill ();
			Height = Dim.Fill () - 1;

            var top = Application.Top;
            

        }
        
    }
    
}

		