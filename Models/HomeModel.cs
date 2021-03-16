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
    public class Word
    {
        public string Value { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public string QuestionValue { get; set; }
    }

    

	public class HomeModel
	{
        //List<SearchResult> results = new List<SearchResult>();
        //public List<SearchResult> Results
        //{
        //	get { return this.results; }
        //}

        //List<HomeItem> results = new List<HomeItem>();
        //public List<HomeItem> Results
        //{
        //    get { return this.results; }
        //}

        public List<StudyItem> Results { get; set; }
        public List<StudyItem> AllResults { get; set; }
        public List<SelectListItem> Studies { get; set; }
        public List<string> SelectedStudies { get; set; }
        public List<SelectListItem> Methods { get; set; }
        public List<string> SelectedMethods { get; set; }
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string Type { get; set; }
        public string ReferenceItems { get; set; }

        public string StudyId { get; set; }
        public IEnumerable<SelectListItem> StudyList { get; set; }

        public string WordSelection { get; set; }
        public string Word { get; set; }
        public List<Word> WordList { get; set; }

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