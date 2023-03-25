using TimetablePlanning.Importers.Model;

namespace TimetablePlanning.Importers.Interfaces;

public interface ILayoutService : ILayoutSource, ILayoutDestination { }

public interface ILayoutSource
{
    ImportResult<Layout> GetLayout(string name);
}

public interface ILayoutDestination
{
    ImportResult<Layout> Save(Layout layout);
}
