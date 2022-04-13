using System.Collections.Generic;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Interfaces
{
    public interface IScheduleRepository : IScheduleReadStore, IScheduleWriteStore { }

    public interface IScheduleReadStore
    {
        RepositoryResult<Schedule> GetSchedule(string filename);
    }

    public interface IScheduleWriteStore
    {
        RepositoryResult<Schedule> Save(Schedule filename);
    }
}
