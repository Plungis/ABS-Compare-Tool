using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ABSProject
{
    public class ComparisonForm : BaseForm
    {
        // Bottom section controls
        private ComboBox cmbLibraryA;
        private ComboBox cmbLibraryB;
        private Button btnRefreshComparison;
        private Button btnViewMatches;
        private Button btnSettings;
        private Panel bottomPanel;
        private Panel searchPanel;
        private Panel controlPanel;
        private Panel filterPanel;
        private Label lblSearch;
        private TextBox txtSearch;
        private ProgressBar progressBar;

        // Main DataGridView for comparison results.
        private DataGridView dgvCombined;
        private Label lblLoading;

        // Filter checkboxes.
        private CheckBox chkHideDone;
        private CheckBox chkShowOnlyDone;
        private CheckBox chkHideUnavailable;
        private CheckBox chkShowOnlyUnavailable;

        // Label for missing counts.
        private Label lblCounts;

        // Data and state.
        private System.Collections.Generic.List<ComparisonResult> lastComparisonResults;
        private const double POSSIBLE_MATCH_THRESHOLD = 0.85;
        private LibrarySettings currentLibraryA;
        private LibrarySettings currentLibraryB;

        public ComparisonForm()
        {
            InitializeComponents();
            this.Shown += async (s, e) => await RunComparison();
            this.FormClosing += (s, e) => SettingsManager.SaveSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "Compare Libraries";
            this.ClientSize = new Size(1200, 600);

            // -------------------------
            // Bottom Panel (holds search, control, filter, and progress bar panels)
            // -------------------------
            bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = SystemColors.ControlLight
            };

            // -------------------------
            // Search Panel (placed at the top of bottomPanel, height = 30)
            // -------------------------
            searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = SystemColors.Control
            };
            lblSearch = new Label { Text = "Search:", AutoSize = true, Location = new Point(10, 5) };
            txtSearch = new TextBox { Location = new Point(70, 2), Width = 200 };
            txtSearch.TextChanged += (s, e) => ApplyFilters();
            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);

            // -------------------------
            // Control Panel (for library selection and action buttons, height = 50)
            // -------------------------
            controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = SystemColors.Control
            };
            Label lblLibA = new Label { Text = "Library A:", AutoSize = true, Location = new Point(10, 15) };
            cmbLibraryA = new ComboBox { Left = 80, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            Label lblLibB = new Label { Text = "Library B:", AutoSize = true, Location = new Point(300, 15) };
            cmbLibraryB = new ComboBox { Left = 370, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

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

            cmbLibraryA.SelectedIndexChanged += async (s, e) =>
            {
                if (cmbLibraryA.SelectedItem is LibrarySettings lib)
                    await LoadBooksForLibrary(lib, dgvCombined);
            };
            cmbLibraryB.SelectedIndexChanged += async (s, e) =>
            {
                if (cmbLibraryB.SelectedItem is LibrarySettings lib)
                    await LoadBooksForLibrary(lib, dgvCombined);
            };

            btnRefreshComparison = new Button { Text = "Refresh", Left = 600, Top = 10, Width = 90 };
            btnRefreshComparison.Click += async (s, e) => await RunComparison();
            btnViewMatches = new Button { Text = "View Possible Matches", Left = 700, Top = 10, Width = 150 };
            btnViewMatches.Click += new EventHandler(OnViewMatches_Click);
            btnSettings = new Button { Text = "Settings", Left = 860, Top = 10, Width = 90 };
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
            controlPanel.Controls.Add(btnRefreshComparison);
            controlPanel.Controls.Add(btnViewMatches);
            controlPanel.Controls.Add(btnSettings);

            // -------------------------
            // Filter Panel (for hide/show checkboxes and missing counts, height = 30)
            // -------------------------
            filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = SystemColors.Control
            };

            chkHideDone = new CheckBox { Text = "Hide Done", AutoSize = true, Left = 10, Top = 5 };
            chkHideDone.CheckedChanged += (s, e) => ApplyFilters();
            chkShowOnlyDone = new CheckBox { Text = "Only Done", AutoSize = true, Left = 120, Top = 5 };
            chkShowOnlyDone.CheckedChanged += (s, e) => ApplyFilters();
            chkHideUnavailable = new CheckBox { Text = "Hide Unavailable", AutoSize = true, Left = 230, Top = 5 };
            chkHideUnavailable.CheckedChanged += (s, e) => ApplyFilters();
            chkShowOnlyUnavailable = new CheckBox { Text = "Only Unavailable", AutoSize = true, Left = 360, Top = 5 };
            chkShowOnlyUnavailable.CheckedChanged += (s, e) => ApplyFilters();

            lblCounts = new Label
            {
                Text = "Missing eBooks: 0, Missing Audiobooks: 0",
                AutoSize = true,
                ForeColor = Color.DarkBlue,
                Location = new Point(chkShowOnlyUnavailable.Right + 20, 5)
            };

            filterPanel.Controls.Add(chkHideDone);
            filterPanel.Controls.Add(chkShowOnlyDone);
            filterPanel.Controls.Add(chkHideUnavailable);
            filterPanel.Controls.Add(chkShowOnlyUnavailable);
            filterPanel.Controls.Add(lblCounts);

            // -------------------------
            // Progress Bar Panel (at bottom of bottomPanel)
            // -------------------------
            Panel progressPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                BackColor = SystemColors.Control
            };
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };
            progressPanel.Controls.Add(progressBar);

            bottomPanel.Controls.Add(progressPanel);
            bottomPanel.Controls.Add(filterPanel);
            bottomPanel.Controls.Add(controlPanel);
            bottomPanel.Controls.Add(searchPanel);
            searchPanel.BringToFront();

            // -------------------------
            // Main DataGridView for comparison results.
            // -------------------------
            dgvCombined = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgvCombined.Columns.Add("Title", "Title");
            dgvCombined.Columns.Add("Author", "Author");
            dgvCombined.Columns.Add("Series", "Series");
            dgvCombined.Columns.Add("MissingVersion", "Missing Version");
            var colProcessed = new DataGridViewCheckBoxColumn { Name = "Processed", HeaderText = "Done?", ValueType = typeof(bool) };
            dgvCombined.Columns.Add(colProcessed);
            var colEBook = new DataGridViewCheckBoxColumn { Name = "NoEbook", HeaderText = "EBook Unavailable?", ValueType = typeof(bool) };
            dgvCombined.Columns.Add(colEBook);
            var colAudio = new DataGridViewCheckBoxColumn { Name = "NoAudio", HeaderText = "Audiobook Unavailable?", ValueType = typeof(bool) };
            dgvCombined.Columns.Add(colAudio);
            foreach (DataGridViewColumn col in dgvCombined.Columns)
                col.SortMode = DataGridViewColumnSortMode.Automatic;

            ContextMenuStrip cmsMain = new ContextMenuStrip();
            ToolStripMenuItem miMAM = new ToolStripMenuItem("Search MAM");
            miMAM.Click += MiSearchMAM_Click;
            cmsMain.Items.Add(miMAM);
            cmsMain.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem miAmazon = new ToolStripMenuItem("Search Amazon");
            miAmazon.Click += MiSearchAmazon_Click;
            cmsMain.Items.Add(miAmazon);
            ToolStripMenuItem miAudible = new ToolStripMenuItem("Search Audible");
            miAudible.Click += MiSearchAudible_Click;
            cmsMain.Items.Add(miAudible);
            ToolStripMenuItem miGoodreads = new ToolStripMenuItem("Search Goodreads");
            miGoodreads.Click += MiSearchGoodreads_Click;
            cmsMain.Items.Add(miGoodreads);
            dgvCombined.ContextMenuStrip = cmsMain;

            dgvCombined.CellValueChanged += DgvCombined_CellValueChanged;
            dgvCombined.CurrentCellDirtyStateChanged += DgvCombined_CurrentCellDirtyStateChanged;
            dgvCombined.CellEndEdit += DgvCombined_CellEndEdit;

            lblLoading = new Label
            {
                Text = "Comparing...",
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Red,
                BackColor = Color.White,
                Visible = false
            };
            lblLoading.Location = new Point((this.ClientSize.Width - lblLoading.Width) / 2,
                                            (this.ClientSize.Height - lblLoading.Height) / 2);
            lblLoading.Anchor = AnchorStyles.None;

            this.contentPanel.Controls.Add(dgvCombined);
            this.contentPanel.Controls.Add(bottomPanel);
            this.Controls.Add(lblLoading);

            this.Load += async (s, e) =>
            {
                await LoadLibrariesAsync();
            };
        }

        private async Task LoadLibrariesAsync()
        {
            if (cmbLibraryA.SelectedItem is LibrarySettings libA)
                await LoadBooksForLibrary(libA, dgvCombined);
            if (cmbLibraryB.SelectedItem is LibrarySettings libB)
                await LoadBooksForLibrary(libB, dgvCombined);
            ApplyFilters();
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
        }

        // Context menu event handlers.
        private void MiSearchMAM_Click(object sender, EventArgs e) { SearchBooksInContext("mam"); }
        private void MiSearchAmazon_Click(object sender, EventArgs e) { SearchBooksInContext("amazon"); }
        private void MiSearchAudible_Click(object sender, EventArgs e) { SearchBooksInContext("audible"); }
        private void MiSearchGoodreads_Click(object sender, EventArgs e) { SearchBooksInContext("goodreads"); }

        private void SearchBooksInContext(string site)
        {
            if (dgvCombined.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book to search.");
                return;
            }
            var row = dgvCombined.SelectedRows[0];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string query = title + " " + author;
            string url = "";
            if (site == "mam")
                url = "https://www.myanonamouse.net/tor/browse.php?tor[text]=" + Uri.EscapeDataString(query);
            else if (site == "amazon")
                url = "https://www.amazon.com/s?k=" + Uri.EscapeDataString(query);
            else if (site == "audible")
                url = "https://www.audible.com/search?keywords=" + Uri.EscapeDataString(query);
            else if (site == "goodreads")
                url = "https://www.goodreads.com/search?q=" + Uri.EscapeDataString(query);
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void DgvCombined_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvCombined.IsCurrentCellDirty && dgvCombined.CurrentCell is DataGridViewCheckBoxCell)
                dgvCombined.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvCombined_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgvCombined.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvCombined_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvCombined.Rows[e.RowIndex];
            string title = row.Cells["Title"].Value?.ToString() ?? "";
            string author = row.Cells["Author"].Value?.ToString() ?? "";
            string series = row.Cells["Series"].Value?.ToString() ?? "";
            string key = SettingsManager.MakeBookKey(title, author, series);
            if (!SettingsManager.Current.BookTracking.ContainsKey(key))
                SettingsManager.Current.BookTracking[key] = new BookStatus();
            var statusEntry = SettingsManager.Current.BookTracking[key];
            statusEntry.Processed = Convert.ToBoolean(row.Cells["Processed"].Value);
            statusEntry.EBookUnavailable = Convert.ToBoolean(row.Cells["NoEbook"].Value);
            statusEntry.AudiobookUnavailable = Convert.ToBoolean(row.Cells["NoAudio"].Value);
            SettingsManager.SaveSettings();
            UpdateRowColor(row);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string search = txtSearch.Text.Trim().ToLowerInvariant();
            foreach (DataGridViewRow row in dgvCombined.Rows)
            {
                if (row.IsNewRow) continue;
                bool visible = true;
                bool isDone = Convert.ToBoolean(row.Cells["Processed"].Value);
                if (chkShowOnlyDone.Checked && !isDone)
                    visible = false;
                if (chkHideDone.Checked && isDone)
                    visible = false;
                bool isUnavailable = Convert.ToBoolean(row.Cells["NoEbook"].Value) || Convert.ToBoolean(row.Cells["NoAudio"].Value);
                if (chkShowOnlyUnavailable.Checked && !isUnavailable)
                    visible = false;
                if (chkHideUnavailable.Checked && isUnavailable)
                    visible = false;
                if (!string.IsNullOrEmpty(search))
                {
                    bool searchMatch = false;
                    for (int i = 0; i < 3; i++)
                    {
                        if (row.Cells[i].Value != null &&
                            row.Cells[i].Value.ToString().ToLowerInvariant().Contains(search))
                        {
                            searchMatch = true;
                            break;
                        }
                    }
                    if (!searchMatch)
                        visible = false;
                }
                row.Visible = visible;
            }

            int missingEbookCount = 0;
            int missingAudiobookCount = 0;
            foreach (DataGridViewRow row in dgvCombined.Rows)
            {
                if (!row.Visible)
                    continue;
                string missingText = row.Cells["MissingVersion"].Value?.ToString() ?? "";
                if (missingText.StartsWith("Missing from"))
                {
                    if (currentLibraryA != null && missingText.Contains(currentLibraryA.Name))
                        missingEbookCount++;
                    if (currentLibraryB != null && missingText.Contains(currentLibraryB.Name))
                        missingAudiobookCount++;
                }
            }
            lblCounts.Text = $"Missing eBooks: {missingEbookCount}, Missing Audiobooks: {missingAudiobookCount}";
        }

        private void UpdateRowColor(DataGridViewRow row)
        {
            bool ebookUnavailable = Convert.ToBoolean(row.Cells["NoEbook"].Value);
            bool audioUnavailable = Convert.ToBoolean(row.Cells["NoAudio"].Value);
            Color rowColor = Color.White;
            if (ebookUnavailable && audioUnavailable)
                rowColor = Color.Plum;
            else if (ebookUnavailable)
                rowColor = Color.LightCoral;
            else if (audioUnavailable)
                rowColor = Color.LightGoldenrodYellow;
            else
            {
                string missingText = row.Cells["MissingVersion"].Value?.ToString() ?? "";
                if (!string.IsNullOrEmpty(missingText))
                {
                    if (missingText.Contains(currentLibraryA?.Name ?? ""))
                        rowColor = Color.LightBlue;
                    else if (missingText.Contains(currentLibraryB?.Name ?? ""))
                        rowColor = Color.LightGreen;
                }
            }
            row.DefaultCellStyle.BackColor = rowColor;
        }

        private async Task RunComparison()
        {
            var libA = cmbLibraryA.SelectedItem as LibrarySettings;
            var libB = cmbLibraryB.SelectedItem as LibrarySettings;
            if (libA == null || libB == null)
            {
                MessageBox.Show("Please select both libraries.");
                return;
            }
            if (libA.Name.Equals(libB.Name, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Please select two different libraries.");
                return;
            }
            currentLibraryA = libA;
            currentLibraryB = libB;
            lblLoading.Visible = true;
            lblLoading.BringToFront();
            lblLoading.Refresh();

            // Initialize progress bar.
            int totalSteps = 0;
            int currentStep = 0;
            var booksA = await FetchBooksAsync(libA);
            var booksB = await FetchBooksAsync(libB);
            totalSteps = (booksA.Count * booksB.Count) + (booksB.Count * booksA.Count);
            progressBar.Value = 0;

            lastComparisonResults = new System.Collections.Generic.List<ComparisonResult>();

            // First loop: compare each book in A with books in B.
            foreach (var bookA in booksA)
            {
                foreach (var bookB in booksB)
                {
                    currentStep++;
                    progressBar.Value = Math.Min(100, (currentStep * 100) / totalSteps);
                    Application.DoEvents();
                }
                bool found = false;
                foreach (var bookB in booksB)
                {
                    if (Utility.IsMatch(bookA, bookB))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    double bestScore = 0;
                    foreach (var bookB in booksB)
                    {
                        double scoreTitle = Utility.Similarity(bookA.media?.metadata?.title ?? "", bookB.media?.metadata?.title ?? "");
                        double scoreAuthor = Utility.Similarity(bookA.media?.metadata?.authorName ?? "", bookB.media?.metadata?.authorName ?? "");
                        double avgScore = (scoreTitle + scoreAuthor) / 2.0;
                        if (avgScore > bestScore)
                            bestScore = avgScore;
                    }
                    string missingVersion = (bestScore >= POSSIBLE_MATCH_THRESHOLD)
                        ? $"Possible match in {libB.Name}"
                        : $"Missing from {libB.Name}";
                    lastComparisonResults.Add(new ComparisonResult
                    {
                        Title = bookA.media?.metadata?.title ?? "",
                        Author = bookA.media?.metadata?.authorName ?? "",
                        Series = bookA.media?.metadata?.seriesName ?? "",
                        MissingVersion = missingVersion,
                        MatchScore = bestScore
                    });
                }
            }

            // Second loop: compare each book in B with books in A.
            foreach (var bookB in booksB)
            {
                foreach (var bookA in booksA)
                {
                    currentStep++;
                    progressBar.Value = Math.Min(100, (currentStep * 100) / totalSteps);
                    Application.DoEvents();
                }
                bool found = false;
                foreach (var bookA in booksA)
                {
                    if (Utility.IsMatch(bookA, bookB))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    double bestScore = 0;
                    foreach (var bookA in booksA)
                    {
                        double scoreTitle = Utility.Similarity(bookB.media?.metadata?.title ?? "", bookA.media?.metadata?.title ?? "");
                        double scoreAuthor = Utility.Similarity(bookB.media?.metadata?.authorName ?? "", bookA.media?.metadata?.authorName ?? "");
                        double avgScore = (scoreTitle + scoreAuthor) / 2.0;
                        if (avgScore > bestScore)
                            bestScore = avgScore;
                    }
                    string missingVersion = (bestScore >= POSSIBLE_MATCH_THRESHOLD)
                        ? $"Possible match in {libA.Name}"
                        : $"Missing from {libA.Name}";
                    lastComparisonResults.Add(new ComparisonResult
                    {
                        Title = bookB.media?.metadata?.title ?? "",
                        Author = bookB.media?.metadata?.authorName ?? "",
                        Series = bookB.media?.metadata?.seriesName ?? "",
                        MissingVersion = missingVersion,
                        MatchScore = bestScore
                    });
                }
            }

            lblLoading.Visible = false;
            dgvCombined.Rows.Clear();
            foreach (var item in lastComparisonResults)
            {
                int idx = dgvCombined.Rows.Add(
                    item.Title,
                    item.Author,
                    item.Series,
                    item.MissingVersion,
                    false,
                    false,
                    false
                );
                string key = SettingsManager.MakeBookKey(item.Title, item.Author, item.Series);
                if (SettingsManager.Current.BookTracking.TryGetValue(key, out BookStatus status))
                {
                    dgvCombined.Rows[idx].Cells["Processed"].Value = status.Processed;
                    dgvCombined.Rows[idx].Cells["NoEbook"].Value = status.EBookUnavailable;
                    dgvCombined.Rows[idx].Cells["NoAudio"].Value = status.AudiobookUnavailable;
                }
                UpdateRowColor(dgvCombined.Rows[idx]);
            }
            ApplyFilters();
        }

        private async Task<System.Collections.Generic.List<Book>> FetchBooksAsync(LibrarySettings lib)
        {
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
                        return apiResponse.results ?? new System.Collections.Generic.List<Book>();
                    }
                    else
                    {
                        MessageBox.Show($"Error fetching from {lib.Name}: {response.StatusCode}");
                        return new System.Collections.Generic.List<Book>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception fetching from {lib.Name}: {ex.Message}");
                return new System.Collections.Generic.List<Book>();
            }
        }

        // Explicit event handler for the "View Possible Matches" button.
        private void OnViewMatches_Click(object sender, EventArgs e)
        {
            if (lastComparisonResults == null || lastComparisonResults.Count == 0)
            {
                MessageBox.Show("Please compare libraries first.");
                return;
            }
            var possibleMatches = lastComparisonResults.FindAll(r => r.MatchScore >= POSSIBLE_MATCH_THRESHOLD);
            if (possibleMatches.Count == 0)
            {
                MessageBox.Show("No possible matches found.");
                return;
            }
            using (var form = new PossibleMatchesForm(possibleMatches))
            {
                form.ShowDialog();
            }
        }
    }
}
