using Microsoft.UI.Xaml.Controls;
using System;

namespace RichNote.Types
{
    public interface IEditorControl
    {
        CommandBar EditorCommandBar { get; }
        RichEditBox? EditorRichEditBox { get; }
        TextBox? EditorTextBox { get; }

        event EventHandler<EditorStateChangedEventArgs> EditorStateChanged;
        EditorStateChangedEventArgs GetCurrentState();

        //bool isModfified { get; set; } (// Use "Placeholder" Symbol icon to signal unsaved docs)
    }
}
