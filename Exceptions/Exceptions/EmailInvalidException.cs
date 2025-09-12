using Core.Exceptions;
using Exceptions.Base;

namespace Exceptions.Exceptions;

public class EmailInvalidException(string? email) : BadRequestException("Почта не валидна", new { Email = email })
{
    
}