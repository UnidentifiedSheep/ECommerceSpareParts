using SkiaSharp;

namespace Core.StaticFunctions;

public static class ConvertTo
{
    public static Stream Webp(Stream input)
    {
        using var bitmap = SKBitmap.Decode(input);
        if (bitmap == null) throw new NullReferenceException("Не удалось декодировать изображение");
        using var image = SKImage.FromBitmap(bitmap);
        if (image == null) throw new NullReferenceException("Не удалось преобразовать из bit map");
        using var data = image.Encode(SKEncodedImageFormat.Webp, 100);
        var resultStream = new MemoryStream();
        data.SaveTo(resultStream);
        resultStream.Seek(0, SeekOrigin.Begin); 
        return resultStream;
    }
}