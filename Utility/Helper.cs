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
	public class Helper
	{
        public static List<Item> GetSelectedItems(string selectedItems)
        {
            List<Item> items = new List<Item>();
            if (selectedItems == null) { return items; }

            List<string> selecteditems = selectedItems.Split(';').ToList();
            foreach (var selecteditem in selecteditems)
            {
                if (selecteditem != "")
                {
                    List<string> lists = selecteditem.Split(',').ToList();
                    Item item = new Item();
                    int i = 0;
                    foreach (var list in lists)
                    {
                        switch (i)
                        {
                            case 0:
                                item.Id = list;
                                break;
                            case 1:
                                item.QuestionValue = list;
                                break;
                        }
                        i++;
                    }
                    items.Add(item);
                }
            }
            return items;
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
            facet.UseDistinctResultItem = false;

            var referencingItemsDescriptions = client.GetRepositoryItemDescriptionsByObject(facet);
            List<RepositoryItemMetadata> referenceItems = referencingItemsDescriptions.ToList();
            var referencingItemsSubject = client.GetRepositoryItemDescriptionsBySubject(facet).ToList();
            return referenceItems;
        }

        public static StudyUnitModel GetAllQuestions(string agency, Guid id)
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
            var infoList = client.GetRepositoryItemDescriptions(matches.ToIdentifierCollection()).ToList();
            var infoList1 = infoList.Where(a => a.DisplayLabel == "maju");
            foreach (var info in infoList1)
            {
                studyModel.Questions.Add(info);
            }

            return studyModel;
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
    }
}