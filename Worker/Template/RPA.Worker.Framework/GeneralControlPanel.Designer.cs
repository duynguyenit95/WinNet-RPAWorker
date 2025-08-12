
using RPA.Core;

namespace RPA.Worker.Framework
{
    partial class GeneralControlPanel<T> where T : WorkerOption
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
            this.lbWorkerName = new System.Windows.Forms.Label();
            this.gbHubLog = new System.Windows.Forms.GroupBox();
            this.listboxHubLog = new System.Windows.Forms.ListBox();
            this.lbWorkerState = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbHubState = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.gbHubLog.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbWorkerName
            // 
            this.lbWorkerName.AutoSize = true;
            this.lbWorkerName.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbWorkerName.Location = new System.Drawing.Point(340, 9);
            this.lbWorkerName.Name = "lbWorkerName";
            this.lbWorkerName.Size = new System.Drawing.Size(187, 33);
            this.lbWorkerName.TabIndex = 0;
            this.lbWorkerName.Text = "WorkerName";
            // 
            // gbHubLog
            // 
            this.gbHubLog.Controls.Add(this.listboxHubLog);
            this.gbHubLog.Location = new System.Drawing.Point(18, 106);
            this.gbHubLog.Name = "gbHubLog";
            this.gbHubLog.Size = new System.Drawing.Size(975, 332);
            this.gbHubLog.TabIndex = 1;
            this.gbHubLog.TabStop = false;
            this.gbHubLog.Text = "Hub Logs";
            // 
            // listboxHubLog
            // 
            this.listboxHubLog.FormattingEnabled = true;
            this.listboxHubLog.HorizontalScrollbar = true;
            this.listboxHubLog.Location = new System.Drawing.Point(6, 19);
            this.listboxHubLog.Name = "listboxHubLog";
            this.listboxHubLog.ScrollAlwaysVisible = true;
            this.listboxHubLog.Size = new System.Drawing.Size(963, 303);
            this.listboxHubLog.TabIndex = 1;
            // 
            // lbWorkerState
            // 
            this.lbWorkerState.AutoSize = true;
            this.lbWorkerState.Location = new System.Drawing.Point(289, 24);
            this.lbWorkerState.Name = "lbWorkerState";
            this.lbWorkerState.Size = new System.Drawing.Size(24, 13);
            this.lbWorkerState.TabIndex = 12;
            this.lbWorkerState.Text = "Idle";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(219, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Worker State:";
            // 
            // lbHubState
            // 
            this.lbHubState.AutoSize = true;
            this.lbHubState.Location = new System.Drawing.Point(74, 24);
            this.lbHubState.Name = "lbHubState";
            this.lbHubState.Size = new System.Drawing.Size(73, 13);
            this.lbHubState.TabIndex = 8;
            this.lbHubState.Text = "Disconnected";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Hub State:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbWorkerState);
            this.groupBox1.Controls.Add(this.lbHubState);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(18, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(975, 56);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Connection";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // GeneralControlPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 443);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbHubLog);
            this.Controls.Add(this.lbWorkerName);
            this.Name = "GeneralControlPanel";
            this.Text = "Worker Control Panel";
            this.Load += new System.EventHandler(this.ControlPanel_Load);
            this.Shown += new System.EventHandler(this.QueueWorkerPanel_Shown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.gbHubLog.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbWorkerName;
        private System.Windows.Forms.GroupBox gbHubLog;
        private System.Windows.Forms.Label lbWorkerState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbHubState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listboxHubLog;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

