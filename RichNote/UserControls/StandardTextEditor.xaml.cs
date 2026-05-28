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

namespace RichNote.UserControls;

public sealed partial class StandardTextEditor : UserControl, IEditorControl
{
    // Initialization
    private TextBox editor;
    private double zoomFactor = 1.0;

    public StandardTextEditor()
    {
        InitializeComponent();
        editor = MyEditorTextBox;
    }

    public CommandBar EditorCommandBar => MyEditorCommandBar;
    public RichEditBox? EditorRichEditBox => null;
    public TextBox? EditorTextBox => MyEditorTextBox;

    // Event handlers
    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor < 5)
        {
            zoomFactor += 0.25;
        editor.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor > 1)
        {
            zoomFactor -= 0.25;
            editor.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };
        }
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        editor.Undo();
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        editor.Redo();
    }
}
