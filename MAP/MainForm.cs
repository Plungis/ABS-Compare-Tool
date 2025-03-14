using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ABSProject
{
    public class MainForm : BaseForm
    {
        private SplitContainer splitContainer;
        private DataGridView dgvLibraryA;
        private DataGridView dgvLibraryB;
        private Panel controlPanel; // Panel at top for controls
        private ComboBox cmbLibraryA;
        private ComboBox cmbLibraryB;
        private Button btnSettings;
        private Button btnRefresh;
        private Button btnCompare;
        private Label lblLibA;
        private Label lblLibB;
        private Label lblCountA;
        private Label lblCountB;
        // Border panels wrapping the DataGridViews and individual search bars.
        private Panel panelLibraryA;
        private Panel panelLibraryB;
        private Panel searchPanelA;
        private Panel searchPanelB;
        // Individual search TextBoxes.
        private TextBox txtSearchA;
        private TextBox txtSearchB;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Library Books Comparison";
            this.ClientSize = new Size(1200, 700);

            // --- Control Panel (remains at top) ---
            controlPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
            lblLibA = new Label { Text = "Library A:", AutoSize = true, Left = 10, Top = 15 };
            cmbLibraryA = new ComboBox { Left = 80, Top = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            lblLibB = new Label { Text = "Library B:", AutoSize = true, Left = 240, Top = 15 };
            cmbLibraryB = new ComboBox { Left = 310, Top = 10, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };

            // Populate combo boxes from SettingsManager.
            foreach (var lib in SettingsManager.Libraries)
            {
                cmbLibraryA.Items.Add(lib);
                cmbLibraryB.Items.Add(lib);
            }
            if (cmbLibraryA.Items.Count > 0)
                cmbLibraryA.SelectedIndex = 0;
            // Library B defaults to the second entry if available.
            if (cmbLibraryB.Items.Count > 1)
                cmbLibraryB.SelectedIndex = 1;
            else if (cmbLibraryB.Items.Count > 0)
                cmbLibraryB.SelectedIndex = 0;

            cmbLibraryA.SelectedIndexChanged += async (s, e) =>
            {
                if (cmbLibraryA.SelectedItem is LibrarySettings lib)
                    await LoadBooksForLibrary(lib, dgvLibraryA);
            };
            cmbLibraryB.SelectedIndexChanged += async (s, e) =>
            {
                if (cmbLibraryB.SelectedItem is LibrarySettings lib)
                    await LoadBooksForLibrary(lib, dgvLibraryB);
            };

            btnRefresh = new Button { Text = "Refresh", Left = 480, Top = 10, Width = 90 };
            btnRefresh.Click += async (s, e) => await LoadLibrariesAsync();
            btnCompare = new Button { Text = "Compare Libraries", Left = 580, Top = 10, Width = 120 };
            btnCompare.Click += (s, e) =>
            {
                using (var compForm = new ComparisonForm())
                {
                    compForm.ShowDialog();
                }
            };
            btnSettings = new Button { Text = "Settings", Left = 710, Top = 10, Width = 90 };
            btnSettings.Click += async (s, e) =>
            {
                using (var settingsForm = new SettingsForm())
                {
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        SettingsManager.SaveSettings();
                        cmbLibraryA.Items.Clear();
                        cmbLibraryB.Items.Clear();
                        foreach (var lib in SettingsManager.Libraries)
                        {
                            cmbLibraryA.Items.Add(lib);
                            cmbLibraryB.Items.Add(lib);
                        }
                        if (cmbLibraryA.Items.Count > 0)
                            cmbLibraryA.SelectedIndex = 0;
                        if (cmbLibraryB.Items.Count > 1)
                            cmbLibraryB.SelectedIndex = 1;
                        else if (cmbLibraryB.Items.Count > 0)
                            cmbLibraryB.SelectedIndex = 0;
                        await LoadLibrariesAsync();
                    }
                }
            };

            controlPanel.Controls.Add(lblLibA);
            controlPanel.Controls.Add(cmbLibraryA);
            controlPanel.Controls.Add(lblLibB);
            controlPanel.Controls.Add(cmbLibraryB);
            controlPanel.Controls.Add(btnRefresh);
            controlPanel.Controls.Add(btnCompare);
            controlPanel.Controls.Add(btnSettings);

            // --- SplitContainer to hold the two libraries side-by-side ---
            splitContainer = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };

            // --- Left Panel: Library A ---
            panelLibraryA = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            // Add an individual search bar for Library A.
            searchPanelA = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = SystemColors.ControlLight };
            Label lblSearchA = new Label { Text = "Search Library A:", AutoSize = true, Left = 10, Top = 5 };
            txtSearchA = new TextBox { Left = 130, Top = 2, Width = 200 };
            txtSearchA.TextChanged += (s, e) => FilterGrid(dgvLibraryA, txtSearchA.Text);
            searchPanelA.Controls.Add(lblSearchA);
            searchPanelA.Controls.Add(txtSearchA);
            // Create DataGridView for Library A.
            dgvLibraryA = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            dgvLibraryA.Columns.Add("Title", "Title");
            dgvLibraryA.Columns.Add("Author", "Author");
            dgvLibraryA.Columns.Add("Series", "Series");
            // Set alternating row colors for Library A.
            dgvLibraryA.DefaultCellStyle.BackColor = Color.LightBlue;
            dgvLibraryA.AlternatingRowsDefaultCellStyle.BackColor = Color.AliceBlue;

            // Add a count label.
            lblCountA = new Label { Text = "0 Books", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DarkBlue };

            panelLibraryA.Controls.Add(dgvLibraryA);
            panelLibraryA.Controls.Add(searchPanelA);
            panelLibraryA.Controls.Add(lblCountA);

            // --- Right Panel: Library B ---
            panelLibraryB = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            // Add an individual search bar for Library B.
            searchPanelB = new Panel { Dock = DockStyle.Top, Height = 30, BackColor = SystemColors.ControlLight };
            Label lblSearchB = new Label { Text = "Search Library B:", AutoSize = true, Left = 10, Top = 5 };
            txtSearchB = new TextBox { Left = 130, Top = 2, Width = 200 };
            txtSearchB.TextChanged += (s, e) => FilterGrid(dgvLibraryB, txtSearchB.Text);
            searchPanelB.Controls.Add(lblSearchB);
            searchPanelB.Controls.Add(txtSearchB);
            // Create DataGridView for Library B.
            dgvLibraryB = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false
            };
            dgvLibraryB.Columns.Add("Title", "Title");
            dgvLibraryB.Columns.Add("Author", "Author");
            dgvLibraryB.Columns.Add("Series", "Series");
            // Set alternating row colors for Library B.
            dgvLibraryB.DefaultCellStyle.BackColor = Color.LightGreen;
            dgvLibraryB.AlternatingRowsDefaultCellStyle.BackColor = Color.Honeydew;

            // Add a count label.
            lblCountB = new Label { Text = "0 Books", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DarkBlue };

            panelLibraryB.Controls.Add(dgvLibraryB);
            panelLibraryB.Controls.Add(searchPanelB);
            panelLibraryB.Controls.Add(lblCountB);

            splitContainer.Panel1.Controls.Add(panelLibraryA);
            splitContainer.Panel2.Controls.Add(panelLibraryB);

            // --- Context menus for website searches ---
            ContextMenuStrip cmsA = new ContextMenuStrip();
            ToolStripMenuItem miMAM_A = new ToolStripMenuItem("Search MAM");
            miMAM_A.Click += MiSearchMAM_Click;
            cmsA.Items.Add(miMAM_A);
            cmsA.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem miAmazon_A = new ToolStripMenuItem("Search Amazon");
            miAmazon_A.Click += MiSearchAmazon_Click;
            cmsA.Items.Add(miAmazon_A);
            ToolStripMenuItem miAudible_A = new ToolStripMenuItem("Search Audible");
            miAudible_A.Click += MiSearchAudible_Click;
            cmsA.Items.Add(miAudible_A);
            ToolStripMenuItem miGoodreads_A = new ToolStripMenuItem("Search Goodreads");
            miGoodreads_A.Click += MiSearchGoodreads_Click;
            cmsA.Items.Add(miGoodreads_A);
            dgvLibraryA.ContextMenuStrip = cmsA;

            ContextMenuStrip cmsB = new ContextMenuStrip();
            ToolStripMenuItem miMAM_B = new ToolStripMenuItem("Search MAM");
            miMAM_B.Click += MiSearchMAM_Click;
            cmsB.Items.Add(miMAM_B);
            cmsB.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem miAmazon_B = new ToolStripMenuItem("Search Amazon");
            miAmazon_B.Click += MiSearchAmazon_Click;
            cmsB.Items.Add(miAmazon_B);
            ToolStripMenuItem miAudible_B = new ToolStripMenuItem("Search Audible");
            miAudible_B.Click += MiSearchAudible_Click;
            cmsB.Items.Add(miAudible_B);
            ToolStripMenuItem miGoodreads_B = new ToolStripMenuItem("Search Goodreads");
            miGoodreads_B.Click += MiSearchGoodreads_Click;
            cmsB.Items.Add(miGoodreads_B);
            dgvLibraryB.ContextMenuStrip = cmsB;

            // --- Add split container and control panel to content panel.
            this.contentPanel.Controls.Add(splitContainer);
            this.contentPanel.Controls.Add(controlPanel);

            this.Load += async (s, e) =>
            {
                splitContainer.SplitterDistance = this.ClientSize.Width / 2;
                await LoadLibrariesAsync();
            };
        }

        private async Task LoadLibrariesAsync()
        {
            if (cmbLibraryA.SelectedItem is LibrarySettings libA)
                await LoadBooksForLibrary(libA, dgvLibraryA);
            if (cmbLibraryB.SelectedItem is LibrarySettings libB)
                await LoadBooksForLibrary(libB, dgvLibraryB);
        }

        private async Task LoadBooksForLibrary(LibrarySettings lib, DataGridView grid)
        {
            grid.Rows.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + lib.ApiKey);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                    string endpoint = $"{lib.ApiUrl}/{lib.LibraryId}/items?sort=media.metadata.title";
                    HttpResponseMessage response = await client.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);
                        if (apiResponse?.results != null)
                        {
                            foreach (var book in apiResponse.results)
                            {
                                string title = book.media?.metadata?.title ?? "";
                                string author = book.media?.metadata?.authorName ?? "";
                                string series = book.media?.metadata?.seriesName ?? "";
                                grid.Rows.Add(title, author, series);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Error fetching from {lib.Name}: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception fetching from {lib.Name}: {ex.Message}");
            }
            if (grid == dgvLibraryA)
                lblCountA.Text = $"{grid.Rows.Count} Books";
            else if (grid == dgvLibraryB)
                lblCountB.Text = $"{grid.Rows.Count} Books";
        }

        private void FilterGrid(DataGridView grid, string search)
        {
            search = search.Trim().ToLowerInvariant();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                bool visible = string.IsNullOrEmpty(search);
                if (!visible)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (row.Cells[i].Value != null && row.Cells[i].Value.ToString().ToLowerInvariant().Contains(search))
                        {
                            visible = true;
                            break;
                        }
                    }
                }
                row.Visible = visible;
            }
        }

        // --- Website search event handlers (same for both DataGridViews) ---
        private void MiSearchMAM_Click(object sender, EventArgs e)
        {
            DataGridView grid = dgvLibraryA.Focused ? dgvLibraryA : dgvLibraryB;
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to search.");
                return;
            }
            var row = grid.SelectedRows[0];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string query = title + " " + author;
            string url = "https://www.myanonamouse.net/tor/browse.php?tor[text]=" + Uri.EscapeDataString(query);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void MiSearchAmazon_Click(object sender, EventArgs e)
        {
            DataGridView grid = dgvLibraryA.Focused ? dgvLibraryA : dgvLibraryB;
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to search.");
                return;
            }
            var row = grid.SelectedRows[0];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string query = title + " " + author;
            string url = "https://www.amazon.com/s?k=" + Uri.EscapeDataString(query);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void MiSearchAudible_Click(object sender, EventArgs e)
        {
            DataGridView grid = dgvLibraryA.Focused ? dgvLibraryA : dgvLibraryB;
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to search.");
                return;
            }
            var row = grid.SelectedRows[0];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string query = title + " " + author;
            string url = "https://www.audible.com/search?keywords=" + Uri.EscapeDataString(query);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void MiSearchGoodreads_Click(object sender, EventArgs e)
        {
            DataGridView grid = dgvLibraryA.Focused ? dgvLibraryA : dgvLibraryB;
            if (grid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to search.");
                return;
            }
            var row = grid.SelectedRows[0];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string query = title + " " + author;
            string url = "https://www.goodreads.com/search?q=" + Uri.EscapeDataString(query);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
