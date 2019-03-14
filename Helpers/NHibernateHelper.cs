using HibernateDBLogger.Domain;
using NHibernate;
using NHibernate.Cfg;

namespace HibernateDBLogger.Helpers
{
    public class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        private static ISession _session;
        private static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    var configuration = new Configuration();
                    configuration.Configure();
                    configuration.AddAssembly(typeof(ErrorLog).Assembly);
                    _sessionFactory = configuration.BuildSessionFactory();
                }
                return _sessionFactory;
            }
        }

        public static ISession OpenSession()
        {
            if (null == _session || _session.IsOpen == false)
                _session = SessionFactory.OpenSession();
            return _session;
        }
    }
}
