using SpotifyAPI.Web;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
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

        private async void PlayMusic(object sender, RoutedEventArgs e)
        {
            string clientId = ConfigurationManager.AppSettings["SpotifyClientId"];
            string redirectUri = ConfigurationManager.AppSettings["SpotifyRedirectUri"];
            string playlistUri = ConfigurationManager.AppSettings["SpotifyPlaylistUri"];

            string tokenPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StartHelper", "spotify_refresh.txt");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(tokenPath));

            SpotifyClient spotify;

            if (File.Exists(tokenPath))
            {
                // Refresh token exists → get new access token
                string refreshToken = File.ReadAllText(tokenPath);
                var oauth = new OAuthClient();
                var tokenResponse = await oauth.RequestToken(new PKCETokenRefreshRequest(clientId, refreshToken));
                spotify = new SpotifyClient(tokenResponse.AccessToken);
            }
            else
            {
                // No refresh token → full PKCE login

                var (verifier, challenge) = PKCEUtil.GenerateCodes();

                // Build login URL
                var loginRequest = new LoginRequest(
                    new Uri(redirectUri),
                    clientId,
                    LoginRequest.ResponseType.Code
                )
                {
                    CodeChallengeMethod = "S256",
                    CodeChallenge = challenge,
                    Scope = new[] { Scopes.UserModifyPlaybackState }
                };

                // Start tiny HTTP listener
                var listener = new HttpListener();
                listener.Prefixes.Add(redirectUri.EndsWith("/") ? redirectUri : redirectUri + "/");
                listener.Start();

                // Open browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = loginRequest.ToUri().ToString(),
                    UseShellExecute = true
                });

                // Wait for Spotify redirect
                var context = await listener.GetContextAsync();
                string code = context.Request.QueryString["code"];

                // Send simple response to browser
                var responseString = "<html><body>Login successful! You can close this window.</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                context.Response.OutputStream.Close();

                listener.Stop();

                // Exchange code for access token
                var oauth = new OAuthClient();
                var tokenResponse = await oauth.RequestToken(new PKCETokenRequest(
                    clientId: clientId,
                    code: code,
                    codeVerifier: verifier,
                    redirectUri: new Uri(redirectUri)
                ));

                // Store refresh token for future use
                File.WriteAllText(tokenPath, tokenResponse.RefreshToken);

                spotify = new SpotifyClient(tokenResponse.AccessToken);
            }

            // Play playlist immediately
            await spotify.Player.ResumePlayback(new PlayerResumePlaybackRequest
            {
                ContextUri = playlistUri
            });
        }

        private async Task<string> WaitForCodeFromLocalHttpListener()
        {
            var listener = new System.Net.HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8000/callback/");
            listener.Start();

            var context = await listener.GetContextAsync(); // waits for Spotify redirect
            var code = context.Request.QueryString["code"];

            // Send a simple response to browser
            var responseString = "<html><body>Login successful! You can close this window.</body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

            listener.Stop();
            return code;
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