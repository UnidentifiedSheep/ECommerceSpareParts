using Exceptions.Base;

namespace Exceptions.Exceptions.Storages;

public class ChangeOfStorageTypeRestrictedException : BadRequestException
{
    public ChangeOfStorageTypeRestrictedException(string reason) : base("Нельзя менять тип склада.", new { Reason = reason })
    {
    }
}