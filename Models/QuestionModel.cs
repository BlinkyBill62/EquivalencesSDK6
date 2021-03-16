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
  
	public class QuestionModel
	{
        public List<RepositoryItemMetadata> AllQuestions { get; set; }
        public List<RepositoryItemMetadata> AllVariables { get; set; }
        public List<RepositoryItemMetadata> AllConcepts { get; set; }

        public List<VariableItem> AllResults { get; set; }

        public List<StudyItem> Results { get; set; }
        public List<string> SelectedStudies { get; set; }

        public List<Word> Equivalences { get; set; }
        public string EquivalenceSelection { get; set; }
   
        public IEnumerable<SelectListItem> Studies { get; set; }

        public string WordSelection { get; set; }
        public string Word { get; set; }
        public List<Word> WordList { get; set; }

        public string FileName { get; set; }
        public string StudyName { get; set; }
        public string Type { get; set; }

        public string GetString(List<string> selectedmethods)
        {
            string wordlist = "";
            if (selectedmethods != null)
            {
                foreach (var item in selectedmethods)
                {
                    wordlist = wordlist + item.ToString() + ",";
                }
            }
            return wordlist;
        }

        public string GetReferenceItems(string agency, Guid id)
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
            return referencingItemsDescriptions.FirstOrDefault().DisplayLabel;
        }

        public int GetQuestionCount(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-US";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;
            SetSearchFacet setFacet = new SetSearchFacet();

            setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

            var matches = client.SearchTypedSet(studyUnit.CompositeId,
                setFacet);
            var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection());
            return infoList.Count();
        }
    }
}