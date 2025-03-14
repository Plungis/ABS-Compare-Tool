using System;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ABSProject
{
    public class SettingsForm : BaseForm
    {
        private DataGridView dataGridViewSettings;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnSetApiKey;
        private Button btnOK;
        private Button btnCancel;

        public SettingsForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Library Settings";
            this.ClientSize = new System.Drawing.Size(800, 450);
            dataGridViewSettings = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 350,
                AutoGenerateColumns = true,
                DataSource = SettingsManager.Libraries,
                AllowUserToAddRows = false
            };
            DataGridViewButtonColumn testButtonCol = new DataGridViewButtonColumn
            {
                Name = "TestConnection",
                HeaderText = "Test Connection",
                Text = "Test",
                UseColumnTextForButtonValue = true
            };
            dataGridViewSettings.Columns.Add(testButtonCol);
            dataGridViewSettings.CellContentClick += DataGridViewSettings_CellContentClick;

            btnAdd = new Button { Text = "Add Library", Left = 10, Top = 360, Width = 120 };
            btnRemove = new Button { Text = "Remove Library", Left = 140, Top = 360, Width = 140 };
            btnSetApiKey = new Button { Text = "Set API Key", Left = 290, Top = 360, Width = 120 };
            btnOK = new Button { Text = "OK", Left = 480, Top = 400, Width = 80, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Left = 570, Top = 400, Width = 80, DialogResult = DialogResult.Cancel };

            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;
            btnSetApiKey.Click += BtnSetApiKey_Click;

            this.contentPanel.Controls.Add(dataGridViewSettings);
            this.contentPanel.Controls.Add(btnAdd);
            this.contentPanel.Controls.Add(btnRemove);
            this.contentPanel.Controls.Add(btnSetApiKey);
            this.contentPanel.Controls.Add(btnOK);
            this.contentPanel.Controls.Add(btnCancel);
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (SettingsManager.Libraries.Count >= 10)
            {
                MessageBox.Show("Maximum of 10 libraries reached.");
                return;
            }
            SettingsManager.Libraries.Add(new LibrarySettings
            {
                Name = "New Library",
                ApiUrl = "https://bookshelf.example.com/api/libraries",
                ApiKey = "",
                LibraryId = ""
            });
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dataGridViewSettings.SelectedRows.Count > 0)
            {
                var item = dataGridViewSettings.SelectedRows[0].DataBoundItem as LibrarySettings;
                if (item != null)
                    SettingsManager.Libraries.Remove(item);
            }
        }

        private void BtnSetApiKey_Click(object sender, EventArgs e)
        {
            string input = ShowInputDialog("Enter API Key:", "Set Default API Key");
            if (!string.IsNullOrEmpty(input))
            {
                foreach (var lib in SettingsManager.Libraries)
                    lib.ApiKey = input;
                dataGridViewSettings.Refresh();
            }
        }

        public static string ShowInputDialog(string text, string caption)
        {
            Form prompt = new Form
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent
            };
            Label textLabel = new Label { Left = 50, Top = 20, Text = text, AutoSize = true };
            TextBox inputBox = new TextBox { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button { Text = "OK", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;
            return (prompt.ShowDialog() == DialogResult.OK) ? inputBox.Text : "";
        }

        private async void DataGridViewSettings_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewSettings.Columns[e.ColumnIndex].Name == "TestConnection" && e.RowIndex >= 0)
            {
                var lib = dataGridViewSettings.Rows[e.RowIndex].DataBoundItem as LibrarySettings;
                if (lib != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + lib.ApiKey);
                            client.DefaultRequestHeaders.Add("Accept", "application/json");
                            string endpoint = $"{lib.ApiUrl}/{lib.LibraryId}/items?sort=media.metadata.title&collapseseries=1";
                            HttpResponseMessage response = await client.GetAsync(endpoint);
                            if (response.IsSuccessStatusCode)
                                MessageBox.Show("Success: Connection to '" + lib.Name + "' is working.");
                            else
                                MessageBox.Show("Error: " + response.StatusCode);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Exception: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
