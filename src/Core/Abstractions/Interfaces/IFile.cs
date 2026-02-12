namespace Abstractions.Interfaces;

public interface IFile
{
    long Length { get; }
    string ContentType { get; }
    string FileName { get; }
    string Name { get; }
    string Extension { get; }
    
    Stream OpenReadStream();
    Task CopyToAsync(Stream target, CancellationToken cancellationToken = default);
    void CopyTo(Stream target);
}