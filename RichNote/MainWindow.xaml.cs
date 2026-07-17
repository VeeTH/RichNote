using IniParser.Model;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using RichNote.Types;
using RichNote.UserControls;
using static RichNote.Types.TabStateModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using WinUIEx;

namespace RichNote
{
    public sealed partial class MainWindow : Window
    {
        // Initialization
        public static MainWindow Instance { get; private set; }
        public IEditorControl currentEditor;
        private ObservableCollection<TabViewItem> tabItems = new ObservableCollection<TabViewItem>();
        
        public string LocalAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RichNote");
        
        private IniData _settings;
        public IniData Settings
        {
            get
            {
                _settings = _settings == null ? SettingsPage.Instance.parsedSettings : _settings;
                return _settings;
            }
        }

        bool loadedPreviousTabs = false;
        bool removedBlankDoc = false;
        bool dummyTabAdded = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            ExtendsContentIntoTitleBar = true;          
            SetTitleBar(AppTitleBar);
            Instance = this;

            if (!Directory.Exists(LocalAppData))
            {
                Directory.CreateDirectory(LocalAppData);
            }
            SettingsPage.CreateDefaultSettings(true);

            StandardNewDoc(Settings["Document"]["DefaultEditor"] == "txt" ? 1 : 2, "New Document");
            DocTabView.TabItemsSource = tabItems;

            Closed += MainWindow_Closed;            
        }

        // Event handlers
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            var tabDataList = new List<TabData>();

            foreach (TabViewItem item in tabItems)
            {
                if (item.Content is IEditorControl editorControl)
                {
                    string type = null;
                    string content = null;

                    if (editorControl.EditorTextBox != null)
                    {
                        type = "Standard";
                        content = editorControl.EditorTextBox.Text;
                    }
                    else if (editorControl.EditorRichEditBox != null)
                    {
                        type = "Rich";
                        editorControl.EditorRichEditBox.Document.GetText(Microsoft.UI.Text.TextGetOptions.FormatRtf, out content);
                    }

                    tabDataList.Add(new TabData
                    {
                        Path = editorControl.FilePath,
                        Header = item.Header.ToString(),
                        Type = type,
                        Content = content
                    });
                }
            }

            var tabState = new TabState { Tabs = tabDataList };
            var bsonDocument = tabState.ToBsonDocument();
            string filePath = Path.Combine(LocalAppData, "tab_state.dat");
            
            if (Settings["Document"]["AutosaveOnClose"] == "true")
            {
                File.WriteAllBytes(filePath, bsonDocument.ToBson());            
            }
        }

        private void TabView_NewDoc(TabView sender, object args)
        {
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) == true)
            {
                StandardNewDoc(Settings["Document"]["DefaultEditor"] == "txt" ? 1 : 2, "New Document");
                return;
            }

            SelectDocFormat.IsOpen = true;
        }

        private async void TabView_CloseDoc(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (sender.TabItems.Count == 1)
            {
                return;
            }

            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) == true)
            {
                tabItems.Remove(args.Tab);
                return;
            }

            ContentDialog dialog = BuildSaveDialog(RootGrid);

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var saveResult = await SaveFile();
                if (saveResult == ContentDialogResult.Primary)
                {
                    ActivityNotif.Title = $"{args.Tab.Header} saved successfully!";   
                    ActivityNotif.IsOpen = true;
                    tabItems.Remove(args.Tab);
                } else if (saveResult == ContentDialogResult.None)
                {
                    ActivityNotif.Title = $"Error saving {args.Tab.Header}!";
                    ActivityNotif.IsOpen = true;
                    User32.MessageBeep((uint)Beep.MB_ICONERROR);
                }
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
                    currentEditor.EditorStateChanged += CurrentEditor_EditorStateChanged;
                    UpdateStatusBar(currentEditor.GetCurrentState());
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

                    if (ToggleWordWrap.IsChecked == false)
                    {
                        currentEditor.EditorTextBox.TextWrapping = TextWrapping.NoWrap;
                    }
                    else
                    {
                        currentEditor.EditorTextBox.TextWrapping = TextWrapping.Wrap;
                    }
                }
                else
                {
                    ToggleWordWrap.Visibility = Visibility.Collapsed;
                }
            } else
            {
                return;
            }

            if (removedBlankDoc == false && Settings["Document"]["OpenBlankDocOnAutoload"] == "false" && Settings["Document"]["AutoloadOnOpen"] == "true")
            {               
                tabItems.RemoveAt(0);
            }
            removedBlankDoc = true;

            if (loadedPreviousTabs == false && Settings["Document"]["AutoloadOnOpen"] == "true")
            {
                loadedPreviousTabs = true;
                var filePath = Path.Combine(LocalAppData, "tab_state.dat");
                byte[] bsonData = File.ReadAllBytes(filePath);
                var tabState = BsonSerializer.Deserialize<TabState>(bsonData);
                foreach (TabData tabData in tabState.Tabs)
                {
                    string path = tabData.Path;
                    string header = tabData.Header;
                    string type = tabData.Type;
                    string content = tabData.Content;

                    switch (type)
                    {
                        case "Standard":
                            StandardNewDoc(1, header);
                            currentEditor.EditorTextBox.Text = content;
                            currentEditor.FilePath = path;
                            break;

                        case "Rich":
                            StandardNewDoc(2, header);
                            currentEditor.EditorRichEditBox.Document.SetText(Microsoft.UI.Text.TextSetOptions.FormatRtf, content);
                            currentEditor.FilePath = path;
                            break;

                        default:
                            break;
                    }
                }
                DocTabView.UpdateLayout();
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

        private void UndoRedo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && currentEditor != null)
            {
                string clickedTag = button.Tag.ToString();

                switch (clickedTag)
                {
                    case "Undo":
                        if (currentEditor.EditorTextBox != null)
                        {
                            currentEditor.EditorTextBox.Undo();
                        }
                        else
                        {
                            currentEditor.EditorRichEditBox.Document.Undo();
                        }
                        break;

                    case "Redo":
                        if (currentEditor.EditorTextBox != null)
                        {
                            currentEditor.EditorTextBox.Redo();
                        }
                        else
                        {
                            currentEditor.EditorRichEditBox.Document.Redo();
                        }
                        break;

                    default:
                        break;
                }
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
                    case "Open...":
                        OpenFile();
                        break;

                    case "Save":
                        SaveFile();
                        break;

                    case "Save As...":
                        SaveAsFile();
                        break;

                    case "Settings":
                        settingsDialog.ShowAsync();
                        break;

                    case "Quit":
                        this.Close();
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
                    case "Select All":
                        App.SelectAll_Click(null, null);
                        break;

                    case "Insert Date and Time":
                        if (currentEditor.EditorTextBox != null)
                        {
                            currentEditor.EditorTextBox.Focus(FocusState.Programmatic);
                            currentEditor.EditorTextBox.Text = new string(currentEditor.EditorTextBox.Text.Insert(currentEditor.EditorTextBox.SelectionStart, $"{DateTime.Now}"));
                            currentEditor.EditorTextBox.SelectionStart = currentEditor.EditorTextBox.Text.Length;
                        }
                        else if (currentEditor.EditorRichEditBox != null)
                        {
                            var startPos = currentEditor.EditorRichEditBox.Document.Selection.StartPosition;
                            var date = $"{DateTime.Now}";
                            currentEditor.EditorRichEditBox.Focus(FocusState.Programmatic);
                            currentEditor.EditorRichEditBox.Document.Selection.SetRange(startPos, startPos);
                            currentEditor.EditorRichEditBox.Document.Selection.SetText(Microsoft.UI.Text.TextSetOptions.None, date);
                            currentEditor.EditorRichEditBox.Document.Selection.StartPosition = startPos + date.Length;
                        }
                        break;

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

        private void SettingsPage_OkClicked(object sender, EventArgs e)
        {
            settingsDialog.Hide();
        }

        private void SettingsPage_StatusBarToggled(object sender, EventArgs e)
        {
            if (sender is SettingsPage settings)
            {
                if (settings.EnableStatusBar == true)
                {
                    MainStatusBar.Visibility = Visibility.Visible;
                } else
                {
                    MainStatusBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void File_OnDraggedIn(object sender, DragEventArgs e)
        {
            DocTabView.CanReorderTabs = false;
            //DocTabView.IsAddTabButtonVisible = false; (too glitchy)
            e.AcceptedOperation = DataPackageOperation.Copy;
            DocTabView.Background = new SolidColorBrush(Color.FromArgb(255, 55, 115, 158));
            if (dummyTabAdded == false)
            {
                dummyTabAdded = true;
                tabItems.Add(new TabViewItem{Header = " ", Background = new SolidColorBrush(Colors.LightSteelBlue), Tag = "dummy"});
            }
        }

        private void File_OnDraggedOut(object sender, DragEventArgs e)
        {
            DocTabView.CanReorderTabs = true;
            //DocTabView.IsAddTabButtonVisible = true; (too glitchy)
            DocTabView.Background = new SolidColorBrush(Colors.SteelBlue);
            tabItems.Remove(tabItems.First(item => item.Tag?.ToString() == "dummy"));
            dummyTabAdded = false;
        }

        private async void File_DraggedIn(object sender, DragEventArgs e)
        {
            DocTabView.CanReorderTabs = true;
            //DocTabView.IsAddTabButtonVisible = true; (too glitchy)
            DocTabView.Background = new SolidColorBrush(Colors.SteelBlue);
            tabItems.Remove(tabItems.First(item => item.Tag?.ToString() == "dummy"));
            dummyTabAdded = false;

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items[0] is StorageFile file)
                {
                    OpenFile(true, file);
                }
            }
        }

        private void File_TabStripDragOver(object sender, DragEventArgs e)
        {
            if (!e.DataView.Contains("TabViewItem"))
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void CurrentEditor_EditorStateChanged(object? sender, EditorStateChangedEventArgs e)
        {
            UpdateStatusBar(e);
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

        private async void OpenFile(bool skipPicker = false, StorageFile file = null)
        {
            if (skipPicker == true)
            {
                goto OpenFile;
            }
            
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".rtf");

            file = await picker.PickSingleFileAsync();

        OpenFile:
            if (file != null)
            {
                switch (file.FileType)
                {
                    case ".txt":
                        StandardNewDoc(1, file.Name);
                        var fileContent = await FileIO.ReadTextAsync(file);
                        currentEditor.EditorTextBox.Text = fileContent;
                        currentEditor.FilePath = file.Path;
                        break;

                    case ".rtf":
                        StandardNewDoc(2, file.Name);
                        var stream = await file.OpenAsync(FileAccessMode.Read);
                        currentEditor.EditorRichEditBox.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, stream);
                        currentEditor.FilePath = file.Path;
                        break;

                    default:
                        ActivityNotif.Title = "File must be a .txt or .rtf file.";
                        ActivityNotif.IsOpen = true;
                        User32.MessageBeep((uint)Beep.MB_ICONERROR);
                        break;
                }
            }
            else
            {
                return;
            }
        }

        private async Task<ContentDialogResult> SaveFile()
        {            
            if (currentEditor.FilePath != null)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(currentEditor.FilePath);
                
                if (file != null)
                {
                    switch (file.FileType)
                    {
                        case ".txt":
                            var txtSaveContent = currentEditor.EditorTextBox.Text;
                            await FileIO.WriteTextAsync(file, txtSaveContent);
                            break;

                        case ".rtf":
                            var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                            currentEditor.EditorRichEditBox.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, stream);
                            break;

                        default:
                            break;

                    }
                }

                return ContentDialogResult.Primary;
            } else
            {
                return await SaveAsFile();
            }
        }

        private async Task<ContentDialogResult> SaveAsFile()
        {            
            var saver = new Windows.Storage.Pickers.FileSavePicker();
            WinRT.Interop.InitializeWithWindow.Initialize(saver, WinRT.Interop.WindowNative.GetWindowHandle(this));            
            saver.FileTypeChoices.Clear();
            if (currentEditor.EditorTextBox != null)
            {
                saver.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            } else if (currentEditor.EditorRichEditBox != null)
            {                
                saver.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
            }

            StorageFile file = await saver.PickSaveFileAsync();

            if (file != null)
            {
                switch (file.FileType)
                {
                    case ".txt":
                        var txtSaveContent = currentEditor.EditorTextBox.Text;
                        await FileIO.WriteTextAsync(file, txtSaveContent);
                        currentEditor.FilePath = file.Path;
                        ((TabViewItem)DocTabView.SelectedItem).Header = file.Name;
                        break;

                    case ".rtf":
                        var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                        currentEditor.EditorRichEditBox.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, stream);
                        currentEditor.FilePath = file.Path;
                        ((TabViewItem)DocTabView.SelectedItem).Header = file.Name;
                        break;

                    default:                        
                        break;

                }

                return ContentDialogResult.Primary;
            }
            else
            {
                return ContentDialogResult.None;
            }
        }

        private void UpdateStatusBar(EditorStateChangedEventArgs e)
        {
            LineAndCol.Text = $"Line {e.Line}, Col {e.Column}";
            PgZoomPcnt.Text = $"Zoom Level: {e.Zoom}";
        }

        private void InitializeWindow()
        {
            AppWindow appWindow = this.AppWindow;

            this.CenterOnScreen();
            this.SetIcon(@"Assets\temp_icon.ico");
            appWindow.SetTaskbarIcon(@"Assets\temp_icon.ico");

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.PreferredMinimumWidth = 985;
                overlappedPresenter.PreferredMinimumHeight = 750;
            }
        }        
    }
}
