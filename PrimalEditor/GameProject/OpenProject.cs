using PrimalEditor.Common;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    [DataContract]
    public class ProjectData
    {
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string ProjectPath { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        public string FullPath => $"{ProjectPath}/{ProjectName}{Project.Extension}";
        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects = new List<ProjectData>();
    }

    public class OpenProject : BindableBase
    {
        private static readonly string s_applicationDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/PrimalEditor";
        private static readonly string s_projectDataPath;
        private static readonly ObservableCollection<ProjectData> s_projects = new ObservableCollection<ProjectData>();

        public static ReadOnlyObservableCollection<ProjectData> Projects { get;  }

        private ProjectData m_selectProject;
        public ProjectData SelectProject
        {
            get => m_selectProject;
            set
            {
                m_selectProject = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenButtonCommand { get; set; }

        public OpenProject()
        {
            OpenButtonCommand = new RelayCommand(OpenSelectProject);
        }

        private void OpenSelectProject(object parameter)
        {
            var project = Open(SelectProject);
            bool dialogResult = false;
            if(project != null)
            {
                dialogResult = true;
            }
            var userControl = parameter as UserControl;
            if (userControl != null)
            {
                var window = Window.GetWindow(userControl);
                if (window != null)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                }
            }
        }

        static OpenProject()
        {
            try
            {
                if (!Directory.Exists(s_applicationDataPath))
                {
                    Directory.CreateDirectory(s_applicationDataPath);
                }
                s_projectDataPath = $"{s_applicationDataPath}/ProjectData.xml";
                ReadProjectData();
                Projects = new ReadOnlyObservableCollection<ProjectData>(s_projects);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void ReadProjectData()
        {
            if (File.Exists(s_projectDataPath))
            {
                var projects = Serializer.FromFile<ProjectDataList>(s_projectDataPath).Projects.OrderByDescending(x => x.Date);
                s_projects.Clear();
                foreach(var project in projects)
                {
                    if (File.Exists(project.FullPath))
                    {
                        try
                        {
                            project.Icon = File.ReadAllBytes(Path.Combine(project.ProjectPath, ".Primal/Icon.png"));
                            project.Screenshot = File.ReadAllBytes(Path.Combine(project.ProjectPath, ".Primal/Screenshot.png"));
                            s_projects.Add(project);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }

        public static Project Open(ProjectData data)
        {
            ReadProjectData();
            var project = s_projects.FirstOrDefault(x => x.FullPath == data.FullPath);
            if(project != null)
            {
                project.Date = DateTime.Now;
            }
            else
            {
                project = data;
                project.Date = DateTime.Now;
                s_projects.Add(project);
            }
            WriteProjectData();

            return null;            
        }

        private static void WriteProjectData()
        {
            var projects = s_projects.OrderBy(x => x.Date).ToList();
            Serializer.ToFile(new ProjectDataList() { Projects = projects }, s_projectDataPath);
        }
    }
}
