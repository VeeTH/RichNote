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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;

namespace RichNote
{
    public sealed partial class MainWindow : Window
    {
        // Initialization
        private ObservableCollection<TabViewItem> tabItems = new ObservableCollection<TabViewItem>();
        private IEditorControl currentEditor;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            ExtendsContentIntoTitleBar = true;          
            SetTitleBar(AppTitleBar);

            StandardNewDoc(1, "New Document");
            DocTabView.TabItemsSource = tabItems;
            Closed += MainWindow_Closed;         
        }

        // Event handlers
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // Autosave code will go here     
            return;
        }

        private void TabView_NewDoc(TabView sender, object args)
        {
            SelectDocFormat.IsOpen = true;
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
                tabItems.Remove(args.Tab);
            } else if (result == ContentDialogResult.Secondary) 
            {
                tabItems.Remove(args.Tab);
            } else
            {
                return;
            }
        }
        private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DocTabView.SelectedItem is TabViewItem selectedTabItem)
            {
                if (selectedTabItem.Content is IEditorControl editorControl)
                {
                    currentEditor = editorControl;
                }
                else
                {
                    currentEditor = null;
                }
            } else
            {
                currentEditor = null;
            }

            if (currentEditor != null)
            {
                if (currentEditor.EditorTextBox != null)
                {
                    ToggleWordWrap.Visibility = Visibility.Visible;
                }
                else
                {
                    ToggleWordWrap.Visibility = Visibility.Collapsed;
                }
            } else
            {
                return;
            }
        }

        private void SelectDocFormat_ActionButtonClick(TeachingTip sender, object args)
        {
            SelectDocFormat.IsOpen = false;
            StandardNewDoc(1, "New Document");
        }

        private void SelectDocFormat_CloseButtonClick(TeachingTip sender, object args)
        {
            StandardNewDoc(2, "New Document");
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
                    case "Open...":
                        OpenFile();
                        break;
                    
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
                    case "Word Wrap":
                        if (ToggleWordWrap.IsChecked == false)
                        {
                            currentEditor.EditorTextBox.TextWrapping = TextWrapping.NoWrap;
                        } else
                        {
                            currentEditor.EditorTextBox.TextWrapping = TextWrapping.Wrap;
                        }
                        break;
                    
                    default:
                        break;
                }
            }
        }

        // Helper methods
        private void StandardNewDoc(int format, string tabName)
        { 
            switch (format)
            {
                case 1:
                    var tabContent = new StandardTextEditor();
                    if (ToggleWordWrap.IsChecked == true)
                    {
                        tabContent.EditorTextBox.TextWrapping = TextWrapping.Wrap;
                    }
                    var tabItem = new TabViewItem();
                    tabItem.Content = tabContent;
                    tabItem.Header = tabName;
                    tabItem.IconSource = new SymbolIconSource() { Symbol = Symbol.Page2 };

                    tabItems.Add(tabItem);
                    DocTabView.SelectedItem = tabItem;

                    break;

                case 2:
                    var richTabContent = new RichTextEditor();
                    var richTabItem = new TabViewItem();
                    richTabItem.Content = richTabContent;
                    richTabItem.Header = tabName;
                    richTabItem.IconSource = new SymbolIconSource() { Symbol = Symbol.Page2 };

                    tabItems.Add(richTabItem);
                    DocTabView.SelectedItem = richTabItem;

                    break;

                default:
                    break;
            }
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

        private async void OpenFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".rtf");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                switch (file.FileType)
                {
                    case ".txt":
                        StandardNewDoc(1, file.Name);
                        var fileContent = await FileIO.ReadTextAsync(file);
                        currentEditor.EditorTextBox.Text = fileContent;
                        break;

                    case ".rtf":
                        StandardNewDoc(2, file.Name);
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        currentEditor.EditorRichEditBox.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, stream);
                        break;

                    default:
                        break;
                }
            } else
            {
                return;
            }
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
