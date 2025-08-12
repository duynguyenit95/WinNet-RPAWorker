
using RPA.Core;

namespace RPA.Worker.Framework
{
    partial class ControlPanel<T> where T : WorkerOption
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanel<T>));
            this.components = new System.ComponentModel.Container();
            this.lbWorkerName = new System.Windows.Forms.Label();
            this.gbHubLog = new System.Windows.Forms.GroupBox();
            this.listboxHubLog = new System.Windows.Forms.ListBox();
            this.gbButton = new System.Windows.Forms.GroupBox();
            this.lbWorkerState = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnExecuteTask = new System.Windows.Forms.Button();
            this.gbExeLog = new System.Windows.Forms.GroupBox();
            this.listboxExeLog = new System.Windows.Forms.ListBox();
            this.lbHubState = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbNextRunTime = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.gbHubLog.SuspendLayout();
            this.gbButton.SuspendLayout();
            this.gbExeLog.SuspendLayout();
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
            this.gbHubLog.Location = new System.Drawing.Point(650, 106);
            this.gbHubLog.Name = "gbHubLog";
            this.gbHubLog.Size = new System.Drawing.Size(343, 332);
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
            this.listboxHubLog.Size = new System.Drawing.Size(331, 303);
            this.listboxHubLog.TabIndex = 1;
            // 
            // gbButton
            // 
            this.gbButton.Controls.Add(this.lbNextRunTime);
            this.gbButton.Controls.Add(this.label4);
            this.gbButton.Controls.Add(this.lbWorkerState);
            this.gbButton.Controls.Add(this.label3);
            this.gbButton.Controls.Add(this.btnExecuteTask);
            this.gbButton.Location = new System.Drawing.Point(61, 45);
            this.gbButton.Name = "gbButton";
            this.gbButton.Size = new System.Drawing.Size(583, 56);
            this.gbButton.TabIndex = 2;
            this.gbButton.TabStop = false;
            this.gbButton.Text = "Interaction";
            // 
            // lbWorkerState
            // 
            this.lbWorkerState.AutoSize = true;
            this.lbWorkerState.Location = new System.Drawing.Point(210, 24);
            this.lbWorkerState.Name = "lbWorkerState";
            this.lbWorkerState.Size = new System.Drawing.Size(24, 13);
            this.lbWorkerState.TabIndex = 12;
            this.lbWorkerState.Text = "Idle";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(140, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Worker State:";
            // 
            // btnExecuteTask
            // 
            this.btnExecuteTask.Location = new System.Drawing.Point(17, 19);
            this.btnExecuteTask.Name = "btnExecuteTask";
            this.btnExecuteTask.Size = new System.Drawing.Size(94, 22);
            this.btnExecuteTask.TabIndex = 0;
            this.btnExecuteTask.Text = "Execute";
            this.btnExecuteTask.UseVisualStyleBackColor = true;
            this.btnExecuteTask.Click += new System.EventHandler(this.btnExecuteTask_Click);
            // 
            // gbExeLog
            // 
            this.gbExeLog.Controls.Add(this.listboxExeLog);
            this.gbExeLog.Location = new System.Drawing.Point(12, 106);
            this.gbExeLog.Name = "gbExeLog";
            this.gbExeLog.Size = new System.Drawing.Size(632, 332);
            this.gbExeLog.TabIndex = 2;
            this.gbExeLog.TabStop = false;
            this.gbExeLog.Text = "Execution Logs";
            // 
            // listboxExeLog
            // 
            this.listboxExeLog.FormattingEnabled = true;
            this.listboxExeLog.HorizontalScrollbar = true;
            this.listboxExeLog.Location = new System.Drawing.Point(6, 19);
            this.listboxExeLog.Name = "listboxExeLog";
            this.listboxExeLog.ScrollAlwaysVisible = true;
            this.listboxExeLog.Size = new System.Drawing.Size(620, 303);
            this.listboxExeLog.TabIndex = 0;
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
            this.groupBox1.Controls.Add(this.lbHubState);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(759, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(175, 56);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Connection";
            // 
            // lbNextRunTime
            // 
            this.lbNextRunTime.AutoSize = true;
            this.lbNextRunTime.Location = new System.Drawing.Point(400, 24);
            this.lbNextRunTime.Name = "lbNextRunTime";
            this.lbNextRunTime.Size = new System.Drawing.Size(76, 13);
            this.lbNextRunTime.TabIndex = 14;
            this.lbNextRunTime.Text = "next Run Time";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(319, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Next Run Time: ";
            // 
            // ControlPanel
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 443);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbExeLog);
            this.Controls.Add(this.gbButton);
            this.Controls.Add(this.gbHubLog);
            this.Controls.Add(this.lbWorkerName);
            this.Name = "ControlPanel";
            this.Text = "Worker Control Panel";
            this.Load += new System.EventHandler(this.ControlPanel_Load);
            this.gbHubLog.ResumeLayout(false);
            this.gbButton.ResumeLayout(false);
            this.gbButton.PerformLayout();
            this.gbExeLog.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbWorkerName;
        private System.Windows.Forms.GroupBox gbHubLog;
        private System.Windows.Forms.GroupBox gbButton;
        private System.Windows.Forms.Button btnExecuteTask;
        private System.Windows.Forms.GroupBox gbExeLog;
        private System.Windows.Forms.Label lbWorkerState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbHubState;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox listboxHubLog;
        private System.Windows.Forms.ListBox listboxExeLog;
        private System.Windows.Forms.Label lbNextRunTime;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
    }
}

