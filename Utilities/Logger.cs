using System;
using log4net;

namespace HibernateDBLogger.Utilities
{
    public class Logger<T> where T : class
    {
        public delegate string GetMessage();
        private ILog log;

        public Logger()
        {
            log = LogManager.GetLogger(typeof(T));
        }

        public void Debug(GetMessage GetMessage, Exception Exception = null)
        {
            if (log.IsDebugEnabled)
                log.Debug(GetMessage(), Exception);
        }

        public void Info(GetMessage GetMessage, Exception Exception = null)
        {
            if (log.IsInfoEnabled)
                log.Info(GetMessage(), Exception);
        }

        public void Warn(GetMessage GetMessage, Exception Exception = null)
        {
            if (log.IsWarnEnabled)
                log.Warn(GetMessage(), Exception);
        }

        public void Error(GetMessage GetMessage, Exception Exception = null)
        {
            if (log.IsErrorEnabled)
                log.Error(GetMessage(), Exception);
        }

        public void Info(string Message, Exception Exception = null)
        {
            if (log.IsInfoEnabled)
                log.Info(Message, Exception);
        }

        public void Debug(string Message, Exception Exception = null)
        {
            if (log.IsDebugEnabled)
                log.Debug(Message, Exception);
        }

        public void Warn(string Message, Exception Exception = null)
        {
            if (log.IsWarnEnabled)
                log.Warn(Message, Exception);
        }

        public void Error(string Message, Exception Exception = null)
        {
            if (log.IsErrorEnabled)
                log.Error(Message, Exception);
        }
    }
}
