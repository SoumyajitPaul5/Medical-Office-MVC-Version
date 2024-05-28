using SkiaSharp;

namespace MedicalOffice.Utilities
{
    // Utility class for image resizing using SkiaSharp
    public static class ResizeImage
    {
        // Method to resize an image to WebP format with specified max height and width
        public static Byte[] shrinkImageWebp(Byte[] originalImage, int max_height = 75, int max_width = 90)
        {
            using SKMemoryStream sourceStream = new SKMemoryStream(originalImage);
            using SKCodec codec = SKCodec.Create(sourceStream);
            sourceStream.Seek(0);

            using SKImage image = SKImage.FromEncodedData(SKData.Create(sourceStream));
            int newHeight = image.Height;
            int newWidth = image.Width;

            // Adjust the height and width based on max dimensions
            if (max_height > 0 && newHeight > max_height)
            {
                double scale = (double)max_height / newHeight;
                newHeight = max_height;
                newWidth = (int)Math.Floor(newWidth * scale);
            }

            if (max_width > 0 && newWidth > max_width)
            {
                double scale = (double)max_width / newWidth;
                newWidth = max_width;
                newHeight = (int)Math.Floor(newHeight * scale);
            }

            // Create image info based on color space
            var info = codec.Info.ColorSpace.IsSrgb ? new SKImageInfo(newWidth, newHeight) : new SKImageInfo(newWidth, newHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            using SKSurface surface = SKSurface.Create(info);
            using SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };

            // Draw the resized image
            surface.Canvas.Clear(SKColors.White);
            var rect = new SKRect(0, 0, newWidth, newHeight);
            surface.Canvas.DrawImage(image, rect, paint);
            surface.Canvas.Flush();

            using SKImage newImage = surface.Snapshot();
            using SKData newImageData = newImage.Encode(SKEncodedImageFormat.Webp, 100);

            return newImageData.ToArray();
        }

        // ViewModel for image data
        public class ImageVM
        {
            public Byte[] Content { get; set; }
            public string MimeType { get; set; }
        }

        // Method to resize an image to specified format with given max height, width, and quality
        public static ImageVM shrinkImage(Byte[] originalImage, int max_height = 100, int max_width = 120, SKEncodedImageFormat selectedFormat = SKEncodedImageFormat.Webp, int quality = 100)
        {
            using SKMemoryStream sourceStream = new SKMemoryStream(originalImage);
            using SKCodec codec = SKCodec.Create(sourceStream);
            sourceStream.Seek(0);

            using SKImage image = SKImage.FromEncodedData(SKData.Create(sourceStream));
            int newHeight = image.Height;
            int newWidth = image.Width;

            // Adjust the height and width based on max dimensions
            if (max_height > 0 && newHeight > max_height)
            {
                double scale = (double)max_height / newHeight;
                newHeight = max_height;
                newWidth = (int)Math.Floor(newWidth * scale);
            }

            if (max_width > 0 && newWidth > max_width)
            {
                double scale = (double)max_width / newWidth;
                newWidth = max_width;
                newHeight = (int)Math.Floor(newHeight * scale);
            }

            // Create image info based on color space
            var info = codec.Info.ColorSpace.IsSrgb ? new SKImageInfo(newWidth, newHeight) : new SKImageInfo(newWidth, newHeight, SKImageInfo.PlatformColorType, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            using SKSurface surface = SKSurface.Create(info);
            using SKPaint paint = new SKPaint
            {
                IsAntialias = true,
                FilterQuality = SKFilterQuality.High
            };

            // Draw the resized image
            surface.Canvas.Clear(SKColors.White);
            var rect = new SKRect(0, 0, newWidth, newHeight);
            surface.Canvas.DrawImage(image, rect, paint);
            surface.Canvas.Flush();

            using SKImage newImage = surface.Snapshot();
            using SKData newImageData = newImage.Encode(selectedFormat, quality);

            // Create ImageVM with resized image data
            ImageVM imageVM = new ImageVM
            {
                Content = newImageData.ToArray(),
                MimeType = selectedFormat switch
                {
                    SKEncodedImageFormat.Bmp => "image/bmp",
                    SKEncodedImageFormat.Gif => "image/gif",
                    SKEncodedImageFormat.Ico => "image/vnd.microsoft.icon",
                    SKEncodedImageFormat.Jpeg => "image/jpeg",
                    SKEncodedImageFormat.Png => "image/png",
                    SKEncodedImageFormat.Wbmp => "image/wbmp",
                    SKEncodedImageFormat.Webp => "image/webp",
                    SKEncodedImageFormat.Pkm => "image/octet-stream",
                    SKEncodedImageFormat.Ktx => "image/ktx",
                    SKEncodedImageFormat.Astc => "image/png",
                    SKEncodedImageFormat.Dng => "image/DNG",
                    SKEncodedImageFormat.Heif => "image/heif",
                    _ => "image/jpeg",
                }
            };

            return imageVM;
        }
    }
}
