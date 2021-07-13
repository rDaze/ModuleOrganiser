using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace ModuleOrganiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// ToDo: Add Error handling (e.g launch CSGO if no dir is there)
    public partial class MainWindow : Window
    {
        string MoonlightDir = null;
        string SteamDir     = null;
        MoonlightModule moonlightModule;

        public MainWindow()
        {
            InitializeComponent();

            LoadSavedDirectories();
        }

        private void GetMoonlightDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowsingDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderBrowsingDialog.ShowDialog();

            if (result.ToString() != string.Empty)
            {
                PathMoonlight.Text = MoonlightDir = folderBrowsingDialog.SelectedPath;
                SaveDirToFile();
                UpdateListBox();
            }
        }

        private void GetSteamDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowsingDialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = folderBrowsingDialog.ShowDialog();

            if (result.ToString() != string.Empty)
            {
                PathSteam.Text = SteamDir = folderBrowsingDialog.SelectedPath;
                SaveDirToFile();
            }
        }

        private void SaveDirToFile()
        {
            File.WriteAllText("ModuleSettings.txt", MoonlightDir + "\r\n" + SteamDir);
        }

        private void UpdateListBox()
        {
            List<MoonlightModule> moonlightModulesList = new List<MoonlightModule>();
            string disabledDir = MoonlightDir + "/moonlight/scripts/disabled";
            string[] disabledModules = Directory.GetFiles(disabledDir, "*.lua");

            foreach (string disabledModule in disabledModules)
            {
                moonlightModule = new MoonlightModule();
                moonlightModule.ModuleName = Path.GetFileNameWithoutExtension(disabledModule);
                moonlightModule.IsModuleLoaded = IsMoonlightModuleLoaded(moduleName: Path.GetFileNameWithoutExtension(disabledModule));
                moonlightModulesList.Add(moonlightModule);
            }
            MoonlightListModules.ItemsSource = moonlightModulesList;
        }

        private bool IsMoonlightModuleLoaded(string moduleName)
        {
            string enabledDir = MoonlightDir + "/moonlight/scripts";
            string[] enabledModules = Directory.GetFiles(enabledDir, "*.lua");

            foreach (string enabledModule in enabledModules)
            {
                if (Path.GetFileNameWithoutExtension(enabledModule).Equals(moduleName))
                {
                    return true;
                }
            }
            return false;
        }

        //If user changes fantasy.moonlight.exe then this is screwed
        private void RunMoonlight(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                Verb = "runas",
                UseShellExecute = true,
                WorkingDirectory = MoonlightDir,
                FileName = "fantasy.moonlight.exe"
            };
            Process.Start(proc);
        }

        private void RunSteam(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo proc = new ProcessStartInfo
            {
                Verb = "runas",
                UseShellExecute = true,
                WorkingDirectory = SteamDir,
                FileName = "steam.exe",
                Arguments = "-applaunch 730"
            };
            Process.Start(proc);
        }

        private void OnAddModule(object sender, RoutedEventArgs e)
        {
           foreach (MoonlightModule moonlightModule in MoonlightListModules.ItemsSource)
           {
                if (moonlightModule.IsModuleLoaded)
                {
                    string disabledDir = MoonlightDir + "/moonlight/scripts/disabled";
                    string enabledDir = MoonlightDir + "/moonlight/scripts";
                    var moduleToAdd = moonlightModule.ModuleName + ".lua";
                    File.Copy(Path.Combine(disabledDir, moduleToAdd), Path.Combine(enabledDir, moduleToAdd), true);
                }
           }
        }

        private void OnRemoveModule(object sender, RoutedEventArgs e)
        {
            foreach (MoonlightModule moonlightModule in MoonlightListModules.ItemsSource)
            {
                if (!moonlightModule.IsModuleLoaded)
                {
                    string enabledDir = MoonlightDir + "/moonlight/scripts";
                    var moduleToRemove = moonlightModule.ModuleName + ".lua";
                    if (File.Exists(Path.Combine(enabledDir, moduleToRemove)))
                    {
                        File.Delete(Path.Combine(enabledDir, moduleToRemove));
                    }
                }
            }
        }

        private void LoadSavedDirectories()
        {
            if (File.Exists("ModuleSettings.txt"))
            {
                try
                {
                    string[] savedDirectories = File.ReadAllLines("ModuleSettings.txt");

                    MoonlightDir = savedDirectories[0];
                    if (MoonlightDir != string.Empty)
                    {
                        PathMoonlight.Text = MoonlightDir;
                        UpdateListBox();
                    }
                    else
                    {
                        PathMoonlight = null;
                    }

                    SteamDir = savedDirectories[1];
                    if (SteamDir != string.Empty)
                    {
                        PathSteam.Text = SteamDir;
                    }
                    else
                    {
                        SteamDir = null;
                    }
                }
                catch
                {
                    // well too bad :)
                }
            }
        }
    }
}