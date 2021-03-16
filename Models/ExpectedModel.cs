using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Algenta.Colectica.Model.Utility;
using ColecticaSdkMvc.Utility;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;

namespace ColecticaSdkMvc.Models
{
    public class ExpectedModel
    {
        public List<EquivalenceItem> AllItems { get; set; }
        public List<EquivalenceItem> AllResults { get; set; }
        public List<ExpectedItem> expecteditems { get; set; }
        public List<string> SelectedStudies { get; set; }
    }
}