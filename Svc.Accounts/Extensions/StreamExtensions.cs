using SkiaSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static log4net.Appender.ColoredConsoleAppender;

namespace Svc.Accounts.Extensions;

internal static class StreamExtensions
{
    internal static Stream ResizeImage(this Stream imageStream, int targetWidth, int targetHeight)
    {
        if (imageStream == null)
            throw new ArgumentNullException(nameof(imageStream));

        using var inputStream = new SKManagedStream(imageStream);
        using var original = SKBitmap.Decode(inputStream);

        var widthRatio = (float)targetWidth / original.Width;
        var heightRatio = (float)targetHeight / original.Height;
        var scale = Math.Min(widthRatio, heightRatio);

        var scaledWidth = (int)(original.Width * scale);
        var scaledHeight = (int)(original.Height * scale);
        var offsetX = (targetWidth - scaledWidth) / 2;
        var offsetY = (targetHeight - scaledHeight) / 2;

        var skImageInfo = new SKImageInfo(targetWidth, targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        var destRect = new SKRect(offsetX, offsetY, offsetX + scaledWidth, offsetY + scaledHeight);

        using var surface = SKSurface.Create(skImageInfo);

        surface.Canvas
            .Clear(SKColors.Transparent);

        surface.Canvas
            .DrawBitmap(original, destRect);

        using var image = surface
            .Snapshot();

        using var encode = image
            .Encode(SKEncodedImageFormat.Png, 100);

        var outputStream = new MemoryStream();

        encode
            .SaveTo(outputStream);

        outputStream.Position = 0;

        return outputStream;
    }

    internal static async Task SaveFileAsync(this Stream stream, string path, CancellationToken cancellationToken = default)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        if (path == null)
            throw new ArgumentNullException(nameof(path));

        var directory = Path.GetDirectoryName(path);

        if (directory == null)
        {
            throw new NullReferenceException(nameof(directory));
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        stream
            .Seek(0, SeekOrigin.Begin);

        await using var fileStream = File.Create(path);

        await stream
            .CopyToAsync(fileStream, cancellationToken);
    }
}