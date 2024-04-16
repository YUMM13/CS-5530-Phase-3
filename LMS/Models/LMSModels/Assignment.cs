using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public uint AssignmentId { get; set; }
        public string Name { get; set; } = null!;
        public uint CatId { get; set; }
        public uint MaxPoints { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime DueDate { get; set; }

        public virtual Category Cat { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
