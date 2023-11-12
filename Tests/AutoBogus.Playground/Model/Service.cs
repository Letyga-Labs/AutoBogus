namespace AutoBogus.Playground.Model;

public sealed class Service
{
    internal static readonly Func<Item, bool> PendingFilter = item => item.Status == ItemStatus.Pending;

    public Service(IRepository repository)
    {
        Repository = repository;
    }

    private IRepository Repository { get; }

    public Item Get(Guid id)
    {
        return Repository.Get(id);
    }

    public IEnumerable<Item> GetAll()
    {
        return Repository.GetAll();
    }

    public IEnumerable<Item> GetPending()
    {
        return Repository.GetFiltered(PendingFilter);
    }
}
