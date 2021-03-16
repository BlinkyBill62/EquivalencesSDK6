using Algenta.Colectica.Model.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class EquivalenceItem
    {        
        public int uniqueId { get; set; }
        public string equivalence { get; set; }
        public int set { get; set; }

        public string name { get; set; }
        public string description { get; set; }

        public string variableAgency { get; set; }
        public string variableName { get; set; }
        public string variableText { get; set; }
        public string variableOrdered { get; set; }
        public string variableItem { get; set; }
        public Guid variableIdentifier { get; set; }

        public string questionName { get; set; }
        public string questionText { get; set; }
        public string questionOrdered { get; set; }
        public string questionItem { get; set; }
        public Guid questionIdentifier { get; set; }

        public string studyGroup { get; set; }
        public string studyGroupIdentifer { get; set; }
        public string study { get; set; }
        public string dataset { get; set; }
        public string concept { get; set; }

        public int column { get; set; }
        public bool selected { get; set; }

        public int counter { get; set; }
        public bool removed { get; set; }
    }

}