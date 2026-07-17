using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RichNote.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;

namespace RichNote.UserControls;

public sealed partial class StandardTextEditor : UserControl, IEditorControl
{
    // Initialization
    private TextBox editor;
    private string filePath;
    private string text;
    private int line = 1;
    private int column = 1;
    private double zoomFactor = 1.0;
    public event EventHandler<EditorStateChangedEventArgs> EditorStateChanged;
    private EditorStateChangedEventArgs args;

    public StandardTextEditor()
    {
        InitializeComponent();
        editor = MyEditorTextBox;
        args = new EditorStateChangedEventArgs(line, column, zoomFactor);
    }

    public CommandBar EditorCommandBar => MyEditorCommandBar;
    public RichEditBox? EditorRichEditBox => null;
    public TextBox? EditorTextBox => MyEditorTextBox;
    public string? FilePath { get => filePath; set => filePath = value; }

    // Event handlers
    private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        text = editor.Text;

        // Calculate line number
        line = text.Substring(0, editor.SelectionStart).Split('\r').Length;
        args.Line = line;

        // Calculate column number
        int startIndex = Math.Max(0, editor.SelectionStart - 1);
        int lastNewline = text.LastIndexOf('\r', startIndex);
        if (lastNewline == -1)
        {
            column = editor.SelectionStart + 1;
            args.Column = column;
        }
        else
        {
            column = editor.SelectionStart - lastNewline;
            args.Column = column;
        }

        EditorStateChanged?.Invoke(this, args);
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor < 5)
        {
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) == true)
            {
                zoomFactor = 5;
            }
            else
            {
            zoomFactor += 0.25;
            }

        editor.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };

            args.Zoom = zoomFactor.ToString("P0");
            EditorStateChanged?.Invoke(this, args);
        } else
        {
            User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
        }
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor > 1)
        {
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) == true)
            {
                zoomFactor = 1;
            }
            else
            {
            zoomFactor -= 0.25;
            }

            editor.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };

            args.Zoom = zoomFactor.ToString("P0");
            EditorStateChanged?.Invoke(this, args);
        } else
        {
            User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
        }
    }

    // Helper methods
    public EditorStateChangedEventArgs GetCurrentState()
    {
        return args;
    }
}
