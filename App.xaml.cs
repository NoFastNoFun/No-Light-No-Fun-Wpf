using System.Configuration;
using System.Data;
using System.Windows;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.ViewModels;

namespace No_Fast_No_Fun_Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private UdpListenerService _listener;

    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        // 1) Crée et démarre le listener une seule fois
        _listener = new UdpListenerService();
        _listener.Start(8765);

        // 2) Instancie ton MainWindowViewModel en passant le listener
        var mainVm = new MainWindowViewModel(_listener);

        // 3) Crée la fenêtre principale, injecte le VM et affiche-la
        var window = new MainWindow {
            DataContext = mainVm
        };
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e) {
        // Stoppe proprement l’écoute UDP
        _listener?.Stop();
        base.OnExit(e);
    }
}

