using System.Windows.Forms;

namespace ABSProject
{
    public class BaseForm : Form
    {
        protected Panel contentPanel;
        public FooterControl footer; // Now FooterControl is public

        public BaseForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Create and dock the content panel.
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            this.Controls.Add(contentPanel);

            // Create and dock the footer.
            footer = new FooterControl();
            footer.Dock = DockStyle.Bottom;
            this.Controls.Add(footer);

            // Other default settings.
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}
