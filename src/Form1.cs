using System;
using System.Data;
using System.Text.Json;
using System.Diagnostics;
using System.Windows.Forms;
using System.Data.Common;
using APIRunner.Models;
using APIRunner.Business;
using System.Reflection;

namespace APIRunner
{
    public partial class Form1 : Form
    {
        public Config Config { get; set; }

        public Form1()
        {
            Config = new Config();
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            if (File.Exists("config-update.json"))
            {
                File.Delete("config.json");
                File.Move("config-update.json", "config.json");
            }
            LoadDataFromFile("config.json", true);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            lblVersion.Text = "v. 1.4";
            await Task.Run(() =>
            {
                CheckNewVersion();
                LoadGitInfos();
            });
        }

        private async void DataGridButtons_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (!Config.Apps[e.RowIndex].FileExists)
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        Config.Apps[e.RowIndex].Path = fbd.SelectedPath;
                        ResetEnvironmentButtonTexts(e.RowIndex);
                        ResetGitButtonTexts(e.RowIndex);
                        await Task.Run(() => { LoadGitInfo(e.RowIndex); });
                        JsonFile.UpdateJson("config.json", new Dictionary<string, string>() { { $"Apps.[{e.RowIndex}].Path", Config.Apps[e.RowIndex].Path } });
                    }
                }
                return;
            }

            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnName"].Index)
            {
                OpenVisualStudio(e.RowIndex);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnGitPull"].Index)
            {
                Pull(e.RowIndex);
                if (Config.Apps[e.RowIndex].Environment == EnvironmentEnum.Local)
                    CMD.RunUpdateDatabase(Config.Apps[e.RowIndex].Path);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnLocal"].Index)
            {
                ButtonAction(e.RowIndex, EnvironmentEnum.Local, "btnLocal", Config.Apps[e.RowIndex].Local);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnStage"].Index)
            {
                ButtonAction(e.RowIndex, EnvironmentEnum.Stage, "btnStage", Config.Apps[e.RowIndex].Stage);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnHomolog"].Index)
            {
                ButtonAction(e.RowIndex, EnvironmentEnum.Homolog, "btnHomolog", Config.Apps[e.RowIndex].Homolog);
            }
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvApps.Columns["btnProd"].Index)
            {
                ButtonAction(e.RowIndex, EnvironmentEnum.Prod, "btnProd", Config.Apps[e.RowIndex].Prod);
            }
        }

        private void LoadDataFromFile(string filePath, bool applyCellClick = false)
        {
            dgvApps.Rows.Clear();
            try
            {
                var config = File.ReadAllText(filePath);
                if (config == null)
                {
                    MessageBox.Show("File is empty.");
                    return;
                }

                Config = JsonSerializer.Deserialize<Config>(config);

                txtEmail.Text = Config.Email;

                for (var i = 0; i < Config.Apps.Count; i++)
                {
                    var app = Config.Apps[i];
                    dgvApps.Rows.Add(i, app.Name, "Buscando");
                    ResetEnvironmentButtonTexts(i, true);
                    ResetGitButtonTexts(i);
                }
                // Set properties for the DataGridView
                dgvApps.AllowUserToAddRows = false;
                // Handle the CellClick event
                if (applyCellClick) dgvApps.CellClick += DataGridButtons_CellClick;
                IpReleased(true);
                ResizeFormToDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}");
            }
        }

        private void CheckNewVersion()
        {
            bool enabled;
            string text;
            if (Directory.Exists(Path.Combine("", ".git")))
            {
                var commits = Git.GetCommitsNotPulled("");
                enabled = commits.Any();
                text = commits.Any() ? "Atualizar API Runner" : "API Runner Atualizado";
            }
            else
            {
                enabled = false;
                text = "Erro";
            }
            btnAtualizarVersao.Invoke(new Action(() =>
            {
                btnAtualizarVersao.Enabled = enabled;
                btnAtualizarVersao.Text = text;
            }));
        }

        private void LoadGitInfos()
        {
            for (var i = 0; i < Config.Apps.Count; i++)
            {
                LoadGitInfo(i);
            }
        }
        private void LoadGitInfo(int index)
        {
            if (Config.Apps[index].FileExists)
            {
                Config.Apps[index].CurrentBranch = Git.GetCurrentGitBranch(Config.Apps[index].Path);
                Config.Apps[index].Commits = Git.GetCommitsNotPulled(Config.Apps[index].Path);
                dgvApps.Rows[index].Cells["Branch"].Value = Config.Apps[index].CurrentBranch + $"({Config.Apps[index].Commits.Count})";
                dgvApps.Rows[index].Cells["Branch"].ToolTipText = Config.Apps[index].CurrentBranch + $"({Config.Apps[index].Commits.Count})";
            }
        }

        private void LoadGitCommit(int index)
        {
            if (Config.Apps[index].FileExists)
            {
                Config.Apps[index].Commits = Git.GetCommitsNotPulled(Config.Apps[index].Path);
                dgvApps.Rows[index].Cells["Branch"].Value = Config.Apps[index].CurrentBranch + $"({Config.Apps[index].Commits.Count})";
                dgvApps.Rows[index].Cells["Branch"].ToolTipText = Config.Apps[index].CurrentBranch + $"({Config.Apps[index].Commits.Count})";
            }
        }

        private async void OpenVisualStudio(int index)
        {
            await Task.Run(() =>
            {
                var path = Config.Apps[index].Path;
                for (int i = 0; i < 3; i++)
                {
                    var solutionFile = Directory.GetFiles(path).FirstOrDefault(f => f.Contains(".sln"));
                    if (solutionFile != null)
                    {
                        CMD.OpenVisualStudio(solutionFile);
                        return;
                    }
                    else
                    {
                        path = Directory.GetParent(path).FullName;
                    }
                }
                MessageBox.Show("Não foi possível encontrar a solução do projeto");
            });
        }

        private async void Pull(int index)
        {
            await Task.Run(() =>
            {
                if (Config.Apps[index].FileExists)
                {
                    Git.Pull(Config.Apps[index].Path);
                    LoadGitCommit(index);
                }
            });
        }

        private void AllowSeed(string path, bool allowSeed)
        {
            path = path + "//Startup.cs";
            var seedCommmand = "services.AddHostedService<SeedConfig>();";
            var startupFile = File.ReadAllLines(path);
            var seedLine = string.Empty;
            var seedLineIndex = 0;
            for (int i = 0; i < startupFile.Length; i++)
            {
                if (startupFile[i].Contains(seedCommmand))
                {
                    seedLine = startupFile[i];
                    seedLineIndex = i;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(seedLine))
            {
                var indexCommand = seedLine.IndexOf(seedCommmand);
                var isCommented = seedLine.Substring(0, indexCommand).Contains("//");
                if (isCommented && allowSeed)
                    seedLine = seedLine.Substring(0, indexCommand).Replace("//", "") + seedLine.Substring(indexCommand);
                else if (!isCommented && !allowSeed)
                    seedLine = seedLine.Insert(indexCommand, "//");
                else
                    return;

                startupFile[seedLineIndex] = seedLine;
                File.WriteAllLines(path, startupFile);
            }
        }

        private void ButtonAction(int rowIndex, EnvironmentEnum environment, string buttonName, Dictionary<string, string> properties)
        {
            if ((environment == EnvironmentEnum.Local && Config.Apps[rowIndex].Local == null) ||
                (environment == EnvironmentEnum.Stage && Config.Apps[rowIndex].Stage == null) ||
                (environment == EnvironmentEnum.Homolog && Config.Apps[rowIndex].Homolog == null) ||
                (environment == EnvironmentEnum.Prod && Config.Apps[rowIndex].Prod == null))
                return;

            CMD.StopProcess(Config.Apps[rowIndex].Process);
            if (Config.Apps[rowIndex].Environment == environment)
            {
                Config.Apps[rowIndex].Environment = null;
                Config.Apps[rowIndex].Process = null;

                dgvApps.Rows[rowIndex].Cells[buttonName].Value = "Iniciar";
            }
            else
            {
                Config.Apps[rowIndex].Environment = environment;
                ResetEnvironmentButtonTexts(rowIndex);
                dgvApps.Rows[rowIndex].Cells[buttonName].Value = "Parar";

                AllowSeed(Config.Apps[rowIndex].Path, Config.Apps[rowIndex].Environment == EnvironmentEnum.Local);

                JsonFile.UpdateJson(Config.Apps[rowIndex].Path + "//appsettings.Development.json", properties);
                Config.Apps[rowIndex].Process = CMD.RunDotnet(Config.Apps[rowIndex].Process, Config.Apps[rowIndex].Path);
            }
        }

        private void ResetEnvironmentButtonTexts(int rowIndex, bool canDisable = false)
        {
            var text = Config.Apps[rowIndex].FileExists ? "Iniciar" : "Carregar";
            var toolTiptext = Config.Apps[rowIndex].FileExists ? "Iniciar" : "Selecione a pasta onde estão as arquivos de appsettings";

            dgvApps.Rows[rowIndex].Cells["btnLocal"].Value = text;
            dgvApps.Rows[rowIndex].Cells["btnStage"].Value = text;
            dgvApps.Rows[rowIndex].Cells["btnHomolog"].Value = text;
            dgvApps.Rows[rowIndex].Cells["btnProd"].Value = text;

            dgvApps.Rows[rowIndex].Cells["btnLocal"].ToolTipText = toolTiptext;
            dgvApps.Rows[rowIndex].Cells["btnStage"].ToolTipText = toolTiptext;
            dgvApps.Rows[rowIndex].Cells["btnHomolog"].ToolTipText = toolTiptext;
            dgvApps.Rows[rowIndex].Cells["btnProd"].ToolTipText = toolTiptext;

            if (canDisable && Config.Apps[rowIndex].FileExists)
            {
                text = "Not Found";
                toolTiptext = "Configuração do ambiente não encontrada";
                if (Config.Apps[rowIndex].Local == null)
                {
                    DisableButton(dgvApps.Rows[rowIndex].Cells["btnLocal"]);
                    dgvApps.Rows[rowIndex].Cells["btnLocal"].Value = text;
                    dgvApps.Rows[rowIndex].Cells["btnLocal"].ToolTipText = toolTiptext;
                }
                if (Config.Apps[rowIndex].Stage == null)
                {
                    DisableButton(dgvApps.Rows[rowIndex].Cells["btnStage"]);
                    dgvApps.Rows[rowIndex].Cells["btnStage"].Value = text;
                    dgvApps.Rows[rowIndex].Cells["btnStage"].ToolTipText = toolTiptext;
                }
                if (Config.Apps[rowIndex].Homolog == null)
                {
                    DisableButton(dgvApps.Rows[rowIndex].Cells["btnHomolog"]);
                    dgvApps.Rows[rowIndex].Cells["btnHomolog"].Value = text;
                    dgvApps.Rows[rowIndex].Cells["btnHomolog"].ToolTipText = toolTiptext;
                }
                if (Config.Apps[rowIndex].Prod == null)
                {
                    DisableButton(dgvApps.Rows[rowIndex].Cells["btnProd"]);
                    dgvApps.Rows[rowIndex].Cells["btnProd"].Value = text;
                    dgvApps.Rows[rowIndex].Cells["btnProd"].ToolTipText = toolTiptext;
                }
            }
        }

        private void DisableButton(DataGridViewCell cell)
        {
            DataGridViewButtonCell buttonCell = (DataGridViewButtonCell)cell;
            buttonCell.FlatStyle = FlatStyle.Popup; // Changes appearance to indicate disabled state
            buttonCell.ReadOnly = true; // Makes the button non-clickable
            buttonCell.Style.ForeColor = Color.Gray; // Optional: change text color
        }

        private void ResetGitButtonTexts(int rowIndex)
        {
            dgvApps.Rows[rowIndex].Cells["btnGitPull"].Value = Config.Apps[rowIndex].FileExists ? "Pull" : "Carregar";
            dgvApps.Rows[rowIndex].Cells["btnGitPull"].ToolTipText = Config.Apps[rowIndex].FileExists ? "Pull" : "Carregar";
        }

        private async void IpReleased(bool tryRelease = false)
        {
            if (tryRelease)
            {
                btnUpdateIP.Enabled = false;
                btnUpdateIP.Text = "Verificando";
            }
            await Task.Run(() =>
            {
                var connectionString = Config.Apps.SelectMany(a => a.Stage).FirstOrDefault(s => s.Key.Contains("ConnectionString"));
                if (!string.IsNullOrEmpty(connectionString.Key))
                {
                    if (Database.VerifyConnection(connectionString.Value))
                    {
                        btnUpdateIP.Invoke(new Action(() =>
                        {
                            btnUpdateIP.Enabled = false;
                            btnUpdateIP.Text = "IP liberado";
                        }));
                    }
                    else if (tryRelease)
                    {
                        btnUpdateIP.Invoke(new Action(() =>
                        {
                            btnUpdateIP.Enabled = false;
                            btnUpdateIP.Text = "Liberando IP";
                        }));
                        ReleaseIP();
                    }
                    else
                    {
                        btnUpdateIP.Invoke(new Action(() =>
                        {
                            btnUpdateIP.Enabled = true;
                            btnUpdateIP.Text = "Liberar IP";
                        }));
                    }
                }
            });
        }

        private async void btnUpdateIP_Click(object sender, EventArgs e)
        {
            btnUpdateIP.Enabled = false;
            btnUpdateIP.Text = "Verificando";
            ReleaseIP();
        }

        private async void ReleaseIP()
        {
            try
            {
                if (!string.IsNullOrEmpty(Config.Email))
                    await OAuth2.UpdateIp(Config.Email);
            }
            finally
            {
                IpReleased();
            }
        }

        private void btnEditEmail_Click(object sender, EventArgs e)
        {
            if (Config.Email != txtEmail.Text)
            {
                JsonFile.UpdateJson("config.json", new Dictionary<string, string>() { { $"Email", txtEmail.Text } });
                Config.Email = txtEmail.Text;
            }
        }

        private async void btnRecarregar_Click(object sender, EventArgs e)
        {
            IpReleased(true);
            for (var i = 0; i < Config.Apps.Count; i++)
            {
                var app = Config.Apps[i];
                dgvApps.Rows[i].Cells[2].Value = "Buscando";
                ResetGitButtonTexts(i);
            }
            await Task.Run(() =>
            {
                LoadGitInfos();
                CheckNewVersion();
            });
        }

        private async void btnAtualizarVersao_Click(object sender, EventArgs e)
        {
            btnAtualizarVersao.Text = "Atualizando";
            await Task.Delay(1000);
            File.Delete("update_version_run.bat");
            File.Move("update_version.bat", "update_version_run.bat");
            File.Move("config.json", "config-update.json");
            CMD.RunBatFile(Directory.GetCurrentDirectory(), "update_version_run.bat");
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (var i = 0; i < Config.Apps.Count; i++)
            {
                CMD.StopProcess(Config.Apps[i].Process);
            }
        }

        private void ResizeFormToDataGridView()
        {
            // Calcula a largura total do DataGridView (largura das colunas mais as bordas)
            int totalWidth = dgvApps.RowHeadersWidth; // Largura do cabeçalho de linha

            // Soma a largura de cada coluna
            foreach (DataGridViewColumn column in dgvApps.Columns)
            {
                if (column.Name != "Id")
                    totalWidth += column.Width;
            }

            // Calcula a altura total do DataGridView (altura das linhas mais as bordas)
            int totalHeight = dgvApps.ColumnHeadersHeight; // Altura do cabeçalho de coluna

            // Soma a altura de cada linha
            foreach (DataGridViewRow row in dgvApps.Rows)
            {
                totalHeight += row.Height;
            }

            // Define a largura e altura mínima para evitar o tamanho muito pequeno
            int minWidth = 200;
            int minHeight = 150;

            dgvApps.Size = new Size(totalWidth, totalHeight);
            // Ajusta a altura e largura da janela de acordo com o tamanho do DataGridView
            this.ClientSize = new Size(
                Math.Max(totalWidth, minWidth) + 25,
                Math.Max(totalHeight, minHeight) + groupBoxHeader.Height + 25
            );

            // Calcula a posição X para mover o label para o limite direito do formulário
            int novaPosicaoX = this.ClientSize.Width - lblVersion.Width -10;

            // Define a nova posição mantendo a posição Y atual
            lblVersion.Location = new Point(novaPosicaoX, lblVersion.Location.Y);

            // Desabilita as barras de rolagem
            dgvApps.ScrollBars = ScrollBars.None;
        }

    }
}
