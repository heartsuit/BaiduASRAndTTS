namespace SpeechProcessing
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comboBoxLan = new System.Windows.Forms.ComboBox();
            this.textBoxFile = new System.Windows.Forms.TextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.richTextBoxResult = new System.Windows.Forms.RichTextBox();
            this.buttonReadStop = new System.Windows.Forms.Button();
            this.buttonReadPause = new System.Windows.Forms.Button();
            this.buttonRead = new System.Windows.Forms.Button();
            this.buttonRecord = new System.Windows.Forms.Button();
            this.buttonRecognize = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBoxLan
            // 
            this.comboBoxLan.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.comboBoxLan.FormattingEnabled = true;
            this.comboBoxLan.Location = new System.Drawing.Point(342, 5);
            this.comboBoxLan.Name = "comboBoxLan";
            this.comboBoxLan.Size = new System.Drawing.Size(64, 25);
            this.comboBoxLan.TabIndex = 6;
            // 
            // textBoxFile
            // 
            this.textBoxFile.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBoxFile.Location = new System.Drawing.Point(2, 6);
            this.textBoxFile.Name = "textBoxFile";
            this.textBoxFile.Size = new System.Drawing.Size(334, 23);
            this.textBoxFile.TabIndex = 5;
            this.textBoxFile.Click += new System.EventHandler(this.textBoxFile_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelInfo.ForeColor = System.Drawing.Color.HotPink;
            this.labelInfo.Location = new System.Drawing.Point(5, 242);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(0, 17);
            this.labelInfo.TabIndex = 4;
            // 
            // richTextBoxResult
            // 
            this.richTextBoxResult.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBoxResult.Location = new System.Drawing.Point(2, 35);
            this.richTextBoxResult.Name = "richTextBoxResult";
            this.richTextBoxResult.Size = new System.Drawing.Size(404, 207);
            this.richTextBoxResult.TabIndex = 7;
            this.richTextBoxResult.Text = resources.GetString("richTextBoxResult.Text");
            // 
            // buttonReadStop
            // 
            this.buttonReadStop.BackgroundImage = global::SpeechProcessing.Properties.Resources.read_stop;
            this.buttonReadStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonReadStop.FlatAppearance.BorderSize = 0;
            this.buttonReadStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReadStop.Location = new System.Drawing.Point(415, 202);
            this.buttonReadStop.Name = "buttonReadStop";
            this.buttonReadStop.Size = new System.Drawing.Size(49, 46);
            this.buttonReadStop.TabIndex = 4;
            this.buttonReadStop.UseVisualStyleBackColor = true;
            this.buttonReadStop.Click += new System.EventHandler(this.buttonReadStop_Click);
            // 
            // buttonReadPause
            // 
            this.buttonReadPause.BackgroundImage = global::SpeechProcessing.Properties.Resources.read_pause;
            this.buttonReadPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonReadPause.FlatAppearance.BorderSize = 0;
            this.buttonReadPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReadPause.Location = new System.Drawing.Point(415, 149);
            this.buttonReadPause.Name = "buttonReadPause";
            this.buttonReadPause.Size = new System.Drawing.Size(49, 46);
            this.buttonReadPause.TabIndex = 3;
            this.buttonReadPause.UseVisualStyleBackColor = true;
            this.buttonReadPause.Click += new System.EventHandler(this.buttonReadPause_Click);
            // 
            // buttonRead
            // 
            this.buttonRead.BackgroundImage = global::SpeechProcessing.Properties.Resources.read;
            this.buttonRead.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonRead.FlatAppearance.BorderSize = 0;
            this.buttonRead.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRead.Location = new System.Drawing.Point(415, 96);
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.Size = new System.Drawing.Size(49, 46);
            this.buttonRead.TabIndex = 2;
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.buttonRead_Click);
            // 
            // buttonRecord
            // 
            this.buttonRecord.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("buttonRecord.BackgroundImage")));
            this.buttonRecord.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonRecord.FlatAppearance.BorderSize = 0;
            this.buttonRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRecord.Location = new System.Drawing.Point(415, 43);
            this.buttonRecord.Name = "buttonRecord";
            this.buttonRecord.Size = new System.Drawing.Size(49, 46);
            this.buttonRecord.TabIndex = 1;
            this.buttonRecord.UseVisualStyleBackColor = true;
            this.buttonRecord.Click += new System.EventHandler(this.buttonRecord_Click);
            // 
            // buttonRecognize
            // 
            this.buttonRecognize.BackgroundImage = global::SpeechProcessing.Properties.Resources.recognize;
            this.buttonRecognize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonRecognize.FlatAppearance.BorderSize = 0;
            this.buttonRecognize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonRecognize.Location = new System.Drawing.Point(415, 4);
            this.buttonRecognize.Name = "buttonRecognize";
            this.buttonRecognize.Size = new System.Drawing.Size(49, 32);
            this.buttonRecognize.TabIndex = 0;
            this.buttonRecognize.UseVisualStyleBackColor = true;
            this.buttonRecognize.Click += new System.EventHandler(this.buttonRecognize_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.ClientSize = new System.Drawing.Size(474, 262);
            this.Controls.Add(this.buttonReadStop);
            this.Controls.Add(this.buttonReadPause);
            this.Controls.Add(this.richTextBoxResult);
            this.Controls.Add(this.buttonRead);
            this.Controls.Add(this.buttonRecord);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.comboBoxLan);
            this.Controls.Add(this.textBoxFile);
            this.Controls.Add(this.buttonRecognize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Text <-->Speech";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLan;
        private System.Windows.Forms.TextBox textBoxFile;
        private System.Windows.Forms.Button buttonRecognize;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonRecord;
        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.RichTextBox richTextBoxResult;
        private System.Windows.Forms.Button buttonReadPause;
        private System.Windows.Forms.Button buttonReadStop;
    }
}

