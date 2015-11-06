﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)
using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LfMerge.LanguageForge.Model
{
	public class LfSense : LfFieldBase, System.ComponentModel.ISupportInitialize
	{
		// Metadata properties
		[BsonElement("id")]
		public string SenseId { get; set; } // Can't call this field "Id", or Mongo thinks it should be an ObjectId
		public Guid LiftId { get; set; }

		// Data properties
		public LfStringField PartOfSpeech { get; set; }
		public LfStringArrayField SemanticDomain { get; set; }
		public LfExample[] Examples { get; set; }
		public BsonDocument CustomFields { get; set; } // Mapped at runtime
		public LfAuthorInfo AuthorInfo { get; set; }
		public LfPicture[] Pictures { get; set; }
		public LfMultiText Definition { get; set; }
		public LfMultiText Gloss { get; set; }
		public LfMultiText ScientificName { get; set; }
		public LfMultiText AnthropologyNote { get; set; }
		public LfMultiText SenseBibliography { get; set; }
		public LfMultiText DiscourseNote { get; set; }
		public LfMultiText EncyclopedicNote { get; set; }
		public LfMultiText GeneralNote { get; set; }
		public LfMultiText GrammarNote { get; set; }
		public LfMultiText PhonologyNote { get; set; }
		public LfMultiText SenseRestrictions { get; set; }
		public LfMultiText SemanticsNote { get; set; }
		public string MorphologyType { get; set; }
		public LfMultiText SociolinguisticsNote { get; set; }
		public LfMultiText Source { get; set; }
		public LfMultiText SenseImportResidue { get; set; }
		public LfStringArrayField Usages { get; set; }
		public LfStringArrayField ReversalEntries { get; set; }
		public LfStringField SenseType { get; set; }
		public LfStringArrayField AcademicDomains { get; set; }
		public LfStringArrayField SensePublishIn { get; set; }
		public LfStringArrayField AnthropologyCategories { get; set; }
		public LfStringArrayField Status { get; set; }

		public void BeginInit() { }

		public void EndInit()
		{
			// Ensure array fields are arrays no matter what
			if (Examples == null)
				Examples = new LfExample[0];
			if (Pictures == null)
				Pictures = new LfPicture[0];
		}

	}
}

