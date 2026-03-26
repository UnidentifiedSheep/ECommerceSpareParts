using Abstractions.Interfaces;

namespace Api.Common.Models;

public class FileModel(IFormFile file) : IFile
{
    public long Length => file.Length;
    public string ContentType => file.ContentType;
    public string FileName => file.ContentType;
    public string Name => file.Name;
    public string Extension => Path.GetExtension(file.FileName);

    public Stream OpenReadStream()
    {
        return file.OpenReadStream();
    }

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        return file.CopyToAsync(target, cancellationToken);
    }

    public void CopyTo(Stream target)
    {
        file.CopyTo(target);
    }

    public static IEnumerable<IFile> GetFileModels(IFormFileCollection collection)
    {
        return collection.Select(x => new FileModel(x));
    }
}