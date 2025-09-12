namespace Application.Interfaces;

public interface ILoggableRequest<in TRequest>
{
    bool IsLoggingNeeded(TRequest request);
    string GetLogPlace(TRequest request);
    object GetLogData(TRequest request);
    string? GetUserId(TRequest request);
}