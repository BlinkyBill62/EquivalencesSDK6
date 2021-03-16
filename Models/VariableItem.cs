using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class VariableItem
    {
        public int uniqueId { get; set; }
        public string equivalence { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int counter { get; set; }
        public string questionName { get; set; }
        public string questionText { get; set; }
        public string studyGroup { get; set; }
        public string study { get; set; }
        public string questionItem { get; set; }
        public string parentitem { get; set; }
        public Guid identifier { get; set; }
        public string concept { get; set; }
        public int column { get; set; }
        public bool selected { get; set; }
        public bool isdeprecated { get; set; }
    }
}