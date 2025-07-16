using System.Drawing;
using Microsoft.AspNetCore.Http;

namespace Core.StaticFunctions;

public static class CheckIf
{
    public static bool FileIsImg(IFormFile file) => file.ContentType.Contains("image");

    public static bool ColorIsValid(this string color)
    {
        try
        { 
            _ = ColorTranslator.FromHtml(color);
            return true;
        }
        catch
        {
            return false;
        }
    }
}