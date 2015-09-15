using System;

namespace HybernateDBLogger.Domain
{
    //DialogLog table entity
    public class DetailLog
    {
        public virtual Guid Id { get; set; }
        public virtual MainLog Main { get; set; }
        public virtual int Scenario { get; set; }
        public virtual string Message { get; set; }
        public virtual ErrorLog Error { get; set; }
        public virtual DateTime Time { get; set; }
    }
}
