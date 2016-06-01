﻿// Copyright (c) 2011-2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Reflection;
using Autofac;
using Chorus.Model;
using IniParser.Parser;
using LfMerge.Actions.Infrastructure;
using LfMerge.FieldWorks;
using LfMerge.Logging;
using LfMerge.MongoConnector;
using LfMerge.Settings;
using LibFLExBridgeChorusPlugin.Infrastructure;
using NUnit.Framework;
using Palaso.IO;
using Palaso.TestUtilities;

namespace LfMerge.Tests
{
	public class TestEnvironment : IDisposable
	{
		private readonly TemporaryFolder _languageForgeServerFolder;
		private bool _resetLfProjectsDuringCleanup;
		public LfMergeSettingsIni Settings;
		public ILogger Logger { get { return MainClass.Logger; }}

		static TestEnvironment()
		{
			// Need to call MongoConnectionDouble.Initialize() exactly once, before any tests are run -- so do it here
			MongoConnectionDouble.Initialize();
		}

		public TestEnvironment(bool registerSettingsModelDouble = true,
			bool registerProcessingStateDouble = true,
			bool resetLfProjectsDuringCleanup = true,
			TemporaryFolder languageForgeServerFolder = null)
		{
			_resetLfProjectsDuringCleanup = resetLfProjectsDuringCleanup;
			if (languageForgeServerFolder == null)
				_languageForgeServerFolder = new TemporaryFolder(TestName + Path.GetRandomFileName());
			else
				_languageForgeServerFolder = languageForgeServerFolder;
			Environment.SetEnvironmentVariable("FW_CommonAppData", _languageForgeServerFolder.Path);
			MainClass.Container = RegisterTypes(registerSettingsModelDouble,
				registerProcessingStateDouble,
				_languageForgeServerFolder.Path).Build();
			Settings = MainClass.Container.Resolve<LfMergeSettingsIni>();
			MainClass.Logger = MainClass.Container.Resolve<ILogger>();
			Directory.CreateDirectory(Settings.ProjectsDirectory);
			Directory.CreateDirectory(Settings.TemplateDirectory);
			Directory.CreateDirectory(Settings.StateDirectory);
		}

		private string TestName
		{
			get
			{
				var testName = TestContext.CurrentContext.Test.Name;
				var firstInvalidChar = testName.IndexOfAny(Path.GetInvalidPathChars());
				if (firstInvalidChar >= 0)
					testName = testName.Substring(0, firstInvalidChar);
				return testName;
			}
		}

		private ContainerBuilder RegisterTypes(bool registerSettingsModel,
			bool registerProcessingStateDouble, string temporaryFolder)
		{
			ContainerBuilder containerBuilder = MainClass.RegisterTypes();
			containerBuilder.RegisterType<LfMergeSettingsDouble>()
				.WithParameter(new TypedParameter(typeof(string), temporaryFolder)).SingleInstance()
				.As<LfMergeSettingsIni>();
			containerBuilder.RegisterType<TestLogger>().SingleInstance().As<ILogger>()
				.WithParameter(new TypedParameter(typeof(string), TestName));


			containerBuilder.RegisterType<MongoConnectionDouble>().As<IMongoConnection>().SingleInstance();

			if (registerSettingsModel)
			{
				containerBuilder.RegisterType<InternetCloneSettingsModelDouble>().As<InternetCloneSettingsModel>();
				containerBuilder.RegisterType<ChorusHelperDouble>().As<ChorusHelper>();
				containerBuilder.RegisterType<UpdateBranchHelperFlexDouble>().As<UpdateBranchHelperFlex>();
				containerBuilder.RegisterType<FlexHelperDouble>().As<FlexHelper>();
				containerBuilder.RegisterType<MongoProjectRecordFactoryDouble>().As<MongoProjectRecordFactory>();
			}

			var ldProj = new LanguageDepotProjectDouble();
			containerBuilder.RegisterInstance(ldProj)
				.As<ILanguageDepotProject>().AsSelf().SingleInstance();

			if (registerProcessingStateDouble)
			{
				containerBuilder.RegisterType<ProcessingStateFactoryDouble>()
					.As<IProcessingStateDeserialize>().AsSelf().SingleInstance();
			}
			return containerBuilder;
		}

		public void Dispose()
		{
			MainClass.Container.Dispose();
			MainClass.Container = null;
			if (_resetLfProjectsDuringCleanup)
				LanguageForgeProjectAccessor.Reset();
			_languageForgeServerFolder.Dispose();
			Settings = null;
		}

		public string LanguageForgeFolder
		{
			// get { return Path.Combine(_languageForgeServerFolder.Path, "webwork"); }
			// Should get this from Settings object, but unfortunately we have to resolve this
			// *before* the Settings object is available.
			get { return LangForgeDirFinder.ProjectsDirectory; }
		}

		public LfMergeSettingsIni LangForgeDirFinder
		{
			get { return Settings; }
		}

		public string ProjectPath(string projectCode)
		{
			return Path.Combine(LanguageForgeFolder, projectCode);
		}

		public void CreateProjectUpdateFolder(string projectCode)
		{
			Directory.CreateDirectory(ProjectPath(projectCode));
		}

		public static void CopyFwProjectTo(string projectCode, string destDir)
		{
			string dataDir = Path.Combine(FindGitRepoRoot(), "data");
			DirectoryUtilities.CopyDirectory(Path.Combine(dataDir, projectCode), destDir);

			// Adjust hgrc file
			var hgrc = Path.Combine(destDir, projectCode, ".hg/hgrc");
			if (File.Exists(hgrc))
			{
				var parser = new IniDataParser();
				var iniData = parser.Parse(File.ReadAllText(hgrc));

				iniData["ui"]["username"] = Environment.UserName;

				var outputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				iniData["merge-tools"]["chorusmerge.executable"] =
					Path.Combine(outputDirectory, "chorusmerge");

				iniData["extensions"]["fixutf8"] = Path.Combine(FindGitRepoRoot(),
					"MercurialExtensions/fixutf8/fixutf8.py");

				var contents = iniData.ToString();
				File.WriteAllText(hgrc, contents);
			}

			Console.WriteLine("Copied {0} to {1}", projectCode, destDir);
		}

		public static string FindGitRepoRoot(string startDir = null)
		{
			if (String.IsNullOrEmpty(startDir))
				startDir = Directory.GetCurrentDirectory();
			while (!Directory.Exists(Path.Combine(startDir, ".git")))
			{
				var di = new DirectoryInfo(startDir);
				if (di.Parent == null) // We've reached the root directory
				{
					// Last-ditch effort: assume we're in output/Debug, even though we never found .git
					return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".."));
				}
				startDir = Path.Combine(startDir, "..");
			}
			return Path.GetFullPath(startDir);
		}

//		public string WriteFile(string fileName, string xmlForEntries, string directory)
//		{
//			string content;
//			using (var writer = File.CreateText(Path.Combine(directory, fileName)))
//			{
//				content = string.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?> " +
//					"<lift version =\"{0}\" producer=\"WeSay.1Pt0Alpha\" " +
//					"xmlns:flex=\"http://fieldworks.sil.org\">{1}" +
//					"</lift>", Validator.LiftVersion, xmlForEntries);
//				writer.Write(content);
//			}
//
//			new FileInfo(Path.Combine(directory, fileName)).LastWriteTime = DateTime.Now.AddSeconds(1);
//
//			return content;
//		}
//
//		public void CreateLiftInputFile(IList<string> data, string fileName, string directory)
//		{
//			// NOTE: if the parameter differentTimeStamps is true we wait a second before
//			// creating the next file. This allows the files to have different timestamps.
//			// Originally the code used 100ms instead of 1s, but the resolution of file
//			// timestamps in the Mono implementation is 1s. However, if is possible in the
//			// real app that two files get created within a few milliseconds and we rely on
//			// the file timestamp to do the sorting of files, then we have a real problem.
//
//			var path = Path.Combine(directory, fileName);
//			if (File.Exists(path))
//				File.Delete(path);
//			using (var wrtr = File.CreateText(path))
//			{
//				for (var i = 0; i < data.Count; ++i)
//					wrtr.WriteLine(data[i]);
//				wrtr.Close();
//			}
//		}
//
//		public void CreateLiftUpdateFile(IList<string> data, string fileName,
//			string directory)
//		{
//			string path = Path.Combine(directory, fileName);
//			if (File.Exists(path))
//				File.Delete(path);
//			var bldr = new StringBuilder();
//			for (var i = 0; i < data.Count; ++i)
//				bldr.AppendLine(data[i]);
//
//			WriteFile(fileName, bldr.ToString(), directory);
//		}

//		public void VerifyEntryInnerText(XmlDocument xmlDoc, string xPath, string innerText)
//		{
//			var selectedEntries = VerifyEntryExists(xmlDoc, xPath);
//			Assert.That(selectedEntries[0].InnerText, Is.EqualTo(innerText), "Text for entry is wrong");
//		}
//
//		public XmlNodeList VerifyEntryExists(XmlDocument xmlDoc, string xPath)
//		{
//			XmlNodeList selectedEntries = xmlDoc.SelectNodes(xPath);
//			Assert.IsNotNull(selectedEntries);
//			Assert.AreEqual(1, selectedEntries.Count,
//				String.Format("An entry with the following criteria should exist:{0}", xPath));
//			return selectedEntries;
//		}
//
//		public void VerifyEntryDoesNotExist(XmlDocument xmlDoc, string xPath)
//		{
//			XmlNodeList selectedEntries = xmlDoc.SelectNodes(xPath);
//			Assert.IsNotNull(selectedEntries);
//			Assert.AreEqual(0, selectedEntries.Count,
//				String.Format("An entry with the following criteria should not exist:{0}", xPath));
//		}
	}
}

