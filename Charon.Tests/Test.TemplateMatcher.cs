using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Charon.Vision;

namespace Charon.Tests
{
    public class TemplateMatcherTests
    {
        // Update these if your extensions are different (.jpg, .bmp)
        private const string ScreenFilename = "Test_Image.png";
        private const string TemplateFilename = "Test_Target.png";

        [Fact]
        public void Find_Returns_Location_When_Target_Is_Present()
        {
            // 1. ARRANGE
            var matcher = new TemplateMatcher();
            string screenPath = GetAssetPath(ScreenFilename);
            string templatePath = GetAssetPath(TemplateFilename);

            using (var screen = new Image<Bgr, byte>(screenPath))
            using (var template = new Image<Bgr, byte>(templatePath))
            {
                // 2. ACT
                Rectangle result = matcher.Find(screen, template, 0.9);

                // 3. ASSERT
                Assert.NotEqual(Rectangle.Empty, result);
                Assert.Equal(template.Width, result.Width);
                Assert.Equal(template.Height, result.Height);
            }
        }

        [Fact]
        public void Find_Returns_Empty_When_Target_Is_Missing()
        {
            // 1. ARRANGE
            var matcher = new TemplateMatcher();
            string templatePath = GetAssetPath(TemplateFilename);

            // Create a black box "screen". Target is definitely not here.
            using (var blankScreen = new Image<Bgr, byte>(800, 600, new Bgr(0, 0, 0)))
            using (var template = new Image<Bgr, byte>(templatePath))
            {
                // 2. ACT
                Rectangle result = matcher.Find(blankScreen, template, 0.9);

                // 3. ASSERT
                Assert.Equal(Rectangle.Empty, result);
            }
        }

        private string GetAssetPath(string filename)
        {
            // Define that we are looking inside the "Assets" subfolder
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string path = Path.Combine(baseDir, "Assets", filename);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"TEST FAILURE: Could not find '{filename}' inside the Assets folder. " +
                    $"checked path: {path}");
            }
            return path;
        }
    }
}