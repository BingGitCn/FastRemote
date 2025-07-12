using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System;
using System.Linq;
using System.Net.NetworkInformation; // Added for Ping
using System.Threading.Tasks; // Added for Task.Delay
using FastRemote.Models;
using Prism.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using FastRemote.Views;

namespace  FastRemote.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = "Fast Remote";

        private ObservableCollection<RemoteConnection> _remoteConnections;
        public ObservableCollection<RemoteConnection> RemoteConnections
        {
            get => _remoteConnections;
            set => SetProperty(ref _remoteConnections, value);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSelectedConnectionCommand))]
        [NotifyCanExecuteChangedFor(nameof(EditSelectedConnectionCommand))]
        private RemoteConnection? _selectedRemoteConnection;

        private string _dataFilePath;

        public MainWindowViewModel()
        {
            _dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FastRemote", "connections.json");
            LoadConnections();
            StartConnectionStatusChecker();
        }

        private void LoadConnections()
        {
            if (File.Exists(_dataFilePath))
            {
                try
                {
                    var jsonString = File.ReadAllText(_dataFilePath);
                    var loadedConnections = JsonSerializer.Deserialize<ObservableCollection<RemoteConnection>>(jsonString);
                    if (loadedConnections != null)
                    {
                        RemoteConnections = loadedConnections;
                        RemoteConnections.CollectionChanged += (sender, e) => SaveConnections();
                        foreach (var connection in RemoteConnections)
                        {
                            connection.PropertyChanged += (sender, e) => SaveConnections();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error or show a message to the user
                    Debug.WriteLine($"Error loading connections: {ex.Message}");
                }
            }
            else
            {
                RemoteConnections = new ObservableCollection<RemoteConnection>();
                RemoteConnections.CollectionChanged += (sender, e) => SaveConnections();
            }
        }

        private void SaveConnections()
        {
            try
            {
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                var jsonString = JsonSerializer.Serialize(RemoteConnections);
                File.WriteAllText(_dataFilePath, jsonString);
            }
            catch (Exception ex)
            {
                // Log the error or show a message to the user
                Debug.WriteLine($"Error saving connections: {ex.Message}");
            }
        }

        private void StartConnectionStatusChecker()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                while (true)
                {
                    await CheckConnectionStatus();
                    await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(30)); // Check every 30 seconds
                }
            });
        }

        private async Task CheckConnectionStatus()
        {
            foreach (var connection in RemoteConnections)
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        var reply = await ping.SendPingAsync(connection.IpAddress, 1000); // 1 second timeout
                        connection.IsReachable = (reply.Status == IPStatus.Success);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error pinging {connection.IpAddress}: {ex.Message}");
                    connection.IsReachable = false;
                }
            }
        }

        [RelayCommand]
        void CreateRemoteConnection()
        {
            var dialog = new CreateConnectionDialog();
            var viewModel = new CreateConnectionViewModel();
            dialog.DataContext = viewModel;

            if (dialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(viewModel.IpAddress))
                {
                    RemoteConnections.Add(new RemoteConnection
                    {
                        Id = Guid.NewGuid(),
                        Remark = viewModel.Remark,
                        IpAddress = viewModel.IpAddress,
                        UserName = viewModel.UserName,
                        Password = viewModel.Password, // WARNING: Storing password in plain text is insecure!
                        DesktopWidth = viewModel.DesktopWidth,
                        DesktopHeight = viewModel.DesktopHeight,
                        ColorDepth = viewModel.ColorDepth,
                        UseMultipleMonitors = viewModel.UseMultipleMonitors,
                        ScreenModeId = viewModel.ScreenModeId,
                        RedirectPrinters = viewModel.RedirectPrinters,
                        RedirectComPorts = viewModel.RedirectComPorts,
                        RedirectSmartCards = viewModel.RedirectSmartCards,
                        RedirectPosDevices = viewModel.RedirectPosDevices,
                        DisableWallpaper = viewModel.DisableWallpaper,
                        AllowFontSmoothing = viewModel.AllowFontSmoothing,
                        AllowDesktopComposition = viewModel.AllowDesktopComposition,
                        DisableFullWindowDrag = viewModel.DisableFullWindowDrag,
                        DisableMenuAnims = viewModel.DisableMenuAnims,
                        DisableThemes = viewModel.DisableThemes,
                        DisableCursorSetting = viewModel.DisableCursorSetting,
                        BitmapCachePersistentEnable = viewModel.BitmapCachePersistentEnable
                    });
                    SaveConnections();
                    // Trigger immediate status check for the new connection
                    _ = CheckConnectionStatus(); 
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanDeleteSelectedConnection))]
        void DeleteSelectedConnection()
        {
            if (SelectedRemoteConnection != null)
            {
                Debug.WriteLine($"Attempting to delete selected connection: {SelectedRemoteConnection.Remark} ({SelectedRemoteConnection.IpAddress})");
                RemoteConnections.Remove(SelectedRemoteConnection);
                SaveConnections();
                SelectedRemoteConnection = null; // Clear selection after deletion
            }
        }

        private bool CanDeleteSelectedConnection() => SelectedRemoteConnection != null;

        [RelayCommand(CanExecute = nameof(CanEditSelectedConnection))]
        void EditSelectedConnection()
        {
            if (SelectedRemoteConnection != null)
            {
                var dialog = new CreateConnectionDialog();
                var viewModel = new CreateConnectionViewModel(SelectedRemoteConnection);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    // Update the existing connection with new values
                    SelectedRemoteConnection.Remark = viewModel.Remark;
                    SelectedRemoteConnection.IpAddress = viewModel.IpAddress;
                    SelectedRemoteConnection.UserName = viewModel.UserName;
                    SelectedRemoteConnection.Password = viewModel.Password; // WARNING: Storing password in plain text is insecure!
                    SelectedRemoteConnection.DesktopWidth = viewModel.DesktopWidth;
                    SelectedRemoteConnection.DesktopHeight = viewModel.DesktopHeight;
                    SelectedRemoteConnection.ColorDepth = viewModel.ColorDepth;
                    SelectedRemoteConnection.UseMultipleMonitors = viewModel.UseMultipleMonitors;
                    SelectedRemoteConnection.ScreenModeId = viewModel.ScreenModeId;
                    SelectedRemoteConnection.RedirectPrinters = viewModel.RedirectPrinters;
                    SelectedRemoteConnection.RedirectComPorts = viewModel.RedirectComPorts;
                    SelectedRemoteConnection.RedirectSmartCards = viewModel.RedirectSmartCards;
                    SelectedRemoteConnection.RedirectPosDevices = viewModel.RedirectPosDevices;
                    SelectedRemoteConnection.DisableWallpaper = viewModel.DisableWallpaper;
                    SelectedRemoteConnection.AllowFontSmoothing = viewModel.AllowFontSmoothing;
                    SelectedRemoteConnection.AllowDesktopComposition = viewModel.AllowDesktopComposition;
                    SelectedRemoteConnection.DisableFullWindowDrag = viewModel.DisableFullWindowDrag;
                    SelectedRemoteConnection.DisableMenuAnims = viewModel.DisableMenuAnims;
                    SelectedRemoteConnection.DisableThemes = viewModel.DisableThemes;
                    SelectedRemoteConnection.DisableCursorSetting = viewModel.DisableCursorSetting;
                    SelectedRemoteConnection.BitmapCachePersistentEnable = viewModel.BitmapCachePersistentEnable;

                    SaveConnections();
                    // Trigger immediate status check for the updated connection
                    _ = CheckConnectionStatus();
                }
            }
        }

        private bool CanEditSelectedConnection() => SelectedRemoteConnection != null;

        [RelayCommand]
        void DeleteConnection(RemoteConnection connection)
        {
            if (connection != null)
            {
                Debug.WriteLine($"Attempting to delete connection: {connection.Remark} ({connection.IpAddress})");
                RemoteConnections.Remove(connection);
                SaveConnections();
            }
            else
            {
                Debug.WriteLine("DeleteConnection command received a null connection.");
            }
        }

        [RelayCommand]
        void ConnectToRemote(RemoteConnection connection)
        {
            if (connection != null)
            {
                // WARNING: Storing password in plain text is insecure!
                // Generating a temporary RDP file for direct connection
                var rdpContent = new System.Text.StringBuilder();
                rdpContent.AppendLine($"full address:s:{connection.IpAddress}");
                rdpContent.AppendLine($"username:s:{connection.UserName}");
                // Removed password:b: line for security and compatibility testing

                // Common RDP settings from user's working content
                rdpContent.AppendLine($"screen mode id:i:{connection.ScreenModeId}");
                rdpContent.AppendLine($"use multimon:i:{(connection.UseMultipleMonitors ? 1 : 0)}");
                rdpContent.AppendLine($"desktopwidth:i:{connection.DesktopWidth}");
                rdpContent.AppendLine($"desktopheight:i:{connection.DesktopHeight}");
                rdpContent.AppendLine($"session bpp:i:{connection.ColorDepth}");
                rdpContent.AppendLine("winposstr:s:0,3,0,0,800,600");
                rdpContent.AppendLine("compression:i:1");
                rdpContent.AppendLine("keyboardhook:i:2");
                rdpContent.AppendLine("audiocapturemode:i:0");
                rdpContent.AppendLine("videoplaybackmode:i:1");
                rdpContent.AppendLine("connection type:i:7");
                rdpContent.AppendLine("networkautodetect:i:1");
                rdpContent.AppendLine("bandwidthautodetect:i:1");
                rdpContent.AppendLine("displayconnectionbar:i:1");
                rdpContent.AppendLine("enableworkspacereconnect:i:0");
                rdpContent.AppendLine("remoteappmousemoveinject:i:1");
                rdpContent.AppendLine($"disable wallpaper:i:{(connection.DisableWallpaper ? 1 : 0)}");
                rdpContent.AppendLine($"allow font smoothing:i:{(connection.AllowFontSmoothing ? 1 : 0)}");
                rdpContent.AppendLine($"allow desktop composition:i:{(connection.AllowDesktopComposition ? 1 : 0)}");
                rdpContent.AppendLine($"disable full window drag:i:{(connection.DisableFullWindowDrag ? 1 : 0)}");
                rdpContent.AppendLine($"disable menu anims:i:{(connection.DisableMenuAnims ? 1 : 0)}");
                rdpContent.AppendLine($"disable themes:i:{(connection.DisableThemes ? 1 : 0)}");
                rdpContent.AppendLine($"disable cursor setting:i:{(connection.DisableCursorSetting ? 1 : 0)}");
                rdpContent.AppendLine($"bitmapcachepersistenable:i:{(connection.BitmapCachePersistentEnable ? 1 : 0)}");
                rdpContent.AppendLine("audiomode:i:0");
                rdpContent.AppendLine("redirectprinters:i:1");
                rdpContent.AppendLine("redirectlocation:i:0");
                rdpContent.AppendLine("redirectcomports:i:0");
                rdpContent.AppendLine("redirectsmartcards:i:1");
                rdpContent.AppendLine("redirectwebauthn:i:1");
                rdpContent.AppendLine("redirectclipboard:i:1");
                rdpContent.AppendLine("redirectposdevices:i:0");
                rdpContent.AppendLine("autoreconnection enabled:i:1");
                rdpContent.AppendLine("authentication level:i:2");
                rdpContent.AppendLine("prompt for credentials:i:0");
                rdpContent.AppendLine("negotiate security layer:i:1");
                rdpContent.AppendLine("remoteapplicationmode:i:0");
                rdpContent.AppendLine("alternate shell:s:");
                rdpContent.AppendLine("shell working directory:s:");
                rdpContent.AppendLine("gatewayhostname:s:");
                rdpContent.AppendLine("gatewayusagemethod:i:4");
                rdpContent.AppendLine("gatewaycredentialssource:i:4");
                rdpContent.AppendLine("gatewayprofileusagemethod:i:0");
                rdpContent.AppendLine("promptcredentialonce:i:0");
                rdpContent.AppendLine("gatewaybrokeringtype:i:0");
                rdpContent.AppendLine("use redirection server name:i:0");
                rdpContent.AppendLine("rdgiskdcproxy:i:0");
                rdpContent.AppendLine("kdcproxyname:s:");
                rdpContent.AppendLine("enablerdsaadauth:i:0");

                var tempRdpFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.rdp");
                
                Debug.WriteLine($"Generated RDP Content:\n{rdpContent.ToString()}");
                Debug.WriteLine($"Temporary RDP File Path: {tempRdpFilePath}");

                File.WriteAllText(tempRdpFilePath, rdpContent.ToString());

                try
                {
                    Process.Start("mstsc.exe", tempRdpFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error launching mstsc: {ex.Message}");
                    MessageBox.Show($"Error launching remote desktop: {ex.Message}\n\nRDP Content:\n{rdpContent.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Clean up the temporary RDP file after a short delay
                    // This is a simple approach; a more robust solution might use a background task
                    System.Threading.Tasks.Task.Delay(5000).ContinueWith(_ =>
                    {
                        try
                        {
                            if (File.Exists(tempRdpFilePath))
                            {
                                File.Delete(tempRdpFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error deleting temporary RDP file: {ex.Message}");
                        }
                    });
                }
            }
        }
    }
}