using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Display;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinUIEx;

namespace RichNote
{
    public sealed partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            this.Title = String.Empty;
            this.SetWindowStyle(WindowStyle.Border);
            this.SetWindowSize(500, 250);
            this.CenterOnScreen();
            this.SetIsShownInSwitchers(false);
            this.SetIsAlwaysOnTop(true);
            this.SetIsMaximizable(false);
            this.SetIsMinimizable(false);
            this.SetIsResizable(false);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}
