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
    public class StudyModel1
    {
        public string Study { get; set; }
        public string Type { get; set; }
        public List<SelectListItem> Studies { get; set; }
        public VariableModel Equivalances { get; set; }
        public List<StudyItem1> Items { get; set; }
    }
}