namespace APIRunner
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            dgvApps = new DataGridView();
            Id = new DataGridViewTextBoxColumn();
            btnName = new DataGridViewButtonColumn();
            Branch = new DataGridViewTextBoxColumn();
            btnGitPull = new DataGridViewButtonColumn();
            btnLocal = new DataGridViewButtonColumn();
            btnStage = new DataGridViewButtonColumn();
            btnHomolog = new DataGridViewButtonColumn();
            btnProd = new DataGridViewButtonColumn();
            apiRunner = new Label();
            label1 = new Label();
            btnUpdateIP = new Button();
            btnEditEmail = new Button();
            txtEmail = new TextBox();
            btnRecarregar = new Button();
            btnAtualizarVersao = new Button();
            lblVersion = new Label();
            groupBoxHeader = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dgvApps).BeginInit();
            SuspendLayout();
            // 
            // dgvApps
            // 
            dgvApps.AllowUserToAddRows = false;
            dgvApps.AllowUserToDeleteRows = false;
            dgvApps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvApps.Columns.AddRange(new DataGridViewColumn[] { Id, btnName, Branch, btnGitPull, btnLocal, btnStage, btnHomolog, btnProd });
            dgvApps.Location = new Point(14, 148);
            dgvApps.Name = "dgvApps";
            dgvApps.ReadOnly = true;
            dgvApps.RowHeadersWidth = 51;
            dgvApps.Size = new Size(931, 379);
            dgvApps.TabIndex = 0;
            // 
            // Id
            // 
            Id.HeaderText = "Id";
            Id.MinimumWidth = 6;
            Id.Name = "Id";
            Id.ReadOnly = true;
            Id.Visible = false;
            Id.Width = 125;
            // 
            // btnName
            // 
            btnName.HeaderText = "Nome";
            btnName.MinimumWidth = 6;
            btnName.Name = "btnName";
            btnName.ReadOnly = true;
            btnName.Resizable = DataGridViewTriState.True;
            btnName.SortMode = DataGridViewColumnSortMode.Automatic;
            btnName.Width = 125;
            // 
            // Branch
            // 
            Branch.HeaderText = "Branch";
            Branch.MinimumWidth = 6;
            Branch.Name = "Branch";
            Branch.ReadOnly = true;
            Branch.Width = 125;
            // 
            // btnGitPull
            // 
            btnGitPull.HeaderText = "Git Pull";
            btnGitPull.MinimumWidth = 6;
            btnGitPull.Name = "btnGitPull";
            btnGitPull.ReadOnly = true;
            btnGitPull.Width = 125;
            // 
            // btnLocal
            // 
            btnLocal.HeaderText = "Local";
            btnLocal.MinimumWidth = 6;
            btnLocal.Name = "btnLocal";
            btnLocal.ReadOnly = true;
            btnLocal.Text = "Iniciar";
            btnLocal.Width = 125;
            // 
            // btnStage
            // 
            btnStage.HeaderText = "Stage";
            btnStage.MinimumWidth = 6;
            btnStage.Name = "btnStage";
            btnStage.ReadOnly = true;
            btnStage.Resizable = DataGridViewTriState.True;
            btnStage.SortMode = DataGridViewColumnSortMode.Automatic;
            btnStage.Text = "Iniciar";
            btnStage.Width = 125;
            // 
            // btnHomolog
            // 
            btnHomolog.HeaderText = "Homolog";
            btnHomolog.MinimumWidth = 6;
            btnHomolog.Name = "btnHomolog";
            btnHomolog.ReadOnly = true;
            btnHomolog.Text = "Iniciar";
            btnHomolog.Width = 125;
            // 
            // btnProd
            // 
            btnProd.HeaderText = "Prod";
            btnProd.MinimumWidth = 6;
            btnProd.Name = "btnProd";
            btnProd.ReadOnly = true;
            btnProd.Text = "Iniciar";
            btnProd.Width = 125;
            // 
            // apiRunner
            // 
            apiRunner.AutoSize = true;
            apiRunner.Font = new Font("Segoe UI", 22F);
            apiRunner.Location = new Point(14, 12);
            apiRunner.Name = "apiRunner";
            apiRunner.Size = new Size(204, 50);
            apiRunner.TabIndex = 1;
            apiRunner.Text = "API Runner";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 67);
            label1.Name = "label1";
            label1.Size = new Size(342, 20);
            label1.TabIndex = 2;
            label1.Text = "Edite o arquivo config.json para configurar as APIs";
            // 
            // btnUpdateIP
            // 
            btnUpdateIP.Location = new Point(840, 112);
            btnUpdateIP.Margin = new Padding(3, 4, 3, 4);
            btnUpdateIP.Name = "btnUpdateIP";
            btnUpdateIP.Size = new Size(105, 31);
            btnUpdateIP.TabIndex = 3;
            btnUpdateIP.Text = "Liberar IP";
            btnUpdateIP.UseVisualStyleBackColor = true;
            btnUpdateIP.Click += btnUpdateIP_Click;
            // 
            // btnEditEmail
            // 
            btnEditEmail.Location = new Point(766, 111);
            btnEditEmail.Margin = new Padding(3, 4, 3, 4);
            btnEditEmail.Name = "btnEditEmail";
            btnEditEmail.Size = new Size(59, 31);
            btnEditEmail.TabIndex = 4;
            btnEditEmail.Text = "Salvar";
            btnEditEmail.UseVisualStyleBackColor = true;
            btnEditEmail.Click += btnEditEmail_Click;
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(431, 112);
            txtEmail.Margin = new Padding(3, 4, 3, 4);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(327, 27);
            txtEmail.TabIndex = 5;
            // 
            // btnRecarregar
            // 
            btnRecarregar.Location = new Point(14, 111);
            btnRecarregar.Margin = new Padding(3, 4, 3, 4);
            btnRecarregar.Name = "btnRecarregar";
            btnRecarregar.Size = new Size(121, 31);
            btnRecarregar.TabIndex = 6;
            btnRecarregar.Text = "Recarregar dados";
            btnRecarregar.UseVisualStyleBackColor = true;
            btnRecarregar.Click += btnRecarregar_Click;
            // 
            // btnAtualizarVersao
            // 
            btnAtualizarVersao.Enabled = false;
            btnAtualizarVersao.Location = new Point(173, 112);
            btnAtualizarVersao.Margin = new Padding(3, 4, 3, 4);
            btnAtualizarVersao.Name = "btnAtualizarVersao";
            btnAtualizarVersao.Size = new Size(201, 31);
            btnAtualizarVersao.TabIndex = 7;
            btnAtualizarVersao.Text = "Verificando atualização";
            btnAtualizarVersao.UseVisualStyleBackColor = true;
            btnAtualizarVersao.Click += btnAtualizarVersao_Click;
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI", 12F);
            lblVersion.Location = new Point(888, 9);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(57, 28);
            lblVersion.TabIndex = 8;
            lblVersion.Text = "v. 1.0";
            // 
            // groupBoxHeader
            // 
            groupBoxHeader.Location = new Point(14, 9);
            groupBoxHeader.Name = "groupBoxHeader";
            groupBoxHeader.Size = new Size(931, 134);
            groupBoxHeader.TabIndex = 9;
            groupBoxHeader.TabStop = false;
            groupBoxHeader.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(957, 539);
            Controls.Add(lblVersion);
            Controls.Add(btnAtualizarVersao);
            Controls.Add(btnRecarregar);
            Controls.Add(txtEmail);
            Controls.Add(btnEditEmail);
            Controls.Add(btnUpdateIP);
            Controls.Add(label1);
            Controls.Add(apiRunner);
            Controls.Add(dgvApps);
            Controls.Add(groupBoxHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "API Runner";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvApps).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvApps;
        private Label apiRunner;
        private Label label1;
        private DataGridViewTextBoxColumn Id;
        private DataGridViewButtonColumn btnName;
        private DataGridViewTextBoxColumn Branch;
        private DataGridViewButtonColumn btnGitPull;
        private DataGridViewButtonColumn btnLocal;
        private DataGridViewButtonColumn btnStage;
        private DataGridViewButtonColumn btnHomolog;
        private DataGridViewButtonColumn btnProd;
        private Button btnUpdateIP;
        private Button btnEditEmail;
        private TextBox txtEmail;
        private Button btnRecarregar;
        private Button btnAtualizarVersao;
        private Label lblVersion;
        private GroupBox groupBoxHeader;
    }
}
