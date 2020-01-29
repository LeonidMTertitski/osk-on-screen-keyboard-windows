// KeyWord.cs - Class to support simulation of keyboard input
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

namespace Input
{
    public class KeyWord
    {
        public KeyWord(string[] keyWord, int iStart, Font useFont)
        {
            if (keyWord.Length > 0)
            {
                string key = keyWord[0].Trim(new char[] { ' ', '\t' });
                int iOffset = keyWord[0].IndexOf(key);
                bool cmd = keyWord.Length > 1 && keyWord[1].StartsWith("cmd:", StringComparison.InvariantCultureIgnoreCase);
                string val = cmd ? keyWord[1].Substring(4) : (keyWord.Length > 1 ? keyWord[1] : key);
                key = key.Replace("\r", "|");
                val = val.Replace("\r", "|");
                val = val.Replace("\\n", "\n");
                string valUpper = val.Length == 1 ? val.ToUpperInvariant() : val;
                string valLower = val.Length == 1 ? val.ToLowerInvariant() : val;
                bool useWithShift = !valUpper.Equals(valLower, StringComparison.InvariantCulture);
                string keyUpper = useWithShift ? key.ToUpperInvariant() : key;
                string keyLower = useWithShift ? key.ToLowerInvariant() : key;
                Start = iStart + iOffset;
                Length = key.Length + 2; // 1 space before and after name
                IndexSameItem = -1;
                Selected = false;
                Highlighted = false;
                Text = keyLower;
                LowerText = keyLower;
                UpperText = keyUpper;
                AttachedText = valLower;
                CanBeUsedWithShift = useWithShift;
                IsShellCommand = cmd;
                FontToUse = useFont;
            }
        }
        public int Start { get; set; }                 // start index in richTextAllKeyWords.Text
        public int Length { get; set; }                // length in richTextAllKeyWords.Text
        public int IndexSameItem { get; set; }         // index of the Same Item
        public bool Selected { get; set; }             // true if selected in richTextAllKeyWords
        public bool Highlighted { get; set; }          // true if highlighted in richTextAllKeyWords
        public string Text { get; set; }               // the KeyWord to be recognized in richTextAllKeyWords.Text
        public string LowerText { get; set; }          // the KeyWord to be recognized in richTextAllKeyWords.Textt
        public string UpperText { get; set; }          // the KeyWord to be recognized in richTextAllKeyWords.Text with Caps or Shift
        public string AttachedText { get; set; }       // the text associated with the KeyWord
        public bool CanBeUsedWithShift { get; set; }   // KeyWord can be used with Shift or Caps
        public bool IsShellCommand { get; set; }       // this KeyWord is an command or not
        public Font FontToUse { get; set; }            // Font
    }
}
