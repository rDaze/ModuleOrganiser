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
    // ToDo: How the fuck do I manage user input error?

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        /// <summary>
        /// Opens a folder browsing dialog and saves the result to MoonlightDir
        /// Calls SaveDirToFile() to save MoonlightDir to .txt
        /// Calls UpdateListBox() to populate ListBox with Moonlight modules
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Opens a folder browsing dialog and saves the result to SteamDir
        /// Calls SaveDirToFile() to save SteamDir to .txt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Saves MoonlightDir and/or SteamDir to a .txt
        /// MoonlightDir will always be in line 1 | SteamDir will always be in line 2
        /// </summary>
        private void SaveDirToFile()
        {
            File.WriteAllText("ModuleSettings.txt", MoonlightDir + "\r\n" + SteamDir);
        }

        /// <summary>
        /// Populates the ListBox
        /// </summary>
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

        /// <summary>
        /// Performs a basic check in order to see if a module is enabled or not
        /// </summary>
        /// <param name="moduleName">Module Name</param>
        /// <returns></returns>
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

        /// <summary>
        /// Runs Moonlight as Administrator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunMoonlight(object sender, RoutedEventArgs e)
        {
            if (MoonlightDir != null)
            {
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch moonlight: \r\n {ex}", "Error", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Failed to get moonlight directory", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Runs Steam as Administrator 
        /// Passes "-applaunch 730" in order to directly launch CS:GO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunSteam(object sender, RoutedEventArgs e)
        {
            if (SteamDir != null)
            {
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch steam: \r\n {ex}", "Error", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show($"Failed to get steam directory", "Error", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Enables the desired module by copying
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Removes the desired module by deleting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Scans for ModuleSettings.txt and reads the MoonlightDir and/or SteamDir
        /// Updates variables, TextBoxes, and ListBox
        /// Sets var to null if string is empty
        /// </summary>
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