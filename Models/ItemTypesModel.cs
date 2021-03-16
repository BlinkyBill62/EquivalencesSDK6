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
    public class QuestionSummary
    {
        public string Key { get; set; }
        public int NumberofQuestions { get; set; }
    }

    public class ItemTypesModel
	{
        public string ItemType { get; set; }
        public List<SelectListItem> ItemTypes { get; set; }
        public List<SearchResult> Results { get; set; }
        public List<QuestionSummary> Questions { get; set; }
       
    }
}