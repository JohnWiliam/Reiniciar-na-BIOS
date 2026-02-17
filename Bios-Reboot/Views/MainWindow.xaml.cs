using Bios Reboot.ViewModels;
using Wpf.Ui.Controls;

namespace Bios Reboot.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
