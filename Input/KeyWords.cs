// KeyWords.cs - Class to support simulation of keyboard input
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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace Input
{
    class KeyWords
    {
        public int shiftIndex;
        public int capsIndex;
        public int ctrlIndex;
        public int altIndex;
        public int selectedIndex;
        public KeyWord[] m_KeyWords = Array.Empty<KeyWord>();
        public string m_AllItems = " ";
        public Font[] m_fonts = Array.Empty<Font>();
        private void SetCase(bool bToUpper, int istartCase, int istopCase)
        {
            int length1 = m_KeyWords[istartCase].Start;
            int length2 = m_KeyWords[istopCase].Start + m_KeyWords[istopCase].Length - length1;
            m_AllItems = m_AllItems.Substring(0, length1) +
                (bToUpper ? m_AllItems.Substring(length1, length2).ToUpperInvariant() : m_AllItems.Substring(length1, length2).ToLowerInvariant()) +
                m_AllItems.Substring(length1 + length2);
        }
        public void UnselectAll(int iHighlightedIndex)
        {
            if (ctrlIndex >= 0)
                m_KeyWords[ctrlIndex].Selected = false;
            if (shiftIndex >= 0)
                m_KeyWords[shiftIndex].Selected = false;
            if (capsIndex >= 0)
                m_KeyWords[capsIndex].Selected = false;
            if (altIndex >= 0)
                m_KeyWords[altIndex].Selected = false;
            if (selectedIndex >= 0)
                m_KeyWords[selectedIndex].Selected = false;
            if (iHighlightedIndex >= 0)
                m_KeyWords[iHighlightedIndex].Highlighted = false;
        }
        public bool SetCase(bool bToUpper)
        {
            int istartCase = -1;
            int istopCase = -1;

            for (int i = 0; i < m_KeyWords.Length; i++)
            {
                if (m_KeyWords[i].CanBeUsedWithShift)
                {
                    m_KeyWords[i].Text = bToUpper ? m_KeyWords[i].UpperText : m_KeyWords[i].LowerText;
                    if (istartCase < 0)
                        istartCase = i;
                    istopCase = i;
                }
                if (istartCase >= 0)
                {
                    SetCase(bToUpper, istartCase, istopCase);
                    istartCase = -1;
                }
            }
            if (istartCase >= 0)
                SetCase(bToUpper, istartCase, istopCase);
            return istopCase >= 0;
        }
        private int GetSameItemKeyWordIndex(KeyWord kw)
        {
            for (int j = 0; j < m_KeyWords.Length; j++)
            {
                int i = m_KeyWords.Length - j - 1;
                if (m_KeyWords[i].LowerText.Equals(kw.LowerText, StringComparison.InvariantCulture) &&
                    m_KeyWords[i].AttachedText.Equals(kw.AttachedText, StringComparison.InvariantCulture))
                {
                    return i;
                }
            }
            return -1;
        }
        private int GetKeyWordIndexByAttachedText(string text)
        {
            for (int j = 0; j < m_KeyWords.Length; j++)
            {
                int i = m_KeyWords.Length - j - 1;
                if (m_KeyWords[i].AttachedText.Equals(text, StringComparison.InvariantCulture))
                {
                    return i;
                }
            }
            return -1;
        }
        public int GetKeyWordIndexByPos(int ipos)
        {
            for (int i = 0; i < m_KeyWords.Length; i++)
            {
                if (m_KeyWords[i].Start <= ipos && m_KeyWords[i].Start + m_KeyWords[i].Length > ipos)
                    return i;
            }
            return -1;
        }
        private void AddKeyWord(string[] keyWord, Font useFont)
        {
            if (keyWord.Length > 0)
            {
                KeyWord kw = new KeyWord(keyWord, m_AllItems.Length, useFont);
                int nspaces = kw.Start - m_AllItems.Length;
                while (nspaces > 0)
                {
                    m_AllItems += " ";
                    nspaces--;
                }
                kw.IndexSameItem = GetSameItemKeyWordIndex(kw);
                // add KeyWord kw to m_KeyWords
                Array.Resize(ref m_KeyWords, m_KeyWords.Length + 1);
                m_KeyWords[m_KeyWords.Length - 1] = kw;
                m_AllItems += " " + kw.Text + " ";
            }
        }
        public void DisposeFonts()
        {
            foreach (Font fontToUse in m_fonts)
            {
                fontToUse.Dispose();
            }
            m_fonts = Array.Empty<Font>();
        }
        public void ProcessKeyWords(string text, Font defaultFont)
        {
            // KeyWords file:
            // Unicode Text file. Can be edited with Notepad. 
            // KeyWords file defines "keywords" for on screen keyboard. Each "keyword" defines:
            //    "keys" (user can click) and corresponding 
            //    "words" (string that will be used as input).
            // To define "keyword" use:
            // a. key|=words    - "words" can be with spaces and \n to define new line.
            // b. key|word      - "word" is a symbol or string 
            // c. key           - "key" is equal to "word"
            // Use "," to separate keywords
            // "--" defines new line in keyboard layout
            // To define values with special keyboard keys use:
            // {SHIFT},{CTRL},{ALT},{CAPSLOCK},{TAB},{Fn},{PRTSC},
            // or use {VK_0xZZ} for key click, {VK_DOWN_0xZZ}, {VK_UP_0xZZ} for key down/up.
            // ZZ is hexadecimal value (keyboard equivalents for the virtual-key code). See all values:
            // https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
            //
            // To convert selected text to upper/lower case use {UPPERCASE}/{LOWERCASE}
            // To define font use pair <Font=Name [Size=N]> ... </Font>
            //
            // To define symbol "comma" (,) or "vertical line" (|) in "key" or "word" use ',' or '|'
            //
            Font useFont = defaultFont; 
            Stack<Font> fonts = new Stack<Font>();
            DisposeFonts();
            string fontNameEnd = "</Font>";
            string fontNameStart = "<Font=";
            text = text.Replace("\r", ""); // remove all '\r'
            string[] lines = text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            float fontSize = defaultFont.Size;
            m_KeyWords = Array.Empty<KeyWord>();
            foreach (string line in lines)
            {
                string[] inputKeyWords = null;
                
                if (line.Trim().Length == 0 ||
                    line.StartsWith("//", StringComparison.InvariantCultureIgnoreCase) ||
                    line.StartsWith("* ", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue; // skip comment and empty lines..
                }
                if (line.StartsWith("--", StringComparison.InvariantCultureIgnoreCase))
                {
                    m_AllItems += "\n"; // end line
                    continue;
                }
                inputKeyWords = line.Split(new string[] { "|=" }, StringSplitOptions.RemoveEmptyEntries);
                if (inputKeyWords.Length == 2 && inputKeyWords[0].Length > 0)
                {
                    // Key|=Several words or sentences
                    AddKeyWord(inputKeyWords, useFont);
                    continue; // only one per line
                }
                if (line.StartsWith(fontNameStart, StringComparison.InvariantCultureIgnoreCase))
                {
                    int ind = line.IndexOf('>');
                    if (ind > 0)
                    {
                        string fontName = line.Substring(fontNameStart.Length, ind - fontNameStart.Length);
                        int iSizeStart = fontName.IndexOf("Size=", StringComparison.InvariantCultureIgnoreCase);
                        if (iSizeStart > 1)
                        {
                            string fontSizeStr = fontName.Substring(iSizeStart + "Size=".Length);
                            fontName = fontName.Substring(0, iSizeStart - 1);
                            if (float.TryParse(fontSizeStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float fontSizeFromFont))
                                fontSize = fontSizeFromFont;
                        }
                        fonts.Push(useFont);
                        fontName = fontName.Trim().Trim(new char[] { ',', ';' });
                        useFont = new Font(fontName, fontSize);
                        Array.Resize(ref m_fonts, m_fonts.Length + 1);
                        m_fonts[m_fonts.Length - 1] = useFont; // needed to dispose fonts later
                    }
                    continue;
                }
                if (line.StartsWith(fontNameEnd, StringComparison.InvariantCultureIgnoreCase))
                {
                    useFont = fonts.Count > 0 ? fonts.Pop() : defaultFont;
                    fontSize = useFont.Size;
                    continue;
                }
                string kline = line.Replace("','", "\r"); // temporary replace "','" to "\r"
                inputKeyWords = kline.Split(new char[] { ',' });
                foreach (string wordInLine in inputKeyWords)
                {
                    // split wordInLine
                    string[] parts;
                    string keyword = wordInLine.Replace("\r", ","); // replace "\r" to ","
                    keyword = keyword.Replace("'|'", "\r"); // temporary replace "'|'" to "\r"
                    parts = keyword.Split(new char[] { '|' });
                    AddKeyWord(parts, useFont);
                }
            }
            ctrlIndex = GetKeyWordIndexByAttachedText("{CTRL}");
            shiftIndex = GetKeyWordIndexByAttachedText("{SHIFT}");
            capsIndex = GetKeyWordIndexByAttachedText("{CAPSLOCK}");
            altIndex = GetKeyWordIndexByAttachedText("{ALT}");
        }
    }
}
