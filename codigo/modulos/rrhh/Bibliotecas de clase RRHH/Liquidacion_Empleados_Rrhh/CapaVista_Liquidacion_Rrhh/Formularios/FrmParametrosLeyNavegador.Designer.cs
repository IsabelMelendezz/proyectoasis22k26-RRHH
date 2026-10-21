namespace CapaVista_Liquidacion_Rrhh.Formularios
{
    partial class FrmParametrosLeyNavegador
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmParametrosLeyNavegador));
            this.RrhhNavParemetroLey = new CapaVista_Navegador.Navegador();
            this.SuspendLayout();
            // 
            // RrhhNavParemetroLey
            // 
            this.RrhhNavParemetroLey.Location = new System.Drawing.Point(12, 13);
            this.RrhhNavParemetroLey.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RrhhNavParemetroLey.Name = "RrhhNavParemetroLey";
            this.RrhhNavParemetroLey.Size = new System.Drawing.Size(1078, 90);
            this.RrhhNavParemetroLey.TabIndex = 0;
            // 
            // FrmParametrosLeyNavegador
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 742);
            this.Controls.Add(this.RrhhNavParemetroLey);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "FrmParametrosLeyNavegador";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "9703 - FrmParametrosLeyNavegador";
            this.ResumeLayout(false);

        }

        #endregion

        private CapaVista_Navegador.Navegador RrhhNavParemetroLey;
    }
}