using System;
using System.Windows.Forms;

namespace ABSProject
{
    public class PossibleMatchesForm : BaseForm
    {
        private DataGridView dgvMatches;
        private Panel topPanel;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnRefresh;
        private System.Collections.Generic.List<ComparisonResult> currentMatches;

        public PossibleMatchesForm(System.Collections.Generic.List<ComparisonResult> matches)
        {
            currentMatches = matches;
            InitializeComponents();
            LoadMatches(currentMatches);
        }

        private void InitializeComponents()
        {
            this.Text = "Possible Matches";
            this.ClientSize = new System.Drawing.Size(800, 500);
            topPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            lblSearch = new Label { Text = "Search:", Left = 10, Top = 10, AutoSize = true };
            txtSearch = new TextBox { Left = 70, Top = 7, Width = 200 };
            txtSearch.TextChanged += (s, e) => ApplySearch();
            btnRefresh = new Button { Text = "Refresh", Left = 280, Top = 5, Width = 100 };
            btnRefresh.Click += (s, e) => LoadMatches(currentMatches);
            topPanel.Controls.Add(lblSearch);
            topPanel.Controls.Add(txtSearch);
            topPanel.Controls.Add(btnRefresh);

            dgvMatches = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
            };
            dgvMatches.ColumnCount = 5;
            dgvMatches.Columns[0].Name = "Title";
            dgvMatches.Columns[1].Name = "Author";
            dgvMatches.Columns[2].Name = "Series";
            dgvMatches.Columns[3].Name = "Missing Version";
            dgvMatches.Columns[4].Name = "Match %";

            this.contentPanel.Controls.Add(topPanel);
            this.contentPanel.Controls.Add(dgvMatches);
        }

        private void LoadMatches(System.Collections.Generic.List<ComparisonResult> matches)
        {
            dgvMatches.Rows.Clear();
            foreach (var item in matches)
            {
                int rowIndex = dgvMatches.Rows.Add(
                    item.Title,
                    item.Author,
                    item.Series,
                    item.MissingVersion,
                    (item.MatchScore * 100).ToString("F1") + "%"
                );
                dgvMatches.Rows[rowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightSalmon;
            }
            ApplySearch();
        }

        private void ApplySearch()
        {
            string search = txtSearch.Text.Trim().ToLowerInvariant();
            foreach (DataGridViewRow row in dgvMatches.Rows)
            {
                if (row.IsNewRow) continue;
                bool visible = false;
                for (int i = 0; i < 3; i++)
                {
                    if (row.Cells[i].Value != null &&
                        row.Cells[i].Value.ToString().ToLowerInvariant().Contains(search))
                    {
                        visible = true;
                        break;
                    }
                }
                row.Visible = visible;
            }
        }
    }
}
