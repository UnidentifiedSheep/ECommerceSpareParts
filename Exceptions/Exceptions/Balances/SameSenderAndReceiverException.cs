using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Balances;

public class SameSenderAndReceiverException() : BadRequestException("Отправитель и получатель не может быть одним и тем жи")
{
    
}