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
	public class EquivalenceHelper
	{
        public static EquivalenceModel BuildStudiesTree(EquivalenceModel model, List<TreeViewNode> nodes)
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
                State cstate = new State();
                cstate.selected = false;

                nodes.Add(new TreeViewNode { id = result.AgencyId, parent = "#", text = result.DisplayLabel, state = cstate });
                studies = BuildSweepsTree(studies, model.SelectedStudies, allsweeps, result.DisplayLabel, result.AgencyId, nodes, result.AgencyId);
                i++;
            }

            model.Results = studies;
            return model;
        }

        public static List<StudyItem> BuildSweepsTree(List<StudyItem> model, List<string> selecteditems, SearchResponse allsweeps, string study, string agency, List<TreeViewNode> nodes, string parent)
        {
            // move
            int i = 1;
            bool found = false;
            foreach (var sweep in allsweeps.Results)
            {
                StudyItem item = new StudyItem();
                item.AgencyId = sweep.AgencyId;
                item.DisplayLabel = sweep.DisplayLabel;
                item.Identifier = sweep.Identifier;
                item.ReferenceItem = study;
                if (sweep.AgencyId == agency)
                {
                    State cstate = new State();
                    List<string> search = new List<string>() { item.AgencyId + "," + item.Identifier.ToString() + "," + item.DisplayLabel };
                    if (selecteditems != null) { found = search.Any(s => selecteditems.Contains(s)); } else { found = false; }
                    cstate.selected = found;
                    nodes.Add(new TreeViewNode { id = parent + " " + item.Identifier.ToString(), parent = parent.ToString(), text = item.DisplayLabel, state = cstate });
                }
                i++;
            }
            model = model.OrderBy(r => r.ReferenceItem).ToList();
            return model;
        }

        public static List<SelectListItem> LoadStudies(EquivalenceModel model, SearchResponse response)
        {
            // move
            List<SelectListItem> studies = new List<SelectListItem>();
            foreach (var result in response.Results)
            {
                studies.Add(new SelectListItem() { Value = result.AgencyId, Text = result.DisplayLabel });
            }
            return studies;
        }

        public static EquivalenceModel GetStudies(EquivalenceModel model, string agency)
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


        public static void ProcessCSV(List<EquivalenceItem> results, string type, string fileName)
        {
            string line;
            var filename = @"C:\Users\qtnvwhn\Downloads\" + fileName;
            filename = filename.Replace("\\","/");
            using (System.IO.StreamWriter file =
                       new System.IO.StreamWriter(filename))
            {
                line = "Search Term, Variable Name, Variable Text, Item, Question Name, Question Text, Item, Question Identifier, Agent, Study, Concept";
                file.WriteLine(line);
                foreach (var item in results)
                {
                    line = item.equivalence + "," + item.variableName + "," + item.variableText.Trim().Replace(",", " ") + "," + item.variableItem + "," +
                        item.questionName + "," + item.questionText.Trim().Replace(",", " ") + "," + item.questionItem +
                        "," + item.study.Trim() + "," + item.studyGroup + "," + item.concept;
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