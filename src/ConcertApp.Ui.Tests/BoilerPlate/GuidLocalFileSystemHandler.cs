using WireMock.Handlers;

namespace WeatherApp.Ui.Tests.BoilerPlate;

public sealed class GuidLocalFileSystemHandler : LocalFileSystemHandler
{
    public override void WriteMappingFile(string path, string text)
    {
        var fileInfoPath = new FileInfo(path);
        path = new FileInfo(Path.Join(fileInfoPath.Directory.FullName, Guid.NewGuid() + "." + fileInfoPath.Name))
            .FullName;
        base.WriteMappingFile(path, text);
    }
}