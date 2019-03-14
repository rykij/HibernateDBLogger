using HibernateDBLogger.Domain;

namespace HibernateDBLogger
{
    public interface IRepository
    {
        MainLog GetLastMain(int ScenarioId);
    }
}
