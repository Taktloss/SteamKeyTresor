using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SteamKeyTresor
{
    public partial class frmMain : Form
    {
        private frmAddGame _addGameForm;
        private List<GameKeyItem> _gamesList;
        private string _currentFilename;
        ClipboardMonitor cbm;
        string newKey;

        public frmMain()
        {
            InitializeComponent();

            dgvGames.AutoGenerateColumns = false;
            dgvGames.Columns["colTitle"].DataPropertyName = "Title";
            dgvGames.Columns["colTotalKeys"].DataPropertyName = "TotalKeys";

            cbm = new ClipboardMonitor();
            cbm.NewKey += Cbm_NewKey;
        }

        #region Events

        private void Cbm_NewKey(string txt)
        {
            newKey = txt;
            TresorNotifyIcon.ShowBalloonTip(600, "Steam Key found.", $"Recognized as SteamKey: {newKey}", ToolTipIcon.Info);
            TresorNotifyIcon.BalloonTipClicked += TresorNotifyIcon_BalloonTipClicked;
        }

        private void NewMenuItem_Click(object sender, EventArgs e)
        {
            dgvGames.DataSource = null;
            dgvGames.Rows.Clear();
            _gamesList = new List<GameKeyItem>();

            ReloadGrid();
            EnableButtons();
            lblFileStatus.Text = "New Database created. Don't forget to save.";
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open Tresor file",
                Filter = "(*.tsor) Tresor File|*.tsor",
                DefaultExt = ".tsor",
                AddExtension = true
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _gamesList = new List<GameKeyItem>();
                    string json = File.ReadAllText(openFileDialog.FileName);
                    _gamesList = JsonConvert.DeserializeObject<List<GameKeyItem>>(json);

                    ReloadGrid();
                    EnableButtons();

                    lblFileStatus.Text = $"File loaded: {openFileDialog.SafeFileName}";
                    _currentFilename = openFileDialog.FileName;
                }
            }
        }

        private void SaveMenuItem_Click(object sender, EventArgs e)
        {
            string json = JsonConvert.SerializeObject(_gamesList, Formatting.Indented);
            File.WriteAllText(_currentFilename, json);
            lblFileStatus.Text = $"File saved as: {_currentFilename}";
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Select Tresor Savepath",
                Filter = "(*.tsor) Tresor File|*.tsor",
                DefaultExt = ".tsor",
                AddExtension = true                
            })
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string json = JsonConvert.SerializeObject(_gamesList, Formatting.Indented);
                    File.WriteAllText(saveFileDialog.FileName, json);
                    lblFileStatus.Text = $"File saved as: {saveFileDialog.FileName}";
                    _currentFilename = saveFileDialog.FileName;
                }
            }
        }

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AddMenuItem_Click(object sender, EventArgs e)
        {
            AddNewGame();
        }

        private void AddGameForm_OnDataAvailable(object sender, EventArgs e)
        {
            AddedGamesEventArgs args = (AddedGamesEventArgs)e;
            if (args.IsNew)
                _gamesList.Add(args.Result);
        }

        private void RemoveMenuItem_Click(object sender, EventArgs e)
        {
            RemoveGame();
        }

        private void dgvGames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                RemoveGame();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                EditGame();
            }

        }

        private void ShowMenuItem_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                WindowState = FormWindowState.Minimized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void TresorNotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            if (_gamesList != null && dgvGames.CurrentRow != null)
            {
                
                if (!_gamesList[dgvGames.CurrentRow.Index].KeysList.Contains(newKey))
                {
                    this.WindowState = FormWindowState.Normal;
                    AddNewGame(newKey);
                    /*_gamesList[dgvGames.CurrentRow.Index].KeysList.Add(newKey);
                    ReloadGrid();
                    this.WindowState = FormWindowState.Normal;*/
                }
            }
        }

        private void dgvGames_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            EditGame();
        }

        #endregion

        private void AddNewGame(string key = "")
        {
            _addGameForm = new frmAddGame(key);
            _addGameForm.OnDataAvailable += AddGameForm_OnDataAvailable;
            _addGameForm.ShowDialog();

            ReloadGrid();
        }

        private void EditGame()
        {
            if (dgvGames.RowCount > 0 && dgvGames.CurrentRow.Index >= 0)
            {
                _addGameForm = new frmAddGame(_gamesList[dgvGames.CurrentRow.Index]);
                _addGameForm.OnDataAvailable += AddGameForm_OnDataAvailable;
                _addGameForm.ShowDialog();
                ReloadGrid();
            }
        }

        private void RemoveGame()
        {
            _gamesList.RemoveAt(dgvGames.CurrentRow.Index);
            ReloadGrid();
        }

        private void ReloadGrid()
        {
            var data = from row in _gamesList select new { Title = row.Title, TotalKeys = row.KeysList.Count };
            dgvGames.DataSource = data.ToList();//.ToArray();
        }

        private void EnableButtons()
        {
            SaveMenuItem.Enabled = true;
            SaveAsMenuItem.Enabled = true;
            AddMenuItem.Enabled = true;
            RemoveMenuItem.Enabled = true;
        }
    }
}