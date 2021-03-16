using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Algenta.Colectica.Model.Utility;
using ColecticaSdkMvc.Utility;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;

namespace ColecticaSdkMvc.Models
{
	public class HomeItem
	{

		public StudyItem result{ get; set; }
        // public List<RepositoryItemMetadata> referencingitems { get; set; }
        //public string typename { get; set; }
        //public string referenceItems { get; set; }
        public bool  selected { get; set; }

        //public HomeItem(SearchResult Result, string TypeName, string ReferenceItems, bool Selected)
        //{
        //    this.result = Result;
        //    this.typename = TypeName;
        //    this.referenceItems = ReferenceItems;
        //    this.selected = Selected;
        //}      

    }
}