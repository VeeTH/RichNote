using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RichNote.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;

namespace RichNote.UserControls;

public sealed partial class RichTextEditor : UserControl, IEditorControl
{
    // Initialization
    private RichEditTextDocument document;
    private double zoomFactor = 1.0;

    public RichTextEditor()
    {
        InitializeComponent();
        document = MyEditorRichEditBox.Document;

        var format = document.GetDefaultCharacterFormat();
        format.Size = 12f;
        document.SetDefaultCharacterFormat(format);
    }

    public CommandBar EditorCommandBar => MyEditorCommandBar;
    public RichEditBox? EditorRichEditBox => MyEditorRichEditBox;
    public TextBox? EditorTextBox => null;

    // Event handlers
    private void Bold_Click(object sender, RoutedEventArgs e)
    {
        document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;        
    }

    private void Italics_Click(object sender, RoutedEventArgs e)
    {
        document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
    }

    private void Underline_Click(object sender, RoutedEventArgs e)
    {
        var underline = document.Selection.CharacterFormat;
        
        if (underline.Underline != UnderlineType.Single)
        {
            underline.Underline = UnderlineType.Single;
        } else
        {
            underline.Underline = UnderlineType.None;
        }
    }

    private void Strikethrough_Click(object sender, RoutedEventArgs e)
    {
        document.Selection.CharacterFormat.Strikethrough = FormatEffect.Toggle;
    }

    private void ChangeFont_Click(object sender, RoutedEventArgs e)
    {
        // Use Win2D GetSystemFontFamilies() to list all installed fonts
    }

    private void FontSizeUp_Click(object sender, RoutedEventArgs e)
    {
        int selectionStart = document.Selection.StartPosition;
        int selectionEnd = document.Selection.EndPosition;

        for (int i = selectionStart; i < selectionEnd; i++)
        {
            ITextRange character = document.GetRange(i, i + 1);
            ITextCharacterFormat charFormat = character.CharacterFormat;
            charFormat.Size += 1;
        }  
    }

    private void FontSizeDown_Click(object sender, RoutedEventArgs e)
    {
        int selectionStart = document.Selection.StartPosition;
        int selectionEnd = document.Selection.EndPosition;

        for (int i = selectionStart; i < selectionEnd; i++)
        {
            ITextRange character = document.GetRange(i, i + 1);
            ITextCharacterFormat charFormat = character.CharacterFormat;
            charFormat.Size -= 1;
        }
    }

    private void BulletList_Click(object sender, RoutedEventArgs e)
    {
        var list = document.Selection.ParagraphFormat;

        if (list.ListType != MarkerType.Bullet)
        {
            list.ListType = MarkerType.Bullet;
        } else
        {
            list.ListType = MarkerType.None;
        }
    }

    private void NumberList_Click(object sender, RoutedEventArgs e)
    {
        var list = document.Selection.ParagraphFormat;

        if (list.ListType != MarkerType.Arabic)
        {
            list.ListType = MarkerType.Arabic;
        }
        else
        {
            list.ListType = MarkerType.None;
        }
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor < 5)
        {
            zoomFactor += 0.25;
        MyEditorRichEditBox.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        if (zoomFactor > 1)
        {
            zoomFactor -= 0.25;
            MyEditorRichEditBox.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };
        }
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        document.Undo();
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        document.Redo();
    }
}