using Exceptions.Base.Localized;

namespace Application.Common.Exceptions;

public class JobNotFoundException(Guid id) : LocalizedNotFoundException("job.not.found", new { Id = id });

public class JobScheduleNotFoundException(Guid id)
    : LocalizedNotFoundException("job.schedule.not.found", new { Id = id });