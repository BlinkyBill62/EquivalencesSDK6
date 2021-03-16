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
   

    public class VariableModel
    {
        // Store all the Questions and Concepts for the selected studies
        public List<RepositoryItemMetadata> AllResults { get; set; }
        public List<RepositoryItemMetadata> AllConcepts { get; set; }

        // Information about Studies for Selection
        public string StudyId { get; set; }
        public string CurrentStudy { get; set; }
        public IEnumerable<SelectListItem> Studies { get; set; }

        public string ItemType { get; set; }
        public List<SelectListItem> ItemTypes { get; set; }
        public List<VariableItem> Results { get; set; }
        public List<string> Waves { get; set; }
        public bool Any { get; set; }
        public bool Name { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
        public string selectedQuestionName { get; set; }
        public string selectedQuestionText { get; set; }
        public List<string> SelectedResults { get; set; }
        public List<string> SelectedQuestions { get; set; }
        public List<QuestionSummary> Questions { get; set; }
        public string SelectedItems { get; set; }
        public string Equivalence { get; set; }
        public List<Word> Equivalences { get; set; }
        public string EquivalenceSelection { get; set; }
        public string Elapsed { get; set; }


        public string GetReferenceItem(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-US";

            var client = ClientHelper.GetClient();

            // Retrieve the requested item from the Repository.
            // Populate the item's children, so we can display information about them.
            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);
            // Use a graph search to find a list of all items that 
            // directly reference this item.
            GraphSearchFacet facet = new GraphSearchFacet();
            facet.TargetItem = item.CompositeId;
            facet.UseDistinctResultItem = true;

            var referencingItemsDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);
            string referenceItem = null;
            if (referencingItemsDescriptions.Count == 3) { referenceItem = referencingItemsDescriptions.Where(r => r.ItemType.ToString() == "91da6c62-c2c2-4173-8958-22c518d1d40d").FirstOrDefault().DisplayLabel; }
            return referenceItem;
        }

        public string GetEquivalenceString(List<string> selectedlist)
        {
            string wordlist = "";
            if (selectedlist != null)
            {
                foreach (var item in selectedlist)
                {
                    wordlist = wordlist + item.ToString() + ",";
                }
            }
            return wordlist;
        }
    }
}