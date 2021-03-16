using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Repository.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ColecticaSdkMvc.Models;
using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Utility;

namespace ColecticaSdkMvc.Utility
{
	public class RepositoryHelper
	{
        public static QuestionModel GetAllQuestions(QuestionModel model, string agency, Guid id, string itemType)
        {
            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;
            //var studyModel = new List<RepositoryItemMetadata>();
            //studyModel.StudyUnit = studyUnit;

            foreach (var qualityStatement in studyUnit.QualityStatements)
            {
                client.PopulateItem(qualityStatement);
            }

            // Use a set search to get a list of all questions that are referenced
            // by the study. A set search will return items that may be several steps
            // away.
            SetSearchFacet setFacet = new SetSearchFacet();
            setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

            var matches = client.SearchTypedSet(studyUnit.CompositeId,
                setFacet);
            var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection());

            foreach (var info in infoList)
            {
                model.AllQuestions.Add(info);
                var references = Helper.GetReferences(info.AgencyId, info.Identifier);

                foreach (var reference in references)
                {
                    if (itemType == "Variable")
                    {
                        if (reference.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d"))
                        {
                            reference.ItemType = info.Identifier;
                            model.AllVariables.Add(reference);
                        }
                    }
                    if (reference.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205"))
                    {
                        reference.ItemType = info.Identifier;
                        model.AllConcepts.Add(reference);
                    }
                }
            }
            return model;

        }
        public static int GetStudyColumn(string selectedstudy, string studyId)
        {
            List<Word> equivalences = new List<Word>();
            var studies = GetRepository("Study", "", equivalences);
            studies = studies.Where(a => a.AgencyId == studyId).ToList();
            int i = 1;
            foreach (var study in studies)
            {
                if (study.DisplayLabel == selectedstudy) { return i; }
                i++;
            }
            return 0;
        }

        public static string GetStudy(string agency, Guid id)
        {
            try
            {
                MultilingualString.CurrentCulture = "en-GB";

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
                var references = client.GetRepositoryItemDescriptionsByObject(facet);
                var referencingItemDescription = (from a in references
                                                  where a.ItemType == new Guid("0a63fcf6-ffdd-4214-b38c-147d6689d6a1")
                                                  select a).FirstOrDefault();
                for (int i = 1; i < 3; i++)
                {
                    if (referencingItemDescription != null)
                    {
                        referencingItemDescription = GetReference(referencingItemDescription.AgencyId, referencingItemDescription.Identifier);
                    }
                }
                if (referencingItemDescription == null)
                {
                    return null;
                }
                else
                {
                    return referencingItemDescription.DisplayLabel;
                }
            }
            catch
            {
                return null;
            }
        }

        public static RepositoryItemMetadata GetReference(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

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

            var referencingItemDescription = client.GetRepositoryItemDescriptionsByObject(facet).FirstOrDefault();
            var referencingItemDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);

            return referencingItemDescription;
        }

        public static List<RepositoryItemMetadata> GetReferences(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

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

            var referencingItemDescriptions = client.GetRepositoryItemDescriptionsByObject(facet).ToList();
            return referencingItemDescriptions;
        }

        public static RepositoryItemMetadata GetConcept(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

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

            var referencingItemDescription = client.GetRepositoryItemDescriptionsByObject(facet).Where(x => x.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205")).FirstOrDefault();
            var referencingItemDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);

            return referencingItemDescription;
        }


        // Used for uploading of Questions using Question Items. Only to be used for development
        // to use set the itemType to "All" in Equivalences
        public static List<SearchResult> GetRepository(string itemType, string searchTerm, List<Word> equivalences)
        {
            DateTime start, finish;
            MultilingualString.CurrentCulture = "en-GB";

            // Create a new SearchFacet that will find all
            // StudyUnits, CodeSchemes, and CategorySchemes.

            SearchFacet facet = new SearchFacet();
            switch (itemType)
            {
                case "Action":
                    facet.ItemTypes.Add(DdiItemType.Archive);
                    break;
                case "Archive":
                    facet.ItemTypes.Add(DdiItemType.Archive);
                    break;
                case "Category":
                    facet.ItemTypes.Add(DdiItemType.Category);
                    break;
                case "Category Group":
                    facet.ItemTypes.Add(DdiItemType.CategoryGroup);
                    break;
                case "Category Set":
                    facet.ItemTypes.Add(DdiItemType.CategoryScheme);
                    break;
                case "Code List":
                    facet.ItemTypes.Add(DdiItemType.CodeList);
                    break;
                case "Code List Group":
                    facet.ItemTypes.Add(DdiItemType.CodeListGroup);
                    break;
                case "Code List Scheme":
                    facet.ItemTypes.Add(DdiItemType.CodeListScheme);
                    break;
                case "Concept":
                    facet.ItemTypes.Add(DdiItemType.Concept);
                    break;
                case "Concept Group":
                    facet.ItemTypes.Add(DdiItemType.ConceptGroup);
                    break;
                case "Concept Scheme":
                    facet.ItemTypes.Add(DdiItemType.ConceptScheme);
                    break;
                case "Data Collection":
                    facet.ItemTypes.Add(DdiItemType.DataCollection);
                    break;
                case "Group":
                    facet.ItemTypes.Add(DdiItemType.Group);
                    break;
                case "Instrument":
                    facet.ItemTypes.Add(DdiItemType.Instrument);
                    break;
                case "Question Item":
                    facet.ItemTypes.Add(DdiItemType.QuestionItem);
                    break;
                case "Question Group":
                    facet.ItemTypes.Add(DdiItemType.QuestionGroup);
                    break;
                case "Study":
                    facet.ItemTypes.Add(DdiItemType.StudyUnit);
                    break;
                case "Variable":
                    facet.ItemTypes.Add(DdiItemType.Variable);
                    break;
                case "Variable Group":
                    facet.ItemTypes.Add(DdiItemType.VariableGroup);
                    break;
                case "Variable Scheme":
                    facet.ItemTypes.Add(DdiItemType.VariableScheme);
                    break;
                case "Variable Statistic":
                    facet.ItemTypes.Add(DdiItemType.VariableStatistic);
                    break;
                default:
                    facet.ItemTypes.Add(DdiItemType.Group);
                    break;

            }

            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;

            // Add SearchTerms to the facet to only return results that contain the specified text.
            if (searchTerm != null) facet.SearchTerms.Add(searchTerm);

            // Add Cultures to only search for text in certain languages.
            facet.Cultures.Add("en-GB");

            // Use MaxResults and ResultOffset to implement paging, if large numbers of items may be returned.
            //facet.MaxResults = pageSize;
            //facet.ResultOffset = (pageSize * page);

            // Now that we have a facet, search for the items in the Repository.
            // The client object takes care of making the Web Services calls.
            start = DateTime.Now;
            var client = ClientHelper.GetClient();
            SearchResponse response = client.Search(facet);
            List<string> words = new List<string>();
            foreach (var item in equivalences)
            {
                words.Add(item.Value);
            }
            // Create the model object, and add all the search results to 
            // the model's list of results so they can be displayed.)
            IEnumerable<SearchResult> results = response.Results;
            List<SearchResult> results1 = new List<SearchResult>();
            results = results.ToList();

            return results.ToList();
        }

        public static QuestionModel BuildStudiesTree(QuestionModel model, List<TreeViewNode> nodes)
        {
            // move
            MultilingualString.CurrentCulture = "en-GB";

            SearchFacet facet = new SearchFacet();
            facet.ItemTypes.Add(DdiItemType.Group);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client1 = ClientHelper.GetClient();
            SearchResponse response = client1.Search(facet);
            facet.ItemTypes.Add(DdiItemType.StudyUnit);
            // facet.ItemTypes.Add(DdiItemType.VariableGroup);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client2 = ClientHelper.GetClient();
            SearchResponse allsweeps = client2.Search(facet);
            model.Studies = LoadStudies(model, response);
            List<StudyItem> studies = new List<StudyItem>();
            int i = 1;
            foreach (var result in response.Results)
            {
                nodes.Add(new TreeViewNode { id = result.AgencyId, parent = "#", text = result.DisplayLabel });
                studies = BuildSweepsTree(studies, allsweeps, result.DisplayLabel, result.AgencyId, nodes, result.AgencyId);
                i++;
            }
            model.Results = studies;
            return model;
        }

        public static List<StudyItem> BuildSweepsTree(List<StudyItem> model, SearchResponse allsweeps, string study, string agency, List<TreeViewNode> nodes, string parent)
        {
            // move
            int i = 1;
            foreach (var sweep in allsweeps.Results)
            {
                StudyItem item = new StudyItem();
                item.AgencyId = sweep.AgencyId;
                item.DisplayLabel = sweep.DisplayLabel;
                item.Identifier = sweep.Identifier;
                item.ReferenceItem = study;
                if (sweep.AgencyId == agency)
                {
                    model.Add(item);
                    nodes.Add(new TreeViewNode { id = parent + " " + item.Identifier.ToString(), parent = parent.ToString(), text = item.DisplayLabel });
                }
                i++;
            }
            model = model.OrderBy(r => r.ReferenceItem).ToList();
            return model;
        }

        public static List<SelectListItem> LoadStudies(QuestionModel model, SearchResponse response)
        {
            // move
            List<SelectListItem> studies = new List<SelectListItem>();
            foreach (var result in response.Results)
            {
                studies.Add(new SelectListItem() { Value = result.AgencyId, Text = result.DisplayLabel });
            }
            return studies;
        }

        public static QuestionModel GetStudies(QuestionModel model, string agency)
        {
            // move
            MultilingualString.CurrentCulture = "en-GB";

            SearchFacet facet = new SearchFacet();
            facet.ItemTypes.Add(DdiItemType.Group);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client1 = ClientHelper.GetClient();
            SearchResponse response = client1.Search(facet);
            facet.ItemTypes.Add(DdiItemType.StudyUnit);
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
            var client2 = ClientHelper.GetClient();
            SearchResponse allsweeps = client2.Search(facet);
            model.Studies = LoadStudies(model, response);
            List<StudyItem> studies = new List<StudyItem>();
            string study = null;
            if (agency != null)
            {
                foreach (var result in response.Results)
                {
                    if (result.AgencyId == agency) { study = result.DisplayLabel; }
                }
                studies = GetSweeps(studies, allsweeps, study, agency);
            }
            else
            {
                foreach (var result in response.Results)
                {
                    studies = GetSweeps(studies, allsweeps, result.DisplayLabel, result.AgencyId);
                }

            }
            model.Results = studies;
            return model;
        }

        public static List<StudyItem> GetSweeps(List<StudyItem> model, SearchResponse allsweeps, string study, string agency)
        {
            // move
            foreach (var sweep in allsweeps.Results)
            {
                StudyItem item = new StudyItem();
                item.AgencyId = sweep.AgencyId;
                item.DisplayLabel = sweep.DisplayLabel;
                item.Identifier = sweep.Identifier;
                item.ReferenceItem = study;
                if (agency == null)
                {
                    model.Add(item);
                }
                else
                {
                    if (sweep.AgencyId == agency) { model.Add(item); }
                }
            }
            model = model.OrderBy(r => r.ReferenceItem).ToList();
            return model;
        }


        public static void ProcessCSV(List<VariableItem> results, string type, string fileName)
        {
            string line;
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(@"C:\Users\qtnvwhn\Downloads\" + fileName))
            {
                if (type == "Variable")
                {
                    line = "Search Term, Variable Name, Variable Text, Item, Question Identifier, Agent, Study, Concept";
                }
                else
                {
                    line = "Search Term, Question Name, Question Text, Item, Question Identifier, Agent, Study, Concept";
                }
                file.WriteLine(line);
                foreach (var item in results)
                {
                    line = item.equivalence + "," + item.questionName + "," + item.questionText.Trim().Replace(",", " ") + "," + item.questionItem +
                        "," + item.parentitem + "," + item.study.Trim() + "," + item.studyGroup + "," + item.concept;
                    file.WriteLine(line);
                }
            }
        }

        public static List<Word> GetList(string wordselection)
        {
            // move
            List<Word> wordlist = new List<Word>();
            List<string> wordlist2 = wordselection.Split(',').ToList();
            foreach (var item in wordlist2)
            {
                Word currentword = new Word();
                currentword.Value = item;
                if (item != "") wordlist.Add(currentword);
            }
            return wordlist;
        }

        public static string GetString(List<string> selectedmethods)
        {
            // move
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
    }
}