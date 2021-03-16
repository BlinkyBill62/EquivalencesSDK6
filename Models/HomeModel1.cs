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
	public class HomeModel1
	{
		List<SearchResult> results = new List<SearchResult>();
		public List<SearchResult> Results
		{
			get { return this.results; }
		}

        public string Type { get; set; }
    }
}