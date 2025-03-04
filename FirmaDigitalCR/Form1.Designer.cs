namespace FirmaDigitalCR
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
            txtDocumento = new TextBox();
            btnSeleccionar = new Button();
            btnFirmar = new Button();
            openFileDialog1 = new OpenFileDialog();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(110, 44);
            label1.Name = "label1";
            label1.Size = new Size(187, 15);
            label1.TabIndex = 0;
            label1.Text = "Selecciona el documento a firmar:";
            // 
            // txtDocumento
            // 
            txtDocumento.Location = new Point(295, 43);
            txtDocumento.Name = "txtDocumento";
            txtDocumento.Size = new Size(361, 23);
            txtDocumento.TabIndex = 1;
            // 
            // btnSeleccionar
            // 
            btnSeleccionar.Location = new Point(663, 48);
            btnSeleccionar.Name = "btnSeleccionar";
            btnSeleccionar.Size = new Size(75, 23);
            btnSeleccionar.TabIndex = 2;
            btnSeleccionar.Text = "Seleccionar";
            btnSeleccionar.UseVisualStyleBackColor = true;
            btnSeleccionar.Click += btnSeleccionar_Click;
            // 
            // btnFirmar
            // 
            btnFirmar.Location = new Point(119, 78);
            btnFirmar.Name = "btnFirmar";
            btnFirmar.Size = new Size(75, 23);
            btnFirmar.TabIndex = 3;
            btnFirmar.Text = "Firmar";
            btnFirmar.UseVisualStyleBackColor = true;
            btnFirmar.Click += btnFirmar_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnFirmar);
            Controls.Add(btnSeleccionar);
            Controls.Add(txtDocumento);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtDocumento;
        private Button btnSeleccionar;
        private Button btnFirmar;
        private OpenFileDialog openFileDialog1;
    }
}
