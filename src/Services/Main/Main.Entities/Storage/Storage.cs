using System.Linq.Expressions;
using BulkValidation.Core.Attributes;
using Domain;
using Domain.Extensions;
using Domain.Interfaces;
using Domain.Validation;
using Exceptions;
using Main.Enums;

namespace Main.Entities.Storage;

public class Storage : AuditableEntity<Storage, string>, ILinqEntity<Storage, string>
{
    private readonly List<StorageOwner> _owners = [];

    private Storage() { }

    private Storage(string code, StorageType type)
    {
        SetCode(code);
        SetType(type);
    }

    [Validate]
    public string Code { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Location { get; private set; }

    public StorageType Type { get; private set; }
    public IReadOnlyCollection<StorageOwner> Owners => _owners;

    public static Expression<Func<Storage, string>> GetKeySelector() { return x => x.Code; }

    public static Expression<Func<Storage, bool>> GetEqualityExpression(string key)
    {
        return x => x.Code == key;
    }

    public static Storage Create(string code, StorageType type) { return new Storage(code, type); }

    private void SetCode(string code)
    {
        Code = code
            .Trim()
            .EnsureNotNullOrEmpty("storage.code.not.empty")
            .EnsureMinLength(6, "storage.code.min.length")
            .EnsureMaxLength(128, "storage.code.max.length");
    }

    public void SetType(StorageType type)
    {
        if (Type == type) return;

        if (type == StorageType.SupplierStorage && _owners.Count != 0)
            throw new InvalidInputException("storage.type.change.restricted");

        Type = type;
    }

    public void SetDescription(string? description)
    {
        Description = description
            .NullIfWhiteSpace()
            ?
            .EnsureMaxLength(256, "storage.description.max.length");
    }

    public void SetLocation(string? location)
    {
        Location = location
            .NullIfWhiteSpace()
            ?
            .EnsureMaxLength(256, "storage.location.max.length");
    }

    public void AddOwner(Guid userId)
    {
        if (_owners.Any(x => x.UserId == userId)) return;
        _owners.Add(StorageOwner.Create(Code, userId));
    }

    public void RemoveOwner(Guid userId)
    {
        var found = _owners.FirstOrDefault(x => x.UserId == userId);
        if (found == null) return;
        _owners.Remove(found);
    }

    public override string GetId() { return Code; }
}
