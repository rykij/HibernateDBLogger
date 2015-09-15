using System;

namespace HybernateDBLogger.Domain
{
    //ErrorLog table entity
    public class ErrorLog
    {
        public virtual int Id { get; set; }
        public virtual string Message { get; set; }
        public virtual DateTime Time { get; set; }
        public virtual string StackTrace { get; set; }
    }
}
