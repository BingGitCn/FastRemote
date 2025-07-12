using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System;
using System.Windows;

namespace FastRemote.ViewModels
{
    public partial class CreateConnectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _ipAddress = string.Empty;

        [ObservableProperty]
        private string _userName = string.Empty;

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        [ObservableProperty]
        private string _remark = "新连接"; // Default remark

        [ObservableProperty]
        private int _desktopWidth = 1920;

        [ObservableProperty]
        private int _desktopHeight = 1080;

        [ObservableProperty]
        private int _colorDepth = 32; // Changed default to 32

        // Display Options
        [ObservableProperty]
        private bool _useMultipleMonitors = false;
        [ObservableProperty]
        private int _screenModeId = 2; // Default to full screen

        // Local Resources
        [ObservableProperty]
        private bool _redirectPrinters = false;
        [ObservableProperty]
        private bool _redirectComPorts = false;
        [ObservableProperty]
        private bool _redirectSmartCards = false;
        [ObservableProperty]
        private bool _redirectPosDevices = false;

        // Experience/Performance Options
        [ObservableProperty]
        private bool _disableWallpaper = false;
        [ObservableProperty]
        private bool _allowFontSmoothing = true;
        [ObservableProperty]
        private bool _allowDesktopComposition = true;
        [ObservableProperty]
        private bool _disableFullWindowDrag = false;
        [ObservableProperty]
        private bool _disableMenuAnims = false;
        [ObservableProperty]
        private bool _disableThemes = false;
        [ObservableProperty]
        private bool _disableCursorSetting = false;
        [ObservableProperty]
        private bool _bitmapCachePersistentEnable = true;

        public PasswordBox? PasswordBox { get; set; }

        public Action<bool?>? RequestClose { get; set; }

        public CreateConnectionViewModel()
        {
        }

        public CreateConnectionViewModel(FastRemote.Models.RemoteConnection connection)
        {
            IpAddress = connection.IpAddress ?? string.Empty;
            UserName = connection.UserName ?? string.Empty;
            Password = connection.Password ?? string.Empty; // WARNING: Reading password in plain text is insecure!
            Remark = connection.Remark ?? string.Empty;
            DesktopWidth = connection.DesktopWidth;
            DesktopHeight = connection.DesktopHeight;
            ColorDepth = connection.ColorDepth;

            // Initialize new properties
            UseMultipleMonitors = connection.UseMultipleMonitors;
            ScreenModeId = connection.ScreenModeId;
            RedirectPrinters = connection.RedirectPrinters;
            RedirectComPorts = connection.RedirectComPorts;
            RedirectSmartCards = connection.RedirectSmartCards;
            RedirectPosDevices = connection.RedirectPosDevices;
            DisableWallpaper = connection.DisableWallpaper;
            AllowFontSmoothing = connection.AllowFontSmoothing;
            AllowDesktopComposition = connection.AllowDesktopComposition;
            DisableFullWindowDrag = connection.DisableFullWindowDrag;
            DisableMenuAnims = connection.DisableMenuAnims;
            DisableThemes = connection.DisableThemes;
            DisableCursorSetting = connection.DisableCursorSetting;
            BitmapCachePersistentEnable = connection.BitmapCachePersistentEnable;
        }

        [RelayCommand]
        private void Ok()
        {
            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                MessageBox.Show("IP 地址不能为空。", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("用户名不能为空。", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PasswordBox != null)
            {
                Password = PasswordBox.Password;
            }
            RequestClose?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke(false);
        }
    }
}