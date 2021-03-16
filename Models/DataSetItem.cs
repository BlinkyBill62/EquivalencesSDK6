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
    public class Item1
    {
        public string Id { get; set; }
        public string QuestionValue { get; set; }
    }

    public class DataSetItem
    {
        public string Agency { get; set; }
        public Guid Identifier { get; set; }
        public long Version { get; set; }
        public string ItemType { get; set; }
        public bool IsDeprecated { get; set; }
        public string Referencing { get; set; }
    }
}