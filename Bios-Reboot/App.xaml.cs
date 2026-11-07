using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BiosReboot.Services;
using BiosReboot.ViewModels;
using BiosReboot.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui.Appearance;

namespace BiosReboot;

public partial class App : Application
{
    private const string MutexName = @"Local\BiosReboot_SingleInstance_Mutex_8F9A2B";
    private Mutex? _singleInstanceMutex;
    private bool _ownsMutex;
    private IHost? _host;

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        DispatcherUnhandledException += OnAppDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex( initiallyOwned: true, MutexName, out _ownsMutex);
        if (!_ownsMutex)
        {
            Shutdown(0);
            return;
        }

        try
        {
            var builder = Host.CreateEmptyApplicationBuilder(settings: null);
            builder.Services.AddSingleton<ISystemPowerService, SystemPowerService>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainWindow>();

            _host = builder.Build();
            await _host.StartAsync();

            // Aplica cor de destaque e tema do sistema antes de criar a janela
            ApplicationAccentColorManager.ApplySystemAccent();
            ApplicationThemeManager.ApplySystemTheme();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro na inicialização do aplicativo:\n\n{ex.Message}",
                "Bios Reboot - Erro de Inicialização",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
        DispatcherUnhandledException -= OnAppDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        try
        {
            if (_host is not null)
            {
                _host.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
                _host.Dispose();
                _host = null;
            }
        }
        catch
        {
            // Ignora exceções durante o encerramento final do host
        }
        finally
        {
            if (_singleInstanceMutex is not null)
            {
                if (_ownsMutex)
                {
                    _singleInstanceMutex.ReleaseMutex();
                }

                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
            }
        }

        base.OnExit(e);
    }

    private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Erro fatal não tratado:\n\n{e.ExceptionObject}",
            "Bios Reboot - Erro Fatal",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void OnAppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Erro na interface:\n\n{e.Exception.Message}",
            "Bios Reboot - Erro",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
    }
}
