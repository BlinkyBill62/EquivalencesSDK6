using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Serialization;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using Algenta.Colectica.Repository.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ColecticaSdkMvc.Utility;
using ColecticaSdkMvc.Models;
using System.Web.Script.Serialization;
using System.IO;

namespace ColecticaSdkMvc.Controllers
{
    class GFG : IComparer<string>
    {
        public int Compare(string x, string y)
        {

            if (x == null || y == null)
            {
                return 0;
            }

            // "CompareTo()" method 
            return x.CompareTo(y);

        }
    }
  
    public class EquivalenceController : Controller
    {
        public ActionResult Index()
        {
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            if (mymodel != null) mymodel.SelectedStudies = new List<string>();
            EquivalenceModel model = new EquivalenceModel();
            model = InitialiseModel(model, mymodel,"");
            if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
            return View(model);
        }

        public EquivalenceModel InitialiseModel(EquivalenceModel model, EquivalenceModel live,string selectedStudies)
        {
            model.Methods = GetAllMethods();
            model.SelectedMethods = new List<string>();
            model.WordList = new List<Word>();
            
            if (live != null) { model.AllItems = live.AllItems; } else { model.AllItems = new List<EquivalenceItem>(); }
            if (live != null) { model.MasterItems = live.MasterItems; } else { model.MasterItems = new List<EquivalenceItem>(); }
            if (live != null) { model.Items = live.Items; } else { model.Items = new List<EquivalenceItem>(); }
            if (live != null)
            {
                // model.SelectedStudies = live.SelectedStudies.ToList();               
                ViewBag.selectedItems = selectedStudies;
            }
            else { model.SelectedStudies = new List<string>(); }
            model.selectStudies = selectedStudies;
            model.Name = false;
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model = EquivalenceHelper.BuildStudiesTree(model, nodes);
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);

            return model;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(EquivalenceModel model, string Study, string selectedItems, string originalItems, string fileName, string command, HttpPostedFileBase postedFile)
       {
            DateTime start, finish;

            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            if (mymodel != null) {items = mymodel.Items; }
            string wordselection = "";
            int i = 0, j = 0;
            model.Methods = GetAllMethods();
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            List<TreeViewNode> selectedstudies = new List<TreeViewNode>(); 
            if (selectedItems != null) { selectedstudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            if (postedFile != null)
            {
                model = GetEquivalences(model, postedFile);
                model.Methods = GetAllMethods();
                if (mymodel == null)
                {
                    model.SelectedMethods = new List<string>();
                }
                else
                {
                    if (mymodel.SelectedMethods == null)
                    {
                        model.SelectedMethods = new List<string>();
                    }
                    else
                    {
                        model.SelectedMethods = mymodel.SelectedMethods;
                    }
                }
                if (mymodel == null) { model.SelectedStudies = new List<string>(); } else { model.SelectedStudies = mymodel.SelectedStudies; }
                if (mymodel == null) { model.AllItems = new List<EquivalenceItem>(); } else { model.AllItems = mymodel.AllItems; }
                if (mymodel == null) { model.MasterItems = new List<EquivalenceItem>(); } else { model.MasterItems = mymodel.MasterItems; }
                if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                model = EquivalenceHelper.BuildStudiesTree(model, nodes);
                ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                ViewBag.selectedItems = selectedItems;
                TempData["EquivalenceModel"] = model;
                return View(model);
            }
            model.Results = new List<StudyItem>();
            model = EquivalenceHelper.BuildStudiesTree(model, nodes);
            model.SelectedStudies = new List<string>();
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            ViewBag.selectedItems = selectedItems;
            

            switch (command)
            {
                case "Save":
                    EquivalenceModel newmodel = new EquivalenceModel();
                    newmodel = SaveItem(newmodel, model.Word, model.WordSelection);
                    var wordlist = newmodel.WordList;
                    var selectedwords = newmodel.WordSelection;
                    newmodel.Results = new List<StudyItem>();
                    newmodel = EquivalenceHelper.BuildStudiesTree(model, nodes);
                    newmodel.SelectedStudies = new List<string>();
                    newmodel.Word = null;
                    newmodel.WordList = wordlist;
                    newmodel.WordSelection = selectedwords;
                    newmodel.Methods = GetAllMethods();
                    newmodel.SelectedMethods = new List<string>();
                    newmodel.AllItems = new List<EquivalenceItem>();
                    newmodel.MasterItems = new List<EquivalenceItem>();
                    if (mymodel == null) newmodel.SelectedMethods = new List<string>();
                    ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                    return View(newmodel);
                case "Equivalent Questions":
                    model.EquivalenceError = GetErrors(model, selectedItems);
                    if (model.EquivalenceError != null)
                    {
                        if (model.WordList == null) model.WordList = new List<Word>();
                        if (model.SelectedMethods == null) model.SelectedMethods = new List<string>();
                        if (model.AllItems == null) model.AllItems = new List<EquivalenceItem>();
                        if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                        if (model.MasterItems == null) model.MasterItems = new List<EquivalenceItem>();
                        return View(model);
                    }
                    else
                    {
                        
                        start = DateTime.Now;
                        EquivalenceModel m2 = ProcessEquivalences(model, mymodel, selectedItems, originalItems, selectedstudies, "Question", fileName);
                        finish = DateTime.Now;
                        var elapsedhours = (finish - start).Hours;
                        var elapsedminutes = (finish - start).Minutes;
                        var elapseseconds = (finish - start).Seconds;
                        m2.Elapsed = elapsedhours.ToString() + ":" + elapsedminutes.ToString() + ":" + elapseseconds.ToString();
                        ViewBag.selectedItems = selectedItems;
                        // var waves = GetAllDatasets2(m2);
                        TempData["EquivalenceModel"] = m2;
                        
                        return View("Display", m2);
                    }
                case "Equivalent Variables":
                    model.EquivalenceError = GetErrors(model, selectedItems);
                    if (model.EquivalenceError != null)
                    {
                        if (model.WordList == null) model.WordList = new List<Word>();
                        if (model.SelectedMethods == null) model.SelectedMethods = new List<string>();
                        if (model.AllItems == null) model.AllItems = new List<EquivalenceItem>();
                        if (model.AllResults == null) model.AllResults = new List<EquivalenceItem>();
                        if (model.MasterItems == null) model.MasterItems = new List<EquivalenceItem>();
                        return View(model);
                    }
                    else
                    {
                        start = DateTime.Now;
                        EquivalenceModel m3 = ProcessEquivalences(model, mymodel, selectedItems, originalItems, selectedstudies, "Variable", fileName);
                        ViewBag.selectedItems = selectedItems;
                        TempData["EquivalenceModel"] = m3;
                        finish = DateTime.Now;
                        var elapsedhours = (finish - start).Hours;
                        var elapsedminutes = (finish - start).Minutes;
                        var elapseseconds = (finish - start).Seconds;
                        m3.Elapsed = elapsedhours.ToString() + ":" + elapsedminutes.ToString() + ":" + elapseseconds.ToString();
                        if (m3.AllResults.Count == 0)
                        {
                            m3 = InitialiseModel(model, mymodel, selectedItems);
                            m3.EquivalenceError = "No matching Items";
                            return View(m3);
                        }
                        else
                        {
                            return View("Display", m3);
                        }
                    }
                default:
                    break;
            }

            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();

            model = EquivalenceHelper.GetStudies(model, null);
            model.SelectedStudies = new List<string>();
            if (wordselection == null) model.WordList = new List<Word>();
            else
            {
                if (wordselection.Length != 0) model.WordList = EquivalenceHelper.GetList(wordselection);
                if (wordselection.Length == 0) model.WordList = new List<Word>();
            }
            if (selectedItems == "")
            {
                return View(model);
            }
          
            model = EquivalenceHelper.GetStudies(model, null);

            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            return View(model);
        }

        public string GetErrors(EquivalenceModel model, string selectedstudies)
        {
            string error = null;
            if (selectedstudies == "")
            {
                error = error + " Select a Study or Studies. ";
            }
            if (model.WordList == null && model.SelectedMethods == null)
            {
                error = error + " Select a method or equivalences";
            }
            else
            {
                if (model.WordList.Count == 0 && model.SelectedMethods == null)
                {
                    error = error + " Select a method or equivalences";
                }
            }
            return error;
        }

        public EquivalenceModel ProcessEquivalences(EquivalenceModel model, EquivalenceModel mymodel, string selectedItems, string originalItems, List<TreeViewNode> selectedstudies, string type, string fileName)
        {
            int i = 0, j = 0;
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model.EquivalenceError = null;
            if (model == null) { model.MasterItems = new List<EquivalenceItem>(); } /// else { model.MasterItems = mymodel.MasterItems; }
            if (model == null) { model.AllItems = new List<EquivalenceItem>(); } //else { model.AllItems = mymodel.AllItems; }
            model = LoadSelectedStudies(model, selectedstudies);
            var result = CompareStudyLists(originalItems, selectedItems);
            if (result == false)
            {
                model.AllItems = new List<EquivalenceItem>();
                model.MasterItems = new List<EquivalenceItem>();
                model = ProcessStudies(model);
            }
            EquivalenceModel m2 = new EquivalenceModel();
            m2.AllResults = ProcessMethods(model.AllItems, model.MasterItems, model.SelectedMethods, type, i, j);
            m2.AllItems = model.AllItems;
            //m2.MasterItems = model.MasterItems;
            m2.SelectedStudies = model.SelectedStudies;
            m2.WordList = model.WordList;
            m2.Name = model.Name;
            if (model.WordList != null) { m2 = PopulateQuestionMessages(m2, nodes, type, i, j); }
            m2.AllItems.SetValue(a => a.removed = false).ToList();
            var removed = m2.AllItems.Where(a => a.removed == false).ToList();

            m2.FileName = fileName;
            m2.Type = type;
            model.selectStudies = selectedItems;
            TempData["EquivalenceModel"] = m2;
            return m2;

        }

        public bool CompareStudyLists(string originalItems, string selectedItems)
        {
            if (selectedItems == null) return false;
            else
            {
                var result = originalItems.Equals(selectedItems, StringComparison.Ordinal);
                return result;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Display(EquivalenceModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;

            //Serialize to JSON string.
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model = EquivalenceHelper.BuildStudiesTree(model, nodes);
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);

            switch (command)
            {
               
                case "Save CSV":
                    EquivalenceHelper.ProcessCSV(model.AllResults, itemType, model.FileName + " - " + itemType + " - " + studyName + ".csv");
                    TempData["AllResults"] = model.AllResults;
                    return View(model);
                case "Previous":
                    model = InitialiseModel(model, mymodel,selectedItems);
                    model.Type = mymodel.Type;
                    ViewBag.selectedItems = selectedItems;
                    TempData["EquivalenceModel"] = model;
                    return View("Index", model);
                case "Next":
                    //model.AllItems = mymodel.AllItems;
                    //model.MasterItems = mymodel.MasterItems;
                    //model.SelectedStudies = mymodel.SelectedStudies;
                    //model.Type = mymodel.Type;
                    model.AllResults = model.AllResults.Where(a => a.selected == true).OrderBy(a => a.uniqueId).ToList();
                    ViewBag.selectedItems = selectedItems;
                    TempData["EquivalenceModel"] = model;
                    return View("Variables", model);
                default:
                    return View(model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Variables(EquivalenceModel model, string selectedItems, string studyName, string itemType, string command)
        {
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;
            //model.AllItems = mymodel.AllItems;
            //model.MasterItems = mymodel.MasterItems;
            //model.SelectedStudies = mymodel.SelectedStudies;
            // model.Type = mymodel.Type;
            switch (command)
            {

                case "Save CSV":
                    model.AllResults = ProcessChanges(model.AllResults);
                    EquivalenceHelper.ProcessCSV(model.AllResults, itemType, model.FileName + ".csv");
                    TempData["EquivalenceModel"] = model;
                    return View(model);
                case "Previous":
                    //model.AllItems = mymodel.AllItems;
                    //model.MasterItems = mymodel.MasterItems;
                    //model.SelectedStudies = mymodel.SelectedStudies;
                    //model.Type = mymodel.Type;
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Display", model);
                case "Next":
                    model.AllItems = mymodel.AllItems;
                    model.MasterItems = mymodel.MasterItems;
                    List<VariableItem> newresults = new List<VariableItem>();
                    var waves = from r in model.AllResults
                                group r by r.study into r1
                                select new { Name = r1.Key };
                
                    ExpectedModel model1 = new ExpectedModel();
                    model1 = GetExpectedItems(model.AllResults);
                    model1.AllItems = model.AllItems;
                    model1.AllResults = model.AllResults;
                    model1.SelectedStudies = model.SelectedStudies;
                    ViewBag.selectedItems = selectedItems;
                    //model1.SelectedStudies = mymodel.SelectedStudies;
                    //model.SelectedStudies = mymodel.SelectedStudies;
                    // model.Type = mymodel.Type;
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Results", model1);              
                default:
                    return View(model);
            }
        }

        public List<EquivalenceItem> ProcessChanges(List<EquivalenceItem> results)
        {
            string currentquestion = "";
            string currentname = "";
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            foreach (var result in results)
            {
                if (result.name != null) { currentquestion = result.name; }
                if (result.description != null) { currentname = result.description; }

                EquivalenceItem item = new EquivalenceItem();
                item.uniqueId = result.uniqueId;
                item.equivalence = result.equivalence;
                item.name = result.name;
                item.description = currentname;
                item.counter = result.counter;
                item.questionName = result.questionName;
                item.questionText = result.questionText;              
                item.questionItem = result.questionItem;
                item.questionIdentifier = result.questionIdentifier;
                item.variableName = currentquestion;
                item.variableText = currentname;
                item.variableAgency = result.variableAgency;
                item.variableItem = result.variableItem;
                item.variableIdentifier = result.questionIdentifier;
                item.studyGroup = result.studyGroup;
                item.study = result.study;
                item.dataset = result.dataset;
                item.concept = result.concept;
                item.column = result.column;
                item.selected = result.selected;
                items.Add(item);
            }
            return items;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Results(ExpectedModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;

            switch (command)
            {
               
                case "Previous":
                    EquivalenceModel model2 = new EquivalenceModel();
                    model2.AllItems = model.AllItems;
                    model2.AllResults = model.AllResults;
                    model2.SelectedStudies = model.SelectedStudies;
                    //model.AllItems = mymodel.AllItems;
                    //model.MasterItems = mymodel.MasterItems;
                    // model.AllResults = mymodel.AllResults;
                    // model.SelectedStudies = mymodel.SelectedStudies;
                    //model.Type = mymodel.Type;
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Variables", model2);
                case "Next":
                    //model.SelectedStudies = mymodel.SelectedStudies;
                    //model = InitialiseModel(model, mymodel, selectedItems);
                    ExpectedModel model1 = new ExpectedModel();
                    model.AllResults.SetValue(a => a.removed = false).ToList();
                    model1 = GetExpectedItems2(model.AllResults, model.SelectedStudies);
                    model1.AllItems = model.AllItems;
                    model1.AllResults = model.AllResults;
                    model1.SelectedStudies = model.SelectedStudies;
                    TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    return View("Results2", model1);
                default:
                    return View(model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Results2(ExpectedModel model, string studyName, string originalItems, string selectedItems, string itemType, string command)
        {
            EquivalenceModel mymodel = TempData["EquivalenceModel"] as EquivalenceModel;          

            switch (command)
            {

                case "Previous":
                    //model.AllItems = mymodel.AllItems;
                    //model.MasterItems = mymodel.MasterItems;
                    //model.AllResults = mymodel.AllResults;
                    // model.SelectedStudies = mymodel.SelectedStudies;
                    //model.Type = mymodel.Type;
                    //TempData["EquivalenceModel"] = model;
                    ViewBag.selectedItems = selectedItems;
                    model.AllResults.SetValue(a => a.removed = false).ToList();
                    ExpectedModel model2 = new ExpectedModel();                    
                    model2 = GetExpectedItems(model.AllResults);
                    model2.AllItems = model.AllItems;
                    model2.AllResults = model.AllResults;
                    model2.SelectedStudies = model.SelectedStudies;
                    ViewBag.selectedItems = selectedItems;
                    return View("Results", model2);
                case "Next":                

                    EquivalenceModel model1 = new EquivalenceModel();
                   
                    // model.SelectedStudies = mymodel.SelectedStudies;
                    model1 = InitialiseModel(model1, mymodel, selectedItems);
                    model1.SelectedStudies = model.SelectedStudies;
                    model1.AllItems = model.AllItems;
                    //Serialize to JSON string.
                    List<TreeViewNode> nodes = new List<TreeViewNode>();
                    model1 = EquivalenceHelper.BuildStudiesTree(model1, nodes);
                    ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);

                    TempData["EquivalenceModel"] = model1;
                    ViewBag.selectedItems = selectedItems;
                    return View("Index", model1);
                default:
                    return View(model);
            }
        }

        public ExpectedModel GetExpectedItems(List<EquivalenceItem> results)
        {
            ExpectedModel model = new ExpectedModel();
            //model.AllResults = results.OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();

            List<ExpectedItem> expecteditems = new List<ExpectedItem>();
            var equivalences = from r in results
                        group r by r.uniqueId into r1
                        select new { Name = r1.Key };
            equivalences = equivalences.ToList();
            
            foreach (var equivalence in equivalences)
            {
                                 
                var questions = results.Where(a => a.uniqueId == equivalence.Name).Where(a => a.removed == false).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();

                while (questions.Count != 0)
                {
                    var waves = GetAllStudies(results);

                    ExpectedItem expecteditem = new ExpectedItem();
                    foreach (var wave in waves)
                    {

                        var question = (from q in questions
                                        where q.studyGroup == wave.StudyName
                                        where q.removed == false
                                        select q).FirstOrDefault();
                        if (question != null)
                        {
                            expecteditem.UniqueId = question.uniqueId;
                            expecteditem.Name = question.equivalence;
                            expecteditem.Description = question.variableText;
                            expecteditem.Topic = question.concept;
                            questions.Where(a => a.variableItem == question.variableItem).SetValue(a => a.removed = true).ToList();
                            waves.Where(a => a.StudyName == wave.StudyName).SetValue(a => a.Value = question.variableName).ToList();
                        }
                    }
                    expecteditem.Waves = waves;
                    expecteditems.Add(expecteditem);
                    questions = questions.Where(a => a.removed == false).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
                }
            }


            model.expecteditems = expecteditems;
            return model;
        }

        public ExpectedModel GetExpectedItems2(List<EquivalenceItem> results, List<string> selecteditems)
        {
            ExpectedModel model = new ExpectedModel();
            results = results.OrderBy(a => a.column).OrderBy(a => a.uniqueId).OrderBy(a => a.dataset).ToList();

            List<ExpectedItem> expecteditems = new List<ExpectedItem>();
            var equivalences = from r in results
                               group r by r.uniqueId into r1
                               select new { Name = r1.Key };
            equivalences = equivalences.ToList();

            var waves = GetAllDatasets(results);
            var waves2 = GetAllDatasets2(results, selecteditems);

            foreach (var equivalence in equivalences)
            {

                ExpectedItem expecteditem = new ExpectedItem();
                List<Study> datasets = new List<Study>();
                foreach (var wave in waves2)
                {
                    var questions = results.Where(a => a.uniqueId == equivalence.Name).Where(a => a.dataset == wave.StudyName).Where(a => a.removed == false).OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();
                    if (questions.Count != 0)
                    {
                        expecteditem.UniqueId = questions.FirstOrDefault().uniqueId;
                        expecteditem.Name = questions.FirstOrDefault().equivalence;
                        expecteditem.Description = questions.FirstOrDefault().variableText;
                        expecteditem.Topic = questions.FirstOrDefault().concept;
                        Study dataset = new Study();
                        foreach (var question in questions)
                        {
                            dataset.ID = equivalence.Name;
                            dataset.StudyName = question.dataset;
                            dataset.Value = dataset.Value + question.variableName + ";";
                        }
                        datasets.Add(dataset);
                    }
                    else
                    {
                        Study dataset2 = new Study();
                        dataset2.ID = equivalence.Name;
                        dataset2.StudyName = wave.StudyName;
                        datasets.Add(dataset2);
                    }

                }
                expecteditem.Waves = datasets;
                expecteditems.Add(expecteditem);
            }
            model.expecteditems = expecteditems;
            return model;
        }

        public List<Study> GetAllStudies(List<EquivalenceItem> results)
        {
            var waves = from r in results
                        group r by r.studyGroup into r1
                        select new { Name = r1.Key };
            
            List<Study> studies = new List<Study>();
            int i = 1;
            foreach (var wave in waves)
            {
                Study study = new Study();
                study.ID = i;
                study.StudyName =  wave.Name;
                studies.Add(study);
                i++;
            }
            return studies;
        }

        public List<Study> GetAllDatasets(List<EquivalenceItem> results)
        {
            var waves = from r in results
                        group r by r.dataset into r1
                        select new { Name = r1.Key };

            List<Study> studies = new List<Study>();
            int i = 1;
            foreach (var wave in waves)
            {
                Study study = new Study();
                study.ID = i;
                study.StudyName = wave.Name;
                studies.Add(study);
                i++;
            }
            return studies;
        }

        public List<Study> GetAllDatasets2(List<EquivalenceItem> results, List<string> selecteditems)
        {

            MultilingualString.CurrentCulture = "en-GB";
            SearchFacet facet = new SearchFacet();
            facet.ItemTypes.Add(new Guid("a51e85bb-6259-4488-8df2-f08cb43485f8"));
            facet.ResultOrdering = SearchResultOrdering.Alphabetical;
            facet.SearchLatestVersion = true;
            var client = ClientHelper.GetClient();
            SearchResponse datasets = client.Search(facet);
            List<SearchResult> datasets1 = new List<SearchResult>(); 
            var waves = from r in results
                        group r by r.dataset into r1
                        select new { Name = r1.Key };
            List<string> waves1 = new List<string>();
            foreach (var wave in waves)
            {
                waves1.Add(wave.Name);
            }
            List<string> selecteddatasets = new List<string>();
            foreach (var selecteditem in selecteditems)
            {
                string selectedwave = selecteditem;
                selectedwave = selectedwave.Substring(selectedwave.IndexOf(",") + 1, selectedwave.Length - selectedwave.IndexOf(",") - 1);
                selectedwave = selectedwave.Substring(selectedwave.IndexOf(",") + 1, selectedwave.Length - selectedwave.IndexOf(",") - 1);
                var selectedagency = selecteditem.Substring(0, selecteditem.IndexOf(","));
                if (datasets1.Count == 0) { datasets1 = datasets.Results.Where(x => x.AgencyId.Contains(selectedagency) == true).ToList(); }

                foreach (var dataset in datasets1)
                {
                    var reference = Helper.GetReferences(dataset.AgencyId, dataset.Identifier).Where(x => x.DisplayLabel == selectedwave).ToList();
                    
                    if (reference.Count() != 0)
                    {
                        selecteddatasets.Add(dataset.DisplayLabel);
                    }
                }
            }
            List<Study> studies = new List<Study>();
            int i = 1;

            foreach (var wave in selecteddatasets)
            {
                foreach (var dataset in datasets1)
                {
                    // var reference = Helper.GetReferences(dataset.AgencyId, dataset.Identifier).FirstOrDefault();
                    if (dataset.DisplayLabel == wave)
                    {
                        Study study = new Study();
                        study.ID = i;
                        study.StudyName = wave;
                        studies.Add(study);
                    }
                    i++;
                }
            }
            return studies;
        }

        public static List<string> ProcessStudies1(EquivalenceModel model)
        {
            int i = 1;

            List<string> items = new List<string>();

            foreach (var study in model.SelectedStudies)
            {
                List<string> studylist = study.Split(',').ToList();
                string agency = "", id = "", group = "";
                int j = 0;
                foreach (var studyitem in studylist)
                {
                    if (j == 0) { agency = studyitem; }
                    if (j == 1) { id = studyitem; }
                    if (j == 2) { group = studyitem; }
                    j++;
                }
                var identifier = new Guid(id);
                items.Add(group);
                i++;

            }

            return items;
        }

        //public static EquivalenceModel ProcessStudies(EquivalenceModel model)
        //{
        //    int i = 1;

        //    List<EquivalenceItem> items = new List<EquivalenceItem>();

        //    foreach (var study in model.SelectedStudies)
        //    {
        //        List<string> studylist = study.Split(',').ToList();
        //        string agency = "", id = "", group = "";
        //        int j = 0;
        //        foreach (var studyitem in studylist)
        //        {
        //            if (j == 0) { agency = studyitem; }
        //            if (j == 1) { id = studyitem; }
        //            if (j == 2) { group = studyitem; }
        //            j++;
        //        }
        //        var identifier = new Guid(id);
        //        model = GetAllRepositoryItems(model, agency, identifier, group, new Guid(id), i);
        //        i++;

        //    }

        //    return model;
        //}

        public static EquivalenceModel ProcessStudies(EquivalenceModel model)
        {
            int i = 1;

            List<EquivalenceItem> items = new List<EquivalenceItem>();

            foreach (var study in model.SelectedStudies)
            {
                List<string> studylist = study.Split(',').ToList();
                string agency = "", id = "", group = "";
                int j = 0;
                foreach (var studyitem in studylist)
                {
                    if (j == 0) { agency = studyitem; }
                    if (j == 1) { id = studyitem; }
                    if (j == 2) { group = studyitem; }
                    j++;
                }
                var identifier = new Guid(id);
                var references = Helper.GetReferences(agency, identifier);
                model = GetAllRepositoryItems(model, agency, identifier, group, new Guid(id), i);
                i++;

            }

            return model;
        }


        public List<VariableItem> ProcessResults(RepositoryItemMetadata result, List<VariableItem> items, EquivalenceModel model, string equivalence, int uniqueId, int counter)
        {
           
            var variables = RepositoryHelper.GetReferences(result.AgencyId, result.Identifier).Where(x => x.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d"));
            foreach (var variable in variables)
            {
                VariableItem item = new VariableItem();
                item.name = null;
                item.description = variable.DisplayLabel;
                item.counter = counter;
                item.questionName = variable.ItemName.FirstOrDefault().Value;
                item.questionText = variable.Label.FirstOrDefault().Value;
                item.questionItem = variable.CompositeId.ToString();
                item.parentitem = result.Identifier.ToString();
                item.studyGroup = variable.AgencyId;
                item.identifier = variable.Identifier;

                var concept = (from a in model.AllConcepts
                               where a.ItemType == result.Identifier
                               select a).FirstOrDefault();
                var v = RepositoryHelper.GetConcept(result.AgencyId, result.Identifier);
                RepositoryItemMetadata mainconcept = new RepositoryItemMetadata();
                if (concept != null) { mainconcept = RepositoryHelper.GetConcept(concept.AgencyId, concept.Identifier); }
                var dataset = RepositoryHelper.GetConcept(variable.AgencyId, variable.Identifier);
                if (concept != null) item.concept = concept.Label.Values.FirstOrDefault() + " - " + mainconcept.Label.Values.FirstOrDefault();
                item.description = variable.Label.Values.FirstOrDefault();
                item.questionText = item.description;
                item.questionName = variable.ItemName.Values.FirstOrDefault();
                item.study = RepositoryHelper.GetStudy(result.AgencyId, result.Identifier);
                item.name = variable.DisplayLabel;

                items.Add(item);
                item.uniqueId = uniqueId;
                item.equivalence = equivalence.Trim();
                // item.column = RepositoryHelper.GetStudyColumn(item.study, model.StudyId);
                item.selected = true;
                item.isdeprecated = variable.IsDeprecated;
            }           
            return items;
        }

        public EquivalenceModel PopulateQuestionMessages(EquivalenceModel model, List<TreeViewNode> selecteditems, string type, int i, int j)
        {
            List<EquivalenceItem> equivalenceitems = new List<EquivalenceItem>();    
            if (model.AllResults == null) { model.AllResults = new List<EquivalenceItem>(); }
            List<Word> words1 = new List<Word>();

            
            var waves = GetAllStudies(model.AllItems);
            List<EquivalenceItem> items = new List<EquivalenceItem>();
            foreach (var selectedwords in model.WordList)
            {

                equivalenceitems = model.AllItems;
                List<Word>  words2 = new List<Word>();
                List<string> wordList2 = selectedwords.Value.Split(' ').ToList();
                foreach (var word2 in wordList2)
                {
                    Word currentword = new Word();
                    currentword.Value = word2;
                    words2.Add(currentword);
                }
                string selectedword = "";
                foreach (var currentword in words2)
                {
                    // selectedword = " " + currentword.Value + " ";
                    selectedword = currentword.Value;
                    switch (type)
                    {
                        case "Question":
                            switch (model.Name)
                            {
                                case false:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.questionText.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                                case true:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.questionName.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                            }
                            break;
                        case "Variable":
                            switch (model.Name)
                            {
                                case false:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.variableText.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                                case true:
                                    equivalenceitems = (from a in equivalenceitems
                                                        where words2.Any(word => a.variableName.ToLower().Contains(selectedword.ToLower()))
                                                        select a).ToList();
                                    break;
                            }
                            break;
                    }
                }

                if (equivalenceitems.Count != 0)
                {
                    i++;

                    foreach (var result in equivalenceitems)
                    {
                        j++;

                        EquivalenceItem item = new EquivalenceItem();
                        item.counter = j;
                        item.name = result.variableName;
                        item.description = result.variableText;
                        item.variableName = result.variableName;
                        item.variableText = result.variableText;
                        item.variableItem = result.variableItem;
                        item.questionName = result.questionName;
                        item.questionText = result.questionText;
                        item.questionItem = result.questionItem;
                        item.study = result.study;
                        item.studyGroup = result.studyGroup;
                        item.dataset = result.dataset;
                        item.concept = result.concept;
                        item.study = result.study;

                        item.uniqueId = i;
                        item.equivalence = selectedwords.Value.Trim();
                        item.column = waves.Where(a => a.StudyName == result.studyGroup).Select(a => { return a.ID; }).FirstOrDefault();
                        item.selected = true;
                        var references = Helper.GetReferences(result.variableAgency, result.variableIdentifier).Where(x => x.ItemType == new Guid("3b438f9f-e039-4eac-a06d-3fa1aedf48bb")).ToList();
                        var dataset = Helper.GetReferences(references.FirstOrDefault().AgencyId, references.FirstOrDefault().Identifier).OrderBy(x => x.Version).LastOrDefault();
                        item.dataset = dataset.ItemName.FirstOrDefault().Value;
                        model.AllResults.Add(item);


                    }
                }
            }
            model.AllResults = model.AllResults.OrderBy(x => x.dataset).OrderBy(x => x.uniqueId).ToList();
            // model.AllResults = items;
            return model;
        }



        public EquivalenceModel GetEquivalences(EquivalenceModel model, HttpPostedFileBase postedFile)
        {

            try
            {
                string fileExtension = Path.GetExtension(postedFile.FileName);
                if (fileExtension != ".csv")
                {
                    return model;
                }
                string wordselection = "";
                List<Word> equivalences = new List<Word>();
                using (var sreader = new StreamReader(postedFile.InputStream))
                {
                    while (!sreader.EndOfStream)
                    {
                        string[] rows = sreader.ReadLine().Split(',');
                        Word word = new Word();
                        word.Value = rows[0].ToString();
                        equivalences.Add(word);
                        wordselection = wordselection + rows[0].ToString() + ",";
                    }
                }
                model.WordList = equivalences;
                model.Results = new List<StudyItem>();
                model.FileName = postedFile.FileName.Replace(".csv","");
                return model;
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return model;

        }

        public EquivalenceModel SaveItem(EquivalenceModel model, string word, string wordselection)
        {
            // keep
            model.WordList = new List<Word>();
            wordselection = wordselection + word + ",";
            model.WordList = EquivalenceHelper.GetList(wordselection);
            model.WordSelection = wordselection;

            return model;
        }

       
        public ActionResult DeleteItem(string selectedItems, string word, string wordselection)
        {
            // keep
            wordselection = wordselection.Replace(word + ",", "");
            return RedirectToAction("Equivalences", new { wordselection = wordselection });
        }

        public EquivalenceModel LoadSelectedStudies(EquivalenceModel model, List<TreeViewNode> items)
        {
            List<string> selectedstudies = new List<string>();

            foreach (var item in items)
            {
                var sweep = model.Results.Where(s => s.AgencyId == item.parent).Where(s => s.DisplayLabel == item.text).FirstOrDefault();
                selectedstudies.Add(item.id.Replace(" ", ",") + "," + item.text);
            }
            model.SelectedStudies = selectedstudies;

            return model;
        }

        public static EquivalenceModel GetAllRepositoryItems(EquivalenceModel model, string agency, Guid id, string group, Guid studyitem, int study)
        {

            MultilingualString.CurrentCulture = "en-GB";

            var client = ClientHelper.GetClient();

            IVersionable item = client.GetLatestItem(id, agency,
                 ChildReferenceProcessing.Populate);

            var studyUnit = item as StudyUnit;
           
            SetSearchFacet setFacet = new SetSearchFacet();
            setFacet.ItemTypes.Add(DdiItemType.QuestionItem);

            if (studyUnit == null) return model;
            var matches = client.SearchTypedSet(studyUnit.CompositeId,
                setFacet);
            var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection());
            //******************************************************
            item = client.GetLatestItem(studyitem, agency,
                 ChildReferenceProcessing.Populate);
            SetSearchFacet setFacet1 = new SetSearchFacet();
            setFacet1.ItemTypes.Add(DdiItemType.DataCollection);
            matches = client.SearchTypedSet(studyUnit.CompositeId,
                           setFacet);
            infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection());
            //******************************************************
            var currentstudy = Helper.GetReferences(agency, studyitem).ToList();
            var currentstudy1 = Helper.GetReferences(currentstudy.FirstOrDefault().AgencyId, currentstudy.FirstOrDefault().Identifier);
            foreach (var info in infoList)
            {

                var variables = Helper.GetReferences(info.AgencyId, info.Identifier).Where(x => x.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d")).ToList();

                foreach (var variable in variables)
                {
                    var latestvariable = from r in variables
                                         where r.Identifier == variable.Identifier
                                         group r by r.Identifier into r1
                                         select new { Identifier = r1.Key, VersionNumber = (from t2 in r1 select t2.Version).Max() };
                    latestvariable = latestvariable.ToList();


                    if (variable.Version == latestvariable.FirstOrDefault().VersionNumber)
                    {
                        EquivalenceItem equivalenceitem = new EquivalenceItem();
                        equivalenceitem.study = info.AgencyId;
                        equivalenceitem.variableAgency = variable.AgencyId;
                        equivalenceitem.variableName = variable.ItemName.FirstOrDefault().Value;
                        equivalenceitem.variableText = variable.Label.FirstOrDefault().Value;
                        if (variable.Label.FirstOrDefault().Value != null) equivalenceitem.variableOrdered = GetOrderedText(variable.Label.FirstOrDefault().Value);
                        equivalenceitem.variableItem = variable.CompositeId.ToString();
                        equivalenceitem.variableIdentifier = variable.Identifier;
                        equivalenceitem.questionName = info.ItemName.FirstOrDefault().Value;
                        equivalenceitem.questionText = info.Summary.FirstOrDefault().Value;
                        if (info.Summary.FirstOrDefault().Value != null) equivalenceitem.questionOrdered = GetOrderedText(info.Summary.FirstOrDefault().Value);
                        equivalenceitem.questionItem = info.CompositeId.ToString();
                        equivalenceitem.questionIdentifier = info.Identifier;
                        equivalenceitem.studyGroup = group;
                        equivalenceitem.set = study;
                        var concept = Helper.GetReferences(info.AgencyId, info.Identifier).Where(x => x.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205")).FirstOrDefault();
                        if (concept != null) equivalenceitem.concept = concept.Label.Values.FirstOrDefault();
                        if (study == 1) { model.MasterItems.Add(equivalenceitem); }   
                        var references = Helper.GetReferences(variable.AgencyId, variable.Identifier).Where(x => x.ItemType == new Guid("3b438f9f-e039-4eac-a06d-3fa1aedf48bb")).ToList();
                        var dataset = Helper.GetReferences(references.FirstOrDefault().AgencyId, references.FirstOrDefault().Identifier).OrderBy(x => x.Version).LastOrDefault();
                        //var version = client.GetLatestVersionNumber(references.FirstOrDefault().Identifier, references.FirstOrDefault().AgencyId);
                        //var dataset2 = client.GetRepositoryItem(references.FirstOrDefault().Identifier, references.FirstOrDefault().AgencyId, version);
                        //PhysicalInstance instance = new PhysicalInstance();
                        
                        //var dataset3 = client.GetLatestRepositoryItem(dataset.Identifier, dataset.AgencyId);
                        //int spos = dataset3.Item.IndexOf("<r:AlternateTitle>");
                        //i/nt epos = dataset3.Item.IndexOf("</r:AlternateTitle");
                        //string alttitle = dataset3.Item.Substring(spos, epos - spos);
                        equivalenceitem.dataset = dataset.ItemName.FirstOrDefault().Value;
                        
                        model.AllItems.Add(equivalenceitem);

                    }
                }
            }                       
            return model;
        }

        public static void ProcessVariables1CSV(List<EquivalenceItem> results, string fileName)
        {
            string line;
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(@"C:\Users\qtnvwhn\" + fileName + " - Variables #1"))
            {
                line = "Variable Name, Variable Text, VariableItem";
                file.WriteLine(line);
                results.OrderBy(x => x.variableItem);
                foreach (var item in results)
                {
                    line = item.variableName + "," + item.variableText.Trim().Replace(",", " ") + "," + item.variableItem; 
                    file.WriteLine(line);
                }
            }
        }

        public static void ProcessVariables2CSV(List<RepositoryItemMetadata> results, string fileName)
        {
            string line;
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(@"C:\Users\qtnvwhn\" + fileName + " - Variables #2"))
            {
                line = "Variable Name, Variable Text, VariableItem";
                file.WriteLine(line);
                results.OrderBy(x => x.CompositeId);
                foreach (var item in results)
                {
                    line = item.ItemName.FirstOrDefault().Value + "," + item.Label.FirstOrDefault().Value.Replace(",", " ") + "," + item.CompositeId;
                    file.WriteLine(line);
                }
            }
        }

        private static string GetOrderedText(string sentance)
        {
            string ordered = null;
            GFG gg = new GFG();
            List<string> words = sentance.Split(' ').ToList();           
            words.Sort(gg);
            foreach (var word in words)
            {
                if (word.Length > 3) ordered = ordered + word + " ";
            }
            return ordered;
        }      

        private List<SelectListItem> GetAllMethods()
        {
            List<SelectListItem> methods = new List<SelectListItem>();
            methods.Add(new SelectListItem() { Text = "Match Text", Value = "0" });
            methods.Add(new SelectListItem() { Text = "Match All Words", Value = "1" });
            return methods;
        }

        public List<EquivalenceItem> ProcessMethods(List<EquivalenceItem> items, List<EquivalenceItem> master, List<string> selectedmethods, string type, int i, int j)
        {
            List<EquivalenceItem> allresults = new List<EquivalenceItem>();
            if (selectedmethods != null)
            {
                foreach (var method in selectedmethods)
                {

                    switch (method)
                    {
                        case "Match Text":
                            allresults = Method1(items, master, type, "text",i,j);
                            break;
                        case "Match All Words":
                            allresults = Method1(items, master,type, "words",i, j);
                            break;
                    }
                }
            }
            return allresults;
           
        }

        private List<EquivalenceItem> Method1(List<EquivalenceItem> items, List<EquivalenceItem> master, string type, string method, int i, int j)
        {
            var waves = GetAllStudies(items);
            List<EquivalenceItem> allresults = new List<EquivalenceItem>();             
            var item3 = from x in master orderby x.questionText select x;
            List<EquivalenceItem> record2 = new List<EquivalenceItem>();
            foreach (var record1 in item3)
            {
                switch (method)
                {
                    case "text":
                        switch (type)
                        {
                            case "Variable":
                                record2 = items.Where(u => u.removed != true).Where(u => u.variableText.Equals(record1.variableText)).ToList();
                                break;
                            case "Question":
                                record2 = items.Where(u => u.removed != true).Where(u => u.questionText.Equals(record1.questionText)).ToList();
                                break;

                        }
                        break;
                    case "words":
                        switch (type)
                        {
                            case "Variable":
                                record2 = items.Where(u => u.removed != true).Where(u => u.variableOrdered.Equals(record1.variableOrdered)).ToList();
                                break;
                            case "Question":
                                record2 = items.Where(u => u.removed != true).Where(u => u.questionOrdered.Equals(record1.questionOrdered)).ToList();
                                break;

                        }
                        break;
                }
                        
                if (record2.Count > 1)
                {
                    i++;
                    foreach (var record in record2)
                    {
                        j++;
                        EquivalenceItem item = new EquivalenceItem();
                        item = record;
                        item.counter = j;                        
                        item.uniqueId = i;
                        switch (method)
                        {
                            case "text":
                                item.equivalence = "Same Text #" + item.uniqueId;
                                break;
                            case "words":
                                item.equivalence = "Same Words #" + item.uniqueId;
                                break;
                        }
                        item.selected = true;
                        item.column = waves.Where(a => a.StudyName == record.studyGroup).Select(a => { return a.ID; }).FirstOrDefault();
                        allresults.Add(item);
                        items.Where(a => a.variableItem == record.variableItem).SetValue(a => a.removed = true).ToList();
                        var removed2 = items.Where(a => a.removed == true).ToList().Count;
                    }                   
                }
            }
            var removed = items.Where(a => a.removed == true).ToList();
            return allresults;
        }
    }

    public static class PHVExtensions
    {
        public static IEnumerable<T> SetValue<T>(this IEnumerable<T> items, Action<T>
             updateMethod)
        {
            foreach (T item in items)
            {
                updateMethod(item);
            }
            return items;
        }
    }

}