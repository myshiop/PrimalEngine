using PrimalEditor.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEditor.GameProject
{
	[DataContract(Name = "Game")]
    public class Project : BindableBase
    {
		public static string Extension = ".primal";
		[DataMember]
		public string Name { get; private set; }
		[DataMember]
		public string Path { get; private set; }
		public string FullPath => $"{Path}/{Name}{Extension}";

		[DataMember(Name = "Scenes")]
		private ObservableCollection<Scene> m_scenes = new ObservableCollection<Scene>();
		public ObservableCollection<Scene> Scenes => m_scenes;

		public Project(string name, string path)
		{
            Name = name;
            Path = path;
			m_scenes.Add(new Scene(this, "Default Scene"));
        }
	}
}
