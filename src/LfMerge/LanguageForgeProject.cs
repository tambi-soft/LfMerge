﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using Autofac;
using LfMerge.FieldWorks;
using LfMerge.Settings;

namespace LfMerge
{
	public class LanguageForgeProject: ILfProject
	{
		protected static Dictionary<string, LanguageForgeProject> CachedProjects =
			new Dictionary<string, LanguageForgeProject>();

		private LfMergeSettingsIni _settings;
		private FwProject _fieldWorksProject;
		private readonly ProcessingState _state;
		private readonly string _projectCode;
		private ILanguageDepotProject _languageDepotProject;

		public static LanguageForgeProject Create(LfMergeSettingsIni settings, string projectCode)
		{
			LanguageForgeProject project;
			if (CachedProjects.TryGetValue(projectCode, out project))
				return project;

			project = new LanguageForgeProject(settings, projectCode);
			CachedProjects.Add(projectCode, project);
			return project;
		}

		protected LanguageForgeProject(LfMergeSettingsIni settings, string projectCode)
		{
			_settings = settings;
			_projectCode = projectCode;
			_state = ProcessingState.Deserialize(projectCode);
		}

		public static void DisposeProjectCache()
		{
			foreach (LanguageForgeProject project in CachedProjects.Values)
			{
				if (project._fieldWorksProject != null)
					project._fieldWorksProject.Dispose();
			}
			CachedProjects.Clear();
		}

		public static void DisposeProjectCache(string projectCode)
		{
			foreach (var item in CachedProjects)
			{
				LanguageForgeProject project = item.Value;
				if (project.LfProjectCode == projectCode)
				{
					project._fieldWorksProject.Dispose();
					CachedProjects.Remove(item.Key);
					break;
				}
			}
		}

		#region ILfProject implementation

		// TODO: ToLowerInvariant() won't necessarily be right in all cases. Find a better way.
		public string LfProjectCode { get { return _projectCode.ToLowerInvariant(); } }
		public string FwProjectCode { get { return _projectCode; } }

		public string MongoDatabaseName { get { return _settings.MongoDatabaseNamePrefix + LfProjectCode; } }

		public FwProject FieldWorksProject
		{
			get
			{
				if (_fieldWorksProject == null)
				{
					// for now we simply use the language forge project code as name for the fwdata file
					_fieldWorksProject = new FwProject(_settings, FwProjectCode);
				}
				return _fieldWorksProject;
			}
		}

		public ProcessingState State
		{
			get { return _state; }
		}

		public ILanguageDepotProject LanguageDepotProject
		{
			get
			{
				if (_languageDepotProject == null)
				{
					_languageDepotProject = MainClass.Container.Resolve<ILanguageDepotProject>();
					_languageDepotProject.Initialize(LfProjectCode);
				}
				return _languageDepotProject;
			}
		}

		public string LanguageDepotProjectUri
		{
			get
			{
				string uri = "http://hg-public.languagedepot.org";
				if (LanguageDepotProject.Repository != null && LanguageDepotProject.Repository.Contains("private"))
					uri = "http://hg-private.languagedepot.org";
				return uri;
			}
		}

		#endregion
	}
}

