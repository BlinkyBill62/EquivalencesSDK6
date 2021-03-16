using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ColecticaSdkMvc.Models
{
    public class DataSetModel
    {
        public string Urn { get; set; }
        public List<string> Urns {get;set;}
        public string DisplayLabel { get; set; }
        public Collection<DataSetItem> DataSet { get; set; }
        public List<Item> ItemTypes { get; set; }
    }
}