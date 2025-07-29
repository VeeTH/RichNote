using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RichNote
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        // Initialization
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        // Event handlers
        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
            
            var flyout = (MenuFlyout)Resources["EditorFlyout"];
            var cut = (MenuFlyoutItem)flyout.Items[0];
            var copy = (MenuFlyoutItem)flyout.Items[1];
            var paste = (MenuFlyoutItem)flyout.Items[2];
            var selectAll = (MenuFlyoutItem)flyout.Items[4];
            selectAll.Click += SelectAll_Click;
            cut.Click += Cut_Click;
            copy.Click += Copy_Click;
            paste.Click += Paste_Click;
        }
        
        public static void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            var editor = MainWindow.Instance.currentEditor;

            if (editor != null)
            {
                if (editor.EditorRichEditBox != null)
                {
                    var richSelection = editor.EditorRichEditBox.Document.Selection;
                    editor.EditorRichEditBox.Focus(FocusState.Programmatic);
                    richSelection.SetRange(0, richSelection.StoryLength - 1);
                } else if (editor.EditorTextBox != null)
                {
                    editor.EditorTextBox.Focus(FocusState.Programmatic);
                    editor.EditorTextBox.SelectAll();
                }
            } else
            {
                return;
            }
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            var editor = MainWindow.Instance.currentEditor;

            if (editor != null)
            {
                if (editor.EditorRichEditBox != null)
                {
                    editor.EditorRichEditBox.Document.Selection.Cut();
                }
                else if (editor.EditorTextBox != null)
                {
                    editor.EditorTextBox.CutSelectionToClipboard();
                }
            }
            else
            {
                return;
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var editor = MainWindow.Instance.currentEditor;

            if (editor != null)
            {
                if (editor.EditorRichEditBox != null)
                {
                    editor.EditorRichEditBox.Document.Selection.Copy();
                }
                else if (editor.EditorTextBox != null)
                {
                    editor.EditorTextBox.CopySelectionToClipboard();
                }
            }
            else
            {
                return;
            }
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            var editor = MainWindow.Instance.currentEditor;
            var keyCtrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            var keyV = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.V).HasFlag(CoreVirtualKeyStates.Down);

            // Prevents conflict with hardcoded TextBox and RichEditBox keybinds
            if (keyCtrl == true && keyV == true) {
                return;
            }

            if (editor != null)
            {
                if (editor.EditorRichEditBox != null)
                {
                    editor.EditorRichEditBox.Document.Selection.Paste(0);
                }
                else if (editor.EditorTextBox != null)
                {
                    editor.EditorTextBox.PasteFromClipboard();
                }
            }
            else
            {
                return;
            }
        }
    }
}
