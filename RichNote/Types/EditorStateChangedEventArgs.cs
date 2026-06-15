using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichNote.Types
{
    public class EditorStateChangedEventArgs : EventArgs
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Zoom { get; set; }

        public EditorStateChangedEventArgs(int line, int column, double zoom)
        {
            Line = line;
            Column = column;
            Zoom = zoom.ToString("P0");
        }
    }
}
