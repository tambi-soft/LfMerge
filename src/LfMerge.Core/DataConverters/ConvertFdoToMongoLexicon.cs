﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LfMerge.Core.FieldWorks;
using LfMerge.Core.LanguageForge.Config;
using LfMerge.Core.LanguageForge.Model;
using LfMerge.Core.Logging;
using LfMerge.Core.MongoConnector;
using MongoDB.Bson;
using SIL.Progress;
using SIL.CoreImpl;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.FDO;

namespace LfMerge.Core.DataConverters
{
	public class ConvertFdoToMongoLexicon
	{
		private ILfProject LfProject { get; set; }
		private FwProject FwProject { get; set; }
		private FdoCache Cache { get; set; }
		private IProgress Progress { get; set; }
		private FwServiceLocatorCache ServiceLocator { get; set; }
		private ILogger Logger { get; set; }
		private IMongoConnection Connection { get; set; }
		private MongoProjectRecordFactory ProjectRecordFactory { get; set; }


		private int _wsEn;

		private ConvertFdoToMongoCustomField _convertCustomField;

		// Shorter names to use in this class since MagicStrings.LfOptionListCodeForGrammaticalInfo (etc.) are real mouthfuls
		private const string GrammarListCode = MagicStrings.LfOptionListCodeForGrammaticalInfo;
		private const string SemDomListCode = MagicStrings.LfOptionListCodeForSemanticDomains;
		private const string AcademicDomainListCode = MagicStrings.LfOptionListCodeForAcademicDomainTypes;
//		private const string EnvironListCode = MagicStrings.LfOptionListCodeForEnvironments;  // Skip since we're not currently converting this (LF data model is too different)
		private const string LocationListCode = MagicStrings.LfOptionListCodeForLocations;
		private const string UsageTypeListCode = MagicStrings.LfOptionListCodeForUsageTypes;
//		private const string ReversalTypeListCode = MagicStrings.LfOptionListCodeForReversalTypes;  // Skip since we're not currently converting this (LF data model is too different)
		private const string SenseTypeListCode = MagicStrings.LfOptionListCodeForSenseTypes;
		private const string AnthroCodeListCode = MagicStrings.LfOptionListCodeForAnthropologyCodes;
		private const string StatusListCode = MagicStrings.LfOptionListCodeForStatus;

		private IDictionary<string, ConvertFdoToMongoOptionList> ListConverters;

		//private ConvertFdoToMongoOptionList _convertAnthroCodesOptionList;

		public ConvertFdoToMongoLexicon(ILfProject lfProject, ILogger logger, IMongoConnection connection, IProgress progress, MongoProjectRecordFactory projectRecordFactory)
		{
			LfProject = lfProject;
			Logger = logger;
			Connection = connection;
			Progress = progress;
			ProjectRecordFactory = projectRecordFactory;

			FwProject = LfProject.FieldWorksProject;
			Cache = FwProject.Cache;
			ServiceLocator = FwProject.ServiceLocator;
			_wsEn = ServiceLocator.WritingSystemFactory.GetWsFromStr("en");

			// Reconcile writing systems from FDO and Mongo
			Dictionary<string, LfInputSystemRecord> lfWsList = FdoWsToLfWs();
			#if FW8_COMPAT
			List<string> VernacularWss = ServiceLocator.LanguageProject.CurrentVernacularWritingSystems.Select(ws => ws.Id).ToList();
			List<string> AnalysisWss = ServiceLocator.LanguageProject.CurrentAnalysisWritingSystems.Select(ws => ws.Id).ToList();
			List<string> PronunciationWss = ServiceLocator.LanguageProject.CurrentPronunciationWritingSystems.Select(ws => ws.Id).ToList();
			#else
			List<string> VernacularWss = ServiceLocator.LanguageProject.CurrentVernacularWritingSystems.Select(ws => ws.LanguageTag).ToList();
			List<string> AnalysisWss = ServiceLocator.LanguageProject.CurrentAnalysisWritingSystems.Select(ws => ws.LanguageTag).ToList();
			List<string> PronunciationWss = ServiceLocator.LanguageProject.CurrentPronunciationWritingSystems.Select(ws => ws.LanguageTag).ToList();
			#endif
			Connection.SetInputSystems(LfProject, lfWsList, VernacularWss, AnalysisWss, PronunciationWss);

			ListConverters = new Dictionary<string, ConvertFdoToMongoOptionList>();
			ListConverters[GrammarListCode] = ConvertOptionListFromFdo(LfProject, GrammarListCode, ServiceLocator.LanguageProject.PartsOfSpeechOA);
			ListConverters[SemDomListCode] = ConvertOptionListFromFdo(LfProject, SemDomListCode, ServiceLocator.LanguageProject.SemanticDomainListOA, updateMongoList: false);
			ListConverters[AcademicDomainListCode] = ConvertOptionListFromFdo(LfProject, AcademicDomainListCode, ServiceLocator.LanguageProject.LexDbOA.DomainTypesOA);
			ListConverters[LocationListCode] = ConvertOptionListFromFdo(LfProject, LocationListCode, ServiceLocator.LanguageProject.LocationsOA);
			ListConverters[UsageTypeListCode] = ConvertOptionListFromFdo(LfProject, UsageTypeListCode, ServiceLocator.LanguageProject.LexDbOA.UsageTypesOA);
			ListConverters[SenseTypeListCode] = ConvertOptionListFromFdo(LfProject, SenseTypeListCode, ServiceLocator.LanguageProject.LexDbOA.SenseTypesOA);
			ListConverters[AnthroCodeListCode] = ConvertOptionListFromFdo(LfProject, AnthroCodeListCode, ServiceLocator.LanguageProject.AnthroListOA);
			ListConverters[StatusListCode] = ConvertOptionListFromFdo(LfProject, StatusListCode, ServiceLocator.LanguageProject.StatusOA);

			_convertCustomField = new ConvertFdoToMongoCustomField(Cache, ServiceLocator, logger);
			foreach (KeyValuePair<string, ICmPossibilityList> pair in _convertCustomField.GetCustomFieldParentLists())
			{
				string listCode = pair.Key;
				ICmPossibilityList parentList = pair.Value;
				if (!ListConverters.ContainsKey(listCode))
					ListConverters[listCode] = ConvertOptionListFromFdo(LfProject, listCode, parentList);
			}
		}

		public void RunConversion()
		{
			Logger.Notice("FdoToMongo: Converting lexicon for project {0}", LfProject.ProjectCode);
			ILexEntryRepository repo = GetInstance<ILexEntryRepository>();
			if (repo == null)
			{
				Logger.Error("Can't find LexEntry repository for FieldWorks project {0}", LfProject.ProjectCode);
				return;
			}

			// Custom field configuration AND view configuration should all be set at once
			Dictionary<string, LfConfigFieldBase> lfCustomFieldList = new Dictionary<string, LfConfigFieldBase>();
			Dictionary<string, string> lfCustomFieldTypes = new Dictionary<string, string>();
			_convertCustomField.WriteCustomFieldConfig(lfCustomFieldList, lfCustomFieldTypes);
			Connection.SetCustomFieldConfig(LfProject, lfCustomFieldList);
			_convertCustomField.CreateCustomFieldsConfigViews(LfProject, lfCustomFieldList, lfCustomFieldTypes);

			Dictionary<Guid, DateTime> previousModificationDates = Connection.GetAllModifiedDatesForEntries(LfProject);

			int i = 1;
			foreach (ILexEntry fdoEntry in repo.AllInstances())
			{
				bool createdEntry = false;
				DateTime previousDateModified;
				if (!previousModificationDates.TryGetValue(fdoEntry.Guid, out previousDateModified))
				{
					// Looks like this entry is new in FDO
					createdEntry = true;
					previousDateModified = DateTime.MinValue; // Ensure it will seem modified when comparing it later
				}
				// Remember that FDO's DateModified is stored in local time for some incomprehensible reason...
				if (!createdEntry && previousDateModified.ToLocalTime() == fdoEntry.DateModified)
				{
					// Hasn't been modified since last time: just skip this record entirely
					continue;
				}
				LfLexEntry lfEntry = FdoLexEntryToLfLexEntry(fdoEntry);
				lfEntry.IsDeleted = false;
				Logger.Info("{3} - FdoToMongo: {0} LfEntry {1} ({2})", createdEntry ? "Created" : "Modified", lfEntry.Guid, ConvertUtilities.EntryNameForDebugging(lfEntry), i++);
				Connection.UpdateRecord(LfProject, lfEntry);
			}
			LfProject.IsInitialClone = false;

			RemoveMongoEntriesDeletedInFdo();
			Logger.Debug("Running FtMComments, should see comments show up below:");
			var commCvtr = new ConvertFdoToMongoComments(Connection, LfProject, Logger, Progress, ProjectRecordFactory);
			commCvtr.RunConversion();
		}

		private void RemoveMongoEntriesDeletedInFdo()
		{
			IEnumerable<LfLexEntry> lfEntries = Connection.GetRecords<LfLexEntry>(LfProject, MagicStrings.LfCollectionNameForLexicon);
			foreach (LfLexEntry lfEntry in lfEntries)
			{
				if (lfEntry.Guid == null)
					continue;
				if (!ServiceLocator.ObjectRepository.IsValidObjectId(lfEntry.Guid.Value) ||
				    !ServiceLocator.ObjectRepository.GetObject(lfEntry.Guid.Value).IsValidObject)
				{
					if (lfEntry.IsDeleted)
						// Don't need to delete this record twice
						continue;

					lfEntry.IsDeleted = true;
					lfEntry.DateModified = DateTime.UtcNow;
					Logger.Info("FdoToMongo: Deleted LfEntry {0} ({1})", lfEntry.Guid, ConvertUtilities.EntryNameForDebugging(lfEntry));
					Connection.UpdateRecord(LfProject, lfEntry);
				}
			}
		}

		// Shorthand for getting an instance from the cache's service locator
		private T GetInstance<T>() where T : class
		{
			return ServiceLocator.GetInstance<T>();
		}

		private LfMultiText ToMultiText(IMultiAccessorBase fdoMultiString)
		{
			if (fdoMultiString == null) return null;
			return LfMultiText.FromFdoMultiString(fdoMultiString, ServiceLocator.WritingSystemManager);
		}

		public static LfMultiText ToMultiText(IMultiAccessorBase fdoMultiString, ILgWritingSystemFactory fdoWritingSystemManager)
		{
			if ((fdoMultiString == null) || (fdoWritingSystemManager == null)) return null;
			return LfMultiText.FromFdoMultiString(fdoMultiString, fdoWritingSystemManager);
		}

		private LfStringField ToStringField(string listCode, ICmPossibility fdoPoss)
		{
			return LfStringField.FromString(ListConverters[listCode].LfItemKeyString(fdoPoss, _wsEn));
		}

		private LfStringArrayField ToStringArrayField(string listCode, IEnumerable<ICmPossibility> fdoPossCollection)
		{
			return LfStringArrayField.FromStrings(ListConverters[listCode].LfItemKeyStrings(fdoPossCollection, _wsEn));
		}

		// Special case: LF sense Status field is a StringArray, but FDO sense status is single possibility
		private LfStringArrayField ToStringArrayField(string listCode, ICmPossibility fdoPoss)
		{
			return LfStringArrayField.FromSingleString(ListConverters[listCode].LfItemKeyString(fdoPoss, _wsEn));
		}

		/// <summary>
		/// Convert FDO lex entry to LF lex entry.
		/// </summary>
		/// <returns>LF entry
		/// <param name="fdoEntry">Fdo entry.</param>
		private LfLexEntry FdoLexEntryToLfLexEntry(ILexEntry fdoEntry)
		{
			if (fdoEntry == null) return null;

			ILgWritingSystem AnalysisWritingSystem = ServiceLocator.LanguageProject.DefaultAnalysisWritingSystem;
			ILgWritingSystem VernacularWritingSystem = ServiceLocator.LanguageProject.DefaultVernacularWritingSystem;

			var lfEntry = new LfLexEntry();

			IMoForm fdoLexeme = fdoEntry.LexemeFormOA;
			if (fdoLexeme == null)
				lfEntry.Lexeme = null;
			else
				lfEntry.Lexeme = ToMultiText(fdoLexeme.Form);
			// Other fields of fdoLexeme (AllomorphEnvironments, LiftResidue, MorphTypeRA, etc.) not mapped

			// Fields below in alphabetical order by ILexSense property, except for Lexeme
			foreach (IMoForm allomorph in fdoEntry.AlternateFormsOS)
			{
				// Do nothing; LanguageForge doesn't currently handle allomorphs, so we don't convert them
			}
			lfEntry.EntryBibliography = ToMultiText(fdoEntry.Bibliography);
			// TODO: Consider whether to use fdoEntry.CitationFormWithAffixType instead
			// (which would produce "-s" instead of "s" for the English plural suffix, for instance)
			lfEntry.CitationForm = ToMultiText(fdoEntry.CitationForm);
			lfEntry.Note = ToMultiText(fdoEntry.Comment);

			// DateModified and DateCreated can be confusing, because LF and FDO are doing two different
			// things with them. In FDO, there is just one DateModified and one DateCreated; simple. But
			// in LF, there is an AuthorInfo record as well, which contains its own ModifiedDate and CreatedDate
			// fields. (Note the word order: there's LfEntry.DateCreated, and LfEntry.AuthorInfo.CreatedDate).

			// The conversion we have chosen to use is: AuthorInfo will correspond to FDO. So FDO.DateCreated
			// becomes AuthorInfo.CreatedDate, and FDO.DateModified becomes AuthorInfo.ModifiedDate. The two
			// fields on the LF entry will instead refer to when the *Mongo record* was created or modified,
			// and the LfEntry.DateCreated and LfEntry.DateModified fields will never be put into FDO.

			var now = DateTime.UtcNow;
			if (LfProject.IsInitialClone)
			{
				lfEntry.DateCreated = now;
			}
			// LanguageForge needs this modified to know there is changed data
			lfEntry.DateModified = now;

			if (lfEntry.AuthorInfo == null)
				lfEntry.AuthorInfo = new LfAuthorInfo();
			lfEntry.AuthorInfo.CreatedByUserRef = null;
			lfEntry.AuthorInfo.CreatedDate = fdoEntry.DateCreated.ToUniversalTime();
			lfEntry.AuthorInfo.ModifiedByUserRef = null;
			lfEntry.AuthorInfo.ModifiedDate = fdoEntry.DateModified.ToUniversalTime();

#if DBVERSION_7000068
			ILexEtymology fdoEtymology = fdoEntry.EtymologyOA;
#else
			// TODO: Once LF's data model is updated from a single etymology to an array,
			// convert all of them instead of just the first. E.g.,
			// foreach (ILexEtymology fdoEtymology in fdoEntry.EtymologyOS) { ... }
			ILexEtymology fdoEtymology = null;
			if (fdoEntry.EtymologyOS.Count > 0)
				fdoEtymology = fdoEntry.EtymologyOS.First();
#endif
			if (fdoEtymology != null)
			{
				lfEntry.Etymology = ToMultiText(fdoEtymology.Form);
				lfEntry.EtymologyComment = ToMultiText(fdoEtymology.Comment);
				lfEntry.EtymologyGloss = ToMultiText(fdoEtymology.Gloss);
#if DBVERSION_7000068
				lfEntry.EtymologySource = LfMultiText.FromSingleStringMapping(AnalysisWritingSystem.Id, fdoEtymology.Source);
#else
				lfEntry.EtymologySource = ToMultiText(fdoEtymology.LanguageNotes);
#endif
				// fdoEtymology.LiftResidue not mapped
			}
			lfEntry.Guid = fdoEntry.Guid;
			if (fdoEntry.LIFTid == null)
			{
				lfEntry.LiftId = null;
			}
			else
			{
				lfEntry.LiftId = fdoEntry.LIFTid.Normalize(System.Text.NormalizationForm.FormC);  // Because LIFT files on disk are NFC and we need to make sure LiftIDs match those on disk
			}
			lfEntry.LiteralMeaning = ToMultiText(fdoEntry.LiteralMeaning);
			if (fdoEntry.PrimaryMorphType != null) {
				lfEntry.MorphologyType = fdoEntry.PrimaryMorphType.NameHierarchyString;
			}
			// TODO: Once LF's data model is updated from a single pronunciation to an array of pronunciations, convert all of them instead of just the first. E.g.,
			//foreach (ILexPronunciation fdoPronunciation in fdoEntry.PronunciationsOS) { ... }
			if (fdoEntry.PronunciationsOS.Count > 0)
			{
				ILexPronunciation fdoPronunciation = fdoEntry.PronunciationsOS.First();
				lfEntry.Pronunciation = ToMultiText(fdoPronunciation.Form);
				lfEntry.CvPattern = LfMultiText.FromSingleITsString(fdoPronunciation.CVPattern, ServiceLocator.WritingSystemFactory);
				lfEntry.Tone = LfMultiText.FromSingleITsString(fdoPronunciation.Tone, ServiceLocator.WritingSystemFactory);
				// TODO: Map fdoPronunciation.MediaFilesOS properly (converting video to sound files if necessary)
				lfEntry.Location = ToStringField(LocationListCode, fdoPronunciation.LocationRA);
			}
			lfEntry.EntryRestrictions = ToMultiText(fdoEntry.Restrictions);
			if (lfEntry.Senses == null) // Shouldn't happen, but let's be careful
				lfEntry.Senses = new List<LfSense>();
			lfEntry.Senses.AddRange(fdoEntry.SensesOS.Select(FdoSenseToLfSense));
			lfEntry.SummaryDefinition = ToMultiText(fdoEntry.SummaryDefinition);

			BsonDocument customFieldsAndGuids = _convertCustomField.GetCustomFieldsForThisCmObject(fdoEntry, "entry", ListConverters);
			BsonDocument customFieldsBson = customFieldsAndGuids["customFields"].AsBsonDocument;
			BsonDocument customFieldGuids = customFieldsAndGuids["customFieldGuids"].AsBsonDocument;

			lfEntry.CustomFields = customFieldsBson;
			lfEntry.CustomFieldGuids = customFieldGuids;

			return lfEntry;

			/* Fields not mapped because it doesn't make sense to map them (e.g., Hvo, backreferences, etc):
			fdoEntry.ComplexFormEntries;
			fdoEntry.ComplexFormEntryRefs;
			fdoEntry.ComplexFormsNotSubentries;
			fdoEntry.EntryRefsOS;
			fdoEntry.HasMoreThanOneSense;
			fdoEntry.HeadWord; // Read-only virtual property
			fdoEntry.IsMorphTypesMixed; // Read-only property
			fdoEntry.LexEntryReferences;
			fdoEntry.MainEntriesOrSensesRS;
			fdoEntry.MinimalLexReferences;
			fdoEntry.MorphoSyntaxAnalysesOC;
			fdoEntry.MorphTypes;
			fdoEntry.NumberOfSensesForEntry;
			fdoEntry.PicturesOfSenses;

			*/

			/* Fields that would make sense to map, but that we don't because LF doesn't handle them (e.g., allomorphs):
			fdoEntry.AllAllomorphs; // LF doesn't handle allomorphs, so skip all allomorph-related fields
			fdoEntry.AlternateFormsOS;
			fdoEntry.CitationFormWithAffixType; // Citation form already mapped
			fdoEntry.DoNotPublishInRC;
			fdoEntry.DoNotShowMainEntryInRC;
			fdoEntry.DoNotUseForParsing;
			fdoEntry.HomographForm;
			fdoEntry.HomographFormKey;
			fdoEntry.HomographNumber;
			fdoEntry.ImportResidue;
			fdoEntry.LiftResidue;
			fdoEntry.PronunciationsOS
			fdoEntry.PublishAsMinorEntry;
			fdoEntry.PublishIn;
			fdoEntry.ShowMainEntryIn;
			fdoEntry.Subentries;
			fdoEntry.VariantEntryRefs;
			fdoEntry.VariantFormEntries;
			fdoEntry.VisibleComplexFormBackRefs;
			fdoEntry.VisibleComplexFormEntries;
			fdoEntry.VisibleVariantEntryRefs;

			*/
		}

		/// <summary>
		/// Convert FDO sense to LF sense.
		/// </summary>
		/// <returns>LF sense
		/// <param name="fdoSense">Fdo sense.</param>
		private LfSense FdoSenseToLfSense(ILexSense fdoSense)
		{
			var lfSense = new LfSense();

			ILgWritingSystem VernacularWritingSystem = ServiceLocator.LanguageProject.DefaultVernacularWritingSystem;
			ILgWritingSystem AnalysisWritingSystem = ServiceLocator.LanguageProject.DefaultAnalysisWritingSystem;

			lfSense.Guid = fdoSense.Guid;
			lfSense.Gloss = ToMultiText(fdoSense.Gloss);
			lfSense.Definition = ToMultiText(fdoSense.Definition);

			// Fields below in alphabetical order by ILexSense property, except for Guid, Gloss and Definition
			lfSense.AcademicDomains = ToStringArrayField(AcademicDomainListCode, fdoSense.DomainTypesRC);
			lfSense.AnthropologyCategories = ToStringArrayField(AnthroCodeListCode, fdoSense.AnthroCodesRC);
			lfSense.AnthropologyNote = ToMultiText(fdoSense.AnthroNote);
			lfSense.DiscourseNote = ToMultiText(fdoSense.DiscourseNote);
			lfSense.EncyclopedicNote = ToMultiText(fdoSense.EncyclopedicInfo);
			if (fdoSense.ExamplesOS != null)
			{
				lfSense.Examples = new List<LfExample>(fdoSense.ExamplesOS.Select(FdoExampleToLfExample));
			}

			lfSense.GeneralNote = ToMultiText(fdoSense.GeneralNote);
			lfSense.GrammarNote = ToMultiText(fdoSense.GrammarNote);
			if (fdoSense.LIFTid == null)
			{
				lfSense.LiftId = null;
			}
			else
			{
				lfSense.LiftId = fdoSense.LIFTid.Normalize(System.Text.NormalizationForm.FormC);  // Because LIFT files on disk are NFC and we need to make sure LiftIDs match those on disk
			}
			if (fdoSense.MorphoSyntaxAnalysisRA != null)
			{
				IPartOfSpeech secondaryPos = null; // Only used in derivational affixes
				IPartOfSpeech pos = ConvertFdoToMongoPartsOfSpeech.FromMSA(fdoSense.MorphoSyntaxAnalysisRA, out secondaryPos);
				// Sometimes the part of speech can be null for legitimate reasons, so check the known class IDs before warning of an unknown MSA type
				if (pos == null && !ConvertFdoToMongoPartsOfSpeech.KnownMsaClassIds.Contains(fdoSense.MorphoSyntaxAnalysisRA.ClassID))
					Logger.Warning("Got MSA of unknown type {0} in sense {1} in project {2}",
						fdoSense.MorphoSyntaxAnalysisRA.GetType().Name,
						fdoSense.Guid,
						LfProject.ProjectCode);
				else
				{
					lfSense.PartOfSpeech = ToStringField(GrammarListCode, pos);
					lfSense.SecondaryPartOfSpeech = ToStringField(GrammarListCode, secondaryPos); // It's fine if secondaryPos is still null here
				}
			}
			lfSense.PhonologyNote = ToMultiText(fdoSense.PhonologyNote);
			if (fdoSense.PicturesOS != null)
			{
				lfSense.Pictures = new List<LfPicture>(fdoSense.PicturesOS.Select(FdoPictureToLfPicture));
				//Use the commented code for debugging into FdoPictureToLfPicture
				//
				//lfSense.Pictures = new List<LfPicture>();
				//foreach (var fdoPic in fdoSense.PicturesOS)
				//	lfSense.Pictures.Add(FdoPictureToLfPicture(fdoPic));
			}
			lfSense.SenseBibliography = ToMultiText(fdoSense.Bibliography);
			lfSense.SenseRestrictions = ToMultiText(fdoSense.Restrictions);

			if (fdoSense.ReversalEntriesRC != null)
			{
				IEnumerable<string> reversalEntries = fdoSense.ReversalEntriesRC.Select(fdoReversalEntry => fdoReversalEntry.LongName);
				lfSense.ReversalEntries = LfStringArrayField.FromStrings(reversalEntries);
			}
			lfSense.ScientificName = LfMultiText.FromSingleITsString(fdoSense.ScientificName, ServiceLocator.WritingSystemFactory);
			lfSense.SemanticDomain = ToStringArrayField(SemDomListCode, fdoSense.SemanticDomainsRC);
			lfSense.SemanticsNote = ToMultiText(fdoSense.SemanticsNote);
			// fdoSense.SensesOS; // Not mapped because LF doesn't handle subsenses. TODO: When LF handles subsenses, map this one.
			lfSense.SenseType = ToStringField(SenseTypeListCode, fdoSense.SenseTypeRA);
			lfSense.SociolinguisticsNote = ToMultiText(fdoSense.SocioLinguisticsNote);
			if (fdoSense.Source != null)
			{
				lfSense.Source = LfMultiText.FromSingleITsString(fdoSense.Source, ServiceLocator.WritingSystemFactory);
			}
			lfSense.Status = ToStringArrayField(StatusListCode, fdoSense.StatusRA);
			lfSense.Usages = ToStringArrayField(UsageTypeListCode, fdoSense.UsageTypesRC);


			/* Fields not mapped because it doesn't make sense to map them (e.g., Hvo, backreferences, etc):
			fdoSense.AllOwnedObjects;
			fdoSense.AllSenses;
			fdoSense.Cache;
			fdoSense.CanDelete;
			fdoSense.ChooserNameTS;
			fdoSense.ClassID;
			fdoSense.ClassName;
			fdoSense.Entry;
			fdoSense.EntryID;
			fdoSense.FullReferenceName;
			fdoSense.GetDesiredMsaType();
			fdoSense.Hvo;
			fdoSense.ImportResidue;
			fdoSense.IndexInOwner;
			fdoSense.IsValidObject;
			fdoSense.LexSenseReferences;
			fdoSense.LongNameTSS;
			fdoSense.ObjectIdName;
			fdoSense.OwnedObjects;
			fdoSense.Owner;
			fdoSense.OwningFlid;
			fdoSense.OwnOrd;
			fdoSense.ReferringObjects;
			fdoSense.ReversalNameForWs(wsVern);
			fdoSense.SandboxMSA; // Set-only property
			fdoSense.Self;
			fdoSense.Services;
			fdoSense.ShortName;
			fdoSense.ShortNameTSS;
			fdoSense.SortKey;
			fdoSense.SortKey2;
			fdoSense.SortKey2Alpha;
			fdoSense.SortKeyWs;
			fdoSense.VariantFormEntryBackRefs;
			fdoSense.VisibleComplexFormBackRefs;
			*/

			/* Fields not mapped because LanguageForge doesn't handle that data:
			fdoSense.AppendixesRC;
			fdoSense.ComplexFormEntries;
			fdoSense.ComplexFormsNotSubentries;
			fdoSense.DoNotPublishInRC;
			fdoSense.Subentries;
			fdoSense.ThesaurusItemsRC;
			fdoSense.LiftResidue;
			fdoSense.LexSenseOutline;
			fdoSense.PublishIn;
			*/

			BsonDocument customFieldsAndGuids = _convertCustomField.GetCustomFieldsForThisCmObject(fdoSense, "senses", ListConverters);
			BsonDocument customFieldsBson = customFieldsAndGuids["customFields"].AsBsonDocument;
			BsonDocument customFieldGuids = customFieldsAndGuids["customFieldGuids"].AsBsonDocument;

			// TODO: Role Views only set on initial clone
			if (LfProject.IsInitialClone)
			{
				;
			}

			// If custom field was deleted in Flex, delete config here


			lfSense.CustomFields = customFieldsBson;
			lfSense.CustomFieldGuids = customFieldGuids;

			return lfSense;
		}

		/// <summary>
		/// Convert FDO example LF example.
		/// </summary>
		/// <returns>LF example
		/// <param name="fdoExample">Fdo example.</param>
		private LfExample FdoExampleToLfExample(ILexExampleSentence fdoExample)
		{
			var lfExample = new LfExample();

			ILgWritingSystem AnalysisWritingSystem = ServiceLocator.LanguageProject.DefaultAnalysisWritingSystem;
			ILgWritingSystem VernacularWritingSystem = ServiceLocator.LanguageProject.DefaultVernacularWritingSystem;

			lfExample.Guid = fdoExample.Guid;
			lfExample.Sentence = ToMultiText(fdoExample.Example);
			lfExample.Reference = LfMultiText.FromSingleITsString(fdoExample.Reference, ServiceLocator.WritingSystemFactory);
			// ILexExampleSentence fields we currently do not convert:
			// fdoExample.DoNotPublishInRC;
			// fdoExample.LiftResidue;
			// fdoExample.PublishIn;

			// NOTE: Currently, LanguageForge only stores one translation per example, whereas FDO can store
			// multiple translations with (possibly) different statuses (as freeform strings, like "old", "updated",
			// "needs fixing"...). Until LanguageForge acquires a data model where translations are stored in a list,
			// we will save only the first translation (if any) to Mongo. We also save the GUID so that the Mongo->FDO
			// direction will know which ICmTranslation object to update with any changes.
			// TODO: Once LF improves its data model for translations, persist all of them instead of just the first.
			foreach (ICmTranslation translation in fdoExample.TranslationsOC.Take(1))
			{
				lfExample.Translation = ToMultiText(translation.Translation);
				lfExample.TranslationGuid = translation.Guid;
			}

			BsonDocument customFieldsAndGuids = _convertCustomField.GetCustomFieldsForThisCmObject(fdoExample, "examples", ListConverters);
			BsonDocument customFieldsBson = customFieldsAndGuids["customFields"].AsBsonDocument;
			BsonDocument customFieldGuids = customFieldsAndGuids["customFieldGuids"].AsBsonDocument;

			lfExample.CustomFields = customFieldsBson;
			lfExample.CustomFieldGuids = customFieldGuids;
			return lfExample;
		}

		private LfPicture FdoPictureToLfPicture(ICmPicture fdoPicture)
		{
			var result = new LfPicture();
			result.Caption = ToMultiText(fdoPicture.Caption);
			if ((fdoPicture.PictureFileRA != null) && (!string.IsNullOrEmpty(fdoPicture.PictureFileRA.InternalPath)))
			{
				result.FileName = FdoPictureFilenameToLfPictureFilename(fdoPicture.PictureFileRA.InternalPath);
			}
			result.Guid = fdoPicture.Guid;
			// Unmapped ICmPicture fields include:
			// fdoPicture.Description;
			// fdoPicture.LayoutPos;
			// fdoPicture.LocationMax;
			// fdoPicture.LocationMin;
			// fdoPicture.LocationRangeType;
			// fdoPicture.ScaleFactor;
			return result;
		}

		private static string FdoPictureFilenameToLfPictureFilename(string fdoInternalFilename)
		{
			// Remove "Pictures" directory from internal path name
			// If the incoming internal path doesn't begin with "Pictures", then preserve the full external path.
			return Regex.Replace(fdoInternalFilename.Normalize(System.Text.NormalizationForm.FormC), @"^Pictures[/\\]", "");
		}

		/// <summary>
		/// Converts FDO writing systems to LF input systems
		/// </summary>
		/// <returns>The list of LF input systems.</returns>
		private Dictionary<string, LfInputSystemRecord> FdoWsToLfWs()
		{
			// Using var here so that we'll stay compatible with both FW 8 and 9 (the type of these two lists changed between 8 and 9).
			var vernacularWSList = ServiceLocator.LanguageProject.CurrentVernacularWritingSystems;
			var analysisWSList = ServiceLocator.LanguageProject.CurrentAnalysisWritingSystems;

			var lfWsList = new Dictionary<string, LfInputSystemRecord>();
			foreach (var fdoWs in ServiceLocator.LanguageProject.AllWritingSystems)
			{
				var lfWs = new LfInputSystemRecord()
				{
					//These are for current libpalaso with SIL Writing Systems.
					// TODO: handle legacy WS definition
					Abbreviation = fdoWs.Abbreviation,
					IsRightToLeft = fdoWs.RightToLeftScript,
					LanguageName = fdoWs.LanguageName,
					#if FW8_COMPAT
					Tag = fdoWs.Id,
					#else
					Tag = fdoWs.LanguageTag,
					#endif
					VernacularWS = vernacularWSList.Contains(fdoWs),
					AnalysisWS = analysisWSList.Contains(fdoWs)
				};

				#if FW8_COMPAT
				lfWsList.Add(fdoWs.Id, lfWs);
				#else
				lfWsList.Add(fdoWs.LanguageTag, lfWs);
				#endif
			}
			return lfWsList;
		}

		private ConvertFdoToMongoOptionList ConvertOptionListFromFdo(ILfProject project, string listCode, ICmPossibilityList fdoOptionList, bool updateMongoList = true)
		{
			LfOptionList lfExistingOptionList = Connection.GetLfOptionListByCode(project, listCode);
			var converter = new ConvertFdoToMongoOptionList(lfExistingOptionList, _wsEn, listCode, Logger, ServiceLocator.WritingSystemFactory);
			LfOptionList lfChangedOptionList = converter.PrepareOptionListUpdate(fdoOptionList);
			if (updateMongoList)
				Connection.UpdateRecord(project, lfChangedOptionList, listCode);
			return new ConvertFdoToMongoOptionList(lfChangedOptionList, _wsEn, listCode, Logger, ServiceLocator.WritingSystemFactory);
		}
	}
}

