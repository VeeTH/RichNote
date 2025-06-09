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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RichNote
{
    public sealed partial class MainWindow : Window
    {
        bool isModified = false; // Use "Placeholder" Symbol icon to signal unsaved docs
        int currentDocument = 0; // Placeholder, won't be an int

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            ExtendsContentIntoTitleBar = true;          
            SetTitleBar(AppTitleBar);

            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // Autosave code will go here     
            return;
        }

        private void TabView_NewDoc(TabView sender, object args)
        {
            TabViewItem item = new TabViewItem();
            item.Header = "New Document";
            item.IconSource = new SymbolIconSource() { Symbol = Symbol.Page2 };
            StandardNewDoc(item);
            
            SelectDocFormat.IsOpen = true;

            sender.TabItems.Add(item);
            sender.SelectedItem = item;
        }

        private async void TabView_CloseDoc(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (sender.TabItems.Count == 1)
            {
                return;
            }

            ContentDialog dialog = BuildSaveDialog(RootGrid);

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ActivityNotif.Title = $"{args.Tab.Header} saved successfully!";   
                ActivityNotif.IsOpen = true;
                sender.TabItems.Remove(args.Tab);
            } else if (result == ContentDialogResult.Secondary) 
            { 
                sender.TabItems.Remove(args.Tab);
            } else
            {
                return;
            }
        }

        private void MenuBarItem_AboutClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem clickedItem)
            {
                string clickedText = clickedItem.Text;

                switch (clickedText)
                {
                    case "About":
                        aboutSDKDialog.DefaultButton = ContentDialogButton.Close;
                        aboutSDKDialog.ShowAsync();
                        //Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:about"));
                        break;

                    case "About RichNote":
                        aboutDialog.DefaultButton = ContentDialogButton.Close;
                        aboutDialog.ShowAsync();
                        break;

                    default:
                        break;
                }
            }                       
        }

        private void MenuBarItem_FileClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem clickedItem)
            {
                string clickedText = clickedItem.Text;

                switch (clickedText)
                {
                    case "Quit":
                        Environment.Exit(0);
                        break;

                    default:
                        break;
                }
            }
        }

        private void MenuBarItem_EditClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem clickedItem)
            {
                string clickedText = clickedItem.Text;

                switch (clickedText)
                {
                    default:
                        break;
                }
            }
        }

        private void MenuBarItem_FormatClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem clickedItem)
            {
                string clickedText = clickedItem.Text;

                switch (clickedText)
                {
                    default:
                        break;
                }
            }
        }

        private static void RichEditBox_TextChanged(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

        // Helper methods
        private static void StandardNewDoc(TabViewItem tabView)
        {
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());

            RichEditBox editBox = new RichEditBox();
            editBox.VerticalAlignment = VerticalAlignment.Stretch;
            editBox.BorderThickness = new Thickness(0);
            editBox.TextChanged += RichEditBox_TextChanged;

            grid.Children.Add(editBox);
            tabView.Content = grid;
        }

        private static ContentDialog BuildSaveDialog(Grid RootGrid)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = RootGrid.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Save your work?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = "Any unsaved content will be lost.";
            dialog.RequestedTheme = ElementTheme.Dark;

            return dialog;
        }

        private void InitializeWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.SetIcon(@"Assets\temp_icon.ico");
            appWindow.SetTaskbarIcon(@"Assets\temp_icon.ico");
            //appWindow.Resize(new Windows.Graphics.SizeInt32(appWindow.Size.Width - 200, appWindow.Size.Height));

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.PreferredMinimumWidth = 775;
                overlappedPresenter.PreferredMinimumHeight = 750;
            }
        }
    }
}
