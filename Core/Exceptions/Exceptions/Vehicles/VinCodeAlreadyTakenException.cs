using Exceptions.Base;

namespace Exceptions.Exceptions.Vehicles;

public class VinCodeAlreadyTakenException(string? vinCode) : BadRequestException($"Вин код '{vinCode}' занят.")
{
}