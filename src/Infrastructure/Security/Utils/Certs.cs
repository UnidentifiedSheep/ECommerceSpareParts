using System.Security.Cryptography.X509Certificates;

namespace Security.Utils;

public static class Certs
{
    public static void RegisterCerts(string certsDir)
    {
        if (!Directory.Exists(certsDir)) return;
        foreach (var certFile in Directory.GetFiles(certsDir, "*.crt"))
        {
            var cert = X509CertificateLoader.LoadCertificateFromFile(certFile);
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
        }
    }
}