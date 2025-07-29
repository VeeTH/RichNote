using Microsoft.UI.Xaml.Controls;

namespace RichNote
{
    public interface IEditorControl
    {
        CommandBar EditorCommandBar { get; }
        RichEditBox? EditorRichEditBox { get; }
        TextBox? EditorTextBox { get; }
        //bool isModfified { get; set; } (// Use "Placeholder" Symbol icon to signal unsaved docs)
    }
}
