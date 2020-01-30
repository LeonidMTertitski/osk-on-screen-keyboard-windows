// KeyWordsRichTextBox.cs - Class to display on screen keyboard
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
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace Input
{
    public partial class KeyWordsRichTextBox : System.Windows.Forms.RichTextBox
    {
        [DllImport("user32.dll")]
        private static extern int HideCaret(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr w);

        private Form m_Form;
        private IntPtr m_currentFocus = IntPtr.Zero;
        private MemoryStream m_ClickSoundStream;
        private readonly Stopwatch m_mouseDownWatch;
        private string m_title = "Input";
        private readonly KeyWords KeyWords = new KeyWords();
        private readonly System.Windows.Forms.Timer m_aTimer = new System.Windows.Forms.Timer();
        private Point m_itemPoint;
        private Point m_mousePoint;
        private Color m_defaultSelBg;
        private string m_selectedKeyWordsName;
        private int m_itemMouseDown;
        private bool m_ignoreLongMouseDown;
        private readonly Font m_richTextAllKeyWordsFont;
        private bool m_moveOnMouseButtonDown;
        private bool m_fade;
        private bool m_playClickSound;
        private readonly KeyboardInput m_keybd;
        static bool m_playClickExit;
        static int m_playClickCount;
        private SoundPlayer m_clickSoundPlayer;
        private Thread m_PlayClickThread;
        private const int WM_SETREDRAW = 0x0b;
        private const int WM_SETFOCUS = 0x0007;
        private const int WM_KILLFOCUS = 0x0008;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        
        private const int MARGIN_WIDTH = 8;
        public int m_itemHighlightedIndex;
        public KeyWordsRichTextBox()
        {
            InitializeComponent();
            this.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.RichTextAllKeyWords_ContentsResized);
            m_itemHighlightedIndex = -1;
            WordWrap = false;
            ScrollBars = RichTextBoxScrollBars.None;
            SetStyle(ControlStyles.Selectable, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            m_keybd = new KeyboardInput();
            m_richTextAllKeyWordsFont = Font;
            m_ignoreLongMouseDown = true;
            m_playClickExit = false;
            m_playClickCount = 0;
            m_mouseDownWatch = new Stopwatch();
            m_mousePoint = new Point(0, 0);
            m_itemPoint = new Point(-1, -1);
            ResetAll();
            m_aTimer.Tick += new EventHandler(OnTimedEvent);
            m_aTimer.Interval = 500;
        }
        public void Activate(Form form, string title)
        {
            m_Form = form;
            m_title = title;
            // set params values saved on last exit
            m_moveOnMouseButtonDown = Properties.Settings.Default.MoveWindow;
            m_fade = Properties.Settings.Default.FadeWindow;
            m_playClickSound = Properties.Settings.Default.PlaySoundOnClick;
            m_selectedKeyWordsName = Properties.Settings.Default.KeyWordsName;
            m_selectedKeyWordsName = GenerateMenuAndGetKeyWordsName(m_selectedKeyWordsName);
            m_defaultSelBg = SelectionBackColor;
            ReadOnly = false; // needed to prevent "beep" sound on "Shift" in win7
            SelectionBackColor = m_defaultSelBg;
            BackColor = m_defaultSelBg;
            SetCurrentCapsShiftCtrlAlt();
            LoadKeyWords();
            Thread thread = new Thread(new ThreadStart(PlayClick));
            m_PlayClickThread = thread;
            m_PlayClickThread.Priority = ThreadPriority.AboveNormal;
            m_PlayClickThread.Start();
        }
        private void RichTextAllKeyWords_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            if (!(sender is RichTextBox rch))
            {
                return;
            }
            if (e.NewRectangle.Width > 20)
            {
                rch.ClientSize = new Size(
                    e.NewRectangle.Width + MARGIN_WIDTH,
                    e.NewRectangle.Height);
                m_Form.Invalidate();
            }
        }
        public void ResizeForm()
        {
            Size size = Size;
            ClientSize = new Size(
                size.Width,
                size.Height);
            Point loc = PointToScreen(Location);
            int top = loc.Y - m_Form.Location.Y;
            int widthOffset = loc.X - m_Form.Location.X;
            int width = (int)size.Width + widthOffset*2;
            int height = (int)size.Height + top + widthOffset / 2;
            if (m_Form.Width != width || m_Form.Height != height)
            {
                m_Form.Width = width;
                m_Form.Height = height;
                m_Form.Invalidate();
            }
        }
        private void SetCase()
        {
            if (m_keybd.CaseChanged)
            {
                bool bToUpper = m_keybd.m_shiftPressed != m_keybd.m_capsPressed;
                m_keybd.CaseChanged = false;
                if (KeyWords.SetCase(bToUpper))
                {
                    SetText(KeyWords.m_AllItems);
                }
            }
        }
        private void SetFont(Font fontToUse, int istartInd, int istopInd)
        {
            if (istartInd < 0 || istartInd >= KeyWords.m_KeyWords.Length ||
                istopInd < 0 || istopInd >= KeyWords.m_KeyWords.Length)
                return;
            int istart = KeyWords.m_KeyWords[istartInd].Start + 1;
            int ilength = KeyWords.m_KeyWords[istopInd].Start + KeyWords.m_KeyWords[istopInd].Length - istart - 1;
            if (ilength > 0)
            {
                Select(istart, ilength);
                SelectionFont = fontToUse;
            }
        }
        private void SetFont()
        {
            int istartInd = 0;
            int istopInd = KeyWords.m_KeyWords.Length - 1;
            Font fontToUse = m_richTextAllKeyWordsFont;

            for (int i = 0; i < KeyWords.m_KeyWords.Length; i++)
            {
                bool bSameFont = fontToUse.Equals(KeyWords.m_KeyWords[i].FontToUse);
                if (bSameFont)
                    istopInd = i;
                else
                {
                    if (i > 0)
                        SetFont(fontToUse, istartInd, istopInd);
                    istartInd = i;
                    istopInd = i;
                    fontToUse = KeyWords.m_KeyWords[i].FontToUse;
                }
            }
            SetFont(fontToUse, istartInd, istopInd);

            Select(0, 0);
        }
        private void SelectCapsShiftCtrlAlt()
        {
            SelectItem(m_keybd.m_ctrlPressed, KeyWords.ctrlIndex);
            SelectItem(m_keybd.m_shiftPressed, KeyWords.shiftIndex);
            SelectItem(m_keybd.m_capsPressed, KeyWords.capsIndex);
            SelectItem(m_keybd.m_altPressed, KeyWords.altIndex);
            SelectItem(true, KeyWords.selectedIndex);
        }
        private void SetCurrentCapsShiftCtrlAlt()
        {
            SuspendDrawing(); // to prevent flickering
            m_keybd.GetCapsShiftCtrlAltState(); // keys states can be changed by a different keyboard
            SetCase(); // show letters with correct case
            SelectCapsShiftCtrlAlt(); // select "pressed" keys
            ResizeForm(); // resize window if needed
            ResumeDrawing(); // resume drawing
        }
        private void ProcessCommand(string cmd)
        {
            Process process = new Process();
            Process proc = process;
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = cmd;
            proc.Start();
        }
        private void ProcessKeyWord(KeyWord keyword)
        {
            IntPtr wnd = GetForegroundWindow();
            if (wnd == m_Form.Handle)
            {
                MessageBox.Show("Please set input area", m_title);
                return;
            }
            if (keyword.AttachedText == null)
                return;
            if (m_playClickSound)
            {
                m_playClickCount++; // play "click" on each mouse click
            }
            if (keyword.IsShellCommand)
            {
                ProcessCommand(keyword.AttachedText);
            }
            else
            {
                m_keybd.ProcessKeyWord(keyword); // process string corresponding to selected key
                SetCurrentCapsShiftCtrlAlt();
            }
        }
        private void ProcessSelectedItem(int ind)
        {
            if (ind >= 0)
            {
                m_aTimer.Stop();
                ProcessKeyWord(KeyWords.m_KeyWords[ind]);
                m_aTimer.Start();
            }
        }
        private string GetFolderNames()
        {
            string folder = Application.StartupPath;
            if (folder.Contains("Release") || folder.Contains("Debug"))
                folder = @"C:\Input\";
            if (!folder.EndsWith(@"\", StringComparison.InvariantCultureIgnoreCase) && !folder.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
                folder += "\\";
            return folder;
        }

        private string GetKeyWordsPrefix()
        {
            return "KeyWords_";
        }
        private string GetKeyWordsExt()
        {
            return ".txt";
        }
        private string GetKeyWordsFileName(string keyWordName)
        {
            string prefix = GetKeyWordsPrefix();
            string ext = GetKeyWordsExt();
            string name = prefix + keyWordName + ext;
            return name;
        }
        private string GenerateMenuAndGetKeyWordsName(string keyWordsName)
        {
            try
            {
                string folder = GetFolderNames();
                string prefix = GetKeyWordsPrefix();
                string ext = GetKeyWordsExt();
                string[] allKeywordsFiles;
                allKeywordsFiles = Directory.GetFiles(folder, GetKeyWordsFileName("*"));
                if (allKeywordsFiles.Length == 0)
                    keyWordsName = "default";
                else
                {
                    int index = -1;
                    string kwNameMenu = "default";
                    inputToolStripMenuItem.DropDownItems.Clear();
                    for (int i = 0; i < allKeywordsFiles.Length; i++)
                    {
                        string fname = allKeywordsFiles[i].Substring(folder.Length);
                        string kwName = fname.Substring(prefix.Length, fname.Length - prefix.Length - ext.Length);
                        if (kwName.Length > 0)
                        {
                            ToolStripMenuItem menuitem = (ToolStripMenuItem)inputToolStripMenuItem.DropDownItems.Add(kwName, null, InputToolStripMenuItem_InputItem_Click);
                            kwNameMenu = kwName;
                            menuitem.ImageScaling = ToolStripItemImageScaling.None;
                            if (keyWordsName.Equals(kwName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                menuitem.Checked = true;
                                index = i;
                            }
                            else
                            {
                                menuitem.Checked = false;
                            }
                        }
                    }
                    if (index == -1)
                        keyWordsName = kwNameMenu; // use last menu name
                }
            }
            catch
            {
                keyWordsName = "default";
            }
            return keyWordsName;
        }
        private void LoadKeyWords()
        {
            // Table with "Key names" and assigned values is a text file located in the same folder as Input.exe in file
            // "KeyWords_X.txt" (X - name of the table)
            m_aTimer.Stop();
            KeyWords.m_AllItems = "";
            try
            {
                string folder = GetFolderNames();
                string text = "";
                while (true)
                {
                    string name = folder + GetKeyWordsFileName(m_selectedKeyWordsName);
                    text = System.IO.File.ReadAllText(name, Encoding.GetEncoding("utf-8"));
                    if (text.Length > 0)
                    {
                        break;
                    }
                    else
                    {
                        if (MessageBox.Show("File not available:\n" + name +
                            "\nDo you want to create 'default' file?", m_title, MessageBoxButtons.YesNo) != DialogResult.Yes)
                        {
                            Application.Exit();
                            return;
                        }
                        m_selectedKeyWordsName = "default";
                        name = folder + GetKeyWordsFileName(m_selectedKeyWordsName);
                        System.IO.File.WriteAllText(name, Properties.Resources.Words, Encoding.GetEncoding("utf-8"));
                    }
                }
                ResetAll();
                m_Form.Text = m_title + ": " + m_selectedKeyWordsName;
                KeyWords.ProcessKeyWords(text, m_richTextAllKeyWordsFont);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("E1: {0} - {1}.", ex.Source, ex.Message), m_title);
                return;
            }
            SetText(KeyWords.m_AllItems);
            m_aTimer.Start();
        }
        private void SetText(string str)
        {
            Text = str;
            KeyWords.UnselectAll(m_itemHighlightedIndex);
            SetFont();
            HighlightItem(true, m_itemHighlightedIndex);
        }
        private bool SameItem(int ind, int index)
        {
            while (ind >= 0)
            {
                if (ind == index)
                    return true;
                ind = KeyWords.m_KeyWords[ind].IndexSameItem;
            }
            return false;
        }
        private bool IsSpecialKey(int ind)
        {
            return SameItem(KeyWords.ctrlIndex, ind) ||
                   SameItem(KeyWords.shiftIndex, ind) ||
                   SameItem(KeyWords.capsIndex, ind) ||
                   SameItem(KeyWords.altIndex, ind);
        }
        private void HighlightItem(bool bHighlight, int ind)
        {
            if (ind >= 0 && ind < KeyWords.m_KeyWords.Length && KeyWords.m_KeyWords[ind].Highlighted != bHighlight)
            {
                // highlight key under current mouse cursor position
                Color bkColor;
                bool specialKey = IsSpecialKey(ind);
                Select(KeyWords.m_KeyWords[ind].Start, KeyWords.m_KeyWords[ind].Length);
                if (bHighlight && specialKey && KeyWords.m_KeyWords[ind].Selected)
                    bkColor = Color.YellowGreen;
                else if (!bHighlight && specialKey && KeyWords.m_KeyWords[ind].Selected)
                    bkColor = Color.Yellow;
                else if (!bHighlight && (ind == KeyWords.selectedIndex) && KeyWords.m_KeyWords[ind].Selected)
                    bkColor = Color.White;
                else if (!bHighlight)
                    bkColor = m_defaultSelBg;
                else
                    bkColor = Color.LightBlue;
                SelectionBackColor = bkColor;
                Select(0, 0);
                KeyWords.m_KeyWords[ind].Highlighted = bHighlight;
                if (!bHighlight && !specialKey && KeyWords.m_KeyWords[ind].Selected)
                    KeyWords.m_KeyWords[ind].Selected = false;
            }
        }
        private void SelectItem(bool bSelect, int ind)
        {
            Color bkColor = (ind == KeyWords.selectedIndex) ? Color.White : Color.Yellow;
            while (ind >= 0 && ind < KeyWords.m_KeyWords.Length && KeyWords.m_KeyWords[ind].Selected != bSelect)
            {
                Select(KeyWords.m_KeyWords[ind].Start, KeyWords.m_KeyWords[ind].Length);
                SelectionBackColor = bSelect ? bkColor : m_defaultSelBg;
                Select(0, 0);
                KeyWords.m_KeyWords[ind].Selected = bSelect;
                ind = KeyWords.m_KeyWords[ind].IndexSameItem;
            }
        }
        private void OnTimedEvent(Object myObject, EventArgs myEventArgs)
        {
            m_aTimer.Stop();
            if (m_Form.WindowState != FormWindowState.Minimized)
            {
                Point xyMouse = System.Windows.Forms.Control.MousePosition;
                Point xyWindow = m_Form.Location;
                int maxDistX = 100;
                int maxDistY = 100;
                bool mouseButtonsUp = System.Windows.Forms.Control.MouseButtons == MouseButtons.None;
                if ((xyMouse.X >= xyWindow.X && xyMouse.X <= xyWindow.X + m_Form.Width &&
                     xyMouse.Y >= xyWindow.Y && xyMouse.Y <= xyWindow.Y + m_Form.Height))
                    mouseButtonsUp = true;
                m_ignoreLongMouseDown = mouseButtonsUp;
                if (Math.Abs(m_mousePoint.X - xyMouse.X) > 3 || Math.Abs(m_mousePoint.Y - xyMouse.Y) > 3)
                    m_ignoreLongMouseDown = true;
                if (mouseButtonsUp ||
                    m_ignoreLongMouseDown ||
                    m_mousePoint.X != xyMouse.X ||
                    m_mousePoint.Y != xyMouse.Y)
                    m_mouseDownWatch.Restart();
                else if (m_moveOnMouseButtonDown && !m_ignoreLongMouseDown && m_mouseDownWatch.ElapsedMilliseconds > 1000)
                {
                    // move window if mouse button down for more than 1 sec
                    xyWindow.X = xyMouse.X + maxDistX;
                    xyWindow.Y = xyMouse.Y + maxDistY;

                    if (xyWindow.X + m_Form.Width > Screen.FromControl(m_Form).WorkingArea.Right)
                        xyWindow.X = Screen.FromControl(m_Form).WorkingArea.Right - m_Form.Width;
                    if (xyWindow.Y + m_Form.Height > Screen.FromControl(m_Form).WorkingArea.Bottom)
                        xyWindow.Y = Screen.FromControl(m_Form).WorkingArea.Bottom - m_Form.Height;
                    m_Form.Location = new Point(xyWindow.X, xyWindow.Y);
                    m_ignoreLongMouseDown = true;
                }
                if (!m_fade || (xyMouse.X >= xyWindow.X && xyMouse.X <= xyWindow.X + m_Form.Width &&
                                xyMouse.Y >= xyWindow.Y && xyMouse.Y <= xyWindow.Y + m_Form.Height))
                {
                    m_Form.Opacity = 1.0;
                }
                else
                {
                    // fade window if mouse cursor outside
                    m_Form.Opacity = 0.5;
                }
                m_mousePoint = xyMouse;
                SetCurrentCapsShiftCtrlAlt();
            }
            m_aTimer.Start();
            return;
        }
        private int GetKeyWordIndexByPos(Point p)
        {
            int ipos = GetCharIndexFromPosition(p);
            return KeyWords.GetKeyWordIndexByPos(ipos);
        }
        private void RichTextAllKeyWords_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_itemPoint.Equals(e.Location))
                return;
            m_itemPoint = e.Location;
            int ind = GetKeyWordIndexByPos(e.Location);
            if (ind >= 0)
            {
                if (m_itemHighlightedIndex != ind || KeyWords.m_KeyWords[ind].Highlighted != true)
                {
                    HighlightItem(false, m_itemHighlightedIndex);
                    m_itemHighlightedIndex = ind;
                    HighlightItem(true, ind);
                }
            }
        }
        private void RichTextAllKeyWords_MouseClick(object sender, MouseEventArgs e)
        {
            m_itemPoint = e.Location;
            int ind = GetKeyWordIndexByPos(e.Location);
            if (ind >= 0 && m_itemMouseDown == ind)
            {
                m_itemHighlightedIndex = ind;
                SelectItem(false, KeyWords.selectedIndex);
                if (!IsSpecialKey(ind))
                {
                    SelectItem(false, KeyWords.selectedIndex);
                    KeyWords.selectedIndex = ind;
                }
                SelectItem(true, ind);
                ProcessSelectedItem(ind);
            }
        }
        private void ResetAll()
        {
            m_itemHighlightedIndex = -1;
            KeyWords.capsIndex = -1;
            KeyWords.shiftIndex = -1;
            KeyWords.ctrlIndex = -1;
            KeyWords.altIndex = -1;
            KeyWords.selectedIndex = -1;
            m_itemMouseDown = -1;
        }
        private void RichTextAllKeyWords_MouseDown(object sender, MouseEventArgs e)
        {
            m_currentFocus = GetForegroundWindow();
            if (e.Button == MouseButtons.Right)
                contextMenuStrip1.Show(PointToScreen(e.Location));
            else
                m_itemMouseDown = GetKeyWordIndexByPos(e.Location);
        }
        private void ContextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            followCursorToolStripMenuItem.Checked = m_moveOnMouseButtonDown;
            autoFadeToolStripMenuItem.Checked = m_fade;
            playSoundOnClickToolStripMenuItem.Checked = m_playClickSound;
            m_selectedKeyWordsName = GenerateMenuAndGetKeyWordsName(m_selectedKeyWordsName);
        }
        private void MySetForegroundWindow()
        {
            if (m_currentFocus != IntPtr.Zero && m_currentFocus != m_Form.Handle)
            {
                for (int i = 0, n = 5; i < n; i++)
                {
                    if (SetForegroundWindow(m_currentFocus) != 0)
                        break;
                    if (i == n - 1)
                        System.Media.SystemSounds.Exclamation.Play();
                    else
                        Thread.Sleep(100);
                }
            }
        }
        private void FollowCursorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySetForegroundWindow();
            m_moveOnMouseButtonDown = !m_moveOnMouseButtonDown;
        }
        private void AutoFadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySetForegroundWindow();
            m_fade = !m_fade;
        }
        private void PlaySoundOnClickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MySetForegroundWindow();
            m_playClickSound = !m_playClickSound;
        }
        private void InputToolStripMenuItem_InputItem_Click(object sender, EventArgs e)
        {
            MySetForegroundWindow();
            m_selectedKeyWordsName = sender.ToString();
            LoadKeyWords();
        }
        private void MyHideCaret()
        {
            HideCaret(this.Handle);
        }
        private void SuspendDrawing()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }
        private void ResumeDrawing()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
            this.Invalidate();
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                SelectionStart = 0;
                SelectionLength = 0;
                return;
            }
            else if (m.Msg == WM_SETFOCUS)
                m.Msg = WM_KILLFOCUS;
            base.WndProc(ref m);
            MyHideCaret();
        }
        public void Close()
        {
            m_aTimer.Stop();
            m_aTimer.Dispose();
            m_keybd.ResetAllKeysStates();
            // save settings
            Properties.Settings.Default.MoveWindow = m_moveOnMouseButtonDown;
            Properties.Settings.Default.FadeWindow = m_fade;
            Properties.Settings.Default.KeyWordsName = m_selectedKeyWordsName;
            Properties.Settings.Default.PlaySoundOnClick = m_playClickSound;
            Properties.Settings.Default.Save();

            KeyWords.DisposeFonts();
            Dispose();

            m_playClickExit = true; // Exit from PlayClick thread
        }
        public void PlayClick()
        {
            // a little bit complicated (to support "click" sound on remote PC):
            //    can't play short sound on remote PC (and listen on local computer)
            //    the code below makes "click" sound longer by adding extra zeros to the end
            //    you are welcome to make it without this "trick".
            if (Properties.Resources.click.Capacity < 44)
                return;
            int minLength = 40000;
            if (minLength < Properties.Resources.click.Capacity)
                minLength = (int)Properties.Resources.click.Capacity;
            byte[] myByteArray = new byte[minLength];
            m_ClickSoundStream = new MemoryStream();
            int read = Properties.Resources.click.Read(myByteArray, 0, myByteArray.Length);
            int length = BitConverter.ToInt32(myByteArray, 40);
            byte[] newLength = BitConverter.GetBytes(length + (myByteArray.Length - read) / 2);
            for (int i = 0; i < 4; i++)
                myByteArray[40 + i] = newLength[i]; // change length in wav header
            m_ClickSoundStream.Write(myByteArray, 0, minLength);
            m_ClickSoundStream.Seek(0, SeekOrigin.Begin);
            m_clickSoundPlayer = new SoundPlayer
            {
                Stream = m_ClickSoundStream
            };
            m_clickSoundPlayer.Load();
            while (!m_playClickExit)
            {
                if (m_playClickCount > 0)
                {
                    m_playClickCount--;
                    m_clickSoundPlayer.PlaySync();
                }
                else
                    Thread.Sleep(50);
            }
            m_clickSoundPlayer.Dispose();
            m_ClickSoundStream.Close();
        }
    }
}
