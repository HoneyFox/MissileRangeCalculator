using System;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
    partial class FormMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.txtSubsonicDrag = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSupersonicDrag = new System.Windows.Forms.TextBox();
            this.txtMotor = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDryMass = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDiameter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtInitSpeed = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtInitAlt = new System.Windows.Forms.TextBox();
            this.txtCutoffSpeed = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSimulate = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtInitAngle = new System.Windows.Forms.TextBox();
            this.picMain = new System.Windows.Forms.PictureBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPitch = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtTargetSpeed = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtTargetDistance = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtDeltaTime = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtDisplayScale = new System.Windows.Forms.TextBox();
            this.txtInducedDragFactor = new System.Windows.Forms.TextBox();
            this.picPlotData = new System.Windows.Forms.PictureBox();
            this.picLegends = new System.Windows.Forms.PictureBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtCLMax = new System.Windows.Forms.TextBox();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnOpenScript = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPlotData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLegends)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSubsonicDrag
            // 
            this.txtSubsonicDrag.Location = new System.Drawing.Point(35, 6);
            this.txtSubsonicDrag.Name = "txtSubsonicDrag";
            this.txtSubsonicDrag.Size = new System.Drawing.Size(33, 21);
            this.txtSubsonicDrag.TabIndex = 0;
            this.txtSubsonicDrag.Text = "0.2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "CD";
            // 
            // txtSupersonicDrag
            // 
            this.txtSupersonicDrag.Location = new System.Drawing.Point(74, 6);
            this.txtSupersonicDrag.Name = "txtSupersonicDrag";
            this.txtSupersonicDrag.Size = new System.Drawing.Size(33, 21);
            this.txtSupersonicDrag.TabIndex = 2;
            this.txtSupersonicDrag.Text = "0.4";
            // 
            // txtMotor
            // 
            this.txtMotor.HideSelection = false;
            this.txtMotor.Location = new System.Drawing.Point(12, 58);
            this.txtMotor.Multiline = true;
            this.txtMotor.Name = "txtMotor";
            this.txtMotor.Size = new System.Drawing.Size(252, 136);
            this.txtMotor.TabIndex = 3;
            this.txtMotor.Text = "7.5,50,276";
            this.txtMotor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtMotor_MouseUp);
            this.txtMotor.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.txtMotor_MouseWheel);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(152, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "DryMass";
            // 
            // txtDryMass
            // 
            this.txtDryMass.Location = new System.Drawing.Point(205, 6);
            this.txtDryMass.Name = "txtDryMass";
            this.txtDryMass.Size = new System.Drawing.Size(46, 21);
            this.txtDryMass.TabIndex = 5;
            this.txtDryMass.Text = "100";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(257, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "Diameter";
            // 
            // txtDiameter
            // 
            this.txtDiameter.Location = new System.Drawing.Point(316, 6);
            this.txtDiameter.Name = "txtDiameter";
            this.txtDiameter.Size = new System.Drawing.Size(56, 21);
            this.txtDiameter.TabIndex = 7;
            this.txtDiameter.Text = "0.18";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(378, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "InitialSpeed";
            // 
            // txtInitSpeed
            // 
            this.txtInitSpeed.Location = new System.Drawing.Point(458, 6);
            this.txtInitSpeed.Name = "txtInitSpeed";
            this.txtInitSpeed.Size = new System.Drawing.Size(56, 21);
            this.txtInitSpeed.TabIndex = 9;
            this.txtInitSpeed.Text = "300";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(520, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "InitialAlt";
            // 
            // txtInitAlt
            // 
            this.txtInitAlt.Location = new System.Drawing.Point(591, 6);
            this.txtInitAlt.Name = "txtInitAlt";
            this.txtInitAlt.Size = new System.Drawing.Size(66, 21);
            this.txtInitAlt.TabIndex = 11;
            this.txtInitAlt.Text = "8000";
            // 
            // txtCutoffSpeed
            // 
            this.txtCutoffSpeed.Location = new System.Drawing.Point(866, 7);
            this.txtCutoffSpeed.Name = "txtCutoffSpeed";
            this.txtCutoffSpeed.Size = new System.Drawing.Size(56, 21);
            this.txtCutoffSpeed.TabIndex = 12;
            this.txtCutoffSpeed.Text = "500";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(789, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "CutoffSpeed";
            // 
            // btnSimulate
            // 
            this.btnSimulate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSimulate.Location = new System.Drawing.Point(927, 30);
            this.btnSimulate.Name = "btnSimulate";
            this.btnSimulate.Size = new System.Drawing.Size(110, 23);
            this.btnSimulate.TabIndex = 14;
            this.btnSimulate.Text = "Simulate";
            this.btnSimulate.UseVisualStyleBackColor = true;
            this.btnSimulate.Click += new System.EventHandler(this.btnSimulate_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 36);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(203, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "T,m,Isp(Range),CDs,Diameter,CLMax";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(663, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "InitialAngle";
            // 
            // txtInitAngle
            // 
            this.txtInitAngle.Location = new System.Drawing.Point(746, 6);
            this.txtInitAngle.Name = "txtInitAngle";
            this.txtInitAngle.Size = new System.Drawing.Size(37, 21);
            this.txtInitAngle.TabIndex = 17;
            this.txtInitAngle.Text = "30";
            // 
            // picMain
            // 
            this.picMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.picMain.Location = new System.Drawing.Point(272, 58);
            this.picMain.Name = "picMain";
            this.picMain.Size = new System.Drawing.Size(650, 545);
            this.picMain.TabIndex = 18;
            this.picMain.TabStop = false;
            this.picMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picMain_MouseDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 208);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(149, 12);
            this.label9.TabIndex = 19;
            this.label9.Text = "T,ω/G,Targetθ,Engineδ";
            // 
            // txtPitch
            // 
            this.txtPitch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPitch.HideSelection = false;
            this.txtPitch.Location = new System.Drawing.Point(12, 228);
            this.txtPitch.Multiline = true;
            this.txtPitch.Name = "txtPitch";
            this.txtPitch.Size = new System.Drawing.Size(252, 126);
            this.txtPitch.TabIndex = 20;
            this.txtPitch.Text = "10,0\r\n10,-0.7\r\n20,-0.8\r\n90,-0.5";
            this.txtPitch.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtPitch_MouseUp);
            this.txtPitch.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.txtPitch_MouseWheel);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(220, 36);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 12);
            this.label10.TabIndex = 21;
            this.label10.Text = "TargetSpeed";
            // 
            // txtTargetSpeed
            // 
            this.txtTargetSpeed.Location = new System.Drawing.Point(297, 31);
            this.txtTargetSpeed.Name = "txtTargetSpeed";
            this.txtTargetSpeed.Size = new System.Drawing.Size(56, 21);
            this.txtTargetSpeed.TabIndex = 22;
            this.txtTargetSpeed.Text = "400";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(359, 36);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(89, 12);
            this.label11.TabIndex = 23;
            this.label11.Text = "TargetDistance";
            // 
            // txtTargetDistance
            // 
            this.txtTargetDistance.Location = new System.Drawing.Point(454, 31);
            this.txtTargetDistance.Name = "txtTargetDistance";
            this.txtTargetDistance.Size = new System.Drawing.Size(90, 21);
            this.txtTargetDistance.TabIndex = 24;
            this.txtTargetDistance.Text = "45000";
            // 
            // label12
            // 
            this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(659, 36);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(47, 12);
            this.label12.TabIndex = 25;
            this.label12.Text = "Scale X";
            // 
            // txtDeltaTime
            // 
            this.txtDeltaTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDeltaTime.Location = new System.Drawing.Point(718, 31);
            this.txtDeltaTime.Name = "txtDeltaTime";
            this.txtDeltaTime.Size = new System.Drawing.Size(66, 21);
            this.txtDeltaTime.TabIndex = 26;
            this.txtDeltaTime.Text = "0.25";
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(790, 36);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(47, 12);
            this.label13.TabIndex = 27;
            this.label13.Text = "Scale Y";
            // 
            // txtDisplayScale
            // 
            this.txtDisplayScale.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDisplayScale.Location = new System.Drawing.Point(856, 31);
            this.txtDisplayScale.Name = "txtDisplayScale";
            this.txtDisplayScale.Size = new System.Drawing.Size(66, 21);
            this.txtDisplayScale.TabIndex = 28;
            this.txtDisplayScale.Text = "0.5";
            // 
            // txtInducedDragFactor
            // 
            this.txtInducedDragFactor.Location = new System.Drawing.Point(113, 6);
            this.txtInducedDragFactor.Name = "txtInducedDragFactor";
            this.txtInducedDragFactor.Size = new System.Drawing.Size(33, 21);
            this.txtInducedDragFactor.TabIndex = 29;
            this.txtInducedDragFactor.Text = "0.08";
            // 
            // picPlotData
            // 
            this.picPlotData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picPlotData.BackColor = System.Drawing.Color.DarkBlue;
            this.picPlotData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPlotData.Location = new System.Drawing.Point(12, 366);
            this.picPlotData.Name = "picPlotData";
            this.picPlotData.Size = new System.Drawing.Size(252, 237);
            this.picPlotData.TabIndex = 30;
            this.picPlotData.TabStop = false;
            // 
            // picLegends
            // 
            this.picLegends.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picLegends.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.picLegends.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picLegends.Location = new System.Drawing.Point(928, 58);
            this.picLegends.Name = "picLegends";
            this.picLegends.Size = new System.Drawing.Size(108, 272);
            this.picLegends.TabIndex = 31;
            this.picLegends.TabStop = false;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(550, 36);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(41, 12);
            this.label14.TabIndex = 32;
            this.label14.Text = "CL Max";
            // 
            // txtCLMax
            // 
            this.txtCLMax.Location = new System.Drawing.Point(597, 31);
            this.txtCLMax.Name = "txtCLMax";
            this.txtCLMax.Size = new System.Drawing.Size(56, 21);
            this.txtCLMax.TabIndex = 33;
            this.txtCLMax.Text = "2.4";
            // 
            // btnPaste
            // 
            this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPaste.Location = new System.Drawing.Point(928, 582);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(109, 20);
            this.btnPaste.TabIndex = 34;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Location = new System.Drawing.Point(928, 556);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(109, 20);
            this.btnCopy.TabIndex = 35;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAs.Location = new System.Drawing.Point(928, 530);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(109, 20);
            this.btnSaveAs.TabIndex = 36;
            this.btnSaveAs.Text = "Save As...";
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "mrc";
            this.saveFileDialog.Filter = "MRC Data files|*.mrc|All files|*.*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "mrc";
            this.openFileDialog.Filter = "MRC Data files|*.mrc|All files|*.*";
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.Location = new System.Drawing.Point(928, 504);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(109, 20);
            this.btnOpen.TabIndex = 37;
            this.btnOpen.Text = "Open...";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnOpenScript
            // 
            this.btnOpenScript.Location = new System.Drawing.Point(177, 204);
            this.btnOpenScript.Name = "btnOpenScript";
            this.btnOpenScript.Size = new System.Drawing.Size(87, 20);
            this.btnOpenScript.TabIndex = 38;
            this.btnOpenScript.Text = "Script ...";
            this.btnOpenScript.UseVisualStyleBackColor = true;
            this.btnOpenScript.Click += new System.EventHandler(this.btnOpenScript_Click);
            // 
            // FormMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 611);
            this.Controls.Add(this.btnOpenScript);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.btnSaveAs);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnPaste);
            this.Controls.Add(this.txtCLMax);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.picLegends);
            this.Controls.Add(this.picPlotData);
            this.Controls.Add(this.txtInducedDragFactor);
            this.Controls.Add(this.txtDisplayScale);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtDeltaTime);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtTargetDistance);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtTargetSpeed);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.txtPitch);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.picMain);
            this.Controls.Add(this.txtInitAngle);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnSimulate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtCutoffSpeed);
            this.Controls.Add(this.txtInitAlt);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtInitSpeed);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDiameter);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDryMass);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMotor);
            this.Controls.Add(this.txtSupersonicDrag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSubsonicDrag);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(1060, 650);
            this.Name = "FormMain";
            this.Text = "Missile Range Calculator";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResizeBegin += new System.EventHandler(this.FormMain_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.FormMain_ResizeEnd);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormMain_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUp);
            this.Resize += new System.EventHandler(this.FormMain_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPlotData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLegends)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.TextBox txtSubsonicDrag;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSupersonicDrag;
        private System.Windows.Forms.TextBox txtMotor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDryMass;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDiameter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtInitSpeed;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtInitAlt;
        private System.Windows.Forms.TextBox txtCutoffSpeed;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSimulate;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtInitAngle;
        private System.Windows.Forms.PictureBox picMain;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtPitch;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtTargetSpeed;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtTargetDistance;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtDeltaTime;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtDisplayScale;
        private System.Windows.Forms.TextBox txtInducedDragFactor;
        private System.Windows.Forms.PictureBox picPlotData;
        private System.Windows.Forms.PictureBox picLegends;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtCLMax;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnOpen;
        private Button btnOpenScript;
    }
}

