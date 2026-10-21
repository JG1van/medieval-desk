using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MedievalDesk.Core;

public static class AssetNormalizer
{
    private const byte AlphaThreshold = 8;

    public static BitmapSource Load(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return CropTransparent(image);
    }

    public static BitmapSource CropTransparent(BitmapSource source)
    {
        Int32Rect bounds = AlphaBounds(source);
        if (bounds.IsEmpty) return source;
        if (bounds.X == 0 && bounds.Y == 0 &&
            bounds.Width == source.PixelWidth && bounds.Height == source.PixelHeight)
            return source;

        var cropped = new CroppedBitmap(source, bounds);
        cropped.Freeze();
        return cropped;
    }

    public static Int32Rect AlphaBounds(BitmapSource source)
    {
        BitmapSource bgra = source.Format == PixelFormats.Bgra32
            ? source
            : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

        int width = bgra.PixelWidth;
        int height = bgra.PixelHeight;
        int stride = width * 4;
        byte[] pixels = new byte[stride * height];
        bgra.CopyPixels(pixels, stride, 0);

        int minX = width, minY = height, maxX = -1, maxY = -1;
        for (int y = 0; y < height; y++)
        {
            int row = y * stride;
            for (int x = 0; x < width; x++)
            {
                if (pixels[row + x * 4 + 3] <= AlphaThreshold) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < minX || maxY < minY) return Int32Rect.Empty;
        return new Int32Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    public static Size UniformFit(int contentWidth, int contentHeight, double boxWidth, double boxHeight)
    {
        if (contentWidth <= 0 || contentHeight <= 0 || boxWidth <= 0 || boxHeight <= 0) return new Size();
        double scale = Math.Min(boxWidth / contentWidth, boxHeight / contentHeight);
        return new Size(contentWidth * scale, contentHeight * scale);
    }
}
