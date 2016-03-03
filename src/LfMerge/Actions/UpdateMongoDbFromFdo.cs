﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LfMerge.DataConverters;
using LfMerge.FieldWorks;
using LfMerge.LanguageForge.Model;
using LfMerge.Logging;
using LfMerge.MongoConnector;
using LfMerge.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using SIL.CoreImpl;
using SIL.FieldWorks.Common.COMInterfaces;
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.Infrastructure;

namespace LfMerge.Actions
{
	public class UpdateMongoDbFromFdo: Action
	{
		protected override ProcessingState.SendReceiveStates StateForCurrentAction
		{
			get { return ProcessingState.SendReceiveStates.UPDATING; }
		}

		private FdoCache _cache;
		private IFdoServiceLocator _servLoc;
		private IMongoConnection _connection;

		private IFwMetaDataCacheManaged _fdoMetaData;
		private ICmPossibilityList _fdoPartsOfSpeech;
		private LfOptionList _lfGrammar;
		private CustomFieldConverter _converter;

		public static bool InitialClone { get; set; }

		public UpdateMongoDbFromFdo(LfMergeSettingsIni settings, ILogger logger, IMongoConnection conn) : base(settings, logger)
		{
			_connection = conn;
		}

		//private List<int> customFieldIds;

		protected override void DoRun(ILfProject project)
		{
			Logger.Notice("MongoDbFromFdo: starting");
			FwProject fwProject = project.FieldWorksProject;
			if (fwProject == null)
			{
				Logger.Error("Can't find FieldWorks project {0}", project.FwProjectCode);
				return;
			}
			Logger.Notice("MongoDbFromFdo: getting cache");
			_cache = fwProject.Cache;
			if (_cache == null)
			{
				Logger.Error("Can't find cache for FieldWorks project {0}", project.FwProjectCode);
				return;
			}
			Logger.Notice("MongoDbFromFdo: serviceLocator");
			_servLoc = _cache.ServiceLocator;
			if (_servLoc == null)
			{
				Logger.Error("Can't find service locator for FieldWorks project {0}", project.FwProjectCode);
				return;
			}
			Logger.Notice("MongoDbFromFdo: LexEntryRepository");
			ILexEntryRepository repo = _servLoc.GetInstance<ILexEntryRepository>();
			if (repo == null)
			{
				Logger.Error("Can't find LexEntry repository for FieldWorks project {0}", project.FwProjectCode);
				return;
			}
			Logger.Notice("MongoDbFromFdo: MetaDataCacheAccessor");
			_fdoMetaData = (IFwMetaDataCacheManaged)_cache.MetaDataCacheAccessor;
			if (_fdoMetaData == null)
			{
				Logger.Warning("Don't have access to the FW metadata; custom fields may fail!");
			}

			// Reconcile writing systems from FDO and Mongo
			Dictionary<string, LfInputSystemRecord> lfWsList = FdoWsToLfWs();
			string VernacularWs = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultVernWs);
			string AnalysisWs = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultAnalWs);
			Logger.Debug("Vernacular {0}, Analysis {1}", VernacularWs, AnalysisWs);
			_connection.SetInputSystems(project, lfWsList, InitialClone, VernacularWs, AnalysisWs);

			_converter = new CustomFieldConverter(_cache);

			// Update grammar before updating entries, so we can reference GUIDs in parts of speech
			_fdoPartsOfSpeech = _cache.LanguageProject.PartsOfSpeechOA;
			_lfGrammar = ConvertOptionListFromFdo(project, MagicStrings.LfOptionListCodeForGrammaticalInfo, _fdoPartsOfSpeech);

			// Sense type
			var fdoSenseType = _cache.LanguageProject.LexDbOA.SenseTypesOA;
			ConvertOptionListFromFdo(project, MagicStrings.LfOptionListCodeForSenseTypes, fdoSenseType);

			foreach (ILexEntry fdoEntry in repo.AllInstances())
			{
				LfLexEntry lfEntry = FdoLexEntryToLfLexEntry(fdoEntry);
				Logger.Info("Populated LfEntry {0}", lfEntry.Guid);
				_connection.UpdateRecord(project, lfEntry);
			}
			InitialClone = false;

			IEnumerable<LfLexEntry> lfEntries = _connection.GetRecords<LfLexEntry>(project, MagicStrings.LfCollectionNameForLexicon);
			List<Guid> entryGuidsToRemove = new List<Guid>();
			foreach (LfLexEntry lfEntry in lfEntries)
			{
				if (lfEntry.Guid == null)
					continue;
				if (!_servLoc.ObjectRepository.IsValidObjectId(lfEntry.Guid.Value))
					entryGuidsToRemove.Add(lfEntry.Guid.Value);
			}
			foreach (Guid guid in entryGuidsToRemove)
			{
				_connection.RemoveRecord(project, guid);

			}
		}

		private LfMultiText ToMultiText(IMultiAccessorBase fdoMultiString)
		{
			if (fdoMultiString == null) return null;
				return LfMultiText.FromFdoMultiString(fdoMultiString, _servLoc.WritingSystemManager);
		}

		static public LfMultiText ToMultiText(IMultiAccessorBase fdoMultiString, WritingSystemManager fdoWritingSystemManager)
		{
			if ((fdoMultiString == null) || (fdoWritingSystemManager == null)) return null;
			return LfMultiText.FromFdoMultiString(fdoMultiString, fdoWritingSystemManager);
		}

		private string ToStringOrNull(ITsString iTsString)
		{
			if (iTsString == null) return null;
			return iTsString.Text;
		}

		/// <summary>
		/// Converts FDO writing systems to LF input systems
		/// </summary>
		/// <returns>The list of LF input systems.</returns>
		private Dictionary<string, LfInputSystemRecord> FdoWsToLfWs()
		{
			IList<CoreWritingSystemDefinition> vernacularWSList = _cache.LangProject.CurrentVernacularWritingSystems;
			IList<CoreWritingSystemDefinition> analysisWSList = _cache.LangProject.CurrentAnalysisWritingSystems;

			var lfWsList = new Dictionary<string, LfInputSystemRecord>();
			foreach (var fdoWs in _cache.LangProject.AllWritingSystems)
			{
				var lfWs = new LfInputSystemRecord()
				{
					//These are for current libpalaso with SIL Writing Systems.
					// TODO: handle legacy WS definition
					Abbreviation = fdoWs.Abbreviation,
					IsRightToLeft = fdoWs.RightToLeftScript,
					LanguageName = fdoWs.LanguageName,
					Tag = fdoWs.LanguageTag,
					VernacularWS = vernacularWSList.Contains(fdoWs),
					AnalysisWS = analysisWSList.Contains(fdoWs)
				};

				lfWsList.Add(fdoWs.LanguageTag, lfWs);
			}
			return lfWsList;
		}

		private LfSense FdoSenseToLfSense(ILexSense fdoSense)
		{
			var lfSense = new LfSense();

			string VernacularWritingSystem = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultVernWs);
			string AnalysisWritingSystem = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultAnalWs);

			// TODO: Currently skipping subsenses. Figure out if we should include them or not.

			lfSense.Guid = fdoSense.Guid;
			lfSense.Gloss = ToMultiText(fdoSense.Gloss);
			lfSense.Definition = ToMultiText(fdoSense.Definition);

			// Fields below in alphabetical order by ILexSense property, except for Guid, Gloss and Definition
			lfSense.AcademicDomains = LfStringArrayField.FromPossibilityAbbrevs(fdoSense.DomainTypesRC);
			lfSense.AnthropologyCategories = LfStringArrayField.FromPossibilityAbbrevs(fdoSense.AnthroCodesRC);
			lfSense.AnthropologyNote = ToMultiText(fdoSense.AnthroNote);
			lfSense.DiscourseNote = ToMultiText(fdoSense.DiscourseNote);
			lfSense.EncyclopedicNote = ToMultiText(fdoSense.EncyclopedicInfo);
			if (fdoSense.ExamplesOS != null)
				lfSense.Examples = new List<LfExample>(fdoSense.ExamplesOS.Select(FdoExampleToLfExample));
			lfSense.GeneralNote = ToMultiText(fdoSense.GeneralNote);
			lfSense.GrammarNote = ToMultiText(fdoSense.GrammarNote);
			lfSense.LiftId = fdoSense.LIFTid;
			if (fdoSense.MorphoSyntaxAnalysisRA != null)
			{
				IPartOfSpeech secondaryPos = null; // Only used in derivational affixes
				IPartOfSpeech pos = PartOfSpeechConverter.FromMSA(fdoSense.MorphoSyntaxAnalysisRA, out secondaryPos);
				// TODO: Write helper function to take BestVernacularAnalysisAlternative and/or other
				// writing system alternatives, and simplify the process of getting a real string
				// (as opposed to a TsString) from it.
				int wsEn = _cache.WritingSystemFactory.GetWsFromStr("en");
				if (pos == null || pos.Abbreviation == null)
					lfSense.PartOfSpeech = null;
				else
					//lfSense.PartOfSpeech = LfStringField.FromString(ToStringOrNull(pos.Abbreviation.get_String(wsEn)));
					lfSense.PartOfSpeech = LfStringField.FromString(
						PartOfSpeechConverter.ToLfPosStringKey(pos, _lfGrammar, wsEn));
				if (secondaryPos == null || secondaryPos.Abbreviation == null)
					lfSense.SecondaryPartOfSpeech = null;
				else
					lfSense.SecondaryPartOfSpeech = LfStringField.FromString(
						PartOfSpeechConverter.ToLfPosStringKey(secondaryPos, _lfGrammar, wsEn));
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
			foreach (LfPicture picture in lfSense.Pictures)
			{
				// TODO: Remove this debugging foreach loop once we know pictures are working
				Logger.Debug("Picture with caption {0} and filename {1}", picture.Caption.FirstNonEmptyString(), picture.FileName);
			}
			lfSense.SenseBibliography = ToMultiText(fdoSense.Bibliography);
			lfSense.SensePublishIn = LfStringArrayField.FromPossibilityAbbrevs(fdoSense.PublishIn);
			lfSense.SenseRestrictions = ToMultiText(fdoSense.Restrictions);

			if (fdoSense.ReversalEntriesRC != null)
			{
				IEnumerable<string> reversalEntries = fdoSense.ReversalEntriesRC.Select(fdoReversalEntry => fdoReversalEntry.LongName);
				lfSense.ReversalEntries = LfStringArrayField.FromStrings(reversalEntries);
			}
			lfSense.ScientificName = LfMultiText.FromSingleITsStringMapping(AnalysisWritingSystem, fdoSense.ScientificName);
			lfSense.SemanticDomain = LfStringArrayField.FromPossibilityAbbrevs(fdoSense.SemanticDomainsRC);


			lfSense.SemanticsNote = ToMultiText(fdoSense.SemanticsNote);
			// fdoSense.SensesOS; // Not mapped because LF doesn't handle subsenses. TODO: When LF handles subsenses, map this one.
			if (fdoSense.SenseTypeRA != null)
				lfSense.SenseType = LfStringField.FromString(fdoSense.SenseTypeRA.NameHierarchyString);
			lfSense.SociolinguisticsNote = ToMultiText(fdoSense.SocioLinguisticsNote);
			if (fdoSense.Source != null)
			{
				lfSense.Source = LfMultiText.FromSingleITsStringMapping(VernacularWritingSystem, fdoSense.Source);
			}
			lfSense.Status = LfStringArrayField.FromSinglePossibilityAbbrev(fdoSense.StatusRA);
			lfSense.Usages = LfStringArrayField.FromPossibilityAbbrevs(fdoSense.UsageTypesRC);

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
			*/

			BsonDocument customFieldsAndGuids = _converter.CustomFieldsForThisCmObject(fdoSense, "senses");
			BsonDocument customFieldsBson = customFieldsAndGuids["customFields"].AsBsonDocument;
			BsonDocument customFieldGuids = customFieldsAndGuids["customFieldGuids"].AsBsonDocument;

			lfSense.CustomFields = customFieldsBson;
			lfSense.CustomFieldGuids = customFieldGuids;
			//Logger.Notice("Custom fields for this sense: {0}", lfSense.CustomFields);
			//Logger.Notice("Custom field GUIDs for this sense: {0}", lfSense.CustomFieldGuids);

			return lfSense;
		}

		private LfExample FdoExampleToLfExample(ILexExampleSentence fdoExample)
		{
			LfExample result = new LfExample();

			string VernacularWritingSystem = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultVernWs);

			result.Guid = fdoExample.Guid;
			result.ExamplePublishIn = LfStringArrayField.FromPossibilityAbbrevs(fdoExample.PublishIn);
			result.Sentence = ToMultiText(fdoExample.Example);
			result.Reference = LfMultiText.FromSingleITsStringMapping(VernacularWritingSystem, fdoExample.Reference);
			// ILexExampleSentence fields we currently do not convert:
			// fdoExample.DoNotPublishInRC;
			// fdoExample.LiftResidue;

			// NOTE: Currently, LanguageForge only stores one translation per example, whereas FDO can store
			// multiple translations with (possibly) different statuses (as freeform strings, like "old", "updated",
			// "needs fixing"...). Until LanguageForge acquires a data model where translations are stored in a list,
			// we will save only the first translation (if any) to Mongo. We also save the GUID so that the Mongo->FDO
			// direction will know which ICmTranslation object to update with any changes.
			// TODO: Once LF improves its data model for translations, persist all of them instead of just the first.
			foreach (ICmTranslation translation in fdoExample.TranslationsOC.Take(1))
			{
				result.Translation = ToMultiText(translation.Translation);
				result.TranslationGuid = translation.Guid;
			}

			BsonDocument customFieldsAndGuids = _converter.CustomFieldsForThisCmObject(fdoExample, "examples");
			BsonDocument customFieldsBson = customFieldsAndGuids["customFields"].AsBsonDocument;
			BsonDocument customFieldGuids = customFieldsAndGuids["customFieldGuids"].AsBsonDocument;

			result.CustomFields = customFieldsBson;
			result.CustomFieldGuids = customFieldGuids;
			//Logger.Notice("Custom fields for this example: {0}", result.CustomFields);
			//Logger.Notice("Custom field GUIDs for this example: {0}", result.CustomFieldGuids);
			return result;
		}

		private LfPicture FdoPictureToLfPicture(ICmPicture fdoPicture)
		{
			var result = new LfPicture();
			result.Caption = ToMultiText(fdoPicture.Caption);
			if ((fdoPicture.PictureFileRA != null) && (!string.IsNullOrEmpty(fdoPicture.PictureFileRA.InternalPath)))
			{
				// Remove "Pictures" directory from internal path name
				// If the incoming internal path doesn't begin with "Pictures", then preserve the full external path.
				Regex regex = new Regex(@"^Pictures[/\\]");
				result.FileName = regex.Replace(fdoPicture.PictureFileRA.InternalPath, "");
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

		private LfLexEntry FdoLexEntryToLfLexEntry(ILexEntry fdoEntry)
		{
			if (fdoEntry == null) return null;
			Logger.Notice("Converting FDO LexEntry with GUID {0}", fdoEntry.Guid);

			string AnalysisWritingSystem = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultAnalWs);
			// string VernacularWritingSystem = _servLoc.WritingSystemManager.GetStrFromWs(_cache.DefaultVernWs);

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
			lfEntry.CitationForm = ToMultiText(fdoEntry.CitationForm);
			lfEntry.Note = ToMultiText(fdoEntry.Comment);

			lfEntry.DateCreated = fdoEntry.DateCreated;
			lfEntry.DateModified = fdoEntry.DateModified;
			if (InitialClone)
			{
				var now = DateTime.Now;
				lfEntry.DateCreated = now;
				lfEntry.DateModified = now;
			}
			// TODO: In some LIFT imports, AuthorInfo.CreatedDate in Mongo doesn't match fdoEntry.DateCreated. Figure out why.
			if (lfEntry.AuthorInfo == null)
				lfEntry.AuthorInfo = new LfAuthorInfo();
			lfEntry.AuthorInfo.CreatedByUserRef = null;
			lfEntry.AuthorInfo.CreatedDate = fdoEntry.DateCreated;
			lfEntry.AuthorInfo.ModifiedByUserRef = null;
			lfEntry.AuthorInfo.ModifiedDate = fdoEntry.DateModified;

			ILexEtymology fdoEtymology = fdoEntry.EtymologyOA;
			if (fdoEtymology != null)
			{
				lfEntry.Etymology = ToMultiText(fdoEtymology.Form); // TODO: Check if ILexEtymology.Form is the right field here
				lfEntry.EtymologyComment = ToMultiText(fdoEtymology.Comment);
				lfEntry.EtymologyGloss = ToMultiText(fdoEtymology.Gloss);
				lfEntry.EtymologySource = LfMultiText.FromSingleStringMapping(AnalysisWritingSystem, fdoEtymology.Source);
				// fdoEtymology.LiftResidue not mapped
			}
			lfEntry.Guid = fdoEntry.Guid;
			lfEntry.LiftId = fdoEntry.LIFTid;
			lfEntry.LiteralMeaning = ToMultiText(fdoEntry.LiteralMeaning);
			if (fdoEntry.PrimaryMorphType != null) {
				lfEntry.MorphologyType = fdoEntry.PrimaryMorphType.NameHierarchyString;
			}
			// TODO: Once LF's data model is updated from a single pronunciation to an array of pronunciations, convert all of them instead of just the first. E.g.,
			//foreach (ILexPronunciation fdoPronunciation in fdoEntry.PronunciationsOS) { ... }
			if (fdoEntry.PronunciationsOS.Count > 0)
			{
				ILexPronunciation fdoPronunciation = fdoEntry.PronunciationsOS.First();
				lfEntry.PronunciationGuid = fdoPronunciation.Guid;
				lfEntry.Pronunciation = ToMultiText(fdoPronunciation.Form);
				lfEntry.CvPattern = LfMultiText.FromSingleITsStringMapping(AnalysisWritingSystem, fdoPronunciation.CVPattern);
				lfEntry.Tone = LfMultiText.FromSingleITsStringMapping(AnalysisWritingSystem, fdoPronunciation.Tone);
				// TODO: Map fdoPronunciation.MediaFilesOS properly (converting video to sound files if necessary)
				//lfEntry.Location = LfStringField.FromString(fdoPronunciation.LocationRA.AbbrAndName);
				lfEntry.Location = LfStringField.FromString(PossibilityListConverter.BestStringFrom(fdoPronunciation.LocationRA));
			}
			lfEntry.EntryRestrictions = ToMultiText(fdoEntry.Restrictions);
			if (lfEntry.Senses == null) // Shouldn't happen, but let's be careful
				lfEntry.Senses = new List<LfSense>();
			lfEntry.Senses.AddRange(fdoEntry.SensesOS.Select(FdoSenseToLfSense));
			lfEntry.SummaryDefinition = ToMultiText(fdoEntry.SummaryDefinition);

			BsonDocument customFieldsAndGuids = _converter.CustomFieldsForThisCmObject(fdoEntry, "entry");
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
			fdoEntry.CitationFormWithAffixType; // TODO: This one should maybe be mapped. Figure out how.
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

		private LfOptionList ConvertOptionListFromFdo(ILfProject project, string listCode, ICmPossibilityList fdoOptionList)
		{
			LfOptionList lfExistingOptionList = _connection
				.GetRecords<LfOptionList>(project, listCode)
				.FirstOrDefault(list => list.Code == listCode);
			int wsEn = _cache.WritingSystemFactory.GetWsFromStr("en");
			var converter = new ConvertOptionList(lfExistingOptionList, wsEn, listCode);
			LfOptionList lfChangedOptionList = converter.PrepareOptionListUpdate(fdoOptionList);
			_connection.UpdateRecord(project, lfChangedOptionList, listCode);
			return lfChangedOptionList;
		}

		protected override ActionNames NextActionName
		{
			get { return ActionNames.None; }
		}
	}
}
