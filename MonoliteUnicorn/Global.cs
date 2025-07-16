namespace MonoliteUnicorn;

public static class Global
{
    public static string BaseUrl => "https://localhost:53222";
    public const int UsdId = 3;
    public const string Domain = "all-vp-n.ru";
    public static string? S3BucketName { get; set; }
    public static string? ServiceUrl { get; set; }
}