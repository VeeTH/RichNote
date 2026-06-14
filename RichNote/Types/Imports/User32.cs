using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RichNote.Types
{
    class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MessageBeep(uint uType);
    }

    public enum Beep
    {
        MB_ICONERROR = 0x00000010,
        MB_ICONEXCLAMATION = 0x00000030,
        MB_ICONINFORMATION = 0x00000040
    }
}
