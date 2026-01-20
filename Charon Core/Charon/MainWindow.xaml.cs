using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Charon.Logic;
using Charon.Logic.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Charon
{
    public partial class MainWindow : Window
    {
        private INavigation? _navigation;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBot();
        }

        private void InitializeBot()
        {
            try
            {
                // BotBootstrapper is in global namespace and exposes Navigation directly
                var bootstrapper = new BotBootstrapper();
                _navigation = bootstrapper.Navigation;
                
                Log("Bot Initialized. Ready.");
            }
            catch (Exception ex)
            {
                Log($"Initialization Failed: {ex.Message}");
            }
        }

        private async void BtnWindow_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Window);
        private async void BtnDrive_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Drive);
        private async void BtnSinners_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Sinners);
        
        private async void BtnLuxEXP_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Luxcavation_EXP);
        private async void BtnLuxThread_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Luxcavation_Thread);
        
        private async void BtnMD_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.MirrorDungeon);
        private async void BtnMDDelving_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.MirrorDungeon_Delving);

        private async void BtnChargeBoxes_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Charge_Boxes);

        private async void BtnChargeModules_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Charge_Modules);

        private async void BtnChargeLunacy_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Charge_Lunacy);

        private async Task RunNav(NavigationState target)
        {
            if (_navigation == null) { Log("Bot not initialized."); return; }
            
            Log($"Navigating to {target}...");
            bool result = await Task.Run(() => _navigation.NavigateTo(target));
            Log(result ? $"Success: Reached {target}" : $"Failed to reach {target}");
            
            Synchronize();
        }

        private void BtnSync_Click(object sender, RoutedEventArgs e)
        {
            Synchronize();
        }

        private void Synchronize()
        {
            if (_navigation == null) return;
            var state = _navigation.SynchronizeState();
            TxtStatus.Text = $"State: {state}";
            Log($"Synchronized: {state}");
        }

        public void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                TxtLog.Text = message;
            });
        }
    }
}