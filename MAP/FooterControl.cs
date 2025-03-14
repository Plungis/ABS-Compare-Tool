using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ABSProject
{
    public partial class FooterControl : UserControl
    {
        public FooterControl()
        {
            InitializeComponent();
        }

        private void btnInstructions_Click(object sender, EventArgs e)
        {
            // Open the InstructionsForm when the button is clicked.
            using (var instrForm = new InstructionsForm())
            {
                instrForm.ShowDialog();
            }
        }

        private void linkLabelDeveloper_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Launch the developer's webpage.
            Process.Start(new ProcessStartInfo("https://www.myanonamouse.net/u/232156")
            {
                UseShellExecute = true
            });
        }
    }
}
