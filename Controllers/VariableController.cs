using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
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
   
    public class VariableController : Controller
    {
        public ActionResult Equivalences(string wordselection)
        {
            // keep
            QuestionModel model = new QuestionModel();
            List<string> smethods = new List<string>();        
            
         
            model.Results = new List<StudyItem>();
            model.SelectedStudies = new List<string>();

            if (wordselection == null)
            {
                model.WordSelection = "";
                wordselection = "";
            }
            if (wordselection.Length != 0) model.WordList = EquivalenceHelper.GetList(wordselection);
            if (wordselection.Length == 0) model.WordList = new List<Word>();
            model.WordSelection = wordselection;
            
            //Serialize to JSON string.
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model = RepositoryHelper.BuildStudiesTree(model, nodes);
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            model.AllQuestions = new List<RepositoryItemMetadata>();
            model.AllVariables = new List<RepositoryItemMetadata>();
            model.AllConcepts = new List<RepositoryItemMetadata>();
            return View(model);
        }

        [HttpPost]
        public ActionResult Equivalences(QuestionModel model, string Study, string selectedItems, string wordselection, string command, HttpPostedFileBase postedFile)
        {        
            // keep
            List<TreeViewNode> items = new List<TreeViewNode>();
            if (selectedItems != null) { items = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            if (postedFile != null)
            {
                GetEquivalences(model, postedFile);
                return RedirectToAction("Equivalences", new { selectedItems = selectedItems, wordselection = model.WordSelection });
            }
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            model.Results = new List<StudyItem>();
            model = RepositoryHelper.BuildStudiesTree(model, nodes);
            model.SelectedStudies = new List<string>();
            ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
            ViewBag.selectedItems = selectedItems;
            

            switch (command)
            {
                case "Save":
                    QuestionModel newmodel = new QuestionModel();
                    newmodel = SaveItem(newmodel, model.Word, model.WordSelection);
                    var wordlist = newmodel.WordList;
                    var selectedwords = newmodel.WordSelection;
                    newmodel.Results = new List<StudyItem>();
                    newmodel = RepositoryHelper.BuildStudiesTree(model, nodes);
                    newmodel.SelectedStudies = new List<string>();
                    newmodel.Word = null;
                    newmodel.WordList = wordlist;
                    newmodel.WordSelection = selectedwords;
                    return RedirectToAction("Equivalences", new { selectedItems = selectedItems, wordselection = newmodel.WordSelection });
                case "Search":
                    model.AllQuestions = new List<RepositoryItemMetadata>();
                    model.AllVariables = new List<RepositoryItemMetadata>();
                    model.AllConcepts = new List<RepositoryItemMetadata>();
                    //model.SelectedStudies = selectedItems;
                    model = LoadSelectedStudies(model, items);
                    model = ProcessStudies(model);
                    var Start = DateTime.Now;
                    QuestionModel m2 = new QuestionModel();

                    m2 = PopulateQuestionMessages(model, items);
                          
                   
                    var Finish = DateTime.Now;
                    var ElapsedMinutes = (Finish - Start).Minutes;
                    var ElapsedSeconds = (Finish - Start).Seconds;
                    TempData["myModel"] = m2;
                    return View("Display", m2);
                default:
                    break;
            }

            DateTime dateTime1 = DateTime.Now;
            ResetMatchesModelStepOne stepOneModel = new ResetMatchesModelStepOne();
            model.Results = new List<StudyItem>();

            model = RepositoryHelper.GetStudies(model, null);
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
            model = RepositoryHelper.GetStudies(model, null);
            model = LoadSelectedStudies(model, items);


            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            return View(model);
        }

        [HttpPost]
        public ActionResult Display(QuestionModel model, string itemType, string command)
        {        
            // keep
            switch (command)
            {
               
                case "Save CSV":
                    RepositoryHelper.ProcessCSV(model.AllResults, "", "Equivalences-Variable.csv");
                    TempData["AllResults"] = model.AllResults;
                    return View(model);
                case "Process":
                    //model.AllResults = mymodel.AllResults;
                    //model.AllConcepts = mymodel.AllConcepts;
                    //model.Results = ProcessResults(model.Results);
                    //TempData["myModel"] = model;
                    //TempData["AllResults"] = model.AllResults;
                    //TempData["AllConcepts"] = model.AllConcepts;
                    return View("Variables", model);

                default:

                    //if (model.StudyId != model.CurrentStudy)
                    //{
                    //    model.AllResults = new List<RepositoryItemMetadata>();
                    //    model.AllConcepts = new List<RepositoryItemMetadata>();
                    //}
                    //model.CurrentStudy = model.StudyId;
                    //model = GetStudies(model);
                    //model.Results = new List<VariableItem>();
                    //TempData["myModel"] = model;
                    return View(model);
            }

        }

        public QuestionModel ProcessStudies(QuestionModel model)
        {

            model.AllQuestions = new List<RepositoryItemMetadata>();
            model.AllVariables = new List<RepositoryItemMetadata>();
            model.AllConcepts = new List<RepositoryItemMetadata>();
            foreach (var study in model.SelectedStudies)
            {
                var agency = study.Substring(0, study.IndexOf(" "));
                var id = model.SelectedStudies[0].Replace(agency, "").Trim();
                var identifier = new Guid(id);
                model = RepositoryHelper.GetAllQuestions(model, agency, identifier, "Variable");
            }


            return model;
        }


        public List<VariableItem> ProcessResults(List<VariableItem> results)
        {
            // delete ?
            string currentquestion = "";
            List<VariableItem> items = new List<VariableItem>();
            foreach (var result in results)
            {
                if (result.description != null) { currentquestion = result.description; }
                if (result.selected == true)
                {
                    VariableItem item = new VariableItem();
                    item.uniqueId = result.uniqueId;
                    item.equivalence = result.equivalence;
                    item.name = result.name;
                    item.description = currentquestion;
                    item.counter = result.counter;
                    item.questionName = result.questionName;
                    item.questionText = currentquestion;
                    item.studyGroup = result.studyGroup;
                    item.study = result.study;
                    item.questionItem = result.questionItem;
                    item.identifier = result.identifier;
                    item.concept = result.concept;
                    item.column = result.column;
                    item.selected = result.selected;
                    items.Add(item);
                }
            }
            return items;
        }

        public QuestionModel PopulateQuestionMessages(QuestionModel model, List<TreeViewNode> selecteditems)
        {
            // keep
            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;

            List<Word> words1 = new List<Word>();
            List<string> wordList1 = model.WordSelection.Split(',').ToList();
            foreach (var word in wordList1)
            {
                if (word.Trim().Length != 0)
                {
                    Word currentword = new Word();
                    currentword.Value = word;
                    words1.Add(currentword);
                }
            }
            
            List<RepositoryItemMetadata> questions1 = new List<RepositoryItemMetadata>();
            List<RepositoryItemMetadata> questions2 = new List<RepositoryItemMetadata>();
            foreach (var selectedword in words1)
            {

                questions1 = model.AllVariables;
                questions2 = model.AllVariables;
                List<Word>  words2 = new List<Word>();
                List<string> wordList2 = selectedword.Value.Split(' ').ToList();
                foreach (var word2 in wordList2)
                {
                    Word currentword = new Word();
                    currentword.Value = word2;
                    words2.Add(currentword);
                }
                foreach (var selectedword2 in words2)
                {
                    questions1 = (from a in questions1
                                 where words2.Any(word => a.Label.FirstOrDefault().Value.ToLower().Contains(selectedword2.Value.ToLower()))
                                 select a).ToList();
                   
                }

                //var questions3 = (from a in questions1
                //                group a by a.Identifier into a1
                //                 select new { Identifier = a1.Key, Count = a1.Count() }).ToList();

                //questions1 = questions1.OrderBy(x => x.Identifier).ToList();
                if (questions1.Count != 0)
                {
                    i++;

                    foreach (var result in questions1)
                    {
                        var question4 = questions1.FirstOrDefault(x => x.Identifier == result.Identifier);

                        j++;
                        VariableItem item = new VariableItem();
                        item.name = null;
                        item.description = question4.DisplayLabel;
                        item.counter = j;
                        item.questionName = question4.ItemName.FirstOrDefault().Value;
                        item.questionText = question4.DisplayLabel;
                        item.questionItem = question4.CompositeId.ToString();
                        item.parentitem = question4.ItemType.ToString();
                        item.studyGroup = question4.AgencyId;
                        item.identifier = question4.Identifier;
                        
                        var concept = (from a in model.AllConcepts
                                       where a.ItemType == question4.ItemType
                                       select a).FirstOrDefault();
                        var v = RepositoryHelper.GetConcept(question4.AgencyId, question4.Identifier);
                        var mainconcept = RepositoryHelper.GetConcept(concept.AgencyId, concept.Identifier);
                        var dataset = RepositoryHelper.GetConcept(question4.AgencyId, question4.Identifier);
                        item.concept = concept.Label.Values.FirstOrDefault() + " - " + mainconcept.Label.Values.FirstOrDefault();
                        item.description = question4.Label.Values.FirstOrDefault();
                        item.questionText = item.description;
                        item.questionName = question4.ItemName.Values.FirstOrDefault();
                        item.study = RepositoryHelper.GetStudy(question4.AgencyId, question4.ItemType);
                        item.name = question4.DisplayLabel;

                        items.Add(item);
                        item.uniqueId = i;
                        item.equivalence = selectedword.Value;
                        // item.column = RepositoryHelper.GetStudyColumn(item.study, model.StudyId);
                        item.selected = true;
                        item.isdeprecated = question4.IsDeprecated;
                        question = question4.DisplayLabel;

                    }
                }
            }           
            model.AllResults = items;
            return model;
        }

       

        public void GetEquivalences(QuestionModel model, HttpPostedFileBase postedFile)
        {

            try
            {
                string fileExtension = Path.GetExtension(postedFile.FileName);
                if (fileExtension != ".csv")
                {
                    // return View(model);
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
                model.WordSelection = wordselection;
                model.Results = new List<StudyItem>();
                TempData["myModel"] = model;
                RedirectToAction("Equivalences", new { equivalenceselection = model.WordSelection });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

        }

        public QuestionModel SaveItem(QuestionModel model, string word, string wordselection)
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

        public QuestionModel LoadSelectedStudies(QuestionModel model, List<TreeViewNode> items)
        {
            List<string> selectedstudies = new List<string>();
            foreach (var item in items)
            {
                var sweep = model.Results.Where(s => s.AgencyId == item.parent).Where(s => s.DisplayLabel == item.text).FirstOrDefault();
                //selectedstudies.Add(sweep.AgencyId + " " + sweep.Identifier.ToString());
                selectedstudies.Add(item.id);
            }
            model.SelectedStudies = selectedstudies;
            return model;
        }

      

     

       
       

       
    }
}