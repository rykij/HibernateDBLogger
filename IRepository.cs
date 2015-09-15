using HybernateDBLogger.Domain;

namespace HybernateDBLogger
{
    public interface IRepository
    {
        MainLog GetLastMain(int ScenarioId);
    }
}
