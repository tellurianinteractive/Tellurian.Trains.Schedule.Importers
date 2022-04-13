using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Interfaces
{
    public interface ILayoutStore : ILayoutReadStore, ILayoutWriteStore { }

    public interface ILayoutReadStore
    {
        RepositoryResult<Layout> GetLayout(string name);
    }

    public interface ILayoutWriteStore
    {
        RepositoryResult<Layout> Save(Layout layout);
    }

    public interface IReadStore : ILayoutReadStore, ITimetableReadStore, IScheduleReadStore { }
}
