// KeyboardInput.cs - Simulate keyboard input
/*
Copyright (c) 2019, Leonid M Tertitski (Leonid.M.Tertitski@gmail.com)
All rights reserved.

Redistribution and use in source and binary forms, without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Input
{
    class KeyboardInput
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetMessageExtraInfo();

        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int key);

        [DllImport("User32.dll")]
        public static extern short GetKeyState(int key);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [StructLayout(LayoutKind.Sequential)]
        internal struct INPUT
        {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }
        [Flags]
        public enum KEYEVENTF
        {
            KEYDOWN = 0,
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008,
        }
        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public HARDWAREINPUT Hardware;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            public uint Msg;
            public ushort ParamL;
            public ushort ParamH;
        }
        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ms646310(v=vs.85).aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }
        /// <summary>
        /// http://social.msdn.microsoft.com/forums/en-US/netfxbcl/thread/2abc6be8-c593-4686-93d2-89785232dacd
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }
        private INPUT[] inputs = Array.Empty<INPUT>();
        public bool m_ctrlPressed;
        public bool m_altPressed;
        public bool m_capsPressed;
        public bool m_shiftPressed;
        public bool CaseChanged = false;

        public const ushort VK_KEY_DOWN = (ushort)0x8000;
        public const ushort VK_KEY_TOGLED = (ushort)0x0001;
        public const byte VK_BACK = 0x08;
        public const byte VK_TAB = 0x09;
        public const byte VK_RETURN = 0x0D;
        public const byte VK_SHIFT = 0x10;
        public const byte VK_CONTROL = 0x11;
        public const byte VK_MENU = 0x12;
        public const byte VK_CAPITAL = 0x14;
        public const byte VK_ESCAPE = 0x1B;
        public const byte VK_SPACE = 0x20;
        public const byte VK_SNAPSHOT = 0x2C;
        public const byte VK_DELETE = 0x2E;
        public const byte VK_F1 = 0x70;

        public const int m_KeyDownProcessDelay = 100;
        public const int MAX_CLIPBOARD_READ_TM = 1000;
        public KeyboardInput()
        {
            ResetAll();
        }
        public void SimulateKeyDown(ushort vk, bool unicode = false)
        {
            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = 0,//unicode ? (ushort)0 : vk,
                Scan = unicode ? vk : (ushort)MapVirtualKey(vk, 0),
                Flags = (uint)KEYEVENTF.KEYDOWN | (unicode ? (uint)KEYEVENTF.UNICODE : (uint)KEYEVENTF.SCANCODE),
                Time = 0,
                ExtraInfo = GetMessageExtraInfo() //IntPtr.Zero,
            };

            Array.Resize(ref inputs, inputs.Length + 1);
            inputs[inputs.Length - 1] = input;
        }
        public void SimulateKeybdKeyDown(ushort vk, bool unicode = false)
        {
            SimulateKeyDown(vk, unicode);
            if (!unicode)
            {
                switch (vk)
                {
                    case VK_MENU:
                        m_altPressed = true;
                        break;
                    case VK_CONTROL:
                        m_ctrlPressed = true;
                        break;
                    case VK_SHIFT:
                        m_shiftPressed = true;
                        CaseChanged = true;
                        break;
                    case VK_CAPITAL:
                        m_capsPressed = !m_capsPressed;
                        SimulateKeyUp(vk, unicode); // to change Caps state you need to click (down+up) Caps
                        CaseChanged = true;
                        break;
                    default:
                        return; // do not wait for Key processed
                }
                SendKeybdInput();
                Thread.Sleep(m_KeyDownProcessDelay); // wait for Key processed
            }
        }
        public void SimulateKeyUp(ushort vk, bool unicode = false)
        {
            INPUT input = new INPUT { Type = 1 };
            input.Data.Keyboard = new KEYBDINPUT()
            {
                Vk = 0,//unicode ? (ushort)0 : vk,
                Scan = unicode ? vk : (ushort)MapVirtualKey(vk, 0),
                Flags = (uint)KEYEVENTF.KEYUP | (unicode ? (uint)KEYEVENTF.UNICODE : (uint)KEYEVENTF.SCANCODE),
                Time = 0,
                ExtraInfo = GetMessageExtraInfo(),  //IntPtr.Zero,
            };
            Array.Resize(ref inputs, inputs.Length + 1);
            inputs[inputs.Length - 1] = input;
        }
        public void SimulateKeybdKeyUp(ushort vk, bool unicode = false)
        {
            if (!unicode)
            {
                switch (vk)
                {
                    case VK_MENU:
                        m_altPressed = false;
                        break;
                    case VK_CONTROL:
                        m_ctrlPressed = false;
                        break;
                    case VK_SHIFT:
                        m_shiftPressed = false;
                        CaseChanged = true;
                        break;
                    case VK_CAPITAL:
                        return; // use only caps down
                    default:
                        break;
                }
            }
            SimulateKeyUp(vk, unicode);
        }
        public void SimulateKeybdKeyClick(ushort vk, bool unicode = false)
        {
            if (!unicode)
            {
                switch (vk)
                {
                    case VK_MENU:
                        if (!m_altPressed)
                            SimulateKeybdKeyDown(vk);
                        else
                            SimulateKeybdKeyUp(vk);
                        return;
                    case VK_SNAPSHOT:
                        SimulateKeybdKeyDown(VK_SNAPSHOT);
                        if (m_altPressed)
                            SimulateKeybdKeyUp(VK_MENU);
                        SimulateKeybdKeyUp(VK_SNAPSHOT);
                        return;
                    case VK_CONTROL:
                        if (!m_ctrlPressed)
                            SimulateKeybdKeyDown(VK_CONTROL);
                        else
                            SimulateKeybdKeyUp(VK_CONTROL);
                        return;
                    case VK_SHIFT:
                        if (!m_shiftPressed)
                            SimulateKeybdKeyDown(VK_SHIFT);
                        else
                            SimulateKeybdKeyUp(VK_SHIFT);
                        return;
                    case VK_CAPITAL:
                        m_capsPressed = !m_capsPressed;
                        SimulateKeybdKeyDown(vk, unicode); // SimulateKeybdKeyDown will call SimulateKeyUp for Caps
                        return;
                    default:
                        break;
                }
            }
            SimulateKeybdKeyDown(vk, unicode);
            SimulateKeybdKeyUp(vk, unicode);
        }
        public void SimulateKeybdString(string str)
        {
            // simulate keyboard iput without check for key words
            while (str != null && str.Length > 0)
            {
                char firstChar = str[0];
                if (firstChar >= 0xbb && firstChar <= 0xbe)
                    SimulateKeybdKeyClick((ushort)(firstChar - 0x90), false); // +,-.
                else if (Char.IsDigit(firstChar))
                    SimulateKeybdKeyClick(firstChar, false); // needed for some web sites for input field with check "is it digit"
                else
                    SimulateKeybdKeyClick(firstChar, true);
                str = str.Substring(1, str.Length - 1);
            }
        }
        public string GetClipboard()
        {
            string cb = "";
            string oldcb = Clipboard.GetText(TextDataFormat.UnicodeText);
            SimulateKeybdKeyClick((ushort)'C'); // copy to clipboard
            SendKeybdInput();
            for (int i = 0; i < MAX_CLIPBOARD_READ_TM; i += m_KeyDownProcessDelay)
            {
                // Note: SW will stay in the loop 1000 ms (MAX_CLIPBOARD_READ_TM) 
                //       if current clipboard text and highlighted text are the same.
                Thread.Sleep(m_KeyDownProcessDelay); // wait for clipboard update
                cb = Clipboard.GetText(TextDataFormat.UnicodeText);
                if (!cb.Equals(oldcb))
                    break;
            }
            return cb;
        }
        public bool ChangeCase(string str)
        {
            if (str == null)
                return false;
            string cb;
            if (str.StartsWith("{LOWERCASE}", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!m_ctrlPressed)
                {
                    SimulateKeybdKeyDown(VK_CONTROL);
                    m_ctrlPressed = true;
                }
                cb = GetClipboard();
                cb = cb.ToLowerInvariant();
            }
            else if (str.StartsWith("{UPPERCASE}", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!m_ctrlPressed)
                {
                    SimulateKeybdKeyDown(VK_CONTROL);
                    m_ctrlPressed = true;
                }
                cb = GetClipboard();
                cb = cb.ToUpperInvariant();
            }
            else
                return false;
            SimulateKeybdKeyUp(VK_CONTROL);
            m_ctrlPressed = false;
            SimulateKeybdString(cb);
            return true;
        }
        public void ResetAllKeysStates()
        {
            if (m_capsPressed)
            {
                m_capsPressed = false;
                SimulateKeybdKeyClick(VK_CAPITAL);
            }
            ResetShiftCtrlAltKeysStates();
        }
        public bool CtrlOrAltPressed()
        {
            return (m_ctrlPressed | m_altPressed);
        }
        public void SendKeybdInput()
        {
            if (inputs.Length > 0)
            {
                try
                {
                    uint intReturn = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                    var lastError = Marshal.GetLastWin32Error();
                    if (intReturn != inputs.Length)
                    {
                        int error = lastError;
                        MessageBox.Show(String.Format("SendInput error: {0}", error), "Input");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("E3: {0} - {1}.", ex.Source, ex.Message), "KeyboardInput");
                }
                Array.Clear(inputs, 0, inputs.Length);
                inputs = Array.Empty<INPUT>();
            }
        }
        public void ProcessKeyWord(KeyWord keyword)
        {
            string str = keyword.AttachedText;
            ProcessKeysString(str, keyword);
        }
        public void ProcessKeysString(string str, KeyWord keyword)
        {
            Array.Clear(inputs, 0, inputs.Length);
            inputs = Array.Empty<INPUT>();
            while (str.Length > 0)
            {
                int itemLength = 0;
                if (str.StartsWith("{", StringComparison.InvariantCultureIgnoreCase) && (itemLength = str.IndexOf('}') + 1) > 0)
                {
                    if (ChangeCase(str))
                    {
                    }
                    else if (str.StartsWith("{BACKSPACE}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_BACK);
                    }
                    else if (str.StartsWith("{ALT}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_altPressed = !m_altPressed;
                        if (m_altPressed)
                            SimulateKeybdKeyDown(VK_MENU);
                        else
                            SimulateKeybdKeyUp(VK_MENU);
                    }
                    else if (str.StartsWith("{PRTSC}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_SNAPSHOT);
                        if (m_altPressed)
                        {
                            m_altPressed = false;
                            SimulateKeybdKeyUp(VK_MENU);
                        }
                    }
                    else if (str.StartsWith("{DEL}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_DELETE);
                    }
                    else if (str.StartsWith("{CTRL}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_ctrlPressed = !m_ctrlPressed;
                        if (m_ctrlPressed)
                        {
                            SimulateKeybdKeyDown(VK_CONTROL);
                        }
                        else
                            SimulateKeybdKeyUp(VK_CONTROL);
                    }
                    else if (str.StartsWith("{SHIFT}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_shiftPressed = !m_shiftPressed;
                        if (m_shiftPressed)
                            SimulateKeybdKeyDown(VK_SHIFT);
                        else
                            SimulateKeybdKeyUp(VK_SHIFT);
                        CaseChanged = true;
                    }
                    else if (str.StartsWith("{CAPSLOCK}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        m_capsPressed = !m_capsPressed;
                        SimulateKeybdKeyClick(VK_CAPITAL);
                        CaseChanged = true;
                    }
                    else if (str.StartsWith("{SPACE}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_SPACE);
                    }
                    else if (str.StartsWith("{ESCAPE}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_ESCAPE);
                    }
                    else if (str.StartsWith("{ENTER}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_RETURN);
                        SendKeybdInput(); // flush current input
                        ResetShiftCtrlAltKeysStates();
                    }
                    else if (str.StartsWith("{TAB}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SimulateKeybdKeyClick(VK_TAB);
                        SendKeybdInput(); // flush current input (needed for alt+tab)
                        ResetShiftCtrlAltKeysStates();
                    }
                    else if (str.StartsWith("{F", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(str.Substring(2, itemLength - 3), out int fv) && fv >= 1 && fv <= 24)
                        {
                            ushort fk = (ushort)(VK_F1 - 1 + fv);
                            SimulateKeybdKeyClick(fk);
                        }
                    }
                    else if (str.StartsWith("{VK_0X", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
                        if (int.TryParse(str.Substring(6, itemLength - 7), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int vk)
                            && vk >= 1 && vk <= 254)// && vk != 0x07 && vk != 0x16 && vk != 0x1a && (vk < 0x3a || vk > 0x40) && (vk < 0x88 || vk > 0x8f)
                        {
                            SimulateKeybdKeyClick((ushort)(vk));
                        }
                    }
                    else if (str.StartsWith("{VK_UP_0X", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(str.Substring(6, itemLength - 7), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int vk)
                            && vk >= 1 && vk <= 254)// && vk != 0x07 && vk != 0x16 && vk != 0x1a && (vk < 0x3a || vk > 0x40) && (vk < 0x88 || vk > 0x8f)
                        {
                            SimulateKeybdKeyUp((ushort)(vk));
                        }
                    }
                    else if (str.StartsWith("{VK_DOWN_0X", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(str.Substring(6, itemLength - 7), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int vk)
                            && vk >= 1 && vk <= 254)// && vk != 0x07 && vk != 0x16 && vk != 0x1a && (vk < 0x3a || vk > 0x40) && (vk < 0x88 || vk > 0x8f)
                        {
                            SimulateKeybdKeyDown((ushort)(vk));
                        }
                    }
                    else
                        itemLength = 0;
                }
                if (itemLength == 0)
                {
                    char firstChar = str[0];
                    itemLength = 1;
                    if (firstChar >= 'a' && firstChar <= 'z' && CtrlOrAltPressed())
                        SimulateKeybdKeyClick((ushort)Char.ToUpperInvariant(firstChar));
                    else if (firstChar >= 'A' && firstChar <= 'Z' && CtrlOrAltPressed())
                        SimulateKeybdKeyClick((ushort)firstChar);
                    else if (keyword.CanBeUsedWithShift)
                    {
                        if (m_shiftPressed != m_capsPressed)
                            SimulateKeybdString(keyword.UpperText);
                        else
                            SimulateKeybdString(keyword.LowerText);
                        itemLength = str.Length;
                    }
                    else
                        SimulateKeybdString(firstChar.ToString());
                    SendKeybdInput();
                    ResetShiftCtrlAltKeysStates();
                }
                str = str.Substring(itemLength, str.Length - itemLength);
            }
            SendKeybdInput();
        }
        public void ResetShiftCtrlAltKeysStates()
        {
            GetCapsShiftCtrlAltState();
            if (m_ctrlPressed)
            {
                m_ctrlPressed = false;
                SimulateKeybdKeyUp(VK_CONTROL);
            }
            if (m_shiftPressed)
            {
                m_shiftPressed = false;
                SimulateKeybdKeyUp(VK_SHIFT);
                CaseChanged = true;
            }
            if (m_altPressed)
            {
                m_altPressed = false;
                SimulateKeybdKeyUp(VK_MENU);
            }
            SendKeybdInput();
        }
        public void GetCapsShiftCtrlAltState()
        {
            bool shiftPressedOld = m_shiftPressed;
            bool capsPressedOld = m_capsPressed;
            m_shiftPressed = (GetAsyncKeyState(VK_SHIFT) & VK_KEY_DOWN) != 0;
            m_ctrlPressed = (GetAsyncKeyState(VK_CONTROL) & VK_KEY_DOWN) != 0;
            m_altPressed = (GetAsyncKeyState(VK_MENU) & VK_KEY_DOWN) != 0;
            m_capsPressed = (GetKeyState(VK_CAPITAL) & VK_KEY_TOGLED) != 0;
            if (shiftPressedOld != m_shiftPressed || capsPressedOld != m_capsPressed)
                CaseChanged = true;// Shift down or Caps ON presed on a different keyboard
        }
        public void ResetAll()
        {
            m_altPressed = false;
            m_ctrlPressed = false;
            m_capsPressed = false;
            m_shiftPressed = false;
        }
    }
}
