using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class Study
    {
        public int ID { get; set; }
        public string Value { get; set; }
        public string StudyName { get; set; }
    }

    public class ExpectedItem
    {
        public int UniqueId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Topic { get; set; }
        public List<Study> Waves { get; set; }

    }
}