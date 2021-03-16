using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
    public class LevenshteinItem
    {
        public string Results { get; set; }
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }

        //public LevenshteinItem(int results, string questionId, string questionText)
        //{
        //    this.Results = results;
        //    this.QuestionId = questionId;
        //    this.QuestionText = questionText;
           
        //}
    }

    public class LevenshteinModel
    {
        public string QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<LevenshteinItem> Results { get; set; }
    }
       
}