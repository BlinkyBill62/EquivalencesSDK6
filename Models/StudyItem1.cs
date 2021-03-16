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
    public class StudyItem1
    {
        public RepositoryItemMetadata QuestionItem { get; set; }
        public RepositoryItemMetadata Question { get; set; }
        //public SearchResult Question { get; set; }
        public SearchResult Group { get; set; }
        public SearchResult Study { get; set; }
        public RepositoryItemMetadata Concept { get; set; }
    }
}