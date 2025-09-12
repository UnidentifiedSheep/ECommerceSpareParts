using Exceptions.Base;

namespace Exceptions.Exceptions;

public class DefaultSettingNotFoundException(string key) : NotFoundException(key)
{
    
}