using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using HibernateDBLogger.Domain;
using HibernateDBLogger.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using HibernateDBLogger.Helpers;

namespace HibernateDBLogger.Test
{
    [TestClass]
    public class LoggerTest
    {
        static private ISessionFactory _sessionFactory;
        static private Configuration _configuration;
        static private Repository _repository;
        static Guid mainGuid;

        //TODO: Change DB_NAME
        private const string DB_NAME = "DB_NAME";
        
        [ClassInitialize]
        public static void SetUp(TestContext TestContext)
        {
            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.AddAssembly(typeof(ErrorLog).Assembly);
            _sessionFactory = _configuration.BuildSessionFactory();
            _repository = new Repository();

            CleanTables();

            MainLog mlog = GetMainLog();
            _repository.Add(mlog);
            mainGuid = mlog.Id;
        }

        private static void CleanTables()
        {
            using (SqlConnection connection = new SqlConnection(
            ConfigurationReader.GetConnectionString()))
            {
                connection.Open();

                string[] commands = new string[]{
                    "delete FROM [" + DB_NAME + "].[dbo].[DetailLog]",
                    "delete FROM [" + DB_NAME + "].[dbo].[MainLog]",
                    "delete FROM ["+ DB_NAME +"].[dbo].[ErrorLog]"
                    };

                foreach (string cmd in commands)
                {
                    SqlCommand command = new SqlCommand(cmd, connection);
                    command.ExecuteNonQuery();
                }
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            CleanTables();
        }

        [TestMethod, Ignore]
        public void CreateSchema()
        {
            new SchemaExport(_configuration).Execute(false, true, false);

        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewLogError()
        {
            ErrorLog error = new ErrorLog() { Message = "test", Time = new DateTime(2014, 12, 31) };
            _repository.Add(error);

            Assert.AreNotEqual(0, error.Id);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewLogDetail()
        {
            MainLog mainLog = _repository.GetMain(mainGuid);
            int count = mainLog.Details.Count;
            DetailLog logDetail = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = new DateTime(2014, 12, 31),
            };

            mainLog.AddDetail(logDetail);
            _repository.Update(mainLog);
            MainLog load = _repository.GetMain(mainGuid);
            Assert.AreEqual(mainGuid, load.Id);
            Assert.AreEqual(count + 1, load.Details.Count);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddIndependentNewLogDetail()
        {
            MainLog mainLog = _repository.GetMain(mainGuid);
            int count = mainLog.Details.Count;
            DetailLog logDetail = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = DateTime.Now,
                Main = new MainLog() { Id = mainGuid }
            };

            _repository.Add(logDetail);
            MainLog load = _repository.GetMain(mainGuid);
            Assert.AreEqual(mainGuid, load.Id);
            Assert.AreEqual(count + 1, load.Details.Count);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddSameMainLogCreateDifferentMainLog()
        {
            MainLog mainLog = _repository.GetMain(mainGuid);
            _repository.Add(mainLog);

            MainLog load = _repository.GetMain(mainGuid);
            Assert.AreNotEqual(mainLog.Id, load.Id);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void UpdateSameMainLogFromNewMainLogSameId()
        {
            MainLog mainLog = _repository.GetMain(mainGuid);
            string message = mainLog.Message;

            _repository.Update(new MainLog()
            {
                Id = mainGuid,
                JobStartTime = DateTime.Now,
                JobEndTime = DateTime.Now,
            });

            MainLog load = _repository.GetMain(mainGuid);
            Assert.AreNotEqual(message, load.Message);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void UpdateNewError()
        {
            MainLog mainLog = _repository.GetMain(mainGuid);
            string message = mainLog.Message;

            mainLog.Error = new ErrorLog()
            {
                Time = DateTime.Now,
                Message = "TestExecution",
                StackTrace = ""
            };

            _repository.Update(mainLog);

            MainLog load = _repository.GetMain(mainGuid);
            Assert.IsNotNull(load.Error);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void Add2NewLogDetail()
        {
            MainLog load = _repository.GetMain(mainGuid);
            int count = load.Details.Count;

            DetailLog log = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = new DateTime(2014, 12, 31),
            };

            load.AddDetail(log);

            DetailLog log1 = new DetailLog()
            {
                Message = "test1",
                Scenario = 12,
                Time = new DateTime(2014, 12, 31),
            };

            load.AddDetail(log1);
            _repository.Update(load);

            load = _repository.GetMain(mainGuid);
            Assert.AreEqual(mainGuid, load.Id);

            Assert.AreEqual(count + 2, load.Details.Count);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddConcurrentLogDetail()
        {
            MainLog load = _repository.GetMain(mainGuid);
            MainLog load1 = new Repository().GetMain(mainGuid);

            int count = load.Details.Count;
            DetailLog log = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = new DateTime(2014, 12, 31),
            };

            load.AddDetail(log);

            DetailLog log1 = new DetailLog()
            {
                Message = "test1",
                Scenario = 12,
                Time = new DateTime(2014, 12, 31),
            };

            load1.AddDetail(log1);
            _repository.Update(load);
            new Repository().Update(load1);

            load = _repository.GetMain(mainGuid);
            Assert.AreEqual(mainGuid, load.Id);
            Assert.AreEqual(count + 2, load.Details.Count);
            load1 = new Repository().GetMain(mainGuid);
            Assert.AreEqual(mainGuid, load1.Id);
            Assert.AreEqual(count + 2, load1.Details.Count);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewLogDetailWithError()
        {
            DateTime time = new DateTime(2014, 12, 31);
            DetailLog log = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = time,
                Error = new ErrorLog()
                {
                    Message = "test",
                    Time = time,
                    StackTrace = "stack"
                }
            };

            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewLogDetailAndGetDetail()
        {
            DateTime time = new DateTime(2014, 12, 31);
            DetailLog log = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = time,
            };

            _repository.Add(log);
            Assert.AreNotEqual("", log.Id.ToString());

            DetailLog loaded = _repository.GetDetail(log.Id);
            Assert.AreEqual("test", loaded.Message);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewLogDetailWithErrorAndGetError()
        {

            DateTime time = new DateTime(2014, 12, 31);
            DetailLog log = new DetailLog()
            {
                Message = "test",
                Scenario = 12,
                Time = time,
                Error = new ErrorLog()
                {
                    Message = "test",
                    Time = time,
                    StackTrace = "stack"
                }
            };

            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());

            DetailLog loaded = _repository.GetDetail(log.Id);
            Assert.IsNotNull(loaded.Error);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void AddNewMainLoglWithErrorAndGetError()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = GetMainLog();

            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());

            MainLog loaded = _repository.GetMain(log.Id);
            Assert.IsNotNull(loaded.Error);
            Assert.IsNotNull(loaded.JobEndTime);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void MainLogErrorLoad()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = GetMainLog();

            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());

            MainLog loaded = _repository.GetMain(log.Id);
            Assert.AreEqual(loaded.Error.Message, log.Error.Message);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void DetailLogErrorLoad()
        {
            DateTime time = new DateTime(2014, 12, 31);
            DetailLog log = new DetailLog()
            {
                Message = "xxx",
                Time = time
            };
            log.Error = new ErrorLog()
            {
                Message = "error",
                Time = time
            };
            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());

            DetailLog loaded = _repository.GetDetail(log.Id);
            Assert.AreEqual(loaded.Error.Message, log.Error.Message);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void TwoRepMainErrorLoad()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = GetMainLog();

            _repository.Add(log);
            Assert.AreNotEqual(0, log.Error.Id);
            Assert.AreNotEqual("", log.Id.ToString());

            MainLog loaded = new Repository().GetMain(log.Id);
            Assert.AreEqual(loaded.Error.Message, log.Error.Message);
        }

        [TestCategory("IntegrationTest"), TestMethod]
        public void TwoRepMainDetailLoad()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = _repository.GetMain(mainGuid);
            int count = log.Details.Count;
            DetailLog det = new DetailLog()
            {
                Time = time,
                Message = "eee"
            };
            log.AddDetail(det);
            _repository.Update(log);
            MainLog loaded = new Repository().GetMain(log.Id);
            Assert.AreEqual(count + 1, loaded.Details.Count);
        }

        private static MainLog GetMainLog()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = new MainLog()
            {
                Message = "test",
                Scenario = 12,
                JobStartTime = time,
                JobEndTime = time,
                Error = new ErrorLog()
                {
                    Message = "test",
                    Time = time,
                    StackTrace = "stack"
                }
            };
            return log;
        }

        [TestMethod]
        public void AddNewMainLoglWithDetails()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = new MainLog()
            {
                Message = "test",
                Scenario = 12,
                JobStartTime = time,
                JobEndTime = time
            };

            DetailLog dlog = new DetailLog()
            {
                Message = "test",
                Time = time,
                Scenario = 12
            };

            log.AddDetail(dlog);
            _repository.Add(log);

            Assert.AreNotEqual(0, dlog.Main.Id);

            MainLog loaded = _repository.GetMain(log.Id);
            Assert.AreEqual(log.Id, loaded.Id);
            Assert.AreEqual(log.Id, loaded.Details.First().Main.Id);
        }

        [TestMethod]
        public void AddNewMainLoglWithDetailsAlreadyLoaded()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = _repository.GetMain(mainGuid);
            int count = log.Details.Count;
            DetailLog dlog = new DetailLog()
            {
                Message = "test",
                Time = time,
                Scenario = 12
            };

            log.AddDetail(dlog);
            _repository.Update(log);

            MainLog loaded = _repository.GetMain(log.Id);
            Assert.AreEqual(count + 1, loaded.Details.Count);

            DetailLog dlog1 = new DetailLog()
            {
                Message = "test1",
                Time = time,
                Scenario = 12
            };

            loaded.AddDetail(dlog1);
            _repository.Update(loaded);

            loaded = _repository.GetMain(log.Id);
            Assert.AreEqual(count + 2, loaded.Details.Count);
        }

        [TestMethod]
        public void SessionSaveLogDetailsInMainLog()
        {

            using (ISession session = NHibernateHelper.OpenSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                DateTime time = new DateTime(2014, 12, 31);
                MainLog log = new MainLog()
                {
                    Message = "test",
                    Scenario = 12,
                    JobStartTime = time,
                    JobEndTime = time
                };

                DetailLog dlog = new DetailLog()
                {
                    Message = "test",
                    Time = time,
                    Scenario = 12,
                };

                log.AddDetail(dlog);
                session.Save(log);

                Assert.AreNotEqual(0, dlog.Main.Id);

                transaction.Commit();

                MainLog loaded = _repository.GetMain(log.Id);
                Assert.AreEqual(log.Id, loaded.Id);
                Assert.AreEqual(log.Id, loaded.Details.First().Main.Id);
            }
        }

        [TestMethod]
        public void GetLasMainLog()
        {
            DateTime time = new DateTime(2014, 12, 31);
            DateTime time1 = new DateTime(2012, 12, 31);
            MainLog log = new MainLog()
            {
                Message = "test",
                Scenario = 1234,
                JobStartTime = time,
                JobEndTime = time
            };

            MainLog log1 = new MainLog()
            {
                Message = "test",
                Scenario = 1234,
                JobStartTime = time1,
                JobEndTime = time1
            };

            _repository.Add(log);
            _repository.Add(log1);

            MainLog loaded = _repository.GetLastMain(1234);
            Assert.AreEqual(log.Id, loaded.Id);
        }

        [TestMethod]
        public void UpdateMainLog()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = new MainLog()
            {
                Message = "test",
                Scenario = 12,
                JobStartTime = time,
                JobEndTime = time
            };

            _repository.Add(log);

            log.Details = new List<DetailLog>();
            log.Details.Add(new DetailLog()
            {
                Message = "test",
                Time = time,
                Scenario = 12,
            });

            _repository.Add(log);

            Assert.AreEqual("test", log.Message);

            log.Message = "t";
            log = _repository.Update(log);

            Assert.AreEqual("t", log.Message);
            MainLog load = _repository.GetMain(log.Id);
            Assert.AreEqual("t", load.Message);
        }

        [TestMethod]
        public void OrderedDetailsInMainLog()
        {
            DateTime time = new DateTime(2014, 12, 31);
            MainLog log = new MainLog()
            {
                Message = "test",
                Scenario = 12,
                JobStartTime = time,
                JobEndTime = time
            };

            _repository.Add(log);

            log.Details = new List<DetailLog>();
            log.Details.Add(new DetailLog()
            {
                Message = "test later later",
                Time = time.AddDays(2),
                Scenario = 12,
            });
            log.Details.Add(new DetailLog()
            {
                Message = "test later",
                Time = time.AddDays(1),
                Scenario = 12,
            });
            log.Details.Add(new DetailLog()
            {
                Message = "test sooner",
                Time = time,
                Scenario = 12,
            });

            _repository.Add(log);

            MainLog load = _repository.GetMain(log.Id);
            Assert.AreEqual("test sooner", load.Details[0].Message);
            Assert.AreEqual("test later", load.Details[1].Message);

        }
    }
}

