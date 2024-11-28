using Avalonia.Controls;
using DesktopNotifications;
using DesktopNotifications.FreeDesktop;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace TrumpSuitGameKnocked.Views;

public partial class MainWindow : Window
{
    private static INotificationManager notification;
    private static INotificationManager CreateManager()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return new FreeDesktopNotificationManager();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new DesktopNotifications.Windows.WindowsNotificationManager(null);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new DesktopNotifications.Apple.AppleNotificationManager();

        throw new PlatformNotSupportedException();
    }
    public MainWindow()
    {
        InitializeComponent();
        notification = CreateManager();
        notification.Initialize();
    }

    public static void MakeNotification(string titolo, string testo)
    {
        Notification not = new Notification
        {
            Title = titolo,
            Body = testo
        };
        notification.ShowNotification(not);

    }
}
