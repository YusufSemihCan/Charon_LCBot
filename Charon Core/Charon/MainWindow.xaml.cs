using System;
using System.Windows;
using System.Windows.Media.Imaging; // For debug images later

namespace Charon
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log("System initialized. Ready to load Bot.");
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            Log("Start button clicked. (Bot not attached yet)");

            // TODO: Initialize and Start GameBot here
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
            Log("Stop button clicked.");

            // TODO: Stop GameBot here
        }

        // Helper to keep the log clean and thread-safe
        public void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                TxtLog.AppendText($"[{timestamp}] {message}\n");
                TxtLog.ScrollToEnd();
            });
        }
    }
}