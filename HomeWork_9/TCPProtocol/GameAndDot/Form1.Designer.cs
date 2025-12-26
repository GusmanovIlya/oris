namespace GameAndDot
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            label2 = new Label();
            usernameLbl = new Label();
            label4 = new Label();
            colorLbl = new Label();
            listBox1 = new ListBox();
            gameField = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)gameField).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(286, 158);
            label1.Name = "label1";
            label1.Size = new Size(178, 28);
            label1.TabIndex = 0;
            label1.Text = "Введите username:";
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 12F);
            textBox1.Location = new Point(306, 189);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(125, 34);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Font = new Font("Segoe UI", 12F);
            button1.Location = new Point(320, 229);
            button1.Name = "button1";
            button1.Size = new Size(94, 37);
            button1.TabIndex = 2;
            button1.Text = "Войти";
            button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(500, 51);
            label2.Name = "label2";
            label2.Size = new Size(103, 28);
            label2.TabIndex = 3;
            label2.Text = "Username:";
            label2.Visible = false;
            // 
            // usernameLbl
            // 
            usernameLbl.AutoSize = true;
            usernameLbl.Font = new Font("Segoe UI", 12F);
            usernameLbl.Location = new Point(628, 51);
            usernameLbl.Name = "usernameLbl";
            usernameLbl.Size = new Size(54, 28);
            usernameLbl.TabIndex = 4;
            usernameLbl.Text = "label";
            usernameLbl.Visible = false;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F);
            label4.Location = new Point(500, 90);
            label4.Name = "label4";
            label4.Size = new Size(64, 28);
            label4.TabIndex = 5;
            label4.Text = "Color:";
            label4.Visible = false;
            // 
            // colorLbl
            // 
            colorLbl.AutoSize = true;
            colorLbl.Font = new Font("Segoe UI", 12F);
            colorLbl.Location = new Point(628, 90);
            colorLbl.Name = "colorLbl";
            colorLbl.Size = new Size(54, 28);
            colorLbl.TabIndex = 6;
            colorLbl.Text = "label";
            colorLbl.Visible = false;
            // 
            // listBox1
            // 
            listBox1.Font = new Font("Segoe UI", 12F);
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 28;
            listBox1.Location = new Point(500, 158);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(262, 256);
            listBox1.TabIndex = 7;
            listBox1.Visible = false;
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.DrawItem += ListBox1_DrawItem;
            // 
            // gameField
            // 
            gameField.BackColor = SystemColors.Window;
            gameField.Location = new Point(35, 56);
            gameField.Name = "gameField";
            gameField.Size = new Size(429, 358);
            gameField.TabIndex = 8;
            gameField.TabStop = false;
            gameField.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(gameField);
            Controls.Add(listBox1);
            Controls.Add(colorLbl);
            Controls.Add(label4);
            Controls.Add(usernameLbl);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)gameField).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private Label label2;
        public Label usernameLbl;
        private Label label4;
        public Label colorLbl;
        public ListBox listBox1;
        public PictureBox gameField;
    }
}
