using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BiosReboot.Services;
using System.Windows;

namespace BiosReboot.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ISystemPowerService _powerService;

    public MainViewModel(ISystemPowerService powerService)
    {
        _powerService = powerService;
    }

    [RelayCommand]
    private void ConfirmRestart()
    {
        try 
        {
            _powerService.RestartToFirmware();
            Application.Current.Shutdown();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Application.Current.Shutdown();
    }
}
