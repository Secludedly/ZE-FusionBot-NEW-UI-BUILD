using PKHeX.Drawing.PokeSprite.Properties;
using SysBot.Pokemon.Helpers;
using SysBot.Pokemon.WinForms.Properties;
using System.Drawing;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms
{

    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>


        private FontAwesome.Sharp.IconButton btnBots;
        private FontAwesome.Sharp.IconButton btnHub;
        private FontAwesome.Sharp.IconButton btnLogs;

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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            panelLeftSide = new Panel();
            iconButton3 = new FontAwesome.Sharp.IconButton();
            iconButton2 = new FontAwesome.Sharp.IconButton();
            iconButton1 = new FontAwesome.Sharp.IconButton();
            panelImageLogo = new Panel();
            panel6 = new Panel();
            panel5 = new Panel();
            panel3 = new Panel();
            pictureLogo = new PictureBox();
            lblTitle = new Label();
            panel4 = new Panel();
            panelTitleBar = new Panel();
            btnClose = new FontAwesome.Sharp.IconPictureBox();
            btnMaximize = new FontAwesome.Sharp.IconPictureBox();
            btnMinimize = new FontAwesome.Sharp.IconPictureBox();
            childFormIcon = new FontAwesome.Sharp.IconPictureBox();
            lblTitleChildForm = new Label();
            shadowPanelTop = new Panel();
            shadowPanelLeft = new Panel();
            panelMain = new Panel();
            panel2 = new Panel();
            panel1 = new Panel();
            panelLeftSide.SuspendLayout();
            panelImageLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureLogo).BeginInit();
            panelTitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnClose).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnMaximize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btnMinimize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)childFormIcon).BeginInit();
            panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // panelLeftSide
            // 
            panelLeftSide.BackColor = Color.FromArgb(31, 30, 68);
            panelLeftSide.Controls.Add(iconButton3);
            panelLeftSide.Controls.Add(iconButton2);
            panelLeftSide.Controls.Add(iconButton1);
            panelLeftSide.Controls.Add(panelImageLogo);
            panelLeftSide.Controls.Add(lblTitle);
            panelLeftSide.Dock = DockStyle.Left;
            panelLeftSide.Location = new Point(0, 0);
            panelLeftSide.Name = "panelLeftSide";
            panelLeftSide.Size = new Size(220, 422);
            panelLeftSide.TabIndex = 0;
            // 
            // iconButton3
            // 
            iconButton3.Dock = DockStyle.Top;
            iconButton3.FlatAppearance.BorderSize = 0;
            iconButton3.FlatStyle = FlatStyle.Flat;
            iconButton3.Font = new Font("Ubuntu Mono", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            iconButton3.ForeColor = Color.White;
            iconButton3.IconChar = FontAwesome.Sharp.IconChar.TextHeight;
            iconButton3.IconColor = Color.White;
            iconButton3.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconButton3.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton3.Location = new Point(0, 245);
            iconButton3.Name = "iconButton3";
            iconButton3.Padding = new Padding(10, 0, 20, 0);
            iconButton3.Size = new Size(220, 60);
            iconButton3.TabIndex = 3;
            iconButton3.Text = "Bot Logs";
            iconButton3.TextAlign = ContentAlignment.MiddleLeft;
            iconButton3.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton3.UseVisualStyleBackColor = true;
            iconButton3.Click += Logs_Click;
            // 
            // iconButton2
            // 
            iconButton2.Dock = DockStyle.Top;
            iconButton2.FlatAppearance.BorderSize = 0;
            iconButton2.FlatStyle = FlatStyle.Flat;
            iconButton2.Font = new Font("Ubuntu Mono", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            iconButton2.ForeColor = Color.White;
            iconButton2.IconChar = FontAwesome.Sharp.IconChar.BarChart;
            iconButton2.IconColor = Color.White;
            iconButton2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconButton2.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton2.Location = new Point(0, 185);
            iconButton2.Name = "iconButton2";
            iconButton2.Padding = new Padding(10, 0, 20, 0);
            iconButton2.Size = new Size(220, 60);
            iconButton2.TabIndex = 2;
            iconButton2.Text = "Bot Hub";
            iconButton2.TextAlign = ContentAlignment.MiddleLeft;
            iconButton2.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton2.UseVisualStyleBackColor = true;
            iconButton2.Click += Hub_Click;
            // 
            // iconButton1
            // 
            iconButton1.Dock = DockStyle.Top;
            iconButton1.FlatAppearance.BorderSize = 0;
            iconButton1.FlatStyle = FlatStyle.Flat;
            iconButton1.Font = new Font("Ubuntu Mono", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            iconButton1.ForeColor = Color.White;
            iconButton1.IconChar = FontAwesome.Sharp.IconChar.AngleRight;
            iconButton1.IconColor = Color.White;
            iconButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconButton1.ImageAlign = ContentAlignment.MiddleLeft;
            iconButton1.Location = new Point(0, 125);
            iconButton1.Name = "iconButton1";
            iconButton1.Padding = new Padding(10, 0, 20, 0);
            iconButton1.Size = new Size(220, 60);
            iconButton1.TabIndex = 1;
            iconButton1.Text = "Bot Controls";
            iconButton1.TextAlign = ContentAlignment.MiddleLeft;
            iconButton1.TextImageRelation = TextImageRelation.ImageBeforeText;
            iconButton1.UseVisualStyleBackColor = true;
            iconButton1.Click += Bots_Click;
            // 
            // panelImageLogo
            // 
            panelImageLogo.BackColor = Color.Transparent;
            panelImageLogo.Controls.Add(panel6);
            panelImageLogo.Controls.Add(panel5);
            panelImageLogo.Controls.Add(panel3);
            panelImageLogo.Controls.Add(pictureLogo);
            panelImageLogo.Cursor = Cursors.Hand;
            panelImageLogo.Dock = DockStyle.Top;
            panelImageLogo.Location = new Point(0, 0);
            panelImageLogo.Name = "panelImageLogo";
            panelImageLogo.Size = new Size(220, 125);
            panelImageLogo.TabIndex = 0;
            // 
            // panel6
            // 
            panel6.BackColor = Color.FromArgb(20, 19, 57);
            panel6.Dock = DockStyle.Left;
            panel6.Location = new Point(0, 12);
            panel6.Name = "panel6";
            panel6.Size = new Size(12, 101);
            panel6.TabIndex = 5;
            // 
            // panel5
            // 
            panel5.BackColor = Color.FromArgb(20, 19, 57);
            panel5.Dock = DockStyle.Top;
            panel5.Location = new Point(0, 0);
            panel5.Name = "panel5";
            panel5.Size = new Size(220, 12);
            panel5.TabIndex = 4;
            // 
            // panel3
            // 
            panel3.BackColor = Color.FromArgb(20, 19, 57);
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 113);
            panel3.Name = "panel3";
            panel3.Size = new Size(220, 12);
            panel3.TabIndex = 3;
            // 
            // pictureLogo
            // 
            pictureLogo.BackColor = Color.Transparent;
            pictureLogo.BackgroundImageLayout = ImageLayout.Stretch;
            pictureLogo.Image = (Image)resources.GetObject("pictureLogo.Image");
            pictureLogo.Location = new Point(-4, 0);
            pictureLogo.Name = "pictureLogo";
            pictureLogo.Size = new Size(224, 205);
            pictureLogo.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureLogo.TabIndex = 0;
            pictureLogo.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Bahnschrift", 7.20000029F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.Gainsboro;
            lblTitle.Location = new Point(-1, 390);
            lblTitle.Margin = new Padding(0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(220, 32);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "ZE FusionBot | v0.0.0 | MODE: XXXX";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel4
            // 
            panel4.BackColor = Color.FromArgb(20, 19, 57);
            panel4.Dock = DockStyle.Left;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(12, 58);
            panel4.TabIndex = 4;
            // 
            // panelTitleBar
            // 
            panelTitleBar.BackColor = Color.FromArgb(31, 30, 68);
            panelTitleBar.Controls.Add(btnClose);
            panelTitleBar.Controls.Add(btnMaximize);
            panelTitleBar.Controls.Add(panel4);
            panelTitleBar.Controls.Add(btnMinimize);
            panelTitleBar.Controls.Add(childFormIcon);
            panelTitleBar.Controls.Add(lblTitleChildForm);
            panelTitleBar.Dock = DockStyle.Top;
            panelTitleBar.Location = new Point(220, 0);
            panelTitleBar.Name = "panelTitleBar";
            panelTitleBar.Size = new Size(769, 58);
            panelTitleBar.TabIndex = 1;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.BackColor = Color.FromArgb(31, 30, 68);
            btnClose.ForeColor = Color.RosyBrown;
            btnClose.IconChar = FontAwesome.Sharp.IconChar.Close;
            btnClose.IconColor = Color.RosyBrown;
            btnClose.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnClose.IconSize = 20;
            btnClose.Location = new Point(745, 6);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(20, 22);
            btnClose.TabIndex = 4;
            btnClose.TabStop = false;
            // 
            // btnMaximize
            // 
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.BackColor = Color.FromArgb(31, 30, 68);
            btnMaximize.IconChar = FontAwesome.Sharp.IconChar.WindowMaximize;
            btnMaximize.IconColor = Color.White;
            btnMaximize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnMaximize.IconSize = 20;
            btnMaximize.Location = new Point(724, 6);
            btnMaximize.Name = "btnMaximize";
            btnMaximize.Size = new Size(20, 22);
            btnMaximize.TabIndex = 3;
            btnMaximize.TabStop = false;
            // 
            // btnMinimize
            // 
            btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimize.BackColor = Color.FromArgb(31, 30, 68);
            btnMinimize.IconChar = FontAwesome.Sharp.IconChar.WindowMinimize;
            btnMinimize.IconColor = Color.White;
            btnMinimize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnMinimize.IconSize = 20;
            btnMinimize.Location = new Point(703, 6);
            btnMinimize.Name = "btnMinimize";
            btnMinimize.Size = new Size(20, 22);
            btnMinimize.TabIndex = 2;
            btnMinimize.TabStop = false;
            // 
            // childFormIcon
            // 
            childFormIcon.BackColor = Color.FromArgb(31, 30, 68);
            childFormIcon.ForeColor = Color.Thistle;
            childFormIcon.IconChar = FontAwesome.Sharp.IconChar.House;
            childFormIcon.IconColor = Color.Thistle;
            childFormIcon.IconFont = FontAwesome.Sharp.IconFont.Auto;
            childFormIcon.IconSize = 40;
            childFormIcon.Location = new Point(12, 12);
            childFormIcon.Name = "childFormIcon";
            childFormIcon.Size = new Size(40, 40);
            childFormIcon.TabIndex = 1;
            childFormIcon.TabStop = false;
            // 
            // lblTitleChildForm
            // 
            lblTitleChildForm.AutoSize = true;
            lblTitleChildForm.Font = new Font("bubbleboddy light", 22.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblTitleChildForm.ForeColor = Color.White;
            lblTitleChildForm.Location = new Point(58, 12);
            lblTitleChildForm.Name = "lblTitleChildForm";
            lblTitleChildForm.Size = new Size(172, 38);
            lblTitleChildForm.TabIndex = 0;
            lblTitleChildForm.Text = "LOADING ...";
            // 
            // shadowPanelTop
            // 
            shadowPanelTop.BackColor = Color.FromArgb(20, 19, 57);
            shadowPanelTop.Dock = DockStyle.Top;
            shadowPanelTop.Location = new Point(220, 58);
            shadowPanelTop.Name = "shadowPanelTop";
            shadowPanelTop.Size = new Size(769, 12);
            shadowPanelTop.TabIndex = 2;
            // 
            // shadowPanelLeft
            // 
            shadowPanelLeft.BackColor = Color.FromArgb(20, 19, 57);
            shadowPanelLeft.Dock = DockStyle.Left;
            shadowPanelLeft.Location = new Point(220, 70);
            shadowPanelLeft.Name = "shadowPanelLeft";
            shadowPanelLeft.Size = new Size(12, 352);
            shadowPanelLeft.TabIndex = 3;
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(10, 10, 40);
            panelMain.Controls.Add(panel2);
            panelMain.Controls.Add(panel1);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(232, 70);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(757, 352);
            panelMain.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(20, 19, 57);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 340);
            panel2.Name = "panel2";
            panel2.Size = new Size(745, 12);
            panel2.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(20, 19, 57);
            panel1.Dock = DockStyle.Right;
            panel1.Location = new Point(745, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(12, 352);
            panel1.TabIndex = 4;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(989, 422);
            Controls.Add(panelMain);
            Controls.Add(shadowPanelLeft);
            Controls.Add(shadowPanelTop);
            Controls.Add(panelTitleBar);
            Controls.Add(panelLeftSide);
            Icon = Properties.Resources.icon;
            Margin = new Padding(5, 4, 5, 4);
            Name = "Main";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ZE FusionBot";
            panelLeftSide.ResumeLayout(false);
            panelImageLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureLogo).EndInit();
            panelTitleBar.ResumeLayout(false);
            panelTitleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)btnClose).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnMaximize).EndInit();
            ((System.ComponentModel.ISupportInitialize)btnMinimize).EndInit();
            ((System.ComponentModel.ISupportInitialize)childFormIcon).EndInit();
            panelMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelLeftSide;
        private FontAwesome.Sharp.IconButton iconButton1;
        private Panel panelImageLogo;
        private FontAwesome.Sharp.IconButton iconButton3;
        private FontAwesome.Sharp.IconButton iconButton2;
        private PictureBox pictureLogo;
        private Panel panelTitleBar;
        private Label lblTitleChildForm;
        private FontAwesome.Sharp.IconPictureBox childFormIcon;
        private FontAwesome.Sharp.IconPictureBox btnMaximize;
        private FontAwesome.Sharp.IconPictureBox btnMinimize;
        private FontAwesome.Sharp.IconPictureBox btnClose;
        private Panel shadowPanelTop;
        private Panel shadowPanelLeft;
        private Panel panelMain;
        private Label lblTitle;
        private Panel panel2;
        private Panel panel1;
        private Panel panel4;
        private Panel panel3;
        private Panel panel6;
        private Panel panel5;
    }
}

