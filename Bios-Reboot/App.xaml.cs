using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using BiosReboot.Services;
using BiosReboot.ViewModels;
using BiosReboot.Views;
using Wpf.Ui;
using System;

namespace BiosReboot;

public partial class App : Application
{
    private static IHost _host = null!;

    public App()
    {
        // Setup global exception handling to catch startup crashes
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            MessageBox.Show($"Fatal Error: {e.ExceptionObject}", "Bios Reboot Crash", MessageBoxButton.OK, MessageBoxImage.Error);

        DispatcherUnhandledException += (s, e) =>
        {
            MessageBox.Show($"UI Error: {e.Exception.Message}", "Bios Reboot Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ISystemPowerService, SystemPowerService>();
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainViewModel>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Host Initialization Failed: {ex}", "Bios Reboot Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // If host fails, we probably can't continue, but let it flow to OnStartup to fail there or just exit
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            if (_host == null)
            {
                MessageBox.Show("Application host is null. Exiting.", "Bios Reboot Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            // Verify if Theme Manager is initializing correctly
            Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();

            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex}", "Bios Reboot Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
