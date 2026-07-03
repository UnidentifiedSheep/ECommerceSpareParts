namespace Domain.Exceptions;

public class JobLeaseLostException(Guid jobId) 
    : Exception($"Job lease was lost. JobId: {jobId}") { }