using System;
using System.Windows.Forms;

namespace ABSProject
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            SettingsManager.LoadSettings();
            if (SettingsManager.Libraries.Count == 0)
            {
                // Use placeholder API URLs and remove sensitive information.
                SettingsManager.Libraries.Add(new LibrarySettings
                {
                    Name = "Default Library",
                    ApiUrl = "https://bookshelf.example.com/api/libraries",
                    ApiKey = "",
                    LibraryId = ""
                });
                SettingsManager.Libraries.Add(new LibrarySettings
                {
                    Name = "Ebooks",
                    ApiUrl = "https://bookshelf.example.com/api/libraries",
                    ApiKey = "",
                    LibraryId = ""
                });
                SettingsManager.SaveSettings();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
