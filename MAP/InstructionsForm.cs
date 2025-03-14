using System;
using System.Drawing;
using System.Windows.Forms;

namespace ABSProject
{
    public class InstructionsForm : BaseForm
    {
        private TextBox txtInstructions;

        public InstructionsForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Instructions";
            this.ClientSize = new Size(600, 400);

            // A multiline, read-only TextBox for your instructions.
            txtInstructions = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10),
                Text =
@"Welcome to ABS Library Compare!

Setup Steps:
1) Click 'Settings' to configure your libraries (ApiUrl, ApiKey, LibraryId).
   Press 'Test Connection' to verify.

2) On the main screen, 'Refresh' each library to load all books.

3) Use the top search bar to filter by Title, Author, or Series.

4) 'Compare Libraries' to see what's missing from each:
   - Mark items as 'Done', 'EBook Unavailable', or 'Audiobook Unavailable'.

5) Right-click any book to search externally on MAM, Amazon, Audible, or Goodreads.

Enjoy!

Notes: Tis not the size of the young knave's blade, but the lust in his thrust by which legends are made. - Sir Hung
"
            };

            // Add the text box to our base form's content panel.
            this.contentPanel.Controls.Add(txtInstructions);
        }
    }
}
