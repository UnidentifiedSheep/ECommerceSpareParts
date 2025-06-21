using Microsoft.AspNetCore.Http;

namespace Core.StaticFunctions;

public static class CheckIf
{
    public static bool FileIsImg(IFormFile file) => file.ContentType.Contains("image");
}