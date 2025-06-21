using Core.Exceptions;

namespace MonoliteUnicorn.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException(string? vinCode) : BadRequestException($"Вин код '{vinCode}' занят.")
{
    
}