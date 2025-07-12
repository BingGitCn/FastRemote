
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace FastRemote.Models
{
    public partial class RemoteConnection : ObservableObject
    {
        [ObservableProperty]
        private Guid id;

        [ObservableProperty]
        private string? ipAddress;

        [ObservableProperty]
        private string? userName;

        [ObservableProperty]
        private string? remark;

        [ObservableProperty]
        private string? password; // Note: In a real-world app, this should be securely stored.

        [ObservableProperty]
        private int desktopWidth;

        [ObservableProperty]
        private int desktopHeight;

        [ObservableProperty]
        private int colorDepth;

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

        [ObservableProperty]
        private bool _isReachable; // Connection status: true for reachable, false for not

    public override bool Equals(object? obj)
        {
            return obj is RemoteConnection connection &&
                   Id.Equals(connection.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
