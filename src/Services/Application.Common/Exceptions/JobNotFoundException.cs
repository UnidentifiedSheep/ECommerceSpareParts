using Exceptions.Base.Localized;

namespace Application.Common.Exceptions;

public class JobNotFoundException : LocalizedNotFoundException
{
    public JobNotFoundException(Guid id) : base("job.not.found", new { Id = id})
    {
    }
}