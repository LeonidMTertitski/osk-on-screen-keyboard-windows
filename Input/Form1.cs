// Input - On screen keyboard
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Input
{
    public partial class Form1 : Form
    {
        private const string m_title = "Input 1.0";
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WM_KEYDOWN = 0x0100; 
        public Form1()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Xlocation == -32000 ||
                Properties.Settings.Default.Ylocation == -32000 ||
                Properties.Settings.Default.Xlocation > Screen.FromControl(this).Bounds.Width - 40 ||
                Properties.Settings.Default.Ylocation > Screen.FromControl(this).Bounds.Height - 40)
            {
                // rare case
                Properties.Settings.Default.Xlocation = 0;
                Properties.Settings.Default.Ylocation = 0;
            }
            this.Location = new Point(Properties.Settings.Default.Xlocation, Properties.Settings.Default.Ylocation);
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (!this.TopMost)
            {
                this.Location = new Point(Properties.Settings.Default.Xlocation, Properties.Settings.Default.Ylocation);
                richTextAllKeyWords.Activate(this, m_title);
                this.KeyPreview = true;
                this.TopMost = true;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Xlocation = this.RestoreBounds.X;
            Properties.Settings.Default.Ylocation = this.RestoreBounds.Y;
            richTextAllKeyWords.Close();
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                richTextAllKeyWords.ResizeForm();
            }
        }
        protected override CreateParams CreateParams
        {
            // needed to prevent focus change
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_NOACTIVATE;
                return createParams;
            }
        }
        protected override bool ProcessKeyPreview(ref Message m)
        {
            // maybe needed if you add more controls
            if (m.Msg == WM_KEYDOWN && (Keys)m.WParam == Keys.Tab)
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    this.SelectNextControl(ActiveControl, false, true, true, true);  // Bring focus to previous control
                }
                else
                {
                    this.SelectNextControl(ActiveControl, true, true, true, true);  // Bring focus to next control
                }
            }

            return base.ProcessKeyPreview(ref m);
        }
        public struct R
        {
            public int x;
            public int y;
            public int right;
            public int bottom;
        }
        protected override void DefWndProc(ref Message m)
        {
            // needed to prevent focus change and to support window position change
            const int WM_MOUSEACTIVATE = 0x21;
            const int MA_NOACTIVATE = 0x0003;
            const int WM_MOVING = 0x0216;
            R loc;
            switch (m.Msg)
            {
                case WM_MOVING:
                    {
                        loc = (R)Marshal.PtrToStructure(m.LParam, typeof(R));
                        Location = new Point(loc.x, loc.y);
                        m.Result = (IntPtr)0;
                        return;
                    }
                case WM_MOUSEACTIVATE:
                    m.Result = (IntPtr)MA_NOACTIVATE;
                    return;
            }
            base.DefWndProc(ref m);
        }
    }
}
