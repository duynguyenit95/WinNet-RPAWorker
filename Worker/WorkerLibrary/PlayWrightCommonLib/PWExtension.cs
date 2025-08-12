using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayWrightCommonLib
{
    public class PageExtension
    {
        private readonly IPage page;

        public PageExtension(IPage _page)
        {
            this.page = _page;
        }
        public async Task<string> Screenshot(string subject, ILocator locator = null, bool fullPage = false)
        {
            try
            {
                var imageCapturePath = Path.Combine(AppContext.BaseDirectory, "ImageCapture");
                if (!Directory.Exists(imageCapturePath))
                {
                    Directory.CreateDirectory(imageCapturePath);
                }
                var imageCaptureName = $"{subject}_{DateTime.Now.ToString("yyyyMMdd_HHmmss.fff")}.png";
                var imageFullPath = Path.Combine(imageCapturePath, imageCaptureName);
                if (locator != null)
                {
                    await locator.ScreenshotAsync(new LocatorScreenshotOptions
                    {
                        Path = imageFullPath
                    });
                }
                else
                {
                    await page.ScreenshotAsync(new PageScreenshotOptions()
                    {

                        Path = imageFullPath,
                        FullPage = fullPage,
                    });
                }
                return imageFullPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
