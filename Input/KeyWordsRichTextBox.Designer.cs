using System.Windows.Forms;

namespace Input
{
    partial class KeyWordsRichTextBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.followCursorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoFadeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playSoundOnClickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.followCursorToolStripMenuItem,
            this.autoFadeToolStripMenuItem,
            this.playSoundOnClickToolStripMenuItem,
            this.inputToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.contextMenuStrip1.ShowCheckMargin = true;
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(451, 236);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1_Opening);
            // 
            // followCursorToolStripMenuItem
            // 
            this.followCursorToolStripMenuItem.Name = "followCursorToolStripMenuItem";
            this.followCursorToolStripMenuItem.Size = new System.Drawing.Size(450, 58);
            this.followCursorToolStripMenuItem.Text = "Auto move window";
            this.followCursorToolStripMenuItem.Click += new System.EventHandler(this.FollowCursorToolStripMenuItem_Click);
            // 
            // autoFadeToolStripMenuItem
            // 
            this.autoFadeToolStripMenuItem.Checked = true;
            this.autoFadeToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoFadeToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.autoFadeToolStripMenuItem.Name = "autoFadeToolStripMenuItem";
            this.autoFadeToolStripMenuItem.Size = new System.Drawing.Size(450, 58);
            this.autoFadeToolStripMenuItem.Text = "Auto fade";
            this.autoFadeToolStripMenuItem.Click += new System.EventHandler(this.AutoFadeToolStripMenuItem_Click);
            // 
            // playSoundOnClickToolStripMenuItem
            // 
            this.playSoundOnClickToolStripMenuItem.Name = "playSoundOnClickToolStripMenuItem";
            this.playSoundOnClickToolStripMenuItem.Size = new System.Drawing.Size(450, 58);
            this.playSoundOnClickToolStripMenuItem.Text = "Play sound on click";
            this.playSoundOnClickToolStripMenuItem.Click += new System.EventHandler(this.PlaySoundOnClickToolStripMenuItem_Click);
            // 
            // inputToolStripMenuItem
            // 
            this.inputToolStripMenuItem.Name = "inputToolStripMenuItem";
            this.inputToolStripMenuItem.Size = new System.Drawing.Size(450, 58);
            this.inputToolStripMenuItem.Text = "Select Keywords Table";
            // 
            // KeyWordsRichTextBox
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CausesValidation = false;
            this.DetectUrls = false;
            this.Location = new System.Drawing.Point(-1, -1);
            this.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
            this.Name = "richTextAllKeyWords";
            this.ReadOnly = true;
            this.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.Size = new System.Drawing.Size(430, 298);
            this.TabStop = false;
            this.WordWrap = false;
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RichTextAllKeyWords_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RichTextAllKeyWords_MouseDown);
            this.MouseLeave += new System.EventHandler(this.KeyWordsRichTextBox_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RichTextAllKeyWords_MouseMove);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem followCursorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playSoundOnClickToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inputToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoFadeToolStripMenuItem;
    }
}
