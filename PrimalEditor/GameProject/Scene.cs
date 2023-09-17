using PrimalEditor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace PrimalEditor.GameProject
{
    [DataContract]
    public class Scene : BindableBase
    {
		private string m_name;

        [DataMember]
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

        [DataMember]
		public Project Project { get; private set; }

        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
        }
    }
}