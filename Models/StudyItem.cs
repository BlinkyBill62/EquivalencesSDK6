using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class StudyItem
    {
        public string AgencyId { get; set; }
        public Guid Identifier { get; set; }
        public string DisplayLabel { get; set; }
        public string ReferenceItem { get; set; }
    }
}