using BiosReboot.ViewModels;
using Wpf.Ui.Controls;

namespace BiosReboot.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
