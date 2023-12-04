using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using TowerUniteMidiDotNet.Windows;

static class Program
{
    // list the required DLLs
    private static readonly string[] RequiredDlls = {
        "Melanchall.DryWetMidi.dll",
        "NHotkey.dll",
        "NHotkey.WindowsForms.dll"
    };

    [STAThread]
    static void Main()
    {
        // before running the application, check for the required DLLs
        var missingDlls = RequiredDlls.Where(dll => !File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dll))).ToList();
        if (missingDlls.Any())
        {
            string missingDllsMessage = "The following required libraries are missing:\n" + string.Join("\n", missingDlls) +
                                        "\n\nPlease ensure you have unpacked all the DLLs included in the archive to the same directory as the executable.";
            MessageBox.Show(missingDllsMessage, "Missing Libraries", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // exit the program if DLLs are missing
            Environment.Exit(1);
        }

        // if all DLLs are present, continue to run the application
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainWindow());
    }
}