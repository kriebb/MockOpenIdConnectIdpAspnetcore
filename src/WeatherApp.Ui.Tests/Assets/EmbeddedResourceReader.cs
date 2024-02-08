using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace WeatherApp.Ui.Tests.Assets;

/// <summary>
///     Contains methods for reading embedded resources.
/// </summary>
/// https://raw.githubusercontent.com/aspnet/JavaScriptServices/master/src/Microsoft.AspNetCore.NodeServices/Util/EmbeddedResourceReader.cs
public static class EmbeddedResourceReader
{
    private static string EmbeddedResourceQualifier = "WeatherApp.Ui.Tests";

    /// <summary>
    ///     Reads the specified embedded resource from a given assembly.
    /// </summary>
    /// <param name="assemblyContainingType">Any <see cref="Type" /> in the assembly whose resource is to be read.</param>
    /// <param name="path">The path of the resource to be read.</param>
    /// <returns>The contents of the resource.</returns>
    private static string Read(Type assemblyContainingType, string path)
    {
        var asm = assemblyContainingType.GetTypeInfo().Assembly;
        var embeddedResourceName = asm.GetName().Name + path.Replace("/", ".");
        embeddedResourceName = embeddedResourceName.Replace("private.", "");
        using var stream = asm.GetManifestResourceStream(embeddedResourceName);
        if (stream == null)
            throw new ArgumentNullException(nameof(path),
                $"{embeddedResourceName} couldn't be found in the asset as embedded resource");
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }

    public static string ReadAsset(string path)
    {
        return Read(typeof(EmbeddedResourceReader), $".{path}");
    }

    public static byte[] GetData(string name)
    {
        byte[] rawBytes;

        var resourceName = $"{EmbeddedResourceQualifier}.Assets.{name}";

        var certificateStream = typeof(EmbeddedResourceReader).Assembly.GetManifestResourceStream(resourceName);

        try
        {
            if (certificateStream == null)
                throw new ArgumentNullException(nameof(name),
                    $"{name} was not found. Tried it with resourceName: {resourceName}");

            rawBytes = new byte[certificateStream.Length];
            for (var i = 0; i < certificateStream.Length; i++) rawBytes[i] = (byte)certificateStream.ReadByte();

            return rawBytes;
        }


        finally
        {
            certificateStream?.Dispose();
        }
    }

    public static X509Certificate2 GetCertificate(string certificateName, string password)
    {
        var rawBytes = GetData(certificateName);
        if (certificateName.EndsWith("pem"))
        {
            var pem = Encoding.Default.GetString(rawBytes);
            var privKey = Encoding.Default.GetString(GetData(password));

            var cert = X509Certificate2.CreateFromPem(pem, privKey);

            return cert;
        }

        var file = Path.Combine(Path.GetTempPath(), "cert-" + Guid.NewGuid());
        try
        {
            File.WriteAllBytes(file, rawBytes);
            return new X509Certificate2(file, password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }
        finally
        {
            File.Delete(file);
        }
    }


    public static async Task<HttpResponseMessage> GetHttpResonseMessage(string resource, string mediaType)
    {
        var resourceName = $"{EmbeddedResourceQualifier}.Assets." + resource;
        using var stream = typeof(EmbeddedResourceReader).Assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentNullException(nameof(resource));
        var content = new StringContent(body, Encoding.UTF8, mediaType);
        return new HttpResponseMessage
        {
            Content = content
        };
    }

    public static IEnumerable<string> ReadAssets(string resourcePrefix, params string[] ignoreParts)
    {
        var resourceNamePrefix = $"{EmbeddedResourceQualifier}." + resourcePrefix;
        foreach (var resource in typeof(EmbeddedResourceReader).Assembly.GetManifestResourceNames()
                     .Where(resourceName => resourceName.StartsWith(resourceNamePrefix)))
        {
            var shouldIgnore = false;
            foreach (var ignorePart in ignoreParts)
                if (!resource.Contains(ignorePart))
                    shouldIgnore = true;

            if (!shouldIgnore)
                using (var stream = typeof(EmbeddedResourceReader).Assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream))
                {
                    yield return reader.ReadToEnd();
                }
        }
    }
}