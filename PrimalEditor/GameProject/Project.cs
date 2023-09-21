using PrimalEditor.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using PrimalEditor.Utilities;

namespace PrimalEditor.GameProject
{
	[DataContract(Name = "Game")]
    public class Project : BindableBase
    {
		public static string Extention = ".primal";
		[DataMember]
		public string Name { get; private set; } = "New Project";
		[DataMember]
		public string Path { get; private set; }
		public string FullPath => $"{Path}/{Name}{Extention}";

		[DataMember(Name = "Scenes")]
		private ObservableCollection<Scene> m_scenes = new ObservableCollection<Scene>();
		public ObservableCollection<Scene> Scenes
		{
			get => m_scenes;
			set
			{
				m_scenes = value;
				OnPropertyChanged();
			}
		}

		private Scene m_activeScene;
		[DataMember]
		public Scene ActiveScene
		{
			get { return m_activeScene; }
			set { m_activeScene = value; OnPropertyChanged(); }
		}


		public static Project Current => Application.Current.MainWindow.DataContext as Project;

		public static Project Load(string file)
		{
			Debug.Assert(File.Exists(file));
			return Serializer.FromFile<Project>(file);
		}
		
		public static void Save(Project project)
		{
			Serializer.ToFile(project, project.FullPath);

        }

		public void AddScene(string sceneName)
		{
			Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
			m_scenes.Add(new Scene(this, sceneName));
		}

		public void RemoveScene(Scene scene)
		{
            Debug.Assert(m_scenes.Contains(scene));
            m_scenes.Remove(scene);
        }

		public void UnLoad()
		{

		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			if(m_scenes != null)
			{
				Scenes = new ObservableCollection<Scene>(m_scenes);
				OnPropertyChanged(nameof(Scenes));
			}
		}

		public Project(string name, string path)
		{
            Name = name;
            Path = path;

			OnDeserialized(new StreamingContext());
        }
	}
}
