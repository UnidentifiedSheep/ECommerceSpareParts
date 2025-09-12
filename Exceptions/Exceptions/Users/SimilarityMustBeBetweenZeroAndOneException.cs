using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class SimilarityMustBeBetweenZeroAndOneException(double similarityLevel) :
    BadRequestException("Уровень симметричности должен быть между 0 и 1", new { SimilarityLevel = similarityLevel })
{
}