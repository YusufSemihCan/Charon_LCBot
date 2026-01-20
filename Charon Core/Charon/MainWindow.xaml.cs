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
        private async void BtnEnterLevel_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.ToBattle);

        private async void BtnTestThreadEntry_Click(object sender, RoutedEventArgs e)
        {
            Log("Testing Thread Entry Sequence...");
            // 1. Go to Thread Tab
            if (await Task.Run(() => _navigation.NavigateTo(NavigationState.Luxcavation_Thread)))
            {
                await Task.Delay(500);
                // 2. Click Enter Level (which should trigger EnterLuxcavationThreadLevel)
                await RunNav(NavigationState.ToBattle);
            }
            else
            {
                Log("Failed to reach Luxcavation Thread tab.");
            }
        }
        
        private async void BtnStartEXP_Click(object sender, RoutedEventArgs e) => await RunBattleSequence(NavigationState.Luxcavation_EXP);
        private async void BtnStartThread_Click(object sender, RoutedEventArgs e) => await RunBattleSequence(NavigationState.Luxcavation_Thread);

        private async Task RunBattleSequence(NavigationState luxType)
        {
            if (_navigation == null) return;
            Log($"Starting Global Battle Sequence for {luxType}...");

            // 1. Navigate to specific Luxcavation Tab
            if (!await Task.Run(() => _navigation.NavigateTo(luxType)))
            {
                Log($"Failed to reach {luxType}. Sequence Aborted.");
                return;
            }

            // 2. Enter Level (Transition to ToBattle)
            Log("Entering Level...");
            await Task.Delay(500);
            if (!await Task.Run(() => _navigation.NavigateTo(NavigationState.ToBattle)))
            {
                 Log("Failed to enter level (ToBattle). Sequence Aborted.");
                 return;
            }

            // 3. Enter Battle (Transition to Battle)
            Log("Entering Battle...");
            await Task.Delay(1000); // Wait for Pre-Battle load
            if (!await Task.Run(() => _navigation.NavigateTo(NavigationState.Battle)))
            {
                Log("Failed to start Battle.");
                return;
            }
            
            Log("Battle Started Successfully!");
        }

        private async void BtnMD_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.MirrorDungeon);
        private async void BtnMDDelving_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.MirrorDungeon_Delving);



        private async void BtnChargeModules_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Charge_Modules);

        private async void BtnChargeLunacy_Click(object sender, RoutedEventArgs e) => await RunNav(NavigationState.Charge_Lunacy);

        // --- Settings Handlers ---
        private void ChkHumanLike_Changed(object sender, RoutedEventArgs e)
        {
            if (_navigation == null) return;
            if (sender is System.Windows.Controls.CheckBox chk) 
            {
                _navigation.HumanLikeMovement = chk.IsChecked ?? true;
                Log($"Human-Like Movement set to: {_navigation.HumanLikeMovement}");
            }
        }

        private void ChkAutoClear_Changed(object sender, RoutedEventArgs e)
        {
            if (_navigation == null) return;
            if (sender is System.Windows.Controls.CheckBox chk) 
            {
                _navigation.AutoClearCursor = chk.IsChecked ?? true;
                Log($"Auto-Clear Cursor set to: {_navigation.AutoClearCursor}");
            }
        }

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