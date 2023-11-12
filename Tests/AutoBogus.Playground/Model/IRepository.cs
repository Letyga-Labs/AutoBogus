using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Playground.Model;

public interface IRepository
{
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
    Item Get(Guid id);

    IEnumerable<Item> GetAll();

    IEnumerable<Item> GetFiltered(Func<Item, bool> filter);
}
