namespace WiFi_Окошко
{
    partial class FormWiFi
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormWiFi));
            this.infoSSID = new System.Windows.Forms.Label();
            this.infoPassword = new System.Windows.Forms.Label();
            this.SSID = new System.Windows.Forms.RichTextBox();
            this.Passwd = new System.Windows.Forms.RichTextBox();
            this.PictureBoxQr = new System.Windows.Forms.PictureBox();
            this.LinkLabelAuthor = new System.Windows.Forms.LinkLabel();
            this.labelAuthor = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxQr)).BeginInit();
            this.SuspendLayout();
            // 
            // infoSSID
            // 
            this.infoSSID.AutoSize = true;
            this.infoSSID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.infoSSID.Location = new System.Drawing.Point(3, 18);
            this.infoSSID.Name = "infoSSID";
            this.infoSSID.Size = new System.Drawing.Size(52, 20);
            this.infoSSID.TabIndex = 1;
            this.infoSSID.Text = "SSID:";
            // 
            // infoPassword
            // 
            this.infoPassword.AutoSize = true;
            this.infoPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.infoPassword.Location = new System.Drawing.Point(3, 50);
            this.infoPassword.Name = "infoPassword";
            this.infoPassword.Size = new System.Drawing.Size(71, 20);
            this.infoPassword.TabIndex = 2;
            this.infoPassword.Text = "Пароль:";
            // 
            // SSID
            // 
            this.SSID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SSID.Cursor = System.Windows.Forms.Cursors.Default;
            this.SSID.DetectUrls = false;
            this.SSID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SSID.Location = new System.Drawing.Point(73, 18);
            this.SSID.Multiline = false;
            this.SSID.Name = "SSID";
            this.SSID.ReadOnly = true;
            this.SSID.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.SSID.Size = new System.Drawing.Size(136, 20);
            this.SSID.TabIndex = 5;
            this.SSID.TabStop = false;
            this.SSID.Text = "";
            // 
            // Passwd
            // 
            this.Passwd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Passwd.Cursor = System.Windows.Forms.Cursors.Default;
            this.Passwd.DetectUrls = false;
            this.Passwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Passwd.Location = new System.Drawing.Point(73, 50);
            this.Passwd.Multiline = false;
            this.Passwd.Name = "Passwd";
            this.Passwd.ReadOnly = true;
            this.Passwd.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.Passwd.Size = new System.Drawing.Size(136, 20);
            this.Passwd.TabIndex = 6;
            this.Passwd.TabStop = false;
            this.Passwd.Text = "";
            // 
            // PictureBoxQr
            // 
            this.PictureBoxQr.Location = new System.Drawing.Point(220, 9);
            this.PictureBoxQr.Name = "PictureBoxQr";
            this.PictureBoxQr.Size = new System.Drawing.Size(110, 110);
            this.PictureBoxQr.TabIndex = 7;
            this.PictureBoxQr.TabStop = false;
            // 
            // LinkLabelAuthor
            // 
            this.LinkLabelAuthor.AutoSize = true;
            this.LinkLabelAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LinkLabelAuthor.Location = new System.Drawing.Point(68, 107);
            this.LinkLabelAuthor.Name = "LinkLabelAuthor";
            this.LinkLabelAuthor.Size = new System.Drawing.Size(51, 12);
            this.LinkLabelAuthor.TabIndex = 9;
            this.LinkLabelAuthor.TabStop = true;
            this.LinkLabelAuthor.Text = "Автор Otto";
            this.LinkLabelAuthor.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelAuthor_LinkClicked);
            // 
            // labelAuthor
            // 
            this.labelAuthor.AutoSize = true;
            this.labelAuthor.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelAuthor.Location = new System.Drawing.Point(5, 107);
            this.labelAuthor.Name = "labelAuthor";
            this.labelAuthor.Size = new System.Drawing.Size(57, 12);
            this.labelAuthor.TabIndex = 8;
            this.labelAuthor.Text = "ver 04.10.25";
            // 
            // FormWiFi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(343, 128);
            this.Controls.Add(this.LinkLabelAuthor);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.PictureBoxQr);
            this.Controls.Add(this.Passwd);
            this.Controls.Add(this.SSID);
            this.Controls.Add(this.infoPassword);
            this.Controls.Add(this.infoSSID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWiFi";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WiFi Окошко";
            this.Load += new System.EventHandler(this.FormWiFi_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxQr)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label infoSSID;
        private System.Windows.Forms.Label infoPassword;
        private System.Windows.Forms.RichTextBox SSID;
        private System.Windows.Forms.RichTextBox Passwd;
        private System.Windows.Forms.PictureBox PictureBoxQr;
        private System.Windows.Forms.LinkLabel LinkLabelAuthor;
        private System.Windows.Forms.Label labelAuthor;
    }
}

