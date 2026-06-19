namespace Main.Application.Interfaces.Services.Storage;

public interface IStorageContentChangeNotifier
{
    void NotifyChanged(IEnumerable<int> productIds);
}