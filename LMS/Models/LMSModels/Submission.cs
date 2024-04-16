using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public DateTime? Submitted { get; set; }
        public uint? Score { get; set; }
        public string? Solution { get; set; }
        public uint AssignmentId { get; set; }
        public string UId { get; set; } = null!;

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
