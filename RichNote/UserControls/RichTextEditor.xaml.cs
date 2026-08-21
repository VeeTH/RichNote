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
using System.Collections.ObjectModel;
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
    private string filePath;
    private string text;
    private int line = 1;
    private int column = 1;
    private double zoomFactor = 1.0;
    private int[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
    private string[] fontFamilies = MainWindow.Instance.systemFonts;
    private ObservableCollection<string> recentFontFamilies = MainWindow.Instance.recentFonts;
    public event EventHandler<EditorStateChangedEventArgs> EditorStateChanged;
    private EditorStateChangedEventArgs args;

    public RichTextEditor()
    {
        InitializeComponent();
        document = MyEditorRichEditBox.Document;
        args = new EditorStateChangedEventArgs(line, column, zoomFactor);

        var format = document.GetDefaultCharacterFormat();
        format.Size = 12f;
        document.SetDefaultCharacterFormat(format);
    }

    public CommandBar EditorCommandBar => MyEditorCommandBar;
    public RichEditBox? EditorRichEditBox => MyEditorRichEditBox;
    public TextBox? EditorTextBox => null;
    public string? FilePath { get => filePath; set => filePath = value; }

    // Event handlers
    private void RichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (document.Selection.CharacterFormat.Size == -9999999f /*tomUndefined*/)
        {
            FontSizeBox.Text = "~~";
        }
        else
        {
            FontSizeBox.Text = document.Selection.CharacterFormat.Size.ToString();
        }

        if (document.Selection.CharacterFormat.Name == null)
        {
            FontFamilyBox.Content = "~~";
        }
        else
        {
            FontFamilyBox.Content = document.Selection.CharacterFormat.Name;
        }

        // Calculate line number
        document.GetText(TextGetOptions.None, out text);
        line = text.Substring(0, document.Selection.StartPosition).Split('\r').Length;
        args.Line = line;

        // Calculate column number
        column = document.Selection.StartPosition - text.Substring(0, document.Selection.StartPosition).LastIndexOf('\r');
        args.Column = column;

        EditorStateChanged?.Invoke(this, args);
    }

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

    private void FontFamilyLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (((ListView)sender).SelectedItem == null)
        {
            return;
        }

        string selFont = fontFamilies.First(f => f == ((ListView)sender).SelectedItem.ToString());
        document.Selection.CharacterFormat.Name = selFont;
        FontFamilyBox.Content = selFont;

        if (sender.Equals(FontFamilyList))
        {
            if (recentFontFamilies.Contains(selFont))
            {
                recentFontFamilies.Remove(selFont);
                recentFontFamilies.Insert(0, selFont);
            }
            else
            {
                recentFontFamilies.Insert(0, selFont);
            }
            if (recentFontFamilies.Count > 5)
    {
                recentFontFamilies.RemoveAt(5);
            }
        }
    }

    private void FontSizeUp_Click(object sender, RoutedEventArgs e)
    {
        ChangeFontSize(0, null);
    }

    private void FontSizeBox_SelectionChangedSize(object sender, SelectionChangedEventArgs e)
        {
        if (int.TryParse(FontSizeBox.SelectedItem.ToString(), out int changeTo) && changeTo > 0)
        {
        MyEditorRichEditBox.Focus(FocusState.Programmatic);
        ChangeFontSize(2, changeTo);
        } else
        {
            User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
        }
        }  

    private void FontSizeBox_TextSubmittedSize(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
    {
        if (int.TryParse(sender.Text, out int changeTo) && changeTo > 0)
        {
        MyEditorRichEditBox.Focus(FocusState.Programmatic);
        ChangeFontSize(2, changeTo);
        } else
        {
            User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
        }
    }

    private void FontSizeDown_Click(object sender, RoutedEventArgs e)
    {
        ChangeFontSize(1, null);
    }

    private void AlignLeft_Click(object sender, RoutedEventArgs e)
    {
        document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Left;
    }

    private void AlignCenter_Click(object sender, RoutedEventArgs e)
        {
        document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        }

    private void AlignRight_Click(object sender, RoutedEventArgs e)
    {
        document.Selection.ParagraphFormat.Alignment = ParagraphAlignment.Right;
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
            if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down) == true)
            {
                zoomFactor = 5;
            }
            else
            {
            zoomFactor += 0.25;
            }

        MyEditorRichEditBox.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };

            args.Zoom = zoomFactor.ToString("P0");
            EditorStateChanged?.Invoke(this, args);
        }
        else
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

            MyEditorRichEditBox.RenderTransform = new ScaleTransform { ScaleX = zoomFactor, ScaleY = zoomFactor };

            args.Zoom = zoomFactor.ToString("P0");
            EditorStateChanged?.Invoke(this, args);
        }
        else
        {
            User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
        }
    }

    // Helper methods
    private void ChangeFontSize(int mode, int? newSize)
    {
        ITextRange character;
        ITextCharacterFormat charFormat;
        float currentSize;

        switch (mode)
        {
            case 0:
                for (int i = document.Selection.StartPosition; i < document.Selection.EndPosition; i++)
                {
                    character = document.GetRange(i, i + 1);
                    charFormat = character.CharacterFormat;
                    currentSize = charFormat.Size;

                    if (currentSize >= 72 && currentSize < 80)
                    {
                        charFormat.Size = 80;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize >= 80 && currentSize % 10 != 0)
                    {
                        charFormat.Size = (float)(Math.Ceiling(currentSize / 10.0) * 10);
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize >= 80)
                    {
                        charFormat.Size += 10;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize < 8)
                    {
                        charFormat.Size += 1;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }

                    int index = Array.BinarySearch(fontSizes, (int)currentSize);
                    if (index >= 0)
                    {
                        charFormat.Size = fontSizes[index + 1];
                    }
                    else
                    {
                        int insertionPoint = ~index;
                        charFormat.Size = fontSizes[insertionPoint];
                    }
                    FontSizeBox.Text = charFormat.Size.ToString();
                }
                break;
            case 1:
                for (int i = document.Selection.StartPosition; i < document.Selection.EndPosition; i++)
                {
                    character = document.GetRange(i, i + 1);
                    charFormat = character.CharacterFormat;
                    currentSize = charFormat.Size;

                    if (currentSize > 72 && currentSize <= 80)
                    {
                        charFormat.Size = 72;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize >= 80 && currentSize % 10 != 0)
                    {
                        charFormat.Size = (float)(Math.Floor(currentSize / 10.0) * 10);
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize >= 80)
                    {
                        charFormat.Size -= 10;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    }
                    else if (currentSize <= 8 && currentSize > 1)
                    {
                        charFormat.Size -= 1;
                        FontSizeBox.Text = charFormat.Size.ToString();
                        break;
                    } else if (currentSize == 1)
                    {
                        User32.MessageBeep((uint)Beep.MB_ICONEXCLAMATION);
                        break;
                    }

                    int index = Array.BinarySearch(fontSizes, (int)currentSize);
                    if (index >= 0)
                    {
                        charFormat.Size = fontSizes[index - 1];
                    }
                    else
                    {
                        int insertionPoint = ~index;
                        charFormat.Size = fontSizes[insertionPoint - 1];
                    }
                    FontSizeBox.Text = charFormat.Size.ToString();
                }
                break;
            case 2:
                if (newSize != null)
                {
                    document.Selection.CharacterFormat.Size = (float)newSize;
                    FontSizeBox.Text = document.Selection.CharacterFormat.Size.ToString();
                }
                break;
            default:
                break;
        }
    }

    public EditorStateChangedEventArgs GetCurrentState()
    {
        return args;
    }
}