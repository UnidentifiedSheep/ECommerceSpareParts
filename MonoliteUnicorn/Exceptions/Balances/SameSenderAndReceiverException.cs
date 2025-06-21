using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Balances;

public class SameSenderAndReceiverException() : BadRequestException("Отправитель и получатель не может быть одним и тем жи")
{
    
}