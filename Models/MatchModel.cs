using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Algenta.Colectica.Model.Repository;
namespace ColecticaSdkMvc.Models
{
    public class ResetMatchesModelStepOne
    {
        public int Questions1 { get; set; }
        public int Questions2 { get; set; }
        public string Duration { get; set; }
        public List<RepositoryItemMetadata> QuestionsSet1 { get; set; }
        public List<RepositoryItemMetadata> QuestionsSet2 { get; set; }
        public List<Match> Matches = new List<Match>();
        
    }

    public class ResetMatchesModelStepTwo
    {
        public ResetMatchesModelStepOne StepOneModel { get; set; }

    }
    
}