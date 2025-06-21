using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions;

public class DefaultSettingNotFoundException(string key) : NotFoundException(key)
{
    
}