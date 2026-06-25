using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManagementSystem.Core.Aggregates
{
    public class BaseModel
    {
        public int ID { get; set; }
        public DateTime CreatedAT { get; set; }
        public DateTime DeletedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
