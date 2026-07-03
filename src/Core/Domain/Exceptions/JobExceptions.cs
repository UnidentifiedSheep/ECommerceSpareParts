namespace Domain.Exceptions;

public class JobLeaseLostException(Guid jobId) 
    : Exception($"Job lease was lost. JobId: {jobId}") { }
    
public class JobCancellationRequestedException(Guid jobId)
    : Exception($"Job cancellation requested. JobId: {jobId}");