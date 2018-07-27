﻿// Copyright (c) 2016 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LfMerge.Core.DataConverters.CanonicalSources;
using LfMerge.Core.FieldWorks;
using LfMerge.Core.LanguageForge.Model;
using LfMerge.Core.Logging;
using LfMerge.Core.MongoConnector;
using LfMerge.Core.Reporting;
using LfMerge.Core.Settings;
using SIL.Progress;
using SIL.CoreImpl; // For TsStringUtils, IWritingSystemManager
using SIL.FieldWorks.Common.COMInterfaces; // For ITsString
using SIL.FieldWorks.FDO;
using SIL.FieldWorks.FDO.DomainServices;
using SIL.FieldWorks.FDO.Infrastructure;

namespace LfMerge.Core.DataConverters
{
	public class ConvertMongoToFdoLexicon
	{
		private LfMergeSettings Settings { get; set; }
		private ILfProject LfProject { get; set; }
		private FwProject FwProject { get; set; }
		private FdoCache Cache { get; set; }
		private IProgress Progress { get; set; }
		private FwServiceLocatorCache ServiceLocator { get; set; }
		private ILogger Logger { get; set; }
		private IMongoConnection Connection { get; set; }
		private MongoProjectRecord ProjectRecord { get; set; }
		private EntryCounts EntryCounts { get; set; }

		//private IEnumerable<ILgWritingSystem> _analysisWritingSystems;
		//private IEnumerable<ILgWritingSystem> _vernacularWritingSystems;

		private int _wsEn;
		private ConvertMongoToFdoCustomField _convertCustomField;

		// Shorter names to use in this class since MagicStrings.LfOptionListCodeForGrammaticalInfo
		// (etc.) are real mouthfuls
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

		private IDictionary<string, ConvertMongoToFdoOptionList> ListConverters;

		private ICmPossibility _freeTranslationType; // Used in LfExampleToFdoExample(), but cached here

		public ConvertMongoToFdoLexicon(LfMergeSettings settings, ILfProject lfproject, ILogger logger, IProgress progress,
			IMongoConnection connection, MongoProjectRecord projectRecord, EntryCounts entryCounts)
		{
			EntryCounts = entryCounts;
			Settings = settings;
			LfProject = lfproject;
			Logger = logger;
			Progress = progress;
			Connection = connection;
			ProjectRecord = projectRecord;

			FwProject = LfProject.FieldWorksProject;
			Cache = FwProject.Cache;
			ServiceLocator = FwProject.ServiceLocator;
			// These writing system search orders will be used in BestStringAndWsFromMultiText and related functions
			//_analysisWritingSystems = ServiceLocator.LanguageProject.CurrentAnalysisWritingSystems;
			//_vernacularWritingSystems = ServiceLocator.LanguageProject.CurrentVernacularWritingSystems;

			ListConverters = new Dictionary<string, ConvertMongoToFdoOptionList>();
			ListConverters[GrammarListCode] = PrepareOptionListConverter(GrammarListCode);
			ListConverters[SemDomListCode] = PrepareOptionListConverter(SemDomListCode);
			ListConverters[AcademicDomainListCode] = PrepareOptionListConverter(AcademicDomainListCode);
			ListConverters[LocationListCode] = PrepareOptionListConverter(LocationListCode);
			ListConverters[UsageTypeListCode] = PrepareOptionListConverter(UsageTypeListCode);
			ListConverters[SenseTypeListCode] = PrepareOptionListConverter(SenseTypeListCode);
			ListConverters[AnthroCodeListCode] = PrepareOptionListConverter(AnthroCodeListCode);
			ListConverters[StatusListCode] = PrepareOptionListConverter(StatusListCode);

			_wsEn = ServiceLocator.WritingSystemFactory.GetWsFromStr("en");

			// Once we allow LanguageForge to create optionlist items with "canonical" values (parts of speech, semantic domains, etc.), replace the code block
			// above with this one (that provides TWO parameters to PrepareOptionListConverter)
			#if false
			ListConverters[GrammarListCode] = PrepareOptionListConverter(GrammarListCode, ServiceLocator.LanguageProject.PartsOfSpeechOA);
			ListConverters[SemDomListCode] = PrepareOptionListConverter(SemDomListCode, ServiceLocator.LanguageProject.SemanticDomainListOA);
			ListConverters[AcademicDomainListCode] = PrepareOptionListConverter(AcademicDomainListCode, ServiceLocator.LanguageProject.LexDbOA.DomainTypesOA);
			ListConverters[LocationListCode] = PrepareOptionListConverter(LocationListCode, ServiceLocator.LanguageProject.LocationsOA);
			ListConverters[UsageTypeListCode] = PrepareOptionListConverter(UsageTypeListCode, ServiceLocator.LanguageProject.LexDbOA.UsageTypesOA);
			ListConverters[SenseTypeListCode] = PrepareOptionListConverter(SenseTypeListCode, ServiceLocator.LanguageProject.LexDbOA.SenseTypesOA);
			ListConverters[AnthroCodeListCode] = PrepareOptionListConverter(AnthroCodeListCode, ServiceLocator.LanguageProject.AnthroListOA);
			ListConverters[StatusListCode] = PrepareOptionListConverter(StatusListCode, ServiceLocator.LanguageProject.StatusOA);
			#endif

			if (ServiceLocator.LanguageProject != null && ServiceLocator.LanguageProject.TranslationTagsOA != null)
			{
				_freeTranslationType = ServiceLocator.ObjectRepository.GetObject(LangProjectTags.kguidTranFreeTranslation)
					as ICmPossibility;
				if (_freeTranslationType == null) // Shouldn't happen, but let's have a fallback possibility
					_freeTranslationType = ServiceLocator.LanguageProject.TranslationTagsOA.PossibilitiesOS.FirstOrDefault();
			}
		}

		private ConvertMongoToFdoOptionList PrepareOptionListConverter(string listCode)
		{
			LfOptionList optionListToConvert = Connection.GetLfOptionListByCode(LfProject, listCode);
			return new ConvertMongoToFdoOptionList(GetInstance<ICmPossibilityRepository>(),
				optionListToConvert, Logger, CanonicalOptionListSource.Create(listCode));
		}

		// Once we allow LanguageForge to create optionlist items with "canonical" values (parts of speech, semantic domains, etc.), replace the function
		// above (that takes ONE parameter) with this one (that takes TWO parameters)
		#if false
		public ConvertMongoToFdoOptionList PrepareOptionListConverter(string listCode, ICmPossibilityList parentList)
		{
			LfOptionList optionListToConvert = Connection.GetLfOptionListByCode(LfProject, listCode);
			return new ConvertMongoToFdoOptionList(GetInstance<ICmPossibilityRepository>(), optionListToConvert, Logger, parentList, _wsEn, CanonicalOptionListSource.Create(listCode));
		}
		#endif

		public void RunConversion()
		{
			Logger.Notice("MongoToFdo: Converting lexicon for project {0}", LfProject.ProjectCode);
			Logger.Debug("Running \"fake\" MtFComments, should see comments show up below:");
			var entryObjectIdToGuidMappings = Connection.GetGuidsByObjectIdForCollection(LfProject, MagicStrings.LfCollectionNameForLexicon);
			EntryCounts.Reset();
			// Update writing systems from project config input systems.  Won't commit till the end
			UndoableUnitOfWorkHelper.DoUsingNewOrCurrentUOW("undo", "redo", Cache.ActionHandlerAccessor, () =>
				LfWsToFdoWs(ProjectRecord.InputSystems));

			// Set English ws handle again in case it changed
			_wsEn = ServiceLocator.WritingSystemFactory.GetWsFromStr("en");

			_convertCustomField = new ConvertMongoToFdoCustomField(Cache, ServiceLocator, Logger, _wsEn);

			IEnumerable<LfLexEntry> lexicon = GetLexicon(LfProject);
			UndoableUnitOfWorkHelper.DoUsingNewOrCurrentUOW("undo", "redo", Cache.ActionHandlerAccessor, () =>
				{
					#if false  // Once we allow LanguageForge to create optionlist items with "canonical" values (parts of speech, semantic domains, etc.), uncomment this block
					foreach (ConvertMongoToFdoOptionList converter in ListConverters.Values)
					{
						converter.UpdateFdoOptionListFromLf(ProjectRecord.InterfaceLanguageCode);
					}
					#endif
					foreach (LfLexEntry lfEntry in lexicon)
						LfLexEntryToFdoLexEntry(lfEntry);
				});
			// Comment conversion gets run AFTER lexicon conversion, so that any comments on new entries are handled correctly in FW
			var commCvtr = new ConvertMongoToFdoComments(Connection, LfProject, Logger, Progress);
			commCvtr.RunConversion(entryObjectIdToGuidMappings);
			if (Settings.CommitWhenDone)
				Cache.ActionHandlerAccessor.Commit();
		}

		// Shorthand for getting an instance from the cache's service locator
		private T GetInstance<T>() where T : class
		{
			return ServiceLocator.GetInstance<T>();
		}

		private IEnumerable<LfLexEntry> GetLexicon(ILfProject project)
		{
			return Connection.GetRecords<LfLexEntry>(project, MagicStrings.LfCollectionNameForLexicon);
		}

		/// <summary>
		/// Converts the list of LF input systems and adds them to FDO writing systems
		/// </summary>
		/// <param name="lfWsList">List of LF input systems.</param>
		private void LfWsToFdoWs(Dictionary<string, LfInputSystemRecord> lfWsList)
		{
			// Between FW 8.2 and 9, a few classes and interfaces were renamed. The ones most relevant here are
			// IWritingSystemManager (interface was removed and replaced with the WritingSystemManager concrete class),
			// and PalasoWritingSystem which was replaced with CoreWritingSystemDefinition. Since their internals
			// didn't change much (and the only changes were in areas we don't access), we can use a simple compiler
			// define to choose the type of the wsm variable here (and the ws variable later) and we're fine.
			// HOWEVER, if the code inside #if...#endif blocks starts to grow, this is not an ideal solution. A better
			// solution if the code grows complex will be to write several classes to the same interface, each of which
			// can deal with one particular version of FW or FDO. Register them all with Autofac with a way to choose among them
			// (http://docs.autofac.org/en/stable/register/registration.html#selection-of-an-implementation-by-parameter-value)
			// and then, at runtime, we can instantiate the particular class that's needed for dealing with *this* FW project.
			//
			// But for now, these #if...#endif blocks are enough. - 2016-03 RM
#if FW8_COMPAT
			// Note that we can't use ILgWritingSystemFactory here, because it doesn't have some methods we need later on.
			IWritingSystemManager wsManager = ServiceLocator.WritingSystemManager;
#else
			WritingSystemManager wsManager = ServiceLocator.WritingSystemManager;
#endif
			if (wsManager == null)
			{
				Logger.Error("Failed to find the writing system manager");
				return;
			}

			string vernacularLanguageCode = ProjectRecord.LanguageCode;
			// TODO: Split the inside of this foreach() out into its own function
			foreach (var lfWs in lfWsList.Values)
			{
#if FW8_COMPAT
				IWritingSystem ws;
#else
				CoreWritingSystemDefinition ws;
#endif

				// It would be nice to call this to add to both analysis and vernacular WS.
				// But we need the flexibility of bringing in LF WS properties.
				/*
				if (!WritingSystemServices.FindOrCreateSomeWritingSystem(
					Cache, null, lfWs.Tag, true, true, out ws))
				{
					// Could neither find NOR create a writing system, probably because the tag was malformed? Log it and move on.
					Logger.Warning("Failed to find or create an FDO writing system corresponding to tag {0}. Is it malformed?", lfWs.Tag);
					continue;
				}
				*/

				// TODO: It might be possible to rewrite this code to NOT rely on TryGet() after all, in which case we could
				// use the ILgWritingSystemFactory interface and remove one point of FW 8-to-9 API incompatibility.

				if (wsManager.TryGet(lfWs.Tag, out ws))
				{
					ws.Abbreviation = lfWs.Abbreviation;
					ws.RightToLeftScript = lfWs.IsRightToLeft;
					wsManager.Replace(ws);
				}
				else
				{
					ws = wsManager.Create(lfWs.Tag);
					ws.Abbreviation = lfWs.Abbreviation;
					ws.RightToLeftScript = lfWs.IsRightToLeft;
					wsManager.Set(ws);

					// LF doesn't distinguish between vernacular/analysis WS, so we'll
					// only assign the project language code to vernacular.
					// All other WS assigned to analysis.

					// TODO: What if our vernacular was Thai, but we added th-ipa? This logic needs to be a bit "fuzzier", really.
					if (lfWs.Tag.Equals(vernacularLanguageCode))
						ServiceLocator.LanguageProject.AddToCurrentVernacularWritingSystems(ws);
					else
						ServiceLocator.LanguageProject.AddToCurrentAnalysisWritingSystems(ws);

				}
			}
		}

		private Tuple<string, int> BestStringAndWsFromMultiText(LfMultiText input, bool isAnalysisField = true)
		{
			if (input == null) return null;
			if (input.Count == 0)
			{
				Logger.Warning("BestStringAndWsFromMultiText got a non-null multitext, but it was empty. Empty LF MultiText objects should be nulls in Mongo. Unfortunately, at this point in the code it's hard to know which multitext it was.");
				return null;
			}

			IEnumerable<ILgWritingSystem> wsesToSearch = isAnalysisField ?
				ServiceLocator.LanguageProject.AnalysisWritingSystems :
				ServiceLocator.LanguageProject.VernacularWritingSystems;
//			List<Tuple<int, string>> wsesToSearch = isAnalysisField ?
//				_analysisWsIdsAndNamesInSearchOrder :
//				_vernacularWsIdsAndNamesInSearchOrder;

			foreach (ILgWritingSystem ws in wsesToSearch)
			{
				LfStringField field;
				if (input.TryGetValue(ws.Id, out field) && field != null && !field.IsEmpty)
				{
//					Logger.Debug("Returning TsString from {0} for writing system {1}", field.Value, ws.Id);
					return new Tuple<string, int>(field.Value, ws.Handle);
				}
			}

			// Last-ditch option: just grab the first non-empty string we can find
			KeyValuePair<int, string> kv = input.WsIdAndFirstNonEmptyString(Cache);
			if (kv.Value == null) return null;
//			Logger.Debug("Returning first non-empty TsString from {0} for writing system with ID {1}",
//				kv.Value, kv.Key);
			return new Tuple<string, int>(kv.Value, kv.Key);
		}

		private ITsString BestTsStringFromMultiText(LfMultiText input, bool isAnalysisField = true)
		{
			Tuple<string, int> stringAndWsId = BestStringAndWsFromMultiText(input, isAnalysisField);
			if (stringAndWsId == null)
				return null;
			return ConvertMongoToFdoTsStrings.SpanStrToTsString(stringAndWsId.Item1, stringAndWsId.Item2, ServiceLocator.WritingSystemFactory);
		}

		private string BestStringFromMultiText(LfMultiText input, bool isAnalysisField = true)
		{
			Tuple<string, int> stringAndWsId = BestStringAndWsFromMultiText(input, isAnalysisField);
			if (stringAndWsId == null)
				return null;
			return stringAndWsId.Item1;
		}

		// This GetOrCreate() function takes an extra out parameter so we can correctly update
		// the entry counts in LfLexEntryToFdoLexEntry(). We don't update the counts here
		// because we don't yet know if the LF entry was deleted (in which case we wouldn't
		// want to update Added or Modified). The wantCreation parameter is there because if
		// the LF entry was deleted, we don't want to actually create the FDO entry (it would
		// just be immediately deleted again).
		private ILexEntry GetOrCreateEntryByGuid(Guid guid, bool wantCreation, out bool createdEntry)
		{
			ILexEntry result;
			createdEntry = false;
			if (!GetInstance<ILexEntryRepository>().TryGetObject(guid, out result))
			{
				if (wantCreation)
				{
					createdEntry = true;
					result = GetInstance<ILexEntryFactory>().Create(guid, ServiceLocator.LanguageProject.LexDbOA);
					// TODO: Consider changing this to the following:
					// var msa = new SandboxGenericMSA();
					// result = GetInstance<ILexEntryFactory>().Create(GetInstance<IMoMorphTypeRepository>().GetObject(MoMorphTypeTags.kguidMorphStem), lexemeFormTs, (ITsString) null, msa);
					// However, this creates an empty Sense object -- and we already set the morph type when we set the LexemeFormOA. That should be enough.

					// If we do make that change, then the function signature will become:
					// private ILexEntry GetOrCreateEntryByGuid(Guid guid, bool wantCreation, ITsString lexemeFormTs, out bool createdEntry)
				}
				else
					result = null;
			}
			return result;
		}

		private ILexExampleSentence GetOrCreateExampleByGuid(Guid guid, ILexSense owner)
		{
			ILexExampleSentence result;
			if (!GetInstance<ILexExampleSentenceRepository>().TryGetObject(guid, out result))
				result = GetInstance<ILexExampleSentenceFactory>().Create(guid, owner);
			return result;
		}

		/// <summary>
		/// Gets or create the FDO picture by GUID.
		/// </summary>
		/// <returns>The picture by GUID.</returns>
		/// <param name="guid">GUID.</param>
		/// <param name="owner">Owning sense</param>
		/// <param name="pictureName">Picture path name.</param>
		/// <param name="caption">Caption.</param>
		/// <param name="captionWs">Caption writing system.</param>
		private ICmPicture GetOrCreatePictureByGuid(Guid guid, ILexSense owner, string pictureName,
			string caption, int captionWs)
		{
			ICmPicture result;
			if (!GetInstance<ICmPictureRepository>().TryGetObject(guid, out result))
			{
				if (caption == null)
				{
					caption = "";
					captionWs = Cache.DefaultAnalWs;
				}
				ITsString captionTss = ConvertMongoToFdoTsStrings.SpanStrToTsString(caption, captionWs, ServiceLocator.WritingSystemFactory);
				result = GetInstance<ICmPictureFactory>().Create(guid);
				result.UpdatePicture(pictureName, captionTss, CmFolderTags.LocalPictures, captionWs);
				owner.PicturesOS.Add(result);
			}
			return result;
		}

		private ILexPronunciation GetOrCreatePronunciationByGuid(Guid guid, ILexEntry owner)
		{
			ILexPronunciation result;
			if (!GetInstance<ILexPronunciationRepository>().TryGetObject(guid, out result))
			{
				result = GetInstance<ILexPronunciationFactory>().Create();
				owner.PronunciationsOS.Add(result);
			}
			return result;
		}

		private ILexSense GetOrCreateSenseByGuid(Guid guid, ILexEntry owner)
		{
			ILexSense result;
			if (!GetInstance<ILexSenseRepository>().TryGetObject(guid, out result))
				result = GetInstance<ILexSenseFactory>().Create(guid, owner);
			return result;
		}

		private ICmTranslation FindOrCreateTranslationByGuid(Guid guid, ILexExampleSentence owner,
			ICmPossibility typeOfNewTranslation)
		{
			// If it's already in the owning list, use that object
			ICmTranslation result = owner.TranslationsOC.FirstOrDefault(t => t.Guid == guid);
			if (result != null)
				return result;
			// Does a translation with that GUID already exist elsewhere?
			if (GetInstance<ICmTranslationRepository>().TryGetObject(guid, out result))
			{
				// Move it "here". No formerOwner.Remove() needed since TranslationsOC.Add() takes care of that.
				owner.TranslationsOC.Add(result);
				return result;
			}
			// Not found anywhere: make a new one.
			return GetInstance<ICmTranslationFactory>().Create(owner, typeOfNewTranslation);
		}

		private IMoForm CreateOwnedLexemeForm(ILexEntry owner, string morphologyType)
		{
			// morphologyType is a string because that's how it's (currently, as of Nov 2015)
			// stored in LF's Mongo database.
			IMoForm result;
			Guid morphGuid;
			var stemFactory = GetInstance<IMoStemAllomorphFactory>();
			var affixFactory = GetInstance<IMoAffixAllomorphFactory>();
			// TODO: This list of hardcoded strings might belong in an enum, rather than here
			switch (morphologyType)
			{
			case "bound root":
			case "bound stem":
			case "root":
			case "stem":
			case "particle":
			case "phrase":
			case "discontiguous phrase":
			case "circumfix":
			// Also consider "stem" as the default in case of null or empty morph type
			case null:
			case "":
				morphGuid = MoMorphTypeTags.kguidMorphStem;
				break;

			// TODO: Decide if "phrase" and "discontiguous phrase" should be treated as stems the way FW's FindMorphType() function
			// does, or if we want to use MoMorphTypeTags.kguidMorphPhrase and MoMorphTypeTags.kguidMorphDiscontiguousPhrase
			// even though FieldWorks doesn't appear to use those.

			case "clitic":
				morphGuid = MoMorphTypeTags.kguidMorphClitic;
				break;
			case "proclitic":
				morphGuid = MoMorphTypeTags.kguidMorphProclitic;
				break;
			case "enclitic":
				morphGuid = MoMorphTypeTags.kguidMorphEnclitic;
				break;
			case "prefix":
			case "prefixing interfix":  // FDO's MorphServices prefers to consider "prefixing interfix" as a prefix
				morphGuid = MoMorphTypeTags.kguidMorphPrefix;
				break;
			case "infix":
			case "infixing interfix":  // FDO's MorphServices prefers to consider "infixing interfix" as an infix
				morphGuid = MoMorphTypeTags.kguidMorphInfix;
				break;
			case "suffix":
			case "suffixing interfix":  // FDO's MorphServices prefers to consider "suffixing interfix" as a suffix
				morphGuid = MoMorphTypeTags.kguidMorphSuffix;
				break;
			case "simulfix":
				morphGuid = MoMorphTypeTags.kguidMorphSimulfix;
				break;
			case "suprafix":
				morphGuid = MoMorphTypeTags.kguidMorphSuprafix;
				break;
			default:
				Logger.Warning("Unrecognized morphology type \"{0}\" in word {1}", morphologyType, owner.Guid);
				morphGuid = MoMorphTypeTags.kguidMorphStem;
				break;
			}
			if (morphGuid == MoMorphTypeTags.kguidMorphStem)
				result = stemFactory.Create();
			else
				result = affixFactory.Create();
			owner.LexemeFormOA = result;  // MUST do this assignment *before* assigning result.MorphTypeRA, otherwise FDO throws a NullReferenceException
			result.MorphTypeRA = GetInstance<IMoMorphTypeRepository>().GetObject(morphGuid);
			Logger.Debug("Just set LexemeFormOA to {0}, with MorphType {1}", result == null ? "(null)" : result.ToString(), result == null ? "(null lexeme form)" : result.MorphTypeRA == null ? "(null morphtype)" : result.MorphTypeRA.ToString());
			return result;
		}

		private void LfLexEntryToFdoLexEntry(LfLexEntry lfEntry)
		{
			Guid guid = lfEntry.Guid ?? Guid.Empty;
			bool createdEntry = false;
			bool wantCreation = !lfEntry.IsDeleted;
			ILexEntry fdoEntry = GetOrCreateEntryByGuid(guid, wantCreation, out createdEntry);
			if (lfEntry.IsDeleted)
			{
				// LF entry deleted: delete the corresponding FDO entry
				if (fdoEntry == null)
					return; // No need to delete an FDO entry that doesn't exist
				if (fdoEntry.CanDelete)
				{
					if (createdEntry)
					{
						// This FDO entry, which was deleted in LF, was apparently "created" by FDO.
						// In reality, it was created just to be deleted, and we should optimize that away.
						Logger.Warning("LfMerge managed to create FDO entry {0} just to immediately delete it again. This is inefficient and should be fixed.",
							fdoEntry.Guid);
					}
					else
					{
						EntryCounts.Deleted++;
					}
					Logger.Info("MongoToFdo: Deleted FdoEntry {0} ({1})", guid, ConvertUtilities.EntryNameForDebugging(lfEntry));
					fdoEntry.Delete();
				}
				else
				{
					Logger.Warning("Problem: need to delete FDO entry {0}, but its CanDelete flag is false.",
						fdoEntry.Guid);
				}
				return; // Don't set fields on a deleted entry
			}
			// Has LF entry changed since last time we set FDO values?
			if (lfEntry.AuthorInfo.ModifiedDate.ToLocalTime() == fdoEntry.DateModified)
			{
				if (createdEntry)
				{
					// Entry was created in LF, so we need to create & populate it in FDO. Don't skip it.
				}
				else
				{
					// No changes detected since last time, so we won't change the FDO entry
					return;
				}
			}

			// Fields in order by lfEntry property, except for Senses and CustomFields, which are handled at the end
			SetMultiStringFrom(fdoEntry.CitationForm, lfEntry.CitationForm);

			// DateModified and DateCreated can be confusing, because LF and FDO are doing two different
			// things with them. In FDO, there is just one DateModified and one DateCreated; simple. But
			// in LF, there is an AuthorInfo record as well, which contains its own ModifiedDate and CreatedDate
			// fields. (Note the word order: there's LfEntry.DateCreated, and LfEntry.AuthorInfo.CreatedDate).

			// The conversion we have chosen to use is: AuthorInfo will correspond to FDO. So FDO.DateCreated
			// becomes AuthorInfo.CreatedDate, and FDO.DateModified becomes AuthorInfo.ModifiedDate. The two
			// fields on the LF entry will instead refer to when the *Mongo record* was created or modified,
			// and the LfEntry.DateCreated and LfEntry.DateModified fields will never be put into FDO.

			// Use AuthorInfo for dates as this should always reflect user changes (Mongo or FDO)
			// Weirdly, FDO expects Dates to be in LOCAL time, not UTC.
			if (lfEntry.AuthorInfo != null)
			{
				fdoEntry.DateCreated = lfEntry.AuthorInfo.CreatedDate.ToLocalTime();
				fdoEntry.DateModified = lfEntry.AuthorInfo.ModifiedDate.ToLocalTime();
			}

			SetMultiStringFrom(fdoEntry.Bibliography, lfEntry.EntryBibliography);
			SetMultiStringFrom(fdoEntry.Restrictions, lfEntry.EntryRestrictions);
			SetEtymologyFields(fdoEntry, lfEntry);
			SetLexeme(fdoEntry, lfEntry);
			// fdoEntry.LIFTid = lfEntry.LiftId; // TODO: Figure out how to handle this one.
			SetMultiStringFrom(fdoEntry.LiteralMeaning, lfEntry.LiteralMeaning);
			SetMultiStringFrom(fdoEntry.Comment, lfEntry.Note);
			SetPronunciation(fdoEntry, lfEntry);
			SetMultiStringFrom(fdoEntry.SummaryDefinition, lfEntry.SummaryDefinition);
			// TODO: Do something like the following line (can't do exactly that because PrimaryMorphType is read-only)
			// fdoEntry.PrimaryMorphType = new PossibilityListConverter(fdoEntry.PrimaryMorphType.OwningList).GetByName(lfEntry.MorphologyType) as IMoMorphType;

			/* LfLexEntry fields not mapped:
			lfEntry.Environments // Don't know how to handle this one. TODO: Research it.
			lfEntry.LiftId // TODO: Figure out how to handle this one. In fdoEntry, it's a constructed value.
			lfEntry.MercurialSha; // Skip: We don't update this until we've committed to the Mercurial repo
			*/

			// lfEntry.Senses -> fdoEntry.SensesOS
			SetFdoListFromLfList(fdoEntry, fdoEntry.SensesOS, lfEntry.Senses, LfSenseToFdoSense);

			_convertCustomField.SetCustomFieldsForThisCmObject(fdoEntry, "entry", lfEntry.CustomFields,
				lfEntry.CustomFieldGuids);

			// If we got this far, we either created or modified this entry
			if (createdEntry)
				EntryCounts.Added++;
			else
				EntryCounts.Modified++;

			Logger.Info("MongoToFdo: {0} FdoEntry {1} ({2})", createdEntry ? "Created" : "Modified", guid, ConvertUtilities.EntryNameForDebugging(lfEntry));
		}

		private void LfExampleToFdoExample(LfExample lfExample, ILexSense owner)
		{
			Guid guid = lfExample.Guid ?? Guid.Empty;
			if (guid == Guid.Empty)
				guid = GuidFromLiftId(lfExample.LiftId);
			ILexExampleSentence fdoExample = GetOrCreateExampleByGuid(guid, owner);
			// Ignoring lfExample.AuthorInfo.CreatedDate;
			// Ignoring lfExample.AuthorInfo.ModifiedDate;
			// Ignoring lfExample.ExampleId; // TODO: is this different from a LIFT ID?
			SetMultiStringFrom(fdoExample.Example, lfExample.Sentence);
//			Logger.Debug("FDO Example just got set to {0} for GUID {1} and HVO {2}",
//				ConvertFdoToMongoTsStrings.SafeTsStringText(fdoExample.Example.BestAnalysisVernacularAlternative),
//				fdoExample.Guid,
//				fdoExample.Hvo
//			);
			fdoExample.Reference = BestTsStringFromMultiText(lfExample.Reference);
			ICmTranslation t = FindOrCreateTranslationByGuid(lfExample.TranslationGuid, fdoExample,
				_freeTranslationType);
			SetMultiStringFrom(t.Translation, lfExample.Translation);
			// Ignoring t.Status since LF won't touch it

			_convertCustomField.SetCustomFieldsForThisCmObject(fdoExample, "examples",
				lfExample.CustomFields, lfExample.CustomFieldGuids);
		}

		/// <summary>
		/// Converts LF picture into FDO picture.  Internal FDO pictures will need to have the
		/// directory path "Pictures/" prepended to the filename.  Externally linked picture names
		/// won't be modifed.
		/// </summary>
		/// <param name="lfPicture">Lf picture.</param>
		/// <param name="owner">Owning sense.</param>
		private void LfPictureToFdoPicture(LfPicture lfPicture, ILexSense owner)
		{
			if (lfPicture == null || lfPicture.FileName == null)
				return;  // Do nothing if there's no picture to convert
			Guid guid = lfPicture.Guid ?? Guid.Empty;
			int captionWs = Cache.DefaultAnalWs;
			string caption = "";
			if (lfPicture.Caption != null)
			{
				KeyValuePair<int, string> kv = lfPicture.Caption.WsIdAndFirstNonEmptyString(Cache);
				captionWs = kv.Key;
				caption = kv.Value;
			}

			// FDO expects internal pictures in a certain path.  If an external path already
			// exists, leave it alone.
			string pictureName = lfPicture.FileName;
			Regex regex = new Regex(@"[/\\]");
			const string fdoPicturePath = "Pictures/";
			string picturePath = regex.Match(pictureName).Success ? pictureName :
				string.Format("{0}{1}", fdoPicturePath, pictureName);

			ICmPicture fdoPicture = GetOrCreatePictureByGuid(guid, owner, picturePath, caption, captionWs);
			// FDO currently only allows one caption to be created with the picture, so set the
			// other captions afterwards
			SetMultiStringFrom(fdoPicture.Caption, lfPicture.Caption);
			// Ignoring fdoPicture.Description and other fdoPicture fields since LF won't touch them
		}

		private void LfSenseToFdoSense(LfSense lfSense, ILexEntry owner)
		{
			Guid guid = lfSense.Guid ?? Guid.Empty;
			if (guid == Guid.Empty)
				guid = GuidFromLiftId(lfSense.LiftId);
			ILexSense fdoSense = GetOrCreateSenseByGuid(guid, owner);

			// Set the Guid on the LfSense object, so we can later track it for deletion purposes
			// (see LfEntryToFdoEntry)
			lfSense.Guid = fdoSense.Guid;

			ListConverters[AcademicDomainListCode].UpdatePossibilitiesFromStringArray(fdoSense.DomainTypesRC,
				lfSense.AcademicDomains);
			ListConverters[AnthroCodeListCode].UpdatePossibilitiesFromStringArray(fdoSense.AnthroCodesRC,
				lfSense.AnthropologyCategories);
			SetMultiStringFrom(fdoSense.AnthroNote, lfSense.AnthropologyNote);
			// Ignoring lfSense.AuthorInfo.CreatedDate;
			// Ignoring lfSense.AuthorInfo.ModifiedDate;
			SetMultiStringFrom(fdoSense.Definition, lfSense.Definition);
			SetMultiStringFrom(fdoSense.DiscourseNote, lfSense.DiscourseNote);
			SetMultiStringFrom(fdoSense.EncyclopedicInfo, lfSense.EncyclopedicNote);
			SetMultiStringFrom(fdoSense.GeneralNote, lfSense.GeneralNote);
			SetMultiStringFrom(fdoSense.Gloss, lfSense.Gloss);
			SetMultiStringFrom(fdoSense.GrammarNote, lfSense.GrammarNote);
			// fdoSense.LIFTid = lfSense.LiftId; // Read-only property in FDO Sense, doesn't make
			// sense to set it. TODO: Is that correct?
			IPartOfSpeech pos = ConvertPos(lfSense.PartOfSpeech, lfSense);
			if (pos != null)
			{
				IPartOfSpeech secondaryPos = ConvertPos(lfSense.SecondaryPartOfSpeech, lfSense); // Only used in derivational affixes, will be null otherwise
				if (fdoSense.MorphoSyntaxAnalysisRA == null)
				{
					// If we've got a brand-new fdoSense object, we'll need to create a new MSA for it.
					// That's what the SandboxGenericMSA class is for: assigning it to the SandboxMSA
					// member of fdoSense will automatically create an MSA of the correct class. Handy!
					MsaType msaType = fdoSense.GetDesiredMsaType();
					SandboxGenericMSA sandboxMsa = SandboxGenericMSA.Create(msaType, pos);
					if (secondaryPos != null)
						sandboxMsa.SecondaryPOS = secondaryPos;
					fdoSense.SandboxMSA = sandboxMsa;
				}
				else
				{
					ConvertMongoToFdoPartsOfSpeech.SetPartOfSpeech(fdoSense.MorphoSyntaxAnalysisRA,
						pos, secondaryPos, Logger); // It's fine if secondaryPos is null
				}
			}
			SetMultiStringFrom(fdoSense.PhonologyNote, lfSense.PhonologyNote);
			// fdoSense.ReversalEntriesRC = lfSense.ReversalEntries; // TODO: More complex than
			// that. Handle it correctly. Maybe.
			fdoSense.ScientificName = BestTsStringFromMultiText(lfSense.ScientificName);
			ListConverters[SemDomListCode].UpdatePossibilitiesFromStringArray(fdoSense.SemanticDomainsRC,
				lfSense.SemanticDomain);
			SetMultiStringFrom(fdoSense.SemanticsNote, lfSense.SemanticsNote);
			SetMultiStringFrom(fdoSense.Bibliography, lfSense.SenseBibliography);

			// lfSense.SenseId; // TODO: What do I do with this one?
			fdoSense.ImportResidue = BestTsStringFromMultiText(lfSense.SenseImportResidue);

			SetMultiStringFrom(fdoSense.Restrictions, lfSense.SenseRestrictions);
			fdoSense.SenseTypeRA = ListConverters[SenseTypeListCode].FromStringField(lfSense.SenseType);
			SetMultiStringFrom(fdoSense.SocioLinguisticsNote, lfSense.SociolinguisticsNote);
			fdoSense.Source = BestTsStringFromMultiText(lfSense.Source);
			fdoSense.StatusRA = ListConverters[StatusListCode].FromStringArrayFieldWithOneCase(lfSense.Status);
			ListConverters[UsageTypeListCode].UpdatePossibilitiesFromStringArray(fdoSense.UsageTypesRC,
				lfSense.Usages);

			// lfSense.Examples -> fdoSense.ExamplesOS
			SetFdoListFromLfList(fdoSense, fdoSense.ExamplesOS, lfSense.Examples, LfExampleToFdoExample);

			// lfSense.Pictures -> fdoSense.PicturesOS
			SetFdoListFromLfList(fdoSense, fdoSense.PicturesOS, lfSense.Pictures, LfPictureToFdoPicture);

			_convertCustomField.SetCustomFieldsForThisCmObject(fdoSense, "senses", lfSense.CustomFields,
				lfSense.CustomFieldGuids);
		}

		// Given a list of LF objects that are "owned" by a parent object (e.g., LfSense.Examples)
		// and the corresponding FDO list (e.g., ILexSense.ExamplesOS), convert the LF list to FDO
		// (with the conversion function passed in as a parameter).
		// Then go through the FDO list and look for any objects that were NOT in the LF list
		// (identifying them by their Guid) and delete them, because their absence from LF means
		// that they were deleted in LF at some point in the past. In addition to the two lists,
		// the FDO parent object is also required, because the conversion needs it as a parameter.
		//
		// This is a pattern that we use several times in the Mongo->FDO conversion
		// (LfSense.Examples, LfSense.Pictures, LfEntry.Senses), so this function exists to
		// generalize that pattern.
		private void SetFdoListFromLfList<TLfChild, TFdoParent, TFdoChild>(
			TFdoParent fdoParent,
			IList<TFdoChild> fdoChildList,
			IList<TLfChild> lfChildList,
			Action<TLfChild, TFdoParent> convertAction
		)
			where TFdoParent : ICmObject
			where TFdoChild : ICmObject
			where TLfChild : IHasNullableGuid
		{
			var guidsFoundInLf = new HashSet<Guid>();
			var objectsToDeleteFromFdo = new HashSet<TFdoChild>();
			foreach (TLfChild lfChild in lfChildList)
			{
				convertAction(lfChild, fdoParent);
				if (lfChild.Guid != null)
					guidsFoundInLf.Add(lfChild.Guid.Value);
			}
			// Any FDO objects that DON'T have a corresponding Guid in LF should now be deleted
			foreach (TFdoChild fdoChild in fdoChildList)
			{
				if (!guidsFoundInLf.Contains(fdoChild.Guid))
					// Don't delete them yet, as that could change the list we're iterating over
					objectsToDeleteFromFdo.Add(fdoChild);
			}
			// Now it's safe to delete them
			foreach (TFdoChild fdoChildToDelete in objectsToDeleteFromFdo)
				fdoChildToDelete.Delete();
		}

		private Guid GuidFromLiftId(string liftId)
		{
			Guid result;
			if (String.IsNullOrEmpty(liftId))
				return default(Guid);
			if (Guid.TryParse(liftId, out result))
				return result;
			int pos = liftId.LastIndexOf('_');
			if (Guid.TryParse(liftId.Substring(pos+1), out result))
				return result;
			return default(Guid);
		}

		/// <summary>
		/// Sets all writing systems in an FDO multi string from a LanguageForge MultiText field.
		/// Destination is first parameter, like the order of an assignment statement.
		/// </summary>
		/// <param name="dest">FDO multi string whose values will be set.</param>
		/// <param name="source">Source of multistring values.</param>
		private void SetMultiStringFrom(IMultiStringAccessor dest, LfMultiText source)
		{
			if (source == null)
				ClearMultiString(dest);
			else
				source.WriteToFdoMultiString(dest, ServiceLocator.WritingSystemManager);
		}

		/// <summary>
		/// Clears all text in all writing systems in an FDO MultiString object
		/// </summary>
		private void ClearMultiString(IMultiStringAccessor multiString)
		{
			if (multiString == null) return;
			foreach (int wsId in multiString.AvailableWritingSystemIds)
				multiString.set_String(wsId, string.Empty);
		}

		private void SetEtymologyFields(ILexEntry fdoEntry, LfLexEntry lfEntry)
		{
#if DBVERSION_7000068
			var fdoEtymology = fdoEntry.EtymologyOA;
#else
			var fdoEtymology = fdoEntry.EtymologyOS.FirstOrDefault();
#endif
			if ((lfEntry.Etymology        == null || lfEntry.Etymology       .IsEmpty) &&
			    (lfEntry.EtymologyComment == null || lfEntry.EtymologyComment.IsEmpty) &&
			    (lfEntry.EtymologyGloss   == null || lfEntry.EtymologyGloss  .IsEmpty) &&
			    (lfEntry.EtymologySource  == null || lfEntry.EtymologySource .IsEmpty))
			{
				if (fdoEtymology == null)
					return; // Don't delete an Etymology object if there was none already
#if DBVERSION_7000068
				fdoEtymology.Delete();
#else
				fdoEntry.EtymologyOS.First().Delete();
#endif
				return;
			}
			if (fdoEtymology == null)
			{
				fdoEtymology = GetInstance<ILexEtymologyFactory>().Create();
#if DBVERSION_7000068
				fdoEntry.EtymologyOA = fdoEtymology;
#else
				fdoEntry.EtymologyOS.Add(fdoEtymology);
#endif
			}

			SetMultiStringFrom(fdoEtymology.Form, lfEntry.Etymology);
			SetMultiStringFrom(fdoEtymology.Comment, lfEntry.EtymologyComment);
			SetMultiStringFrom(fdoEtymology.Gloss, lfEntry.EtymologyGloss);
			if (lfEntry.EtymologySource != null)
#if DBVERSION_7000068
				fdoEtymology.Source = BestStringFromMultiText(lfEntry.EtymologySource);
#else
				SetMultiStringFrom(fdoEtymology.LanguageNotes, lfEntry.EtymologySource);
#endif
		}

		private void SetLexeme(ILexEntry fdoEntry, LfLexEntry lfEntry)
		{
			IMoForm fdoLexeme = fdoEntry.LexemeFormOA;
			if (fdoLexeme == null)
				fdoLexeme = CreateOwnedLexemeForm(fdoEntry, lfEntry.MorphologyType); // Also sets owning field on fdoEntry
			if (lfEntry.Lexeme == null || lfEntry.Lexeme.IsEmpty)
			{
				ClearMultiString(fdoLexeme.Form);
				return;
			}
			// TODO: Fold the "ClearMultiString" logic into SetMultiStringFrom so that it will *reset* any MultiString fields that aren't there in LF.
			// TODO: But first, check if that's necessary.
			SetMultiStringFrom(fdoLexeme.Form, lfEntry.Lexeme);
		}

		private void SetPronunciation(ILexEntry fdoEntry, LfLexEntry lfEntry)
		{
			// var fdoPronunciation = GetOrCreatePronunciationByGuid(lfEntry.PronunciationGuid, fdoEntry);
			ILexPronunciation fdoPronunciation = fdoEntry.PronunciationsOS.FirstOrDefault();
			if ((lfEntry.Pronunciation == null || lfEntry.Pronunciation.IsEmpty) &&
			    (lfEntry.CvPattern     == null || lfEntry.CvPattern    .IsEmpty) &&
			    (lfEntry.Tone          == null || lfEntry.Tone         .IsEmpty) &&
			    (lfEntry.Location      == null || lfEntry.Location     .IsEmpty))
			{
				// No pronunication at all in LF: either there was never one, or we deleted it
				if (fdoPronunciation == null)
					return;  // There was never a pronunciation; we're fine
				else
					fdoEntry.PronunciationsOS.First().Delete();
					return;
			}
			if (fdoPronunciation == null)
			{
				fdoPronunciation = GetInstance<ILexPronunciationFactory>().Create();
				fdoEntry.PronunciationsOS.Add(fdoPronunciation);
			}
			fdoPronunciation.CVPattern = BestTsStringFromMultiText(lfEntry.CvPattern);
			fdoPronunciation.Tone = BestTsStringFromMultiText(lfEntry.Tone);
			SetMultiStringFrom(fdoPronunciation.Form, lfEntry.Pronunciation);
			fdoPronunciation.LocationRA =
				(ICmLocation)ListConverters[LocationListCode].FromStringField(lfEntry.Location);
			// Not handling fdoPronunciation.MediaFilesOS. TODO: At some point we may want to handle
			// media files as well.
			// Not handling fdoPronunciation.LiftResidue
		}

		private IPartOfSpeech ConvertPos(LfStringField source, LfSense owner)
		{
			return ListConverters[GrammarListCode].FromStringField(source) as IPartOfSpeech;
		}
	}
}
