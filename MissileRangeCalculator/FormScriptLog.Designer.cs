
namespace MissileRangeCalculator
{
    partial class FormScriptLog
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
            this.components = new System.ComponentModel.Container();
            this.txtScriptLogs = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.timerFlushLog = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // txtScriptLogs
            // 
            this.txtScriptLogs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptLogs.Location = new System.Drawing.Point(6, 6);
            this.txtScriptLogs.Multiline = true;
            this.txtScriptLogs.Name = "txtScriptLogs";
            this.txtScriptLogs.ReadOnly = true;
            this.txtScriptLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtScriptLogs.Size = new System.Drawing.Size(272, 201);
            this.txtScriptLogs.TabIndex = 0;
            this.txtScriptLogs.WordWrap = false;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(284, 5);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(94, 23);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // timerFlushLog
            // 
            this.timerFlushLog.Enabled = true;
            this.timerFlushLog.Interval = 300;
            this.timerFlushLog.Tick += new System.EventHandler(this.timerFlushLog_Tick);
            // 
            // FormScriptLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 211);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.txtScriptLogs);
            this.MinimumSize = new System.Drawing.Size(400, 250);
            this.Name = "FormScriptLog";
            this.Text = "Script Logs";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormScriptLog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtScriptLogs;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Timer timerFlushLog;
    }
}