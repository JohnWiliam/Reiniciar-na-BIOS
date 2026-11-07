using System;
using BiosReboot.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BiosReboot.Views;

public sealed partial class MainWindow : FluentWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
        InitializeComponent();

        SystemThemeWatcher.Watch(this);
    }
}
