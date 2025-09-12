using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Storages;

public class StorageIsUnknownException() : 
    BadRequestException("Нельзя менять количество артикула, не выбрав склад и не разрешая менять количество на всех складах")
{
    
}