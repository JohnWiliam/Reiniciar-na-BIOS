using System;
using System.Threading.Tasks;
using System.Windows;
using BiosReboot.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiosReboot.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly ISystemPowerService _powerService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmRestartCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isBusy;

    public MainViewModel(ISystemPowerService powerService)
    {
        ArgumentNullException.ThrowIfNull(powerService);
        _powerService = powerService;
    }

    private bool CanExecuteActions() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private async Task ConfirmRestartAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _powerService.RestartToFirmwareAsync();
            Application.Current.Shutdown(0);
        }
        catch (Exception ex)
        {
            IsBusy = false;
            MessageBox.Show(
                $"Erro ao reiniciar para a BIOS/UEFI:\n\n{ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteActions))]
    private void Cancel()
    {
        Application.Current.Shutdown(0);
    }
}
