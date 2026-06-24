using System;
using System.Windows.Forms;
using CybersecurityChatbot.GUI;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Application entry point. Launches the WinForms GUI.
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
