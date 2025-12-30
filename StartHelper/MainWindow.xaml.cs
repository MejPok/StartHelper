using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StartHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartOver(object sender, RoutedEventArgs e)
        {
            StartApp("overwolf");

        }
        private void StartHotkey(object sender, RoutedEventArgs e)
        {
            StartApp("hotkey");

        }
        private void StartVisual(object sender, RoutedEventArgs e)
        {
            StartApp("visual");
        }

        private void KillOver(object sender, RoutedEventArgs e)
        {
            StopApp("Overwolf");
        }
        private void KillRiot(object sender, RoutedEventArgs e)
        {
            StopApp("Riot Client");
        }

        private void PlayMusic(object sender, RoutedEventArgs e)
        {
            string spotifyUri = ConfigurationManager.AppSettings["SpotifyPlaylist"];
            Process.Start(new ProcessStartInfo
            {
                FileName = spotifyUri,
                UseShellExecute = true
            });
        }
        
        private void OpenGithub(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ConfigurationManager.AppSettings["Opera"],
                Arguments = "https://github.com/MejPok",
                UseShellExecute = true
            });
        }
        
        private void StartApp(string app)
        {
            if(app == "hotkey")
            {
                string hotkeyScriptPath = ConfigurationManager.AppSettings["HotkeyScriptPath"];
                Process.Start(hotkeyScriptPath);
            } else if(app == "visual")
            {
                string visualStudioPath = ConfigurationManager.AppSettings["VisualStudioPath"];
                Process.Start(visualStudioPath);
            } else if(app == "overwolf")
            {
                string overwolfPath = ConfigurationManager.AppSettings["Overwolf"];
                Process.Start(overwolfPath);
            }
            
        }

        private void StopApp(string app)
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(app))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

    }
}