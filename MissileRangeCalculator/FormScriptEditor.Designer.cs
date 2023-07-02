namespace MissileRangeCalculator
{
    partial class FormScriptEditor
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
            this.txtScript = new System.Windows.Forms.TextBox();
            this.lblInputInfo = new System.Windows.Forms.Label();
            this.txtScriptInfo = new System.Windows.Forms.TextBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDefaultScript = new System.Windows.Forms.Button();
            this.txtErrors = new System.Windows.Forms.TextBox();
            this.splitScript = new System.Windows.Forms.SplitContainer();
            this.splitLibrary = new System.Windows.Forms.SplitContainer();
            this.treeViewClasses = new System.Windows.Forms.TreeView();
            this.lblScriptInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitScript)).BeginInit();
            this.splitScript.Panel1.SuspendLayout();
            this.splitScript.Panel2.SuspendLayout();
            this.splitScript.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLibrary)).BeginInit();
            this.splitLibrary.Panel1.SuspendLayout();
            this.splitLibrary.Panel2.SuspendLayout();
            this.splitLibrary.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtScript
            // 
            this.txtScript.AcceptsReturn = true;
            this.txtScript.AcceptsTab = true;
            this.txtScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScript.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtScript.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.RecentlyUsedList;
            this.txtScript.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtScript.HideSelection = false;
            this.txtScript.Location = new System.Drawing.Point(0, 0);
            this.txtScript.Multiline = true;
            this.txtScript.Name = "txtScript";
            this.txtScript.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtScript.Size = new System.Drawing.Size(357, 528);
            this.txtScript.TabIndex = 5;
            this.txtScript.Text = "\r\n";
            this.txtScript.WordWrap = false;
            this.txtScript.SizeChanged += new System.EventHandler(this.txtScript_SizeChanged);
            this.txtScript.TextChanged += new System.EventHandler(this.txtScript_TextChanged);
            this.txtScript.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtScript_KeyDown);
            this.txtScript.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtScript_KeyUp);
            this.txtScript.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtScript_MouseDown);
            this.txtScript.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtScript_MouseMove);
            this.txtScript.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtScript_MouseUp);
            // 
            // lblInputInfo
            // 
            this.lblInputInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInputInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblInputInfo.Location = new System.Drawing.Point(0, 531);
            this.lblInputInfo.Name = "lblInputInfo";
            this.lblInputInfo.Size = new System.Drawing.Size(357, 20);
            this.lblInputInfo.TabIndex = 4;
            // 
            // txtScriptInfo
            // 
            this.txtScriptInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptInfo.HideSelection = false;
            this.txtScriptInfo.Location = new System.Drawing.Point(0, 18);
            this.txtScriptInfo.Multiline = true;
            this.txtScriptInfo.Name = "txtScriptInfo";
            this.txtScriptInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtScriptInfo.Size = new System.Drawing.Size(252, 343);
            this.txtScriptInfo.TabIndex = 6;
            this.txtScriptInfo.WordWrap = false;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(-1, 519);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(123, 33);
            this.btnApply.TabIndex = 7;
            this.btnApply.Text = "Apply";
            this.btnApply.UseMnemonic = false;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(126, 519);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(127, 33);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDefaultScript
            // 
            this.btnDefaultScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDefaultScript.Location = new System.Drawing.Point(82, 247);
            this.btnDefaultScript.MaximumSize = new System.Drawing.Size(187, 37);
            this.btnDefaultScript.MinimumSize = new System.Drawing.Size(187, 37);
            this.btnDefaultScript.Name = "btnDefaultScript";
            this.btnDefaultScript.Size = new System.Drawing.Size(187, 37);
            this.btnDefaultScript.TabIndex = 9;
            this.btnDefaultScript.Text = "Add Default Script";
            this.btnDefaultScript.UseVisualStyleBackColor = true;
            this.btnDefaultScript.Click += new System.EventHandler(this.btnDefaultScript_Click);
            // 
            // txtErrors
            // 
            this.txtErrors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtErrors.HideSelection = false;
            this.txtErrors.Location = new System.Drawing.Point(0, 365);
            this.txtErrors.Multiline = true;
            this.txtErrors.Name = "txtErrors";
            this.txtErrors.ReadOnly = true;
            this.txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtErrors.Size = new System.Drawing.Size(252, 150);
            this.txtErrors.TabIndex = 10;
            this.txtErrors.WordWrap = false;
            this.txtErrors.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtErrors_MouseDoubleClick);
            // 
            // splitScript
            // 
            this.splitScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitScript.Location = new System.Drawing.Point(5, 4);
            this.splitScript.Name = "splitScript";
            // 
            // splitScript.Panel1
            // 
            this.splitScript.Panel1.Controls.Add(this.splitLibrary);
            this.splitScript.Panel1MinSize = 350;
            // 
            // splitScript.Panel2
            // 
            this.splitScript.Panel2.Controls.Add(this.lblScriptInfo);
            this.splitScript.Panel2.Controls.Add(this.txtScriptInfo);
            this.splitScript.Panel2.Controls.Add(this.txtErrors);
            this.splitScript.Panel2.Controls.Add(this.btnApply);
            this.splitScript.Panel2.Controls.Add(this.btnClose);
            this.splitScript.Panel2MinSize = 250;
            this.splitScript.Size = new System.Drawing.Size(874, 551);
            this.splitScript.SplitterDistance = 618;
            this.splitScript.TabIndex = 11;
            this.splitScript.SizeChanged += new System.EventHandler(this.splitScript_SizeChanged);
            // 
            // splitLibrary
            // 
            this.splitLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitLibrary.Location = new System.Drawing.Point(0, 0);
            this.splitLibrary.Name = "splitLibrary";
            // 
            // splitLibrary.Panel1
            // 
            this.splitLibrary.Panel1.Controls.Add(this.btnDefaultScript);
            this.splitLibrary.Panel1.Controls.Add(this.txtScript);
            this.splitLibrary.Panel1.Controls.Add(this.lblInputInfo);
            this.splitLibrary.Panel1MinSize = 300;
            // 
            // splitLibrary.Panel2
            // 
            this.splitLibrary.Panel2.Controls.Add(this.treeViewClasses);
            this.splitLibrary.Panel2MinSize = 250;
            this.splitLibrary.Size = new System.Drawing.Size(619, 551);
            this.splitLibrary.SplitterDistance = 357;
            this.splitLibrary.TabIndex = 10;
            // 
            // treeViewClasses
            // 
            this.treeViewClasses.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewClasses.HideSelection = false;
            this.treeViewClasses.Location = new System.Drawing.Point(0, 0);
            this.treeViewClasses.Name = "treeViewClasses";
            this.treeViewClasses.Size = new System.Drawing.Size(255, 551);
            this.treeViewClasses.TabIndex = 10;
            // 
            // lblScriptInfo
            // 
            this.lblScriptInfo.AutoSize = true;
            this.lblScriptInfo.Location = new System.Drawing.Point(0, 3);
            this.lblScriptInfo.Name = "lblScriptInfo";
            this.lblScriptInfo.Size = new System.Drawing.Size(221, 12);
            this.lblScriptInfo.TabIndex = 11;
            this.lblScriptInfo.Text = "T(Range)/*,UpdateFunc,PostUpdateFunc";
            // 
            // FormScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.splitScript);
            this.MinimumSize = new System.Drawing.Size(900, 600);
            this.Name = "FormScriptEditor";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Script Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormScriptEditor_FormClosing);
            this.splitScript.Panel1.ResumeLayout(false);
            this.splitScript.Panel2.ResumeLayout(false);
            this.splitScript.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitScript)).EndInit();
            this.splitScript.ResumeLayout(false);
            this.splitLibrary.Panel1.ResumeLayout(false);
            this.splitLibrary.Panel1.PerformLayout();
            this.splitLibrary.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLibrary)).EndInit();
            this.splitLibrary.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtScript;
        private System.Windows.Forms.Label lblInputInfo;
        private System.Windows.Forms.TextBox txtScriptInfo;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDefaultScript;
        private System.Windows.Forms.TextBox txtErrors;
        private System.Windows.Forms.SplitContainer splitScript;
        private System.Windows.Forms.SplitContainer splitLibrary;
        private System.Windows.Forms.TreeView treeViewClasses;
        private System.Windows.Forms.Label lblScriptInfo;
    }
}