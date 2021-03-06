using Algenta.Colectica.Model.Ddi;
using Algenta.Colectica.Model.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ColecticaSdkMvc.Models
{
	public class StudyUnitModel : ItemModel
	{
		public StudyUnit StudyUnit { get; set; }

		List<RepositoryItemMetadata> questions = new List<RepositoryItemMetadata>();
		public List<RepositoryItemMetadata> Questions
		{
			get { return this.questions; }
		}
	}

    public class StudyUnitModel2 : ItemModel
    {
        public StudyUnit StudyUnit { get; set; }

        public List<RepositoryItemMetadata> Questions { get; set; }
        public List<Match> Matches { get; set; }
    }
}