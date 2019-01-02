using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SteamKeyTresor
{
    public partial class frmAddGame : Form
    {
        //Event that fires when data is available
        public event EventHandler OnDataAvailable;
        //Properties 
        GameKeyItem _gameKeyItem { get; set; }
        public bool isNew;

        #region Constructor

        public frmAddGame(string key)
        {
            InitializeComponent();
            isNew = true;
            
                txtKey.Text = key;
            if (key.Length > 0)
                AddKey(key);
        }

        public frmAddGame(GameKeyItem gameKeyItem)
        {
            InitializeComponent();

            isNew = false;
            this._gameKeyItem = gameKeyItem;
            txtName.Text = gameKeyItem.Title;

            var result = from row in gameKeyItem.KeysList select new { Key = row };
            dgvKeys.DataSource = result.ToArray();
        }

        #endregion

        #region Events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddKey(txtKey.Text);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            RemoveKey();
        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            if (txtName.Text != string.Empty)
            {
                this._gameKeyItem.Title = txtName.Text;
                AddedGamesEventArgs myCustom = new AddedGamesEventArgs(_gameKeyItem, isNew);

                if (OnDataAvailable != null)
                    OnDataAvailable(this, myCustom);

                Close();
            }
            else
                MessageBox.Show("Please enter a Name for the game");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                RemoveKey();
            }
        }
        #endregion

        private void ReloadKeyGrid()
        {
            var result = from row in _gameKeyItem.KeysList select new { Key = row };
            dgvKeys.DataSource = result.ToArray();
        }

        private void AddKey(string key)
        {
            //Add new Key
            string steamkeyPattern = "([a-zA-Z0-9]{5}-[a-zA-Z0-9]{5}-[a-zA-Z0-9]{5})";
            if (key != string.Empty && Regex.IsMatch(txtKey.Text, steamkeyPattern))
            {
                if (_gameKeyItem == null && isNew)
                {
                    _gameKeyItem = new GameKeyItem
                    {
                        KeysList = new List<string>()
                    };
                }
                _gameKeyItem.KeysList.Add(txtKey.Text);
                ReloadKeyGrid();
            }
            else
                MessageBox.Show("No proper Key entered");
        }

        private void RemoveKey()
        {
            this._gameKeyItem.KeysList.RemoveAt(dgvKeys.CurrentRow.Index);
            ReloadKeyGrid();
        }

        
    }

    // Define class to hold custom event info
    public class AddedGamesEventArgs : EventArgs
    {
        GameKeyItem _result;       
        bool _isNew;

        public AddedGamesEventArgs(GameKeyItem gamekey, bool isNew)
        {
            _result = gamekey;
            _isNew = isNew;
        }
        
        public bool IsNew { get => _isNew; set => _isNew = value; }
        public GameKeyItem Result { get => _result; set => _result = value; }
    }

}