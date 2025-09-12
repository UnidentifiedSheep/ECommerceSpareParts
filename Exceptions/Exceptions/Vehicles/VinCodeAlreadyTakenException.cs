using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException(string? vinCode) : BadRequestException($"Вин код '{vinCode}' занят.")
{
    
}