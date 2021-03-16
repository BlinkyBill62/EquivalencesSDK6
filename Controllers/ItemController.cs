using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ColecticaSdkMvc.Controllers
{
    public class ItemController : Controller
    {
        //
        // GET: /Item/

        public ActionResult Index(string agency, Guid id)
        {
            string viewName = string.Empty;
            var model = GetRepository(agency, id);
            
            if (model is StudyUnitModel) { viewName = "StudyUnit"; }
            else { viewName = "GenericItem"; }

           
            return View(viewName, model);
        }

        public ActionResult Levenshtein(string agency, Guid studyid, Guid questionid, string questiontext)
        {
            List<LevenshteinItem> items = new List<LevenshteinItem>();
           
            StudyUnitModel item1 = GetAllQuestions(agency, studyid);
            var item3 = from x in item1.Questions
                        orderby x.DisplayLabel
                        select x;


            string string1 = "In your household what is the number of bedrooms";
            string string2 = "What are the number of bedrooms in your household";
            var test2 = LevenshteinDistance.Calculate(string1, string2);

            var test = new LevenshteinItem()
            {
                QuestionId = questionid.ToString(),
                QuestionText = string2,
                Results = test2.ToString()
            };
            items.Add(test);
            foreach (var question in item3)
            {
                var item = new LevenshteinItem()
                {
                    QuestionId = question.DisplayLabel,
                    QuestionText = question.Summary.FirstOrDefault().Value.ToString(),
                    Results = LevenshteinDistance.Calculate(question.Summary.FirstOrDefault().Value.ToString(), questiontext).ToString()

                };
                items.Add(item);
             
            }
            

            LevenshteinModel model = new LevenshteinModel();
            model.QuestionId = questionid.ToString();
            model.QuestionText = string1;
            model.Results = items;
            return View(model);
        }

        public ActionResult StringCompare(string agency, Guid studyid, Guid questionid, string questiontext)
        {
            DateTime dateTime1 = DateTime.Now;

            List<LevenshteinItem> items = new List<LevenshteinItem>();

            StudyUnitModel item1 = GetAllQuestions(agency, studyid);
            var item3 = from x in item1.Questions
                        orderby x.DisplayLabel
                        select x;

            foreach (var question in item3)
            {
                if (Math.Abs(questiontext.Length - question.Summary.FirstOrDefault().Value.ToString().Length) <= 4)
                {

                    var item = new LevenshteinItem()
                    {
                        QuestionId = question.DisplayLabel,
                        QuestionText = question.Summary.FirstOrDefault().Value.ToString(),
                        Results = CompareString.Calculate(questiontext, question.Summary.FirstOrDefault().Value.ToString())
                    };
                    items.Add(item);
                }
            }
            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            LevenshteinModel model = new LevenshteinModel();
            model.QuestionId = res;
            model.QuestionText = questiontext;
            model.Results = items;
            return View(model);
        }       

        public object GetRepository(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-US";

            var client = ClientHelper.GetClient();

            // Retrieve the requested item from the Repository.
            // Populate the item's children, so we can display information about them.
            var v = client.GetLatestVersionNumber(id,agency);
            IVersionable item1 = client.GetItem(id, agency, v);


            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);



          

            // To populate more than one level of children, you can use the GraphPopulator.
            //GraphPopulator populator = new GraphPopulator(client);
            //item.Accept(populator);

            // The type of model and the view we want depends on the item type.
            // This sample only provides specific support for a few item types,
            // so we will just hard-code the type checking below.
            ItemModel model = null;
            string viewName = string.Empty;
           
            if (item is CategoryScheme)
            {
                var categoryList = item as CategoryScheme;

                // Create the model and set the item as a property, so it's contents can be displayed
                var categorySchemeModel = new CategorySchemeModel();
                categorySchemeModel.CategoryScheme = categoryList;

                model = categorySchemeModel;
                viewName = "CategoryList";
            }
            else if (item is StudyUnit)
            {
                var studyUnit = item as StudyUnit;

                // Create the model and set the item as a property, so it's contents can be displayed
                var studyModel = new StudyUnitModel();
                studyModel.StudyUnit = studyUnit;
                var QualityStatements = studyUnit.QualityStatements.OrderBy(x => x.Identifier).ToList();
                foreach (var qualityStatement in QualityStatements)
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
                var infoList1 = from x in infoList
                                orderby x.DisplayLabel
                                select x;

                foreach (var info in infoList1)
                {
                    studyModel.Questions.Add(info);
                }
                
                model = studyModel;
                viewName = "StudyUnit";
            }
            else if (item is CodeList)
            {
                var codeList = item as CodeList;

                // Create the model and set the item as a property, so it's contents can be displayed
                var codeListModel = new CodeListModel();
                codeListModel.CodeList = codeList;

                model = codeListModel;
                viewName = "CodeList";
            }
            else if (item is QualityStatement)
            {
                var qualityStatement = item as QualityStatement;

                var qualityStatementModel = new QualityStatementModel(qualityStatement);

                model = qualityStatementModel;
                viewName = "QualityStatement";
            }
            else
            {
                model = new ItemModel();
                viewName = "GenericItem";
            }

            // Fopr all item types, get the version history of the item,
            // and add the information to the model.
            var history = client.GetVersionHistory(id, agency);
            foreach (var version in history)
            {
                model.History.Add(version);
            }

            // Use a graph search to find a list of all items that 
            // directly reference this item.
            GraphSearchFacet facet = new GraphSearchFacet();
            facet.TargetItem = item.CompositeId;
            facet.UseDistinctResultItem = true;
            
            var referencingItemsDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);

            // Add the list of referencing items to the model.

            foreach (var info in referencingItemsDescriptions)
            {
                model.ReferencingItems.Add(info);
            }
            return model;
        }

        private static StudyUnitModel GetAllQuestions(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-US";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;
            var studyModel = new StudyUnitModel();
            studyModel.StudyUnit = studyUnit;

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
                studyModel.Questions.Add(info);
            }

            return studyModel;
        }

    }

    public static class LevenshteinDistance
    {
        /// <summary>
        ///     Calculate the difference between 2 strings using the Levenshtein distance algorithm
        /// </summary>
        /// <param name="source1">First string</param>
        /// <param name="source2">Second string</param>
        /// <returns></returns>
        public static int Calculate(string source1, string source2) //O(n*m)
        {
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
            for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            // return result
            return matrix[source1Length, source2Length];
        }


    }

    public static class CompareString
    {
        public static string Calculate(string string1, string string2)
        {
            if (string1 != null && string2 != null)
            {
                string[] list1 = string1.ToLower().Split();
                string[] list2 = string2.ToLower().Split();

                int matches = 0;
                for (int i = 0; i < list1.Count(); i++)
                {
                    bool exists = list2.Any(s => s.Contains(list1[i]));
                    if (exists)
                    {
                        matches++;
                    }
                }
                double num3 = (((double)matches / (double)list1.Count()) * 100);
                return (num3.ToString(("#.##")) + "%");
            }
            else return null;
        }

    }

    public static class CompareString1
    {
        public static double Calculate(string string1, string string2)
        {
            if (string1 != null && string2 != null)
            {
                string[] list1 = string1.ToLower().Split();
                string[] list2 = string2.ToLower().Split();

                int matches = 0;
                for (int i = 0; i < list1.Count(); i++)
                {
                    bool exists = list2.Any(s => s.Contains(list1[i]));
                    if (exists)
                    {
                        matches++;
                    }
                }
                double num3 = (((double)matches / (double)list1.Count()) * 100);
                return num3;
            }
            else return 0;
        }

    }
    public static class CompareString2
    {
        public static double Calculate(string string1, string string2)
        {
            if (string1 != null && string2 != null)
            {
                string[] list1 = string1.ToLower().Split();
                string[] list2 = string2.ToLower().Split();

                int matches = 0;
                int listcount = 0;
                for (int i = 0; i < list1.Count(); i++)
                {
                    if (list1[i].Length > 3)
                    {
                        listcount++;
                        bool exists = list2.Any(s => s.Contains(list1[i]));
                        if (exists)
                        {
                            
                            matches++;
                        }
                    }
                }
                double num3 = (((double)matches / (double)listcount) * 100);
                return num3;
            }
            else return 0;
        }

    }

}
