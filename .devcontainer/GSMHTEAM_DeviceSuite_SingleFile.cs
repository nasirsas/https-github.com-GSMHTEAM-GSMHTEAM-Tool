// GSMHTEAM DeviceSuite - Single File Windows Application
// .NET 8 WPF single-file starter.
// This build provides: login, local license/demo validation, device detection,
// SQLite database, API health check, update manifest check, logs and UI.
// Device-servicing/bypass/IMEI/serial/account-modification operations are NOT implemented.
//
// Create a WPF project targeting net8.0-windows, replace App.xaml.cs with this file,
// and remove the generated MainWindow files. Add Microsoft.Data.Sqlite package.
// For a simple build:
//   dotnet new wpf -n GSMHTEAMDeviceSuite
//   dotnet add package Microsoft.Data.Sqlite
//   replace App.xaml.cs with this file
//   dotnet run
//
// Publish:
//   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;

namespace GSMHTEAMDeviceSuite;

public class App : Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.Startup += (_, _) => app.MainWindow = new MainWindow();
        app.Run();
    }
}

public sealed class MainWindow : Window
{
    const string AppName = "GSMHTEAM DeviceSuite";
    const string Version = "1.0.0";

    readonly string DbPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "GSMHTEAM", "devicesuite.db");

    readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };
    readonly ListBox LogBox = new();
    readonly TextBlock Status = new();
    readonly TextBlock DeviceLabel = new();
    readonly TextBlock LicenseLabel = new();
    readonly TextBlock ApiLabel = new();
    readonly StackPanel Workspace = new();

    public MainWindow()
    {
        Title = $"{AppName} V{Version}";
        Width = 1200;
        Height = 760;
        MinWidth = 1000;
        MinHeight = 650;
        Background = Brush("#030712");
        Foreground = Brushes.White;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        InitializeDatabase();

        if (!ShowLogin())
        {
            Close();
            return;
        }

        BuildUi();
        DetectDevices();
        Log("GSMHTEAM DeviceSuite initialized.", "SUCCESS");
        Log("Protected diagnostic build started.", "INFO");
    }

    bool ShowLogin()
    {
        var login = new Window
        {
            Title = "GSMHTEAM Secure Login",
            Width = 430,
            Height = 360,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            Background = Brush("#07101f"),
            Foreground = Brushes.White
        };

        var panel = new StackPanel { Margin = new Thickness(35) };
        panel.Children.Add(Text("GSMHTEAM", 28, Brushes.Orange, true));
        panel.Children.Add(Text("DeviceSuite Secure Login", 16, Brushes.White, true));
        panel.Children.Add(Text("Local development credentials are used by this starter.", 11, Brushes.Gray));

        var user = new TextBox { Margin = new Thickness(0, 22, 0, 8), Padding = new Thickness(10), Text = "admin" };
        var pass = new PasswordBox { Padding = new Thickness(10) };
        var result = Text("", 11, Brushes.OrangeRed);

        var button = Button("LOGIN", Brushes.OrangeRed);
        button.Click += (_, _) =>
        {
            // Development-only login. Production authentication belongs on the backend.
            if (user.Text.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
                && pass.Password == "GSMHTEAM")
            {
                login.DialogResult = true;
                login.Close();
            }
            else
            {
                result.Text = "Invalid development credentials.";
            }
        };

        panel.Children.Add(Text("Username", 11, Brushes.Gray));
        panel.Children.Add(user);
        panel.Children.Add(Text("Password", 11, Brushes.Gray));
        panel.Children.Add(pass);
        panel.Children.Add(button);
        panel.Children.Add(result);

        login.Content = panel;
        return login.ShowDialog() == true;
    }

    void BuildUi()
    {
        var root = new Grid { Margin = new Thickness(14) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(145) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
        var title = Text("GSMHTEAM  A12-A13 Pro Suite", 22, Brushes.White, true);
        DockPanel.SetDock(title, Dock.Left);
        header.Children.Add(title);

        var version = Text($"V{Version}  •  Protected Diagnostic Build", 11, Brushes.Orange);
        version.HorizontalAlignment = HorizontalAlignment.Right;
        header.Children.Add(version);
        root.Children.Add(header);

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) });
        Grid.SetRow(columns, 1);
        root.Children.Add(columns);

        var devicePanel = Card();
        Grid.SetColumn(devicePanel, 0);
        columns.Children.Add(devicePanel);

        devicePanel.Children.Add(Text("📱 DEVICE INFORMATION", 13, Brushes.SkyBlue, true));
        DeviceLabel.Text = "No compatible device detected.";
        DeviceLabel.Margin = new Thickness(0, 18, 0, 18);
        DeviceLabel.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        DeviceLabel.FontSize = 13;
        devicePanel.Children.Add(DeviceLabel);

        var detect = Button("DETECT DEVICES", Brushes.OrangeRed);
        detect.Click += (_, _) => DetectDevices();
        devicePanel.Children.Add(detect);

        var diagnostics = Button("RUN SAFE DIAGNOSTICS", Brushes.SteelBlue);
        diagnostics.Click += (_, _) =>
        {
            Log("Safe diagnostic scan requested.", "INFO");
            Log("No protected-device modification operation is executed.", "WARNING");
        };
        devicePanel.Children.Add(diagnostics);

        var license = Card();
        Grid.SetColumn(license, 1);
        columns.Children.Add(license);

        license.Children.Add(Text("🔐 GSMHTEAM CONTROL HUB", 13, Brushes.SkyBlue, true));
        LicenseLabel.Text = "License: DEVELOPMENT";
        LicenseLabel.Margin = new Thickness(0, 12, 0, 4);
        license.Children.Add(LicenseLabel);

        ApiLabel.Text = "API: NOT CHECKED";
        license.Children.Add(ApiLabel);

        var row1 = new WrapPanel { Margin = new Thickness(0, 20, 0, 0) };
        var api = Button("CHECK API", Brushes.OrangeRed);
        api.Click += async (_, _) => await CheckApi();
        row1.Children.Add(api);

        var update = Button("CHECK UPDATE", Brushes.SteelBlue);
        update.Click += async (_, _) => await CheckUpdate();
        row1.Children.Add(update);

        var save = Button("SAVE SESSION", Brushes.DarkSlateGray);
        save.Click += (_, _) =>
        {
            SaveSession();
            Log("Session saved to local SQLite database.", "SUCCESS");
        };
        row1.Children.Add(save);

        var clear = Button("CLEAR LOG", Brushes.DarkSlateGray);
        clear.Click += (_, _) => LogBox.Items.Clear();
        row1.Children.Add(clear);

        license.Children.Add(row1);

        var note = Text(
            "Architecture ready for a GSMHTEAM backend: license validation, customer/device records, API authentication and signed update manifests.",
            11, Brushes.Gray);
        note.TextWrapping = TextWrapping.Wrap;
        note.Margin = new Thickness(0, 18, 0, 0);
        license.Children.Add(note);

        var logCard = Card();
        Grid.SetRow(logCard, 2);
        root.Children.Add(logCard);

        var logHeader = new DockPanel();
        logHeader.Children.Add(Text("💻 SECURE TERMINAL LOG", 12, Brushes.SkyBlue, true));
        logCard.Children.Add(logHeader);

        LogBox.Background = Brush("#050811");
        LogBox.Foreground = Brushes.LightGray;
        LogBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        LogBox.FontSize = 11;
        LogBox.BorderThickness = new Thickness(0);
        logCard.Children.Add(LogBox);

        var footer = new DockPanel { Margin = new Thickness(0, 10, 0, 0) };
        Status.Text = "● STATUS: READY";
        Status.Foreground = Brushes.LightGreen;
        footer.Children.Add(Status);

        var enterprise = Text("ENTERPRISE: GSMHTEAM", 11, Brushes.Orange, true);
        enterprise.HorizontalAlignment = HorizontalAlignment.Right;
        footer.Children.Add(enterprise);

        Grid.SetRow(footer, 3);
        root.Children.Add(footer);

        Content = root;
    }

    void DetectDevices()
    {
        try
        {
            var devices = new List<string>();

            using var searcher = new ManagementObjectSearcher(
                "SELECT Name, Manufacturer, PNPDeviceID FROM Win32_PnPEntity WHERE PNPDeviceID IS NOT NULL");

            foreach (ManagementObject item in searcher.Get())
            {
                var name = item["Name"]?.ToString() ?? "";
                var manufacturer = item["Manufacturer"]?.ToString() ?? "";

                if (name.Contains("Apple", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("USB", StringComparison.OrdinalIgnoreCase))
                {
                    devices.Add($"{name} | {manufacturer}");
                }
            }

            if (devices.Count == 0)
            {
                DeviceLabel.Text = "No supported device detected.\n\nConnect a USB device and press Detect.";
                Status.Text = "● STATUS: WAITING FOR DEVICE";
                Status.Foreground = Brushes.Gold;
                Log("No matching USB/mobile device detected.", "WARNING");
                return;
            }

            DeviceLabel.Text = string.Join("\n\n", devices.Take(8));
            Status.Text = "● STATUS: DEVICE DETECTED";
            Status.Foreground = Brushes.LightGreen;

            foreach (var d in devices.Take(8))
                Log($"Detected: {d}", "SUCCESS");
        }
        catch (Exception ex)
        {
            Log($"Device detection error: {ex.Message}", "ERROR");
        }
    }

    async Task CheckApi()
    {
        // Replace with your real GSMHTEAM HTTPS endpoint after deploying the backend.
        const string endpoint = "https://example.com/health";

        try
        {
            ApiLabel.Text = "API: CHECKING...";
            using var response = await Http.GetAsync(endpoint);
            ApiLabel.Text = $"API: {(response.IsSuccessStatusCode ? "ONLINE" : "ERROR")}";
            Log($"API health check: HTTP {(int)response.StatusCode}.", response.IsSuccessStatusCode ? "SUCCESS" : "ERROR");
        }
        catch
        {
            ApiLabel.Text = "API: NOT CONFIGURED";
            Log("API endpoint is not configured or unreachable.", "WARNING");
        }
    }

    async Task CheckUpdate()
    {
        // Replace with your signed GSMHTEAM update-manifest URL.
        const string manifestUrl = "https://example.com/gsmhteam/update.json";

        try
        {
            var json = await Http.GetStringAsync(manifestUrl);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(json);

            if (manifest is null)
            {
                Log("Update manifest was empty or invalid.", "ERROR");
                return;
            }

            if (Version != manifest.Version)
                Log($"Update available: V{manifest.Version}. Download: {manifest.Url}", "WARNING");
            else
                Log("GSMHTEAM DeviceSuite is up to date.", "SUCCESS");
        }
        catch
        {
            Log("Update server is not configured or unreachable.", "WARNING");
        }
    }

    void InitializeDatabase()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);

        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Sessions(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreatedUtc TEXT NOT NULL,
                DeviceInfo TEXT,
                ApplicationVersion TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Logs(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreatedUtc TEXT NOT NULL,
                Level TEXT NOT NULL,
                Message TEXT NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    void SaveSession()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "INSERT INTO Sessions(CreatedUtc, DeviceInfo, ApplicationVersion) VALUES($date,$device,$version)";
        cmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$device", DeviceLabel.Text);
        cmd.Parameters.AddWithValue("$version", Version);
        cmd.ExecuteNonQuery();
    }

    void Log(string message, string level)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}";
        LogBox.Items.Add(line);
        if (LogBox.Items.Count > 0)
            LogBox.ScrollIntoView(LogBox.Items[^1]);

        try
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Logs(CreatedUtc,Level,Message) VALUES($date,$level,$message)";
            cmd.Parameters.AddWithValue("$date", DateTime.UtcNow.ToString("O"));
            cmd.Parameters.AddWithValue("$level", level);
            cmd.Parameters.AddWithValue("$message", message);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    static Border Card() =>
        new()
        {
            Background = Brush("#0b1329"),
            BorderBrush = Brush("#1e293b"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Margin = new Thickness(6)
        };

    static TextBlock Text(string value, double size, Brush color, bool bold = false) =>
        new()
        {
            Text = value,
            FontSize = size,
            Foreground = color,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Margin = new Thickness(0, 3, 0, 3)
        };

    static Button Button(string caption, Brush background) =>
        new()
        {
            Content = caption,
            Background = background,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(14, 9, 14, 9),
            Margin = new Thickness(0, 5, 8, 5),
            FontWeight = FontWeights.Bold
        };

    static Brush Brush(string hex) =>
        (Brush)new BrushConverter().ConvertFrom(hex)!;

    sealed class UpdateManifest
    {
        public string Version { get; set; } = "";
        public string Url { get; set; } = "";
        public string Sha256 { get; set; } = "";
    }
}
