using System;
using System.Collections.Generic;

namespace HybernateDBLogger.Domain
{
    //MainLog table entity
    public class MainLog
    {
        private IList<DetailLog> details = new List<DetailLog>();

        public virtual int Job { get; set; }
        public virtual Guid Id { get; set; }
        public virtual int Scenario { get; set; }
        public virtual string Message { get; set; }
        public virtual string JobStatus { get; set; }
        public virtual DateTime JobStartTime { get; set; }
        public virtual DateTime JobEndTime { get; set; }
        public virtual string CallingUser { get; set; }
        public virtual ErrorLog Error { get; set; }
        public virtual IList<DetailLog> Details { get { return details; } set { details = value;} }

        public virtual void AddDetail(DetailLog Detail)
        {
            Detail.Main = this;
            Details.Add(Detail);
        }
    }
}
