using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using PrimalEditor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;
using PrimalEditor.Utilities;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.GameProject
{
	[DataContract]
	public class ProjectTemplate
	{
		[DataMember]
		public string ProjectType { get; set; }
		[DataMember]
		public string ProjectFile { get; set; }
		[DataMember]
		public List<string> Folders { get; set; }
		public byte[] Icon { get; set; }
		public byte[] Screenshot { get; set; }
		public string IconFilePath { get; set; }
		public string ScreenshotPath { get; set; }
		public string ProjectFilePath { get; set; }

    }

    public class NewProject : BindableBase
    {
		private string m_projectName = "NewProject";

		/// <summary>
		/// 项目名称
		/// </summary>
		public string ProjectName
		{
			get { return m_projectName; }
			set 
			{
				m_projectName = value; 
				ValidateProjectPath();  
				OnPropertyChanged(); 
			}
		}

		private string m_projectPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/PrimalProjects/";

		/// <summary>
		/// 项目的父路径
		/// </summary>
		public string ProjectPath
		{
			get { return m_projectPath; }
			set
			{
				m_projectPath = value; 
				ValidateProjectPath();  
				OnPropertyChanged(); 
			}
		}

		private bool m_isVaild;

		public bool IsVaild
		{
			get { return m_isVaild; }
			set { m_isVaild = value; OnPropertyChanged(); }
		}

		private string m_errorMsg;

		public string ErrorMsg
		{
			get { return m_errorMsg; }
			set { m_errorMsg = value; OnPropertyChanged(); }
		}

        private ProjectTemplate m_selectTemplate;
		public ProjectTemplate SelectTemplate
		{
			get => m_selectTemplate;
			set
			{
				m_selectTemplate = value;
				OnPropertyChanged();
			}
		}

		private readonly string m_templatePath = "../../../PrimalEditor/ProjectTemplates";

		private ObservableCollection<ProjectTemplate> m_projectTemplates = new ObservableCollection<ProjectTemplate>();

        public ObservableCollection<ProjectTemplate> ProjectTemplates
		{
            get { return m_projectTemplates; }
        }

		public ICommand CreateButtonCommand { get; set; }
		public ICommand ExitButtonCommand { get; set; }

        public NewProject()
        {
			InitCommands();
			ReadLocalProjectTemplates();
        }

		public void InitCommands()
		{
			CreateButtonCommand = new RelayCommand(CreateProject);
            ExitButtonCommand = new RelayCommand(Exit);
        }

		private void ReadLocalProjectTemplates()
		{
            try
            {
                var templateFiles = Directory.GetFiles(m_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Any());
                foreach (var templateFile in templateFiles)
                {
                    var template = Serializer.FromFile<ProjectTemplate>(templateFile);
					template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), "Icon.png"));
					template.Icon = File.ReadAllBytes(template.IconFilePath);
					template.ScreenshotPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), "Screenshot.png"));
					template.Screenshot = File.ReadAllBytes(template.ScreenshotPath);
					template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), template.ProjectFile));

                    m_projectTemplates.Add(template);
                }
                ValidateProjectPath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

		private void CreateProject(object parameter)
		{
            var projectPath = CreateProjectTemplate();
			bool dialogResult = false;
            if (!string.IsNullOrEmpty(projectPath))
            {
				dialogResult = true;           
				var project = OpenProject.Open(new ProjectData() { ProjectName = ProjectName, ProjectPath = projectPath });
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

		private string CreateProjectTemplate()
		{			
			ValidateProjectPath();

			if (!IsVaild)
			{
                return string.Empty;
			}
            if (!Path.EndsInDirectorySeparator(ProjectPath))
            {
                ProjectPath += Path.DirectorySeparatorChar;
            }
			var path = $@"{ProjectPath}{ProjectName}";

			try
			{
				if(!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
                }
				foreach(var floder in SelectTemplate.Folders)
				{
					Directory.CreateDirectory(Path.Combine(path, floder));
				}
				var dirInfo = new DirectoryInfo(Path.Combine(path, ".Primal"));
				dirInfo.Attributes = FileAttributes.Hidden;
				File.Copy(SelectTemplate.IconFilePath, Path.Combine(dirInfo.FullName, "Icon.png"), true);
				File.Copy(SelectTemplate.ScreenshotPath, Path.Combine(dirInfo.FullName, "Screenshot.png"), true);

				var projectXml = File.ReadAllText(SelectTemplate.ProjectFilePath);
				projectXml = string.Format(projectXml, ProjectName, path);
				string projectPath = Path.Combine(path, ProjectName + Project.Extension);
				File.WriteAllText(projectPath, projectXml);

				return path;
            }
			catch(Exception ex)
			{
				Debug.WriteLine(ex.Message);
				return string.Empty;
			}
        }

		private void Exit(object parameter)
		{

		}

		private bool ValidateProjectPath()
		{
			var path = ProjectPath;
			if(!Path.EndsInDirectorySeparator(path))
			{
                path += Path.DirectorySeparatorChar;
            }
			path += $@"{ProjectName}";

			IsVaild = false;
			if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
			{
				ErrorMsg = "Project name can't be empty.";
			}
			else if(ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
			{
                ErrorMsg = "Project name contains invalid characters.";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMsg = "Project path can't be empty.";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMsg = "Project path contains invalid characters.";
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
			{
                ErrorMsg = "Project already exists.";
            }
            else
			{
				ErrorMsg = string.Empty;
                IsVaild = true;
            }

			return IsVaild;
		}
    }
}
