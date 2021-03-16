using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class StudyModel
    {
        public SearchResult Study { get; set; }
        public List<StudyItem> Sweeps { get; set; }
    }
}