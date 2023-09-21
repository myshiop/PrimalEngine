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
		public string ScreenshotFilePath { get; set; }
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

		private bool m_isValid;

		public bool IsValid
		{
			get { return m_isValid; }
			set { m_isValid = value; OnPropertyChanged(); }
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

        public NewProject()
        {			
			ReadLocalProjectTemplates();
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
					template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(templateFile), "Screenshot.png"));
					template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
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

		public string CreateProject(ProjectTemplate template)
		{
            ValidateProjectPath();
            if (!IsValid)
            {
                return string.Empty;
            }
            var path = ProjectPath;
            if (!path.EndsWith(@"\"))
            {
                path += @"\";
            }
            path += $@"{ProjectName}\";

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    foreach (var folder in template.Folders)
                    {
                        Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                    }
                }
                var dirInfo = new DirectoryInfo(path + @".primal\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "screenshot.png")));

                //var project = new Project(ProjectName, path);
                //Serializer.ToFile(project, path + $"{ProjectName}" + Project.Extention); b
                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = String.Format(projectXml, ProjectName, path);
                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extention}"));
                File.WriteAllText(projectPath, projectXml);

                //CreateMSVCSolution(template, path);

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                //Logger.Log(MessageType.Error, $"Failed to create project {ProjectName} in {path} using template {template.ProjectType}");
                throw;
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

			IsValid = false;
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
                IsValid = true;
            }

			return IsValid;
		}
    }
}
