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
   
    public class QuestionItemController : Controller
    {
        //public ActionResult Equivalences(string wordselection)
        public ActionResult Equivalences()
        {
            // keep
            QuestionModel model = new QuestionModel();
            List<string> smethods = new List<string>();
            string wordselection = "";
         
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
        //public ActionResult Equivalences(QuestionModel model, string Study, string selectedItems, string wordselection, string command, HttpPostedFileBase postedFile)
       public ActionResult Equivalences(QuestionModel model, string Study, string selectedItems, string fileName, string command, HttpPostedFileBase postedFile)
       {
            string wordselection = "";
            List<TreeViewNode> nodes = new List<TreeViewNode>();
            List<TreeViewNode> selectedstudies = new List<TreeViewNode>(); 
            if (selectedItems != null) { selectedstudies = (new JavaScriptSerializer()).Deserialize<List<TreeViewNode>>(selectedItems); }
            if (postedFile != null)
            {
                model = GetEquivalences(model, postedFile);
                model = RepositoryHelper.BuildStudiesTree(model, nodes);
                ViewBag.Json = (new JavaScriptSerializer()).Serialize(nodes);
                return View(model);
            }
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
                case "Display Questions":
                    model.AllQuestions = new List<RepositoryItemMetadata>();
                    model.AllVariables = new List<RepositoryItemMetadata>();
                    model.AllConcepts = new List<RepositoryItemMetadata>();
                    // model.SelectedStudies = selectedstudies;
                    model = LoadSelectedStudies(model, selectedstudies);
                    model = ProcessStudies(model);
                    var Start = DateTime.Now;
                    QuestionModel m2 = new QuestionModel();                  
                    m2 = PopulateQuestionMessages(model, nodes, "Question");
                    m2.FileName = fileName;
                    m2.Type = "Question";
                    var Finish = DateTime.Now;
                    var ElapsedMinutes = (Finish - Start).Minutes;
                    var ElapsedSeconds = (Finish - Start).Seconds;
                    TempData["myModel"] = m2;
                    return View("Display", m2);
                case "Display Variables":
                    model.AllQuestions = new List<RepositoryItemMetadata>();
                    model.AllVariables = new List<RepositoryItemMetadata>();
                    model.AllConcepts = new List<RepositoryItemMetadata>();
                    // model.SelectedStudies = selectedstudies;
                    model = LoadSelectedStudies(model, selectedstudies);
                    model = ProcessStudies(model);
                    QuestionModel m3 = new QuestionModel();                   
                    m3 = PopulateQuestionMessages(model, nodes, "Variable");
                    m3.FileName = fileName;
                    m3.Type = "Variable";
                    TempData["myModel"] = m3;
                    return View("Display", m3);
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
            //if (selectedItems != "")
            //{
            //    switch (nodes)
            //    {
            //        case 0:
            //            return View(model);
            //        case 1:
            //            return View(model);
            //    }
            //}
            model = RepositoryHelper.GetStudies(model, null);
            model = LoadSelectedStudies(model, nodes);


            DateTime dateTime2 = DateTime.Now;
            var diff = dateTime2.Subtract(dateTime1);
            var res = String.Format("{0}:{1}:{2}", diff.Hours, diff.Minutes, diff.Seconds);
            stepOneModel.Duration = res.ToString();
            return View(model);
        }

        [HttpPost]
        public ActionResult Display(QuestionModel model, string studyName, string itemType, string command)
        {        
            switch (command)
            {
               
                case "Save CSV":
                    RepositoryHelper.ProcessCSV(model.AllResults, itemType, model.FileName + " - " + itemType + " - " + studyName + ".csv");
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

        public static QuestionModel ProcessStudies(QuestionModel model)
        {

            model.AllQuestions = new List<RepositoryItemMetadata>();
            model.AllVariables = new List<RepositoryItemMetadata>();
            model.AllConcepts = new List<RepositoryItemMetadata>();
            foreach (var study in model.SelectedStudies)
            {
                var agency = study.Substring(0, study.IndexOf(" "));
                var id = model.SelectedStudies[0].Replace(agency, "").Trim();
                var identifier = new Guid(id);
                model = RepositoryHelper.GetAllQuestions(model, agency, identifier, "Question");
            }


            return model;
        }


        public List<VariableItem> ProcessResults(RepositoryItemMetadata result, List<VariableItem> items, QuestionModel model, string equivalence, int uniqueId, int counter)
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

        public QuestionModel PopulateQuestionMessages(QuestionModel model, List<TreeViewNode> selecteditems, string type)
        {
            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;

            List<Word> words1 = new List<Word>();
                   
            List<RepositoryItemMetadata> questions = new List<RepositoryItemMetadata>();
            foreach (var selectedwords in model.WordList)
            {

                questions = model.AllQuestions;
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
                    selectedword = " " + currentword.Value + " ";
                    questions = (from a in questions
                                 where words2.Any(word => a.Summary.FirstOrDefault().Value.ToLower().Contains(selectedword.ToLower()))
                                 select a).ToList();

                }

                if (questions.Count != 0)
                {
                    i++;

                    foreach (var result in questions)
                    {
                        var question4 = questions.FirstOrDefault(x => x.Identifier == result.Identifier);

                        j++;
                        if (type == "Question")
                        {
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
                                           where a.ItemType == question4.Identifier
                                           select a).FirstOrDefault();
                            var v = RepositoryHelper.GetConcept(question4.AgencyId, question4.Identifier);
                            RepositoryItemMetadata mainconcept = new RepositoryItemMetadata();
                            if (concept != null) { mainconcept = RepositoryHelper.GetConcept(concept.AgencyId, concept.Identifier); }
                            var dataset = RepositoryHelper.GetConcept(question4.AgencyId, question4.Identifier);
                            if (concept != null) item.concept = concept.Label.Values.FirstOrDefault() + " - " + mainconcept.Label.Values.FirstOrDefault();
                            item.description = question4.Summary.Values.FirstOrDefault();
                            item.questionText = item.description;
                            item.questionName = question4.ItemName.Values.FirstOrDefault();
                            item.study = RepositoryHelper.GetStudy(question4.AgencyId, question4.Identifier);
                            item.name = question4.DisplayLabel;

                            items.Add(item);
                            item.uniqueId = i;
                            item.equivalence = selectedword.Trim();
                            // item.column = RepositoryHelper.GetStudyColumn(item.study, model.StudyId);
                            item.selected = true;
                            item.isdeprecated = question4.IsDeprecated;
                            question = question4.DisplayLabel;
                        }
                        else
                        {
                            items = ProcessResults(result, items, model, selectedword, i, j);
                        }
                        if (model.SelectedStudies.Count == 1) { model.StudyName = RepositoryHelper.GetStudy(result.AgencyId, result.Identifier); }
                    }
                }
            }           
            model.AllResults = items;
            return model;
        }

       

        public QuestionModel GetEquivalences(QuestionModel model, HttpPostedFileBase postedFile)
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
                selectedstudies.Add(item.id);
            }
            model.SelectedStudies = selectedstudies;
            
            return model;
        }

      

     

       
       

       
    }
}