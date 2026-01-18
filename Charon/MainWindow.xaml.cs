using System.Windows;
using System.Drawing;
using Emgu.CV;

// IMPORTANT: This namespace makes the .ToBitmap() extension method visible
// If using Emgu.CV v4.4+, this is often usually implicit, but verify you have System.Drawing imported.

namespace Charon
{
    public partial class MainWindow : Window
    {
        // Keep a single instance of the service if it doesn't hold state, 
        // or instantiate per use. Here we instantiate per click for simplicity.
        private readonly VisionService _visionService;

        public MainWindow()
        {
            InitializeComponent();
            _visionService = new VisionService();
        }

        private void BtnTestVision_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Define the Region of Interest (ROI)
                // Let's capture a 500x500 box starting at the top-left of the screen
                var captureZone = new Rectangle(0, 0, 500, 500);

                // 2. Capture the screen
                // 'using' statements are critical here. 
                // Images utilize unmanaged memory; if you don't dispose them, 
                // your bot will crash with an "Out of Memory" error after a few minutes.
                using (var emguImage = _visionService.CaptureRegion(captureZone))
                using (var standardBitmap = emguImage.ToBitmap())
                {
                    // 3. Convert to WPF format and display
                    // We use our helper class to make the bridge
                    ImgDebugView.Source = BitmapSourceConvert.ToBitmapSource(standardBitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Vision Error: {ex.Message}");
            }
        }
    }
}