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
using System.Linq;
using System.Web;
// using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ColecticaSdkMvc.Controllers
{

    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult List(ItemTypesModel model)
        {
            model.ItemTypes = GetItemTypes();

            // Since all the information in the sample Repository is
            // in en-US, we can set the CurrentCulture here and 
            // use MultilingualString's Current property to access
            // the text.
            //
            // If your data has multiple languages, you may want to
            // access those specific languages instead.
            MultilingualString.CurrentCulture = "en-US";

            // Create a new SearchFacet that will find all
            // StudyUnits, CodeSchemes, and CategorySchemes.

            SearchFacet facet = new SearchFacet();
            switch (model.ItemType)
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
                case "Code Set":
                    facet.ItemTypes.Add(new Guid("8b108ef8-b642-4484-9c49-f88e4bf7cf1d"));
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
                case "Data File":
                    facet.ItemTypes.Add(new Guid("a51e85bb-6259-4488-8df2-f08cb43485f8"));
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
                case "Question Set":
                    facet.ItemTypes.Add(DdiItemType.QuestionScheme);
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


            // Set the sort order of the results. Options are 
            // Alphabetical, ItemType, MetadataRank, and VersionDate.
            facet.ResultOrdering = SearchResultOrdering.Alphabetical;
            facet.SearchLatestVersion = true;
            
            // Add SearchTerms to the facet to only return results that contain the specified text.
            // facet.SearchTerms.Add("ca_indresp_w");

            // Add Cultures to only search for text in certain languages.
            //facet.Cultures.Add("da-DK");

            // Use MaxResults and ResultOffset to implement paging, if large numbers of items may be returned.
            //facet.MaxResults = 100;
            //facet.ResultOffset = 0;

            // Now that we have a facet, search for the items in the Repository.
            // The client object takes care of making the Web Services calls.
            var client = ClientHelper.GetClient();
            SearchResponse response = client.Search(facet);

            // Create the model object, and add all the search results to 
            // the model's list of results so they can be displayed.
            // response.Results.Where(x => x.Identifier == new Guid("c01b5d30-035c-4daf-ae1c-dc3e709a4e9c"));       
            model.Results = response.Results.OrderBy(a => a.VersionDate).ToList();
           
            // Return the view, passing in the model.
            return View(model);
        }

        public ActionResult Studies(string type, string agency = null, string id = null, string question = null)
        {
            HomeModel model = new HomeModel();
            model = GetStudies(model,"");
            model.SelectedStudies = new List<string>();
            model.Methods = GetAllMethods();
            model.SelectedMethods = new List<string>();

            switch (type)
            {
                case "Matching":                    
                    model = GetStudies(model,"");
                    model.Type = type;
                    model.QuestionId = new Guid(id);
                    model.QuestionText = question;
                    return View(model);
                case "Study":
                    model = GetStudies(model,null);
                    return View(model);
            }
            model = GetStudies(model,"");
            return View(model);
        }        

        public ActionResult Select(HomeModel model)
        {
            model.Results = new List<StudyItem>();
            model = GetStudies(model,"");
            model.SelectedStudies = new List<string>();
            model.Methods = GetAllMethods();
            model.SelectedMethods = new List<string>();
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Select(HomeModel model, SelectListItem Study, string command, string StudyId = null)
        {
            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();
            if (model.SelectedStudies == null)
            {
                model = GetStudies(model, StudyId);
                model.SelectedStudies = new List<string>();
                model.Methods = GetAllMethods();
                model.SelectedMethods = new List<string>();
                //model = LoadStudies(model);
                return View(model);
            }
            else
            {
                if (model.SelectedStudies.Count() < 2)
                {
                    {
                        model = GetStudies(model,"");                       
                        model.Methods = GetAllMethods();
                        model.SelectedMethods = new List<string>();
                        // model = LoadStudies(model);
                        return View(model);
                    }
                }
                else
                {
                    model = GetStudies(model,"");
                   stepOneModel = ProcessMethods(model);
                }
            }
           
            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);           
            stepOneModel.Duration = res.ToString();
            return DisplayMatches(new ResetMatchesModelStepTwo { StepOneModel = stepOneModel });
        }

        public ActionResult Menu2(HomeModel model)
        {
            model.Results = new List<StudyItem>();
            model = GetStudies(model,null);
            model.SelectedStudies = new List<string>();
            model.Methods = GetAllMethods();
            model.SelectedMethods = new List<string>();
          
            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Menu2(HomeModel model, string Study, string command)
        {
            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();
            if (model.SelectedStudies == null)
            {
                model = GetStudies(model,null);
                model.SelectedStudies = new List<string>();
                model.Methods = GetAllMethods();
                model.SelectedMethods = new List<string>();
                return View(model);
            }
            else
            {
                if (model.SelectedStudies.Count() < 2)
                {
                    {
                        
                        model = GetStudies(model,null);
                        model.SelectedStudies = new List<string>();
                        model.Methods = GetAllMethods();
                        model.SelectedMethods = new List<string>();
                        return View(model);
                    }
                }
                else
                {
                    
                    model = GetStudies(model,null);
                    model.SelectedStudies = new List<string>();
                    model.Methods = GetAllMethods();
                    model.SelectedMethods = new List<string>();
                    stepOneModel = ProcessMethods(model);
                }
            }

            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            return DisplayMatches(new ResetMatchesModelStepTwo { StepOneModel = stepOneModel });
        }
             

        public ActionResult Menu3(string wordselection, string selectedmethods, string selectedItems)
        {

            HomeModel model = new HomeModel();
            List<string> smethods = new List<string>();
           
            if (selectedmethods == null)
            {
                smethods = new List<string>();
                model.SelectedMethods = new List<string>();
            }
            else
            {
                smethods = selectedmethods.Split(',').ToList();
            }
            model.Results = new List<StudyItem>();
            model.SelectedStudies = new List<string>();
            model.Methods = GetAllMethods();            
            if (smethods.Count != 0) model.SelectedMethods = smethods; 

            if (wordselection == null)
            {
                model.WordSelection = "";
                wordselection = "";
            }
            if (wordselection.Length != 0) model.WordList = GetList(wordselection);
            if (wordselection.Length == 0) model.WordList = new List<Word>();
            model.WordSelection = wordselection;
            //}
            //Serialize to JSON string.
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model = BuildStudiesTree(model, nodes);
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);

            return View(model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Menu3(HomeModel model, string Study, string selectedItems, string wordselection, string command)
        {
            List<TreeViewNode> items = new List<TreeViewNode>();
            if (selectedItems != null) { items = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }

            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model.Results = new List<StudyItem>();
            model = BuildStudiesTree(model, nodes);
            model.SelectedStudies = new List<string>();
            model.Methods = GetAllMethods();
            if (model.SelectedMethods == null) model.SelectedMethods = new List<string>(); else model.SelectedMethods = model.SelectedMethods;
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            ViewBag.selectedItems = selectedItems;
            //ViewData["selectedItems"] = selectedItems;
            switch (command)
            {
                case "Save":
                    HomeModel newmodel = new HomeModel();
                    newmodel = SaveItem(newmodel, model.Word, model.WordSelection);
                    var wordlist = newmodel.WordList;
                    var selectedwords = newmodel.WordSelection;
                    newmodel.Results = new List<StudyItem>();
                    newmodel = BuildStudiesTree(model, nodes);
                    newmodel.SelectedStudies = new List<string>();
                    newmodel.Methods = GetAllMethods();
                    newmodel.Word = null;
                    newmodel.WordList = wordlist;
                    newmodel.WordSelection = selectedwords;
                    newmodel.SelectedMethods = model.SelectedMethods;
                    string selectedmethods = GetString(model.SelectedMethods);
                    return RedirectToAction("Menu3", new { selectedItems = selectedItems, wordselection = newmodel.WordSelection, selectedmethods = selectedmethods });
                default:
                    break;
            }
            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();

            model = GetStudies(model, null);
            model.SelectedStudies = new List<string>();
            if (wordselection == null) model.WordList = new List<Word>();
            else
            {
                if (wordselection.Length != 0) model.WordList = GetList(wordselection);
                if (wordselection.Length == 0) model.WordList = new List<Word>();
            }
            if (selectedItems ==  "")
            {
                return View(model);
            }
            if (selectedItems != "")
            {
                switch (items.Count)
                {
                    case 0:                       
                        return View(model);
                    case 1:                       
                        return View(model);
                }
            }
            model = GetStudies(model, null);
            model = LoadSelectedStudies(model, items);
            model.Methods = GetAllMethods();
            stepOneModel = ProcessMethods(model);


            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            switch (command)
            {
                case "Display Matches":
                    return DisplayMatches(new ResetMatchesModelStepTwo { StepOneModel = stepOneModel });
                case "Display Differences":
                    return DisplayDifferences(new ResetMatchesModelStepTwo { StepOneModel = stepOneModel });
            }
            return DisplayMatches(new ResetMatchesModelStepTwo { StepOneModel = stepOneModel });
        }

        public HomeModel SaveItem(HomeModel model,string word, string wordselection)
        {
            model.WordList = new List<Word>();
            wordselection = wordselection + word + ",";
            model.WordList = GetList(wordselection);           
            model.WordSelection = wordselection;
            
            return model;
        }

        public List<Word> GetList(string wordselection)
        {
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
        public ActionResult DeleteItem(string selectedItems, string word, string wordselection, string selectedmethods)
        {
            wordselection = wordselection.Replace(word + ",", "");
            return RedirectToAction("Menu3", new { wordselection = wordselection, selectedmethods = selectedmethods });
        }     

        public HomeModel LoadSelectedStudies(HomeModel model, List<TreeViewNode> items)
        {
            List<string> selectedstudies = new List<string>();
            foreach (var item in items)
            {
                var sweep = model.Results.Where(s => s.AgencyId == item.parent).Where(s => s.DisplayLabel == item.text).FirstOrDefault();
                selectedstudies.Add(sweep.AgencyId + " " + sweep.Identifier.ToString());
            }
            model.SelectedStudies = selectedstudies;
            return model;
        }

        public ResetMatchesModelStepOne ProcessMethods(HomeModel model)
        {
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();

            var agency1 = model.SelectedStudies[0].Substring(0, model.SelectedStudies[0].IndexOf(" "));
            var identifier1 = new Guid(model.SelectedStudies[0].Substring(model.SelectedStudies[0].IndexOf(" ") + 1, model.SelectedStudies[0].Length - model.SelectedStudies[0].IndexOf(" ") - 1).Trim());
            var agency2 = model.SelectedStudies[1].Substring(0, model.SelectedStudies[1].IndexOf(" "));
            var identifier2 = new Guid(model.SelectedStudies[1].Substring(model.SelectedStudies[1].IndexOf(" ") + 1, model.SelectedStudies[1].Length - model.SelectedStudies[1].IndexOf(" ") - 1).Trim());
            StudyUnitModel item1 = GetAllQuestions(agency1, identifier1);
            StudyUnitModel item2 = GetAllQuestions(agency2, identifier2);

            stepOneModel.QuestionsSet1 = item1.Questions;
            stepOneModel.QuestionsSet2 = item2.Questions;
            if (model.SelectedMethods != null)
            {
                foreach (var method in model.SelectedMethods)
                {

                    switch (method)
                    {
                        case "Match Question and Text":
                            stepOneModel = Method1(stepOneModel, model, method);
                            break;
                        case "Match Question Text":
                            stepOneModel = Method1(stepOneModel, model, method);
                            break;
                        case "Match All Words in Question":
                            stepOneModel = Method2(stepOneModel, model, method);
                            break;
                    }
                }
            }
            if (model.WordSelection != null) { stepOneModel = Method3(stepOneModel, model); }
            stepOneModel.Questions1 = item1.Questions.Count();
            stepOneModel.Questions2 = item2.Questions.Count();
            return stepOneModel;
        }

        public ActionResult DisplayMatches(ResetMatchesModelStepTwo stepTwoModel)
        {
            if (stepTwoModel.StepOneModel == null)
            {
                HomeModel model = new HomeModel();
                model = GetStudies(model,null);
                return View("Select", model);
            }
            ResetMatchesModelStepOne stepOneModel = stepTwoModel.StepOneModel;
            
            return View("DisplayMatches", stepOneModel);
        }

        public ActionResult DisplayDifferences(ResetMatchesModelStepTwo stepTwoModel)
        {
            if (stepTwoModel.StepOneModel == null)
            {
                HomeModel model = new HomeModel();
                model = GetStudies(model, null);
                return View("Select", model);
            }
            ResetMatchesModelStepOne stepOneModel = stepTwoModel.StepOneModel;

            return View("DisplayDifferences", stepOneModel);
        }
        private List<SelectListItem> GetAllMethods()
        {
            List<SelectListItem> methods = new List<SelectListItem>();
            methods.Add(new SelectListItem() { Text = "Match Question Text", Value = "0" });
            methods.Add(new SelectListItem() { Text = "Match Question and Text", Value = "1" });
            methods.Add(new SelectListItem() { Text = "Match All Words in Question", Value = "2" });
            return methods;
        }

        private List<SelectListItem> GetItemTypes()
        {
            List<SelectListItem> itemtypes = new List<SelectListItem>();           
            itemtypes.Add(new SelectListItem() { Text =  "Archive", Value = "Archive" });
            itemtypes.Add(new SelectListItem() { Text =  "Category", Value = "Category" });
            itemtypes.Add(new SelectListItem() { Text =  "Category Group", Value = "Category Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Category Set", Value = "Category Set" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List", Value = "Code List" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List Group", Value = "Code List Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Code List Scheme", Value = "Code List Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Code Set", Value = "Code Set" });
            itemtypes.Add(new SelectListItem() { Text =  "Concept", Value = "Concept" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Group", Value = "Concept Group" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Scheme", Value = "Concept Scheme" });
            itemtypes.Add(new SelectListItem() { Text =  "Data Collection", Value = "Data Collection" });
            itemtypes.Add(new SelectListItem() { Text = "Data File", Value = "Data File" });
            itemtypes.Add(new SelectListItem() { Text =  "Group", Value = "Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Instrument", Value = "Instrument" });
            itemtypes.Add(new SelectListItem() { Text = "Question", Value = "Question" });
            itemtypes.Add(new SelectListItem() { Text = "Question Item", Value = "Question Item" });
            itemtypes.Add(new SelectListItem() { Text = "Question Group", Value = "Question Group" });
            itemtypes.Add(new SelectListItem() { Text =  "Study", Value = "Study" });
            itemtypes.Add(new SelectListItem() { Text = "Variable", Value = "Variable" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Group", Value = "Variable Group" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Scheme", Value = "Variable Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Statistic", Value = "Variable Statistic" });
            return itemtypes;
        }

        private ResetMatchesModelStepOne Method1(ResetMatchesModelStepOne  stepOneModel, HomeModel model, string command)
        {
            List<RepositoryItemMetadata> questionset1 = stepOneModel.QuestionsSet1;
            List<RepositoryItemMetadata> questionset2 = stepOneModel.QuestionsSet2;

            var item3 = from x in questionset1 orderby x.DisplayLabel select x;
            RepositoryItemMetadata record2 = new RepositoryItemMetadata();
            int matches = 0;
            foreach (var record1 in item3)
            {
                switch (command)
                {
                    case "Match Question and Text":
                        record2 = questionset2.Where(u => u.DisplayLabel.Equals(record1.DisplayLabel)).Where(u => u.Summary.FirstOrDefault().Value.Equals(record1.Summary.FirstOrDefault().Value)).FirstOrDefault();
                        break;
                    case "Match Question Text":
                        record2 = questionset2.Where(u => u.Summary.FirstOrDefault().Value.Equals(record1.Summary.FirstOrDefault().Value)).FirstOrDefault();
                        break;
                }
                if (record2 != null)
                {
                    var record3 = record2;
                    matches++;
                    Match match = new Match();
                    match.DisplayName1 = record1.DisplayLabel;
                    match.Question1 = record1.Summary.FirstOrDefault().Value;
                    match.DisplayName2 = record2.DisplayLabel;
                    match.Question2 = record2.Summary.FirstOrDefault().Value;
                    stepOneModel.Matches.Add(match);
                    questionset1.Remove(record1);
                    questionset2.Remove(record2);
                }
            }
            stepOneModel.QuestionsSet1 = questionset1;
            stepOneModel.QuestionsSet2 = questionset2;
            return stepOneModel;
        }

        private ResetMatchesModelStepOne Method2(ResetMatchesModelStepOne stepOneModel, HomeModel model, string command)
        {
            List<RepositoryItemMetadata> questionset1 = stepOneModel.QuestionsSet1;
            List<RepositoryItemMetadata> questionset2 = stepOneModel.QuestionsSet2;
            var item3 = from x in questionset1
                        orderby x.DisplayLabel
                        select x;
            int matches = 0;
            foreach (var record1 in questionset1.ToList())
            {
                foreach (var record2 in questionset2.ToList())
                {
                    var Results = Helper.CompareString1.Calculate(record1.Summary.FirstOrDefault().Value, record2.Summary.FirstOrDefault().Value);
                    if (Results >= 100)
                    {
                        matches++;
                        Match match = new Match();
                        match.DisplayName1 = record1.DisplayLabel;
                        match.Question1 = record1.Summary.FirstOrDefault().Value;
                        match.DisplayName2 = record2.DisplayLabel;
                        match.Question2 = record2.Summary.FirstOrDefault().Value;
                        stepOneModel.Matches.Add(match);
                        questionset1.Remove(record1);
                        questionset2.Remove(record2);
                    }
                }              
            }
            stepOneModel.QuestionsSet1 = questionset1;
            stepOneModel.QuestionsSet2 = questionset2;
            return stepOneModel;
        }

        private ResetMatchesModelStepOne Method3(ResetMatchesModelStepOne stepOneModel, HomeModel model)
        {
            model.WordList = GetList(model.WordSelection);

            List<RepositoryItemMetadata> questionset1 = stepOneModel.QuestionsSet1;
            List<RepositoryItemMetadata> questionset2 = stepOneModel.QuestionsSet2;
         
            int matches = 0;
           
            var suggestions1 = (from a in questionset1
                               where model.WordList.All(word => a.DisplayLabel.ToLower().Contains(word.Value.ToLower()))
                               select a);
            var suggestions2 = (from a in questionset2
                               where model.WordList.All(word => a.DisplayLabel.ToLower().Contains(word.Value.ToLower()))
                               select a);   

            foreach (var record1 in suggestions1.ToList())
            {
                foreach (var record2 in suggestions2.ToList())
                {
                    var Results = Helper.CompareString1.Calculate(record1.Summary.FirstOrDefault().Value, record2.Summary.FirstOrDefault().Value);
                    if (Results >= 100)
                    {
                        matches++;
                        Match match = new Match();
                        match.Identifer1 = record1.Identifier.ToString();
                        match.DisplayName1 = record1.DisplayLabel;
                        match.Question1 = record1.Summary.FirstOrDefault().Value;
                        match.Identifier2 = record2.Identifier.ToString();
                        match.DisplayName2 = record2.DisplayLabel;
                        match.Question2 = record2.Summary.FirstOrDefault().Value;
                        stepOneModel.Matches.Add(match);
                        questionset1.Remove(record1);
                        questionset2.Remove(record2);
                    }
                }
            }

            stepOneModel.QuestionsSet1 = questionset1;
            stepOneModel.QuestionsSet2 = questionset2;
            return stepOneModel;
        }

        private HomeModel GetStudies(HomeModel model,string agency)
        {
            MultilingualString.CurrentCulture = "en-US";

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
            model.Studies = LoadStudies(model,response);
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

        private HomeModel BuildStudiesTree(HomeModel model, List<TreeViewNode> nodes)
        {
            MultilingualString.CurrentCulture = "en-US";

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
            int i = 1;
            foreach (var result in response.Results)
            {
                nodes.Add(new TreeViewNode { id = result.AgencyId, parent = "#", text = result.DisplayLabel });
                studies = BuildSweepsTree(studies, allsweeps, result.DisplayLabel, result.AgencyId,nodes, result.AgencyId);
                i++;
            }
            model.Results = studies;
            return model;
        }

        public List<SelectListItem> LoadStudies(HomeModel model, SearchResponse response)
        {
            List<SelectListItem> studies = new List<SelectListItem>();
            foreach (var result in response.Results)
            {
                studies.Add(new SelectListItem() { Value = result.AgencyId, Text = result.DisplayLabel });   
            }
            return studies;
        }

        private static List<StudyItem> GetSweeps(List<StudyItem> model, SearchResponse allsweeps, string study, string agency)
        {
            
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

        private static List<StudyItem> BuildSweepsTree(List<StudyItem> model, SearchResponse allsweeps, string study, string agency, List<TreeViewNode> nodes, string parent)
        {
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

        private static StudyUnitModel GetAllQuestions(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-GB";

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

        //public HomeModel LoadStudies(HomeModel model)
        //{
            //var model1 = model.AllResults.ToList().OrderBy(s => s.result.ReferenceItem);
            
            //List<SelectListItem> studies = new List<SelectListItem>();

            //string studyname = null;
            
            //foreach (var study in model1)
            //{
            //    if (study.result.ReferenceItem != studyname)
            //    {
            //        studies.Add(new SelectListItem() { Value = study.result.ReferenceItem, Text = study.result.ReferenceItem });
            //        studyname = study.result.ReferenceItem;
            //        if (studyname == null) { model.StudyId = study.result.Identifier.ToString(); }
            //    }
            //}       
            //model.Studies = studies;
        //    return model;
        //}

        private ItemModel GetAllQuestions1(string agency, Guid id)
        {
            MultilingualString.CurrentCulture = "en-US";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            ItemModel model = null;
            string viewName = string.Empty;


            var studyUnit = item as StudyUnit;

            // Create the model and set the item as a property, so it's contents can be displayed
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

            model = studyModel;
            viewName = "StudyUnit";

            var history = client.GetVersionHistory(id, agency);
            foreach (var version in history)
            {
                model.History.Add(version);
            }

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

    }
}
