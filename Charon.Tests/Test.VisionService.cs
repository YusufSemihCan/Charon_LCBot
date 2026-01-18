using Xunit;
using System.Drawing; // System.Drawing.Common
using Charon.Vision;  // Your main project

namespace Charon.Tests
{
    public class VisionServiceTests
    {
        [Fact]
        [Trait("Category", "RequiresMonitor")]
        public void CaptureRegion_Returns_Image_With_Correct_Dimensions()
        {
            // 1. ARRANGE
            var service = new VisionService();
            int width = 100;
            int height = 100;
            var region = new Rectangle(0, 0, width, height);

            // 2. ACT
            // This attempts to take a real screenshot
            using (var result = service.CaptureRegion(region))
            {
                // 3. ASSERT
                Assert.NotNull(result);
                Assert.Equal(width, result.Width);
                Assert.Equal(height, result.Height);
            }
        }
    }
}