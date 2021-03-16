using Algenta.Colectica.Model;
using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Ddi.Utility;
using Algenta.Colectica.Model.Repository;
using Algenta.Colectica.Model.Utility;
using ColecticaSdkMvc.Models;
using ColecticaSdkMvc.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ColecticaSdkMvc.Controllers
{
   

    public class QuestionController : Controller
    {

       
        public ActionResult Equivalences(string equivalenceselection, bool any = false, bool name = false)
        {
            VariableModel mymodel = TempData["myModel"] as VariableModel;
            List<RepositoryItemMetadata> allresults = TempData["AllResults"] as List<RepositoryItemMetadata>;
            List<RepositoryItemMetadata> allconcepts = TempData["AllConcepts"] as List<RepositoryItemMetadata>;
            VariableModel model = new VariableModel();
            model.Results = new List<VariableItem>();
            if (mymodel != null)
            {
                if (mymodel.AllResults != null) { model.AllResults = mymodel.AllResults; } else { model.AllResults = new List<RepositoryItemMetadata>(); }
                if (mymodel.AllConcepts != null) { model.AllConcepts = mymodel.AllConcepts; } else { model.AllConcepts = new List<RepositoryItemMetadata>(); }
                
                model.EquivalenceSelection = equivalenceselection;
                // model.Equivalences = mymodel.Equivalences;
                model.Equivalences = GetList(equivalenceselection);
                model.Studies = mymodel.Studies;
                model.StudyId = mymodel.StudyId;
            }
            
            model.SelectedQuestions = new List<string>();
            model.Results = new List<VariableItem>();
            model = GetStudies(model);            
            model.Any = any; 
            model.Name = name;
            TempData["myModel"] = model;
            TempData["AllResults"] = model.AllResults;
            TempData["AllConcepts"] = model.AllConcepts;
            return View(model);
        }

        [HttpPost]
        public ActionResult Equivalences(VariableModel model, string itemType, string command, HttpPostedFileBase postedFile, string StudyId = null)
        {
            VariableModel mymodel = TempData["myModel"] as VariableModel;
            List<RepositoryItemMetadata> allresults = TempData["AllResults"] as List<RepositoryItemMetadata>;
            List<RepositoryItemMetadata> allconcepts = TempData["AllConcepts"] as List<RepositoryItemMetadata>;
            if (model.AllResults == null)
            {
                if (mymodel == null)
                {
                    if (allresults == null) model.AllResults = new List<RepositoryItemMetadata>();
                    if (allconcepts == null) model.AllConcepts = new List<RepositoryItemMetadata>();                    
                }
                else
                {
                    if (mymodel.AllResults == null)
                    {
                        if (allresults == null) { mymodel.AllResults = new List<RepositoryItemMetadata>(); } else mymodel.AllResults = allresults;
                    }
                    if (mymodel.AllConcepts == null)
                    {
                        if (allconcepts == null) { mymodel.AllConcepts = new List<RepositoryItemMetadata>(); } else mymodel.AllConcepts = allconcepts;
                    }
                    model.AllResults = mymodel.AllResults;
                    model.AllConcepts = mymodel.AllConcepts;
                    model.Studies = mymodel.Studies;
                    model.CurrentStudy = mymodel.CurrentStudy;
                }
            }
            
            // itemType = "Question";
            itemType = "All";
            model.ItemTypes = GetItemTypes1();
            model.SelectedResults = new List<string>();
            if (postedFile != null)
            {
                try
                {
                    string fileExtension = Path.GetExtension(postedFile.FileName);
                    if (fileExtension != ".csv")
                    {
                        return View(model);
                    }
                    string equivalencesselection = "";
                    List<Word> equivalences = new List<Word>();
                    using (var sreader = new StreamReader(postedFile.InputStream))
                    {
                        while (!sreader.EndOfStream)
                        {
                            string[] rows = sreader.ReadLine().Split(',');
                            Word word = new Word();
                            word.Value = rows[0].ToString();
                            equivalences.Add(word);
                            equivalencesselection = equivalencesselection  + rows[0].ToString() + ",";
                        }
                    }
                    model.Equivalences = equivalences;
                    model.EquivalenceSelection = equivalencesselection;
                    model.Results = new List<VariableItem>();
                    TempData["myModel"] = model;
                    TempData["AllResults"] = model.AllResults;
                    TempData["AllConcepts"] = model.AllConcepts;
                    return RedirectToAction("Equivalences", new { equivalenceselection = model.EquivalenceSelection, any = model.Any, name = model.Name });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                }
            }

            switch (command)
            {
                case "Save":
                    VariableModel newmodel = new VariableModel();
                    string equivalence = "" ;
                    if (mymodel != null) { equivalence = mymodel.EquivalenceSelection; }
                    newmodel = SaveItem(newmodel, model.Equivalence, equivalence);
                    var wordlist = newmodel.Equivalences;
                    var selectedwords = newmodel.EquivalenceSelection;                   
                    newmodel.Equivalence = null;
                    newmodel.Equivalences = wordlist;
                    newmodel.EquivalenceSelection = selectedwords;
                    if (mymodel != null) newmodel.AllResults = mymodel.AllResults;
                    if (mymodel != null) newmodel.AllConcepts = mymodel.AllConcepts;
                    newmodel.Results = new List<VariableItem>();
                    newmodel.StudyId = model.StudyId;
                    newmodel = GetStudies(newmodel);
                    TempData["myModel"] = newmodel;
                    TempData["AllResults"] = model.AllResults;
                    TempData["AllConcepts"] = model.AllConcepts;
                    return RedirectToAction("Equivalences", new { equivalenceselection = selectedwords, any = model.Any, name = model.Name });
                case "Save CSV":
                    string csv = ProcessCSV(model.Results);
                    TempData["AllResults"] = model.AllResults;
                    TempData["AllConcepts"] = model.AllConcepts;
                    TempData["myModel"] = model;
                    return View(model);
                case "Process":
                    model.AllResults = mymodel.AllResults;
                    model.AllConcepts = mymodel.AllConcepts;
                    model.Results = ProcessResults(model.Results);
                    TempData["myModel"] = model;
                    TempData["AllResults"] = model.AllResults;
                    TempData["AllConcepts"] = model.AllConcepts;
                    return View("Variables", model);
                case "Display Selected":
                    if (mymodel.AllResults.Count != 0) model.AllResults = mymodel.AllResults;
                    if (mymodel.AllConcepts.Count != 0) model.AllConcepts = mymodel.AllConcepts;
                    model.Equivalences = mymodel.Equivalences;
                    model.CurrentStudy = mymodel.CurrentStudy;
                    var Start = DateTime.Now;
                    VariableModel m2 = new VariableModel();
                    m2.ItemTypes = model.ItemTypes;
                    switch (itemType)
                    {
                        case "All":
                            m2 = PopulateAllMessages(model, model.ItemType, mymodel.EquivalenceSelection);
                            break;
                        case "Question":
                            m2 = PopulateQuestionMessages(model, model.ItemType, mymodel.EquivalenceSelection);
                            break;
                    }
                    m2.SelectedQuestions = model.SelectedQuestions;
                    m2.StudyId = model.StudyId;
                    m2.CurrentStudy = model.CurrentStudy;
                    m2 = GetStudies(m2);
                    var Finish = DateTime.Now;
                    var ElapsedMinutes = (Finish - Start).Minutes;
                    var ElapsedSeconds = (Finish - Start).Seconds;
                    m2.Elapsed = ElapsedMinutes + " min " + ElapsedSeconds + " sec ";
                    TempData["myModel"] = m2;
                    TempData["AllResults"] = m2.AllResults;
                    TempData["AllConcepts"] = m2.AllConcepts;
                    return View(m2);
                case "Display All":
                    var Start2 = DateTime.Now;
                    VariableModel m3 = new VariableModel();
                    m3.ItemTypes = model.ItemTypes;
                    m3 = PopulateAllEquivalences(model);
                    m3.SelectedQuestions = model.SelectedQuestions;
                    var results2 = m3.Results.ToList();
                    var Finish2 = DateTime.Now;
                    var ElapsedMinutes2 = (Finish2 - Start2).Minutes;
                    var ElapsedSeconds2 = (Finish2 - Start2).Seconds;
                    m3.Elapsed = ElapsedMinutes2 + " min " + ElapsedSeconds2 + " sec ";
                    TempData["myModel"] = m3;
                    TempData["AllResults"] = m3.AllResults;
                    TempData["AllConcepts"] = m3.AllConcepts;
                    return View(m3);
                default:      
                    
                    if (model.StudyId != model.CurrentStudy)
                    {
                        model.AllResults = new List<RepositoryItemMetadata>();
                        model.AllConcepts = new List<RepositoryItemMetadata>();
                    }
                    model.CurrentStudy = model.StudyId;
                    model = GetStudies(model);
                    model.Results = new List<VariableItem>();
                    TempData["myModel"] = model;
                    return View(model);
            }

        }

        public string ProcessCSV(List<VariableItem> results)
        {
            // string downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
            string csv = String.Empty;
            results = ProcessResults(results);
            if (results.Count != 0)
            {
                foreach (var result in results)
                {
                    csv += result.questionName + ",";
                    csv += result.description + ",";
                    csv += result.questionItem;
                    csv += "\r\n";
                }

                //Download the CSV file.
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=Equivalences.csv");
                Response.Charset = "";
                Response.ContentType = "application/text";
                Response.Output.Write(csv);
                Response.Flush();
                Response.End();

                return csv;
            }
            else { return null; }
        }

        public List<VariableItem> ProcessResults(List<VariableItem> results)
        {
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

        public List<VariableItem> GetItems(string selectedQuestionName, string selectedQuestionText)
        {

            VariableModel model = new VariableModel();
            List<Item> items = Helper.GetSelectedItems(selectedQuestionName);
            List<string> selectedquestions = selectedQuestionText.Split(';').ToList();
            List<VariableItem> results = new List<VariableItem>();
            string currentItem = "";
            foreach (var selectedquestion in selectedquestions)
            {
                if (selectedquestion != "")
                {
                    List<string> lists = selectedquestion.Split(',').ToList();
                    VariableItem item = new VariableItem();
                    int i = 0;
                    foreach (var list in lists)
                    {
                        if (i == 0) { currentItem = list; }
                        switch (i)
                        {
                            case 0:
                                item.uniqueId = Convert.ToInt32(list);
                                break;
                            case 1:
                                item.equivalence = list;
                                break;
                            case 2:
                                item.questionName = list;
                                break;
                            case 3:
                                item.questionText = (from b in items
                                                     where b.Id == currentItem
                                                     select b.QuestionValue).FirstOrDefault();
                                break;
                            case 4:
                                item.questionItem = list;
                                break;
                            case 5:
                                item.studyGroup = list;
                                break;
                            case 6:
                                item.study = list;
                                break;
                            case 7:
                                item.concept = list;
                                break;
                            case 8:
                                item.column = Convert.ToInt32(list);
                                break;
                        }
                        i++;
                    }
                    results.Add(item);
                }
            }
            return results;
        }

        [HttpPost]
        public ActionResult Variables(VariableModel model,string selectedNames, string selectedDescriptions, string selectedQuestions, string command)
        {
            VariableModel results = TempData["myModel"] as VariableModel;
            List<RepositoryItemMetadata> allresults = TempData["AllResults"] as List<RepositoryItemMetadata>;
            List<RepositoryItemMetadata> allconcepts = TempData["AllConcepts"] as List<RepositoryItemMetadata>; 
            results.CurrentStudy = results.StudyId;
            model.Results = ProcessItems(model.Results);
            switch (command)
            {
                case "Process":
                    List<VariableItem> newresults = new List<VariableItem>();
                    var waves = from r in results.Results
                                group r by r.study into r1
                                select new { Name = r1.Key };
                    string expected = "";
                    foreach (var result in model.Results)
                    {
                        expected = expected + result.uniqueId + ",";
                        expected = expected + result.equivalence + ",";
                        expected = expected + result.questionName + ",";
                        expected = expected + result.questionText + ",";
                        expected = expected + result.studyGroup + ",";
                        expected = expected + result.study + ",";
                        expected = expected + result.questionItem + ",";
                        expected = expected + result.identifier + ",";
                        expected = expected + result.concept + ",";
                        expected = expected + result.column + ";";
                    }
                    ExpectedModel model1 = new ExpectedModel();
                    //model1.AllResults = results.AllResults;
                    model1 = GetExpectedItems(model.Results, results.StudyId);
                    TempData["myModel"] = results;
                    TempData["AllResults"] = results.AllResults;
                    TempData["AllConcepts"] = results.AllConcepts;
                    return View("Results", model1);
                default:
                    string csv = ProcessCSV(model.Results);
                    model.AllResults = results.AllResults;
                    model.AllConcepts = results.AllConcepts;
                    model.Equivalences = results.Equivalences;
                    model.EquivalenceSelection = results.EquivalenceSelection;
                    model.Studies = results.Studies;
                    model.StudyId = results.StudyId;
                    TempData["myModel"] = model;
                    TempData["AllResults"] = model.AllResults;
                    TempData["AllConcepts"] = model.AllConcepts;
                    return View(model);
            }
        }

        public List<VariableItem> ProcessItems(List<VariableItem> results)
        {
            string currentquestion = "";
            string currentname = "";
            List<VariableItem> items = new List<VariableItem>();
            foreach (var result in results)
            {
                if (result.questionName != null) { currentquestion = result.questionName; }
                if (result.description != null) { currentname = result.description; }

                VariableItem item = new VariableItem();
                item.uniqueId = result.uniqueId;
                item.equivalence = result.equivalence;
                item.name = result.name;
                item.description = currentname;
                item.counter = result.counter;
                item.questionName = currentquestion;
                item.questionText = currentname;
                item.studyGroup = result.studyGroup;
                item.study = result.study;
                item.questionItem = result.questionItem;
                item.identifier = result.identifier;
                item.concept = result.concept;
                item.column = result.column;
                item.selected = result.selected;
                items.Add(item);
            }            
            return items;
        }

        public ExpectedModel GetExpectedItems(List<VariableItem> results, string studyId)
        {
            VariableModel model = new VariableModel();
            model.Results = results.OrderBy(a => a.column).OrderBy(a => a.uniqueId).ToList();

            List<ExpectedItem> expecteditems = new List<ExpectedItem>();
            string lastquestion = "";
            int lastcolumn = 0;
            ExpectedItem expecteditem = new ExpectedItem();
            expecteditem.Waves = GetAllStudies(studyId);
            foreach (var result in results)
            {
                if (lastquestion != "")
                {
                    if (result.equivalence != lastquestion)
                    {
                        expecteditems.Add(expecteditem);
                        expecteditem = new ExpectedItem();
                        expecteditem.Waves = GetAllStudies(studyId);

                    }
                }
                if (lastcolumn != 0)
                {
                    if (result.column <= lastcolumn)
                    {
                        expecteditems.Add(expecteditem);
                        expecteditem = new ExpectedItem();
                        expecteditem.Waves = GetAllStudies(studyId);
                    }
                }

                expecteditem.UniqueId = result.uniqueId;
                expecteditem.Name = result.equivalence;
                expecteditem.Description = result.questionText;
                expecteditem.Topic = result.concept;
                var wave = expecteditem.Waves.Where(a => a.ID == result.column).FirstOrDefault();
                if (wave != null)
                {
                    List<Study> astudy = expecteditem.Waves.Where(a => a.ID != result.column).Select(a => { return a; }).ToList();
                    List<Study> sstudy = expecteditem.Waves.Where(a => a.ID == result.column).Select(a => { a.Value = result.questionName; return a; }).ToList();
                    foreach (var sitem in sstudy)
                    {
                        astudy.Add(sitem);
                    }
                    expecteditem.Waves = astudy.OrderBy(a => a.ID).ToList();
                }

                lastquestion = result.equivalence;
                lastcolumn = result.column;
            }
            expecteditems.Add(expecteditem);
            ExpectedModel model1 = new ExpectedModel();
            model1.expecteditems = expecteditems;
            return model1;
        }
     



        [HttpPost]
        public ActionResult Results(ExpectedModel inputmodel)
        {
            VariableModel results = TempData["myModel"] as VariableModel;
            List<RepositoryItemMetadata> allresults = TempData["AllResults"] as List<RepositoryItemMetadata>;
            List<RepositoryItemMetadata> allconcepts = TempData["AllConcepts"] as List<RepositoryItemMetadata>;
            VariableModel model = new VariableModel();
            model.Results = new List<VariableItem>();

            model.EquivalenceSelection = "";
            model.Equivalences = new List<Word>();
            model.SelectedQuestions = new List<string>();

            model.AllResults = results.AllResults;
            model.AllConcepts = results.AllConcepts;
            model.Studies = results.Studies;
            model.StudyId = results.StudyId;
            model.CurrentStudy = results.CurrentStudy;
            TempData["myModel"] = model;
            TempData["AllResults"] = model.AllResults;
            TempData["AllConcepts"] = model.AllConcepts;
            return View("Equivalences", model);
        }

        public List<Study> GetAllStudies(string studyId)
        {
            List<Study> allstudies1 = new List<Study>();
            List<Word> equivalences = new List<Word>();
            var allstudies2 = GetRepository("Study", "", equivalences);
            allstudies2 = allstudies2.Where(a => a.AgencyId == studyId).ToList();
            int j = 1;
            foreach (var item in allstudies2)
            {
                Study study = new Study();
                study.ID = j;
                study.Value = "";
                study.StudyName = item.DisplayLabel;
                allstudies1.Add(study);
               
                j++;
            }
            return allstudies1;
        }

        // Used for uploading of Questions using Question Items. Only to be used for development
        // to use set the itemType to "All" in Equivalences
        public VariableModel PopulateAllMessages(VariableModel model, string itemType, string equivalenceselection)
        {

            model.Equivalences = GetList(model.EquivalenceSelection);
                        

            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;

            foreach (var selectedword in model.Equivalences)
            {
                var variables = GetRepository("Question Item", selectedword.Value, model.Equivalences);
                i++;
                foreach (var result in variables)
                {
                    var references = GetConcept(result.AgencyId, result.Identifier);
                    var questions = (from a in references
                                     where a.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d")
                                     select a);
                    foreach (var qitem in questions)
                    {
                        j++;
                        VariableItem item = new VariableItem();
                        item.name = null;
                        item.questionItem = result.DisplayLabel;
                        item.description = result.DisplayLabel;
                        item.counter = j;
                        item.questionName = result.ItemName.FirstOrDefault().Value;
                        item.questionText = result.DisplayLabel;
                        item.questionItem = result.CompositeId.ToString();
                        item.studyGroup = result.AgencyId;
                        item.identifier = result.Identifier;
                        item.concept = (from a in references
                                        where a.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205")
                                        select a.Label.Values.FirstOrDefault()).FirstOrDefault();
                        item.description = qitem.Label.Values.FirstOrDefault();
                        item.questionText = item.description;
                        item.questionName = qitem.ItemName.Values.FirstOrDefault();
                        item.study = GetStudy(result.AgencyId, result.Identifier);
                        item.name = result.DisplayLabel;
                        items.Add(item);
                        item.uniqueId = i;
                        item.equivalence = selectedword.Value;
                        item.column = GetStudyColumn(item.study, model.StudyId);
                        item.selected = true;
                    }
                    question = result.DisplayLabel;
                }
            }
            model.Results = items;
            return model;
        }

        // Used for uploading of Questions. Used for live
        // to use set the itemType to "Question" in Equivalences
        public VariableModel PopulateQuestionMessages(VariableModel model, string itemType, string equivalenceselection)
        {

            model.Equivalences = GetList(equivalenceselection);


            List<VariableItem> items = new List<VariableItem>();
            List<RepositoryItemMetadata> variables = new List<RepositoryItemMetadata>();
            if (model.AllResults != null)
            {
                if (model.AllResults.Count == 0)
                {
                    model = GetQuestionRepository(model, model.StudyId);
                }
            }
            else model = GetQuestionRepository(model, model.StudyId);

            switch (model.Any)
            {
                case true:
                    model = ProcessAnyEquivalences(model, itemType, equivalenceselection);
                    break;
                case false:
                    model = ProcessAllEquivalences(model, itemType, equivalenceselection);
                    break;
            }
            return model;
        }

        public VariableModel ProcessAnyEquivalences(VariableModel model, string itemType, string equivalenceselection)
        {
            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;

            foreach (var selectedword in model.Equivalences)
            {
                i++;
                List<RepositoryItemMetadata> questions = new List<RepositoryItemMetadata>();
                switch (model.Name)
                {
                    case true:
                        questions = (from a in model.AllResults
                                     where model.Equivalences.Any(word => a.ItemName.FirstOrDefault().Value.ToLower().Contains(selectedword.Value))
                                     select a).ToList();
                        break;
                    case false:
                        questions = (from a in model.AllResults
                                     where model.Equivalences.Any(word => a.Label.FirstOrDefault().Value.ToLower().Contains(selectedword.Value))
                                     select a).ToList();
                        break;
                }


                foreach (var result in questions)
                {

                    j++;
                    VariableItem item = new VariableItem();
                    item.name = null;
                    item.description = result.DisplayLabel;
                    item.counter = j;
                    item.questionName = result.ItemName.FirstOrDefault().Value;
                    item.questionText = result.DisplayLabel;
                    item.questionItem = result.CompositeId.ToString();
                    item.studyGroup = result.AgencyId;
                    item.identifier = result.Identifier;
                    item.concept = (from a in model.AllConcepts
                                    where a.Identifier == result.Identifier
                                    select a.Label.Values.FirstOrDefault()).FirstOrDefault();
                    item.description = result.Label.Values.FirstOrDefault();
                    item.questionText = item.description;
                    item.questionName = result.ItemName.Values.FirstOrDefault();
                    item.study = GetStudy(result.AgencyId, result.Identifier);
                    item.name = result.DisplayLabel;
                    items.Add(item);
                    item.uniqueId = i;
                    item.equivalence = selectedword.Value;
                    item.column = GetStudyColumn(item.study, model.StudyId);
                    item.selected = true;
                    item.isdeprecated = result.IsDeprecated;
                    question = result.DisplayLabel;
                }


            }
            model.Results = items;
            return model;
        }

        public VariableModel ProcessAllEquivalences(VariableModel model, string itemType, string equivalenceselection)
        {
            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;

            List<string> words = new List<string>();
            foreach (var word in model.Equivalences)
            {
                words.Add(word.Value);
            }

            i++;
            List<RepositoryItemMetadata> questions = new List<RepositoryItemMetadata>();
            questions = model.AllResults;
            switch (model.Name)
            {
                case true:
                    foreach (var selectedword in model.Equivalences)
                    {
                        questions = (from a in questions
                                     where model.Equivalences.Any(word => a.ItemName.FirstOrDefault().Value.ToLower().Contains(selectedword.Value))
                                     select a).ToList();
                    }
                    break;
                case false:
                    foreach (var selectedword in model.Equivalences)
                    {
                        questions = (from a in questions
                                     where model.Equivalences.Any(word => a.Label.FirstOrDefault().Value.ToLower().Contains(selectedword.Value))
                                     select a).ToList();
                    }
                    break;
            }
           

                foreach (var result in questions)
            {

                j++;
                VariableItem item = new VariableItem();
                item.name = null;
                item.description = result.DisplayLabel;
                item.counter = j;
                item.questionName = result.ItemName.FirstOrDefault().Value;
                item.questionText = result.DisplayLabel;
                item.questionItem = result.CompositeId.ToString();
                item.studyGroup = result.AgencyId;
                item.identifier = result.Identifier;
                item.concept = (from a in model.AllConcepts
                                where a.Identifier == result.Identifier
                                select a.Label.Values.FirstOrDefault()).FirstOrDefault();
                item.description = result.Label.Values.FirstOrDefault();
                item.questionText = item.description;
                item.questionName = result.ItemName.Values.FirstOrDefault();
                item.study = GetStudy(result.AgencyId, result.Identifier);
                item.name = result.DisplayLabel;
                items.Add(item);
                item.uniqueId = i;
                item.equivalence = equivalenceselection;
                item.column = GetStudyColumn(item.study, model.StudyId);
                item.selected = true;
                item.isdeprecated = result.IsDeprecated;
                question = result.DisplayLabel;

            }
            
            model.Results = items;
            return model;
        }

        public VariableModel PopulateAllEquivalences(VariableModel model)
        {

            
            List<VariableItem> items = new List<VariableItem>();
            int i = 0;
            int j = 0;
            string question = null;
            List<RepositoryItemMetadata> variables = new List<RepositoryItemMetadata>();
            if (model.AllResults.Count == 0)
            {
                model = GetQuestionRepository(model, model.StudyId);
            }
            var matches = from r in model.AllResults
                                  group r by r.Label.FirstOrDefault().Value into r1
                                  select new { Name = r1.Key, NumberofMatches = r1.Count() };
            matches = matches.OrderByDescending(a => a.NumberofMatches).Where(a => a.NumberofMatches >= model.Minimum).Where(a => a.NumberofMatches <= model.Maximum).ToList();
            foreach (var match in matches)
            {
                i++;
                var questions = (from r in model.AllResults
                                 where r.Label.FirstOrDefault().Value == match.Name
                                 select r).ToList();
                foreach (var result in questions)
                {
                    j++;
                    VariableItem item = new VariableItem();
                    item.name = null;
                    item.description = result.DisplayLabel;
                    item.counter = j;
                    item.questionName = result.ItemName.FirstOrDefault().Value;
                    item.questionText = result.DisplayLabel;
                    item.questionItem = result.CompositeId.ToString();
                    item.studyGroup = result.AgencyId;
                    item.identifier = result.Identifier;
                    item.concept = (from a in model.AllConcepts
                                    where a.Identifier == result.Identifier
                                    select a.Label.Values.FirstOrDefault()).FirstOrDefault();
                    item.description = result.Label.Values.FirstOrDefault();
                    item.questionText = item.description;
                    item.questionName = result.ItemName.Values.FirstOrDefault();
                    item.study = GetStudy(result.AgencyId, result.Identifier);
                    item.name = result.DisplayLabel;
                    item.uniqueId = i;
                    item.column = GetStudyColumn(item.study, model.StudyId);
                    item.selected = true;
                    items.Add(item);
                    
                    question = result.DisplayLabel;
                }
            }
            model.Results = items;
            return model;
        }


        public int GetStudyColumn(string selectedstudy, string studyId)
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

        public List<RepositoryItemMetadata> GetConcept(string agency, Guid id)
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
            List<RepositoryItemMetadata> referenceItems = referencingItemsDescriptions.ToList();
            List<RepositoryItemMetadata> variables = referenceItems.Where(a => a.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d")).ToList();
            foreach (var variable in variables)
            {
                IVersionable item1 = client.GetLatestItem(variable.Identifier, variable.AgencyId, ChildReferenceProcessing.Populate);                
            }
            return referenceItems;
        }

        public RepositoryItemMetadata GetReference(string agency, Guid id)
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
            
            var referencingItemDescription = client.GetRepositoryItemDescriptionsByObject(facet).FirstOrDefault();
            var referencingItemDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);
            
            return referencingItemDescription;
        }

        public string GetStudy(string agency, Guid id)
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

        // Used for uploading of Questions using Question Items. Only to be used for development
        // to use set the itemType to "All" in Equivalences
        public List<SearchResult> GetRepository(string itemType, string searchTerm, List<Word> equivalences)
        {
            DateTime start, finish;
            MultilingualString.CurrentCulture = "en-US";

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
            //facet.Cultures.Add("da-DK");

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

        // Used for uploading of Questions. Used for live
        // to use set the itemType to "Question" in Equivalences
        public VariableModel GetQuestionRepository(VariableModel model,string searchTerm)
        {
            DateTime start, finish;
            MultilingualString.CurrentCulture = "en-US";

            // Create a new SearchFacet that will find all
            // StudyUnits, CodeSchemes, and CategorySchemes.

            SearchFacet facet = new SearchFacet();
        
            facet.ItemTypes.Add(DdiItemType.QuestionItem);
             
            facet.ResultOrdering = SearchResultOrdering.ItemType;
            facet.SearchLatestVersion = true;
                       
            start = DateTime.Now;
            var client = ClientHelper.GetClient();
            SearchResponse response = client.Search(facet);
           
            IEnumerable<SearchResult> results = response.Results; 
            List<SearchResult> results1 = new List<SearchResult>();
            results = results.Where(a => a.AgencyId == searchTerm).ToList();

            List<SearchResult> results2 = new List<SearchResult>();
            List<RepositoryItemMetadata> references2 = new List<RepositoryItemMetadata>();
            List<RepositoryItemMetadata> concepts = new List<RepositoryItemMetadata>();
            foreach (var result in results)
            {
                results2.Add(result);
                var references = GetConcept(result.AgencyId, result.Identifier);
                foreach (var reference in references)
                {
                    if (reference.ItemType == new Guid("683889c6-f74b-4d5e-92ed-908c0a42bb2d"))
                    {
                        reference.AgencyId = result.AgencyId;
                        reference.Identifier = result.Identifier;
                        references2.Add(reference);
                    }
                    if (reference.ItemType == new Guid("5cc915a1-23c9-4487-9613-779c62f8c205"))
                    {
                        reference.AgencyId = result.AgencyId;
                        reference.Identifier = result.Identifier;
                        concepts.Add(reference);
                    }
                }

            }
            var resultmatches = from r in results
                        group r by r.Summary.FirstOrDefault() into r1
                        select new { Name = r1.Key, NumberofMatches = r1.Count() };
            resultmatches = resultmatches.OrderByDescending(a => a.NumberofMatches).Where(a => a.NumberofMatches > 1).ToList();
            var questionmatches = from r in references2
                                group r by r.Label.FirstOrDefault().Value into r1
                                select new { Name = r1.Key, NumberofMatches = r1.Count() };
            questionmatches = questionmatches.OrderByDescending(a => a.NumberofMatches).Where(a => a.NumberofMatches > 1).ToList();
            finish = DateTime.Now;
            var elapsedminutes = (finish - start).Minutes;
            var elapseseconds = (finish - start).Seconds;
            model.AllResults = references2.ToList();
            model.AllConcepts = concepts.ToList();
            return model;
        }

        private List<SelectListItem> GetItemTypes()
        {
            List<SelectListItem> itemtypes = new List<SelectListItem>();
            itemtypes.Add(new SelectListItem() { Text = "Archive", Value = "Archive" });
            itemtypes.Add(new SelectListItem() { Text = "Category", Value = "Category" });
            itemtypes.Add(new SelectListItem() { Text = "Category Group", Value = "Category Group" });
            itemtypes.Add(new SelectListItem() { Text = "Category Set", Value = "Category Set" });
            itemtypes.Add(new SelectListItem() { Text = "Code List", Value = "Code List" });
            itemtypes.Add(new SelectListItem() { Text = "Code List Group", Value = "Code List Group" });
            itemtypes.Add(new SelectListItem() { Text = "Code List Scheme", Value = "Code List Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Concept", Value = "Concept" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Group", Value = "Concept Group" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Scheme", Value = "Concept Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Data Collection", Value = "Data Collection" });
            itemtypes.Add(new SelectListItem() { Text = "Group", Value = "Group" });
            itemtypes.Add(new SelectListItem() { Text = "Instrument", Value = "Instrument" });
            itemtypes.Add(new SelectListItem() { Text = "Question Item", Value = "Question Item" });
            itemtypes.Add(new SelectListItem() { Text = "Question Group", Value = "Question Group" });
            itemtypes.Add(new SelectListItem() { Text = "Study", Value = "Study" });
            itemtypes.Add(new SelectListItem() { Text = "Variable", Value = "Variable" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Group", Value = "Variable Group" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Scheme", Value = "Variable Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Statistic", Value = "Variable Statistic" });
            return itemtypes;
        }

        private List<SelectListItem> GetItemTypes1()
        {
            List<SelectListItem> itemtypes = new List<SelectListItem>();
            itemtypes.Add(new SelectListItem() { Text = "Concept", Value = "Concept" });
            itemtypes.Add(new SelectListItem() { Text = "Concept Group", Value = "Concept Group" });
            itemtypes.Add(new SelectListItem() { Text = "Question Item", Value = "Question Item" });
            itemtypes.Add(new SelectListItem() { Text = "Question Group", Value = "Question Group" });
            itemtypes.Add(new SelectListItem() { Text = "Variable", Value = "Variable" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Group", Value = "Variable Group" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Scheme", Value = "Variable Scheme" });
            itemtypes.Add(new SelectListItem() { Text = "Variable Statistic", Value = "Variable Statistic" });
            return itemtypes;
        }

        public VariableModel SaveItem(VariableModel model, string equivalence, string equivalenceselection)
        {
            model.Equivalences = new List<Word>();
            equivalenceselection = equivalenceselection + equivalence + ",";
            model.Equivalences = GetList(equivalenceselection);
            model.EquivalenceSelection = equivalenceselection;

            return model;
        }

        public List<Word> GetList(string wordselection)
        {

            List<Word> wordlist = new List<Word>();
            if (wordselection != null)
            {
                List<string> wordlist2 = wordselection.Split(',').ToList();
                foreach (var item in wordlist2)
                {
                    Word currentword = new Word();
                    currentword.Value = item;
                    if (item != "") wordlist.Add(currentword);
                }
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

        public ActionResult DeleteItem(string selectedItems, string word, string wordselection)
        {
            wordselection = wordselection.Replace(word + ",", "");
            return RedirectToAction("Equivalences", new { equivalenceselection = wordselection });
        }

        private VariableModel GetStudies(VariableModel model)
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
            model.Studies = LoadStudies(response);         
            return model;
        }

        public static List<SelectListItem> LoadStudies(SearchResponse response)
        {
            List<SelectListItem> studies = new List<SelectListItem>();
            foreach (var result in response.Results)
            {
                studies.Add(new SelectListItem() { Value = result.AgencyId, Text = result.DisplayLabel });
            }
            return studies;
        }

    }

}
