using System;
using System.Linq;
using HibernateDBLogger.Domain;
using HibernateDBLogger.Helpers;
using NHibernate;

namespace HibernateDBLogger
{
    public class Repository : IRepository
    {
        public void Add(ErrorLog Error) {
            Add<ErrorLog>(Error);
        }

        public void Add(DetailLog Detail)
        {
            Add<DetailLog>(Detail);
        }

        public void Add(MainLog Main)
        {
            Add<MainLog>(Main);
        }

        private void Add<T>(T DomainObj)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(DomainObj);
                transaction.Commit();
            }
        }

        public DetailLog GetDetail(Guid Id) {
            using (ISession session = NHibernateHelper.OpenSession())
                return session.Get<DetailLog>(Id);
        }

        public MainLog GetMain(Guid Id)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                return session.Get<MainLog>(Id);
            }
        }

        public MainLog GetLastMain(int ScenarioId)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                var mainLogs = session.QueryOver<MainLog>().Where(m=>m.Scenario == ScenarioId).List();
                
                return mainLogs.Where(ml=>ml.JobStartTime.Equals(mainLogs.Max(m => m.JobStartTime))).First();
            }
        }

        public MainLog Update(MainLog MainLog)
        {
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(MainLog);
                    transaction.Commit();
                    return MainLog;
                }
            }
        }
    }
}
