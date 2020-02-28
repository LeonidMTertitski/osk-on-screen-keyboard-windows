namespace Input
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextAllKeyWords = new Input.KeyWordsRichTextBox();
            this.SuspendLayout();
            // 
            // richTextAllKeyWords
            // 
            this.richTextAllKeyWords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextAllKeyWords.CausesValidation = false;
            this.richTextAllKeyWords.DetectUrls = false;
            this.richTextAllKeyWords.Location = new System.Drawing.Point(0, 0);
            this.richTextAllKeyWords.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
            this.richTextAllKeyWords.Name = "richTextAllKeyWords";
            this.richTextAllKeyWords.ReadOnly = true;
            this.richTextAllKeyWords.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextAllKeyWords.Size = new System.Drawing.Size(136, 105);
            this.richTextAllKeyWords.TabIndex = 0;
            this.richTextAllKeyWords.TabStop = false;
            this.richTextAllKeyWords.Text = "";
            this.richTextAllKeyWords.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(136, 104);
            this.Controls.Add(this.richTextAllKeyWords);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Input";
            this.Activated += new System.EventHandler(this.Form1_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        //private System.Windows.Forms.RichTextBox richTextAllKeyWords;
        private KeyWordsRichTextBox richTextAllKeyWords;
    }
}

