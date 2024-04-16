using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Category
    {
        public Category()
        {
            Assignments = new HashSet<Assignment>();
        }

        public string Name { get; set; } = null!;
        public uint Weight { get; set; }
        public uint CatId { get; set; }
        public uint ClassId { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
