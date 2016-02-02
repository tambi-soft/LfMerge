﻿// Copyright (c) 2015 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System.Collections.Generic;

namespace LfMerge
{
	public static class PartOfSpeechMasterList
	{
		public static readonly Dictionary<string, string> FlatPosAbbrevs = new Dictionary<string, string> {
			{ "30d07580-5052-4d91-bc24-469b8b2d7df9", "adj" },
			{ "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5", "adp" },
			{ "18f1b2b8-0ce3-4889-90e9-003fed6a969f", "post" },
			{ "923e5aed-d84a-48b0-9b7a-331c1336864a", "prep" },
			{ "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9", "adv" },
			{ "31616603-aadd-47af-a710-cb565fbbbd57", "clf" },
			{ "131209ac-b8f1-44aa-b4f0-9a983e3d5bad", "nclf" },
			{ "6e0682a7-efd4-43c9-b083-22c4ce245419", "conn" },
			{ "75ac4332-a445-4ce8-918e-b27b04073745", "coordconn" },
			{ "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7", "correlconn" },
			{ "09052f32-bf65-400a-8179-6a1c54ef30c9", "subordconn" },
			{ "a0a9906d-d987-42cb-8a65-f549462b16fc", "advlizer" },
			{ "8f7ba502-e7c9-4bc4-a881-b0cb1b630466", "comp" },
			{ "c5f222a3-e1aa-4250-b196-d94f0eb0d47b", "rel" },
			{ "6df1c8ee-5530-4180-99e8-be2afab0f60d", "det" },
			{ "af3f65de-b0d5-4243-a196-53b67bd6a4df", "art" },
			{ "92ab3e14-e1af-4e7f-8492-e37a1f386e3f", "def" },
			{ "d07c625d-ff8b-4c4e-99be-e32b2924626e", "indf" },
			{ "093264d7-06c3-42e1-bc4d-5a965ce63887", "dem" },
			{ "a4a759b4-5a10-4d7a-80a3-accf5bd840b1", "quant" },
			{ "e680330e-2b41-4bec-b96b-514743c47cae", "num" },
			{ "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd", "cardnum" },
			{ "b330eb7d-f43d-4531-846e-5cd5820851ad", "distrnum" },
			{ "1c030229-affa-4729-9773-878100c1fd28", "multipnum" },
			{ "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd", "ordnum" },
			{ "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04", "partnum" },
			{ "25b2ef8c-d87e-4868-898c-8f5375afeeb3", "existmrkr" },
			{ "cc60cb18-5067-442b-a740-d3b913b2610a", "expl" },
			{ "d32dff62-4117-4762-a887-96478406769b", "interj" },
			{ "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5", "n" },
			{ "085360ef-166c-4324-a5c4-5f4d8eabf75a", "nom" },
			{ "18b523c8-7dca-4361-93d8-6d039335f26d", "ger" },
			{ "a8d31dff-1cdd-4bc7-ab3a-befef67711c2", "nprop" },
			{ "584412e9-3a96-4251-99e2-438f1394432e", "subs" },
			{ "83fdec31-e15b-40e7-bcd2-69134c5a0fcd", "ptcp" },
			{ "6e758bbc-2b40-427d-b79f-bf14a7c96c6a", "prt" },
			{ "c6853f58-a74c-483e-9494-732002a7ab5b", "nomprt" },
			{ "07455e91-118a-4d7d-848c-39bedd355a3d", "q" },
			{ "12435333-c423-43d7-acf9-6028e3d20a42", "verbprt" },
			{ "f1ac9eab-5f8c-41cf-b234-e53405aaaac5", "prenoun" },
			{ "4e676ad2-542d-461d-9d78-1dbcb55ec456", "preverb" },
			{ "a4fc78d6-7591-4fb3-8edd-82f10ae3739d", "pro-form" },
			{ "e5e7d0cb-36c5-497d-831c-cb614e283d7c", "interrog" },
			{ "e28bb667-fcaa-4c6e-944b-9b90683a2570", "pro-adj" },
			{ "d599dd69-b445-4627-a7f3-b9647abdb905", "pro-adv" },
			{ "a3274cfd-225f-45fd-8851-a7b1a1e1037a", "pro" },
			{ "b5d9ab85-fa93-4d6a-892b-837efb299ef7", "indfpro" },
			{ "2fad3a89-47d7-472a-a8cc-270e7e3e0239", "pers" },
			{ "98f5507f-bf51-43e4-8e08-e066c36c6d6e", "emph" },
			{ "64e3b502-90f5-4df0-9212-65f6e5c96c30", "poss" },
			{ "605c54f9-a81f-4bca-8d8c-a6fb08c29676", "refl" },
			{ "d9a90c6c-3575-4937-bfaf-b3585a1954a9", "recp" },
			{ "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a", "relpro" },
			{ "86ff66f6-0774-407a-a0dc-3eeaf873daf7", "v" },
			{ "55f2a00e-5f07-4ace-8a44-8794ed1a38a8", "cop" },
			{ "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc", "vd" },
			{ "4459ff09-9ee0-4b50-8787-ae40fd76d3b7", "vi" },
			{ "54712931-442f-42d5-8634-f12bd2e310ce", "vt" }
		};

		public static readonly Dictionary<string, string> HierarchicalPosAbbrevs = new Dictionary<string, string> {
			{ "30d07580-5052-4d91-bc24-469b8b2d7df9", "adj" },
			{ "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5", "adp" },
			{ "18f1b2b8-0ce3-4889-90e9-003fed6a969f", "adposition\ufffcpost" },
			{ "923e5aed-d84a-48b0-9b7a-331c1336864a", "adposition\ufffcprep" },
			{ "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9", "adv" },
			{ "31616603-aadd-47af-a710-cb565fbbbd57", "clf" },
			{ "131209ac-b8f1-44aa-b4f0-9a983e3d5bad", "classifier\ufffcnclf" },
			{ "6e0682a7-efd4-43c9-b083-22c4ce245419", "conn" },
			{ "75ac4332-a445-4ce8-918e-b27b04073745", "connective\ufffccoordconn" },
			{ "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7", "connective\ufffccoordinating connective\ufffccorrelconn" },
			{ "09052f32-bf65-400a-8179-6a1c54ef30c9", "connective\ufffcsubordconn" },
			{ "a0a9906d-d987-42cb-8a65-f549462b16fc", "connective\ufffcsubordinating connective\ufffcadvlizer" },
			{ "8f7ba502-e7c9-4bc4-a881-b0cb1b630466", "connective\ufffcsubordinating connective\ufffccomp" },
			{ "c5f222a3-e1aa-4250-b196-d94f0eb0d47b", "connective\ufffcsubordinating connective\ufffcrel" },
			{ "6df1c8ee-5530-4180-99e8-be2afab0f60d", "det" },
			{ "af3f65de-b0d5-4243-a196-53b67bd6a4df", "determiner\ufffcart" },
			{ "92ab3e14-e1af-4e7f-8492-e37a1f386e3f", "determiner\ufffcarticle\ufffcdef" },
			{ "d07c625d-ff8b-4c4e-99be-e32b2924626e", "determiner\ufffcarticle\ufffcindf" },
			{ "093264d7-06c3-42e1-bc4d-5a965ce63887", "determiner\ufffcdem" },
			{ "a4a759b4-5a10-4d7a-80a3-accf5bd840b1", "determiner\ufffcquant" },
			{ "e680330e-2b41-4bec-b96b-514743c47cae", "determiner\ufffcquantifier\ufffcnum" },
			{ "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd", "determiner\ufffcquantifier\ufffcnumeral\ufffccardnum" },
			{ "b330eb7d-f43d-4531-846e-5cd5820851ad", "determiner\ufffcquantifier\ufffcnumeral\ufffcdistrnum" },
			{ "1c030229-affa-4729-9773-878100c1fd28", "determiner\ufffcquantifier\ufffcnumeral\ufffcmultipnum" },
			{ "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd", "determiner\ufffcquantifier\ufffcnumeral\ufffcordnum" },
			{ "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04", "determiner\ufffcquantifier\ufffcnumeral\ufffcpartnum" },
			{ "25b2ef8c-d87e-4868-898c-8f5375afeeb3", "existmrkr" },
			{ "cc60cb18-5067-442b-a740-d3b913b2610a", "expl" },
			{ "d32dff62-4117-4762-a887-96478406769b", "interj" },
			{ "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5", "n" },
			{ "085360ef-166c-4324-a5c4-5f4d8eabf75a", "noun\ufffcnom" },
			{ "18b523c8-7dca-4361-93d8-6d039335f26d", "noun\ufffcnominal\ufffcger" },
			{ "a8d31dff-1cdd-4bc7-ab3a-befef67711c2", "noun\ufffcnprop" },
			{ "584412e9-3a96-4251-99e2-438f1394432e", "noun\ufffcsubs" },
			{ "83fdec31-e15b-40e7-bcd2-69134c5a0fcd", "ptcp" },
			{ "6e758bbc-2b40-427d-b79f-bf14a7c96c6a", "prt" },
			{ "c6853f58-a74c-483e-9494-732002a7ab5b", "particle\ufffcnomprt" },
			{ "07455e91-118a-4d7d-848c-39bedd355a3d", "particle\ufffcq" },
			{ "12435333-c423-43d7-acf9-6028e3d20a42", "particle\ufffcverbprt" },
			{ "f1ac9eab-5f8c-41cf-b234-e53405aaaac5", "prenoun" },
			{ "4e676ad2-542d-461d-9d78-1dbcb55ec456", "preverb" },
			{ "a4fc78d6-7591-4fb3-8edd-82f10ae3739d", "pro-form" },
			{ "e5e7d0cb-36c5-497d-831c-cb614e283d7c", "pro-form\ufffcinterrog" },
			{ "e28bb667-fcaa-4c6e-944b-9b90683a2570", "pro-form\ufffcpro-adj" },
			{ "d599dd69-b445-4627-a7f3-b9647abdb905", "pro-form\ufffcpro-adv" },
			{ "a3274cfd-225f-45fd-8851-a7b1a1e1037a", "pro-form\ufffcpro" },
			{ "b5d9ab85-fa93-4d6a-892b-837efb299ef7", "pro-form\ufffcpronoun\ufffcindfpro" },
			{ "2fad3a89-47d7-472a-a8cc-270e7e3e0239", "pro-form\ufffcpronoun\ufffcpers" },
			{ "98f5507f-bf51-43e4-8e08-e066c36c6d6e", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcemph" },
			{ "64e3b502-90f5-4df0-9212-65f6e5c96c30", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcposs" },
			{ "605c54f9-a81f-4bca-8d8c-a6fb08c29676", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcrefl" },
			{ "d9a90c6c-3575-4937-bfaf-b3585a1954a9", "pro-form\ufffcpronoun\ufffcrecp" },
			{ "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a", "pro-form\ufffcpronoun\ufffcrelpro" },
			{ "86ff66f6-0774-407a-a0dc-3eeaf873daf7", "v" },
			{ "55f2a00e-5f07-4ace-8a44-8794ed1a38a8", "verb\ufffccop" },
			{ "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc", "verb\ufffcvd" },
			{ "4459ff09-9ee0-4b50-8787-ae40fd76d3b7", "verb\ufffcvi" },
			{ "54712931-442f-42d5-8634-f12bd2e310ce", "verb\ufffcvt" }
		};

		public static readonly Dictionary<string, string> FlatPosGuidsFromAbbrevs = new Dictionary<string, string> {
			{ "adj", "30d07580-5052-4d91-bc24-469b8b2d7df9" },
			{ "adp", "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5" },
			{ "post", "18f1b2b8-0ce3-4889-90e9-003fed6a969f" },
			{ "prep", "923e5aed-d84a-48b0-9b7a-331c1336864a" },
			{ "adv", "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9" },
			{ "clf", "31616603-aadd-47af-a710-cb565fbbbd57" },
			{ "nclf", "131209ac-b8f1-44aa-b4f0-9a983e3d5bad" },
			{ "conn", "6e0682a7-efd4-43c9-b083-22c4ce245419" },
			{ "coordconn", "75ac4332-a445-4ce8-918e-b27b04073745" },
			{ "correlconn", "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7" },
			{ "subordconn", "09052f32-bf65-400a-8179-6a1c54ef30c9" },
			{ "advlizer", "a0a9906d-d987-42cb-8a65-f549462b16fc" },
			{ "comp", "8f7ba502-e7c9-4bc4-a881-b0cb1b630466" },
			{ "rel", "c5f222a3-e1aa-4250-b196-d94f0eb0d47b" },
			{ "det", "6df1c8ee-5530-4180-99e8-be2afab0f60d" },
			{ "art", "af3f65de-b0d5-4243-a196-53b67bd6a4df" },
			{ "def", "92ab3e14-e1af-4e7f-8492-e37a1f386e3f" },
			{ "indf", "d07c625d-ff8b-4c4e-99be-e32b2924626e" },
			{ "dem", "093264d7-06c3-42e1-bc4d-5a965ce63887" },
			{ "quant", "a4a759b4-5a10-4d7a-80a3-accf5bd840b1" },
			{ "num", "e680330e-2b41-4bec-b96b-514743c47cae" },
			{ "cardnum", "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd" },
			{ "distrnum", "b330eb7d-f43d-4531-846e-5cd5820851ad" },
			{ "multipnum", "1c030229-affa-4729-9773-878100c1fd28" },
			{ "ordnum", "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd" },
			{ "partnum", "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04" },
			{ "existmrkr", "25b2ef8c-d87e-4868-898c-8f5375afeeb3" },
			{ "expl", "cc60cb18-5067-442b-a740-d3b913b2610a" },
			{ "interj", "d32dff62-4117-4762-a887-96478406769b" },
			{ "n", "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5" },
			{ "nom", "085360ef-166c-4324-a5c4-5f4d8eabf75a" },
			{ "ger", "18b523c8-7dca-4361-93d8-6d039335f26d" },
			{ "nprop", "a8d31dff-1cdd-4bc7-ab3a-befef67711c2" },
			{ "subs", "584412e9-3a96-4251-99e2-438f1394432e" },
			{ "ptcp", "83fdec31-e15b-40e7-bcd2-69134c5a0fcd" },
			{ "prt", "6e758bbc-2b40-427d-b79f-bf14a7c96c6a" },
			{ "nomprt", "c6853f58-a74c-483e-9494-732002a7ab5b" },
			{ "q", "07455e91-118a-4d7d-848c-39bedd355a3d" },
			{ "verbprt", "12435333-c423-43d7-acf9-6028e3d20a42" },
			{ "prenoun", "f1ac9eab-5f8c-41cf-b234-e53405aaaac5" },
			{ "preverb", "4e676ad2-542d-461d-9d78-1dbcb55ec456" },
			{ "pro-form", "a4fc78d6-7591-4fb3-8edd-82f10ae3739d" },
			{ "interrog", "e5e7d0cb-36c5-497d-831c-cb614e283d7c" },
			{ "pro-adj", "e28bb667-fcaa-4c6e-944b-9b90683a2570" },
			{ "pro-adv", "d599dd69-b445-4627-a7f3-b9647abdb905" },
			{ "pro", "a3274cfd-225f-45fd-8851-a7b1a1e1037a" },
			{ "indfpro", "b5d9ab85-fa93-4d6a-892b-837efb299ef7" },
			{ "pers", "2fad3a89-47d7-472a-a8cc-270e7e3e0239" },
			{ "emph", "98f5507f-bf51-43e4-8e08-e066c36c6d6e" },
			{ "poss", "64e3b502-90f5-4df0-9212-65f6e5c96c30" },
			{ "refl", "605c54f9-a81f-4bca-8d8c-a6fb08c29676" },
			{ "recp", "d9a90c6c-3575-4937-bfaf-b3585a1954a9" },
			{ "relpro", "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a" },
			{ "v", "86ff66f6-0774-407a-a0dc-3eeaf873daf7" },
			{ "cop", "55f2a00e-5f07-4ace-8a44-8794ed1a38a8" },
			{ "vd", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "vi", "4459ff09-9ee0-4b50-8787-ae40fd76d3b7" },
			{ "vt", "54712931-442f-42d5-8634-f12bd2e310ce" }
		};

		public static readonly Dictionary<string, string> HierarchicalPosGuidsFromAbbrevs = new Dictionary<string, string> {
			{ "adj", "30d07580-5052-4d91-bc24-469b8b2d7df9" },
			{ "adp", "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5" },
			{ "adposition\ufffcpost", "18f1b2b8-0ce3-4889-90e9-003fed6a969f" },
			{ "adposition\ufffcprep", "923e5aed-d84a-48b0-9b7a-331c1336864a" },
			{ "adv", "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9" },
			{ "clf", "31616603-aadd-47af-a710-cb565fbbbd57" },
			{ "classifier\ufffcnclf", "131209ac-b8f1-44aa-b4f0-9a983e3d5bad" },
			{ "conn", "6e0682a7-efd4-43c9-b083-22c4ce245419" },
			{ "connective\ufffccoordconn", "75ac4332-a445-4ce8-918e-b27b04073745" },
			{ "connective\ufffccoordinating connective\ufffccorrelconn", "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7" },
			{ "connective\ufffcsubordconn", "09052f32-bf65-400a-8179-6a1c54ef30c9" },
			{ "connective\ufffcsubordinating connective\ufffcadvlizer", "a0a9906d-d987-42cb-8a65-f549462b16fc" },
			{ "connective\ufffcsubordinating connective\ufffccomp", "8f7ba502-e7c9-4bc4-a881-b0cb1b630466" },
			{ "connective\ufffcsubordinating connective\ufffcrel", "c5f222a3-e1aa-4250-b196-d94f0eb0d47b" },
			{ "det", "6df1c8ee-5530-4180-99e8-be2afab0f60d" },
			{ "determiner\ufffcart", "af3f65de-b0d5-4243-a196-53b67bd6a4df" },
			{ "determiner\ufffcarticle\ufffcdef", "92ab3e14-e1af-4e7f-8492-e37a1f386e3f" },
			{ "determiner\ufffcarticle\ufffcindf", "d07c625d-ff8b-4c4e-99be-e32b2924626e" },
			{ "determiner\ufffcdem", "093264d7-06c3-42e1-bc4d-5a965ce63887" },
			{ "determiner\ufffcquant", "a4a759b4-5a10-4d7a-80a3-accf5bd840b1" },
			{ "determiner\ufffcquantifier\ufffcnum", "e680330e-2b41-4bec-b96b-514743c47cae" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffccardnum", "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcdistrnum", "b330eb7d-f43d-4531-846e-5cd5820851ad" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcmultipnum", "1c030229-affa-4729-9773-878100c1fd28" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcordnum", "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcpartnum", "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04" },
			{ "existmrkr", "25b2ef8c-d87e-4868-898c-8f5375afeeb3" },
			{ "expl", "cc60cb18-5067-442b-a740-d3b913b2610a" },
			{ "interj", "d32dff62-4117-4762-a887-96478406769b" },
			{ "n", "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5" },
			{ "noun\ufffcnom", "085360ef-166c-4324-a5c4-5f4d8eabf75a" },
			{ "noun\ufffcnominal\ufffcger", "18b523c8-7dca-4361-93d8-6d039335f26d" },
			{ "noun\ufffcnprop", "a8d31dff-1cdd-4bc7-ab3a-befef67711c2" },
			{ "noun\ufffcsubs", "584412e9-3a96-4251-99e2-438f1394432e" },
			{ "ptcp", "83fdec31-e15b-40e7-bcd2-69134c5a0fcd" },
			{ "prt", "6e758bbc-2b40-427d-b79f-bf14a7c96c6a" },
			{ "particle\ufffcnomprt", "c6853f58-a74c-483e-9494-732002a7ab5b" },
			{ "particle\ufffcq", "07455e91-118a-4d7d-848c-39bedd355a3d" },
			{ "particle\ufffcverbprt", "12435333-c423-43d7-acf9-6028e3d20a42" },
			{ "prenoun", "f1ac9eab-5f8c-41cf-b234-e53405aaaac5" },
			{ "preverb", "4e676ad2-542d-461d-9d78-1dbcb55ec456" },
			{ "pro-form", "a4fc78d6-7591-4fb3-8edd-82f10ae3739d" },
			{ "pro-form\ufffcinterrog", "e5e7d0cb-36c5-497d-831c-cb614e283d7c" },
			{ "pro-form\ufffcpro-adj", "e28bb667-fcaa-4c6e-944b-9b90683a2570" },
			{ "pro-form\ufffcpro-adv", "d599dd69-b445-4627-a7f3-b9647abdb905" },
			{ "pro-form\ufffcpro", "a3274cfd-225f-45fd-8851-a7b1a1e1037a" },
			{ "pro-form\ufffcpronoun\ufffcindfpro", "b5d9ab85-fa93-4d6a-892b-837efb299ef7" },
			{ "pro-form\ufffcpronoun\ufffcpers", "2fad3a89-47d7-472a-a8cc-270e7e3e0239" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcemph", "98f5507f-bf51-43e4-8e08-e066c36c6d6e" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcposs", "64e3b502-90f5-4df0-9212-65f6e5c96c30" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcrefl", "605c54f9-a81f-4bca-8d8c-a6fb08c29676" },
			{ "pro-form\ufffcpronoun\ufffcrecp", "d9a90c6c-3575-4937-bfaf-b3585a1954a9" },
			{ "pro-form\ufffcpronoun\ufffcrelpro", "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a" },
			{ "v", "86ff66f6-0774-407a-a0dc-3eeaf873daf7" },
			{ "verb\ufffccop", "55f2a00e-5f07-4ace-8a44-8794ed1a38a8" },
			{ "verb\ufffcvd", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "verb\ufffcvi", "4459ff09-9ee0-4b50-8787-ae40fd76d3b7" },
			{ "verb\ufffcvt", "54712931-442f-42d5-8634-f12bd2e310ce" }
		};

		public static readonly Dictionary<string, string> FlatPosGuids = new Dictionary<string, string> { // TODO: Rename this to FlatPosGuidsFromNames
			{ "adjective", "30d07580-5052-4d91-bc24-469b8b2d7df9" },
			{ "adposition", "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5" },
			{ "postposition", "18f1b2b8-0ce3-4889-90e9-003fed6a969f" },
			{ "preposition", "923e5aed-d84a-48b0-9b7a-331c1336864a" },
			{ "adverb", "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9" },
			{ "classifier", "31616603-aadd-47af-a710-cb565fbbbd57" },
			{ "noun classifier", "131209ac-b8f1-44aa-b4f0-9a983e3d5bad" },
			{ "connective", "6e0682a7-efd4-43c9-b083-22c4ce245419" },
			{ "coordinating connective", "75ac4332-a445-4ce8-918e-b27b04073745" },
			{ "correlative connective", "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7" },
			{ "subordinating connective", "09052f32-bf65-400a-8179-6a1c54ef30c9" },
			{ "adverbializer", "a0a9906d-d987-42cb-8a65-f549462b16fc" },
			{ "complementizer", "8f7ba502-e7c9-4bc4-a881-b0cb1b630466" },
			{ "relativizer", "c5f222a3-e1aa-4250-b196-d94f0eb0d47b" },
			{ "determiner", "6df1c8ee-5530-4180-99e8-be2afab0f60d" },
			{ "article", "af3f65de-b0d5-4243-a196-53b67bd6a4df" },
			{ "definite article", "92ab3e14-e1af-4e7f-8492-e37a1f386e3f" },
			{ "indefinite article", "d07c625d-ff8b-4c4e-99be-e32b2924626e" },
			{ "demonstrative", "093264d7-06c3-42e1-bc4d-5a965ce63887" },
			{ "quantifier", "a4a759b4-5a10-4d7a-80a3-accf5bd840b1" },
			{ "numeral", "e680330e-2b41-4bec-b96b-514743c47cae" },
			{ "cardinal numeral", "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd" },
			{ "distributive numeral", "b330eb7d-f43d-4531-846e-5cd5820851ad" },
			{ "multiplicative numeral", "1c030229-affa-4729-9773-878100c1fd28" },
			{ "ordinal numeral", "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd" },
			{ "partitive numeral", "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04" },
			{ "existential marker", "25b2ef8c-d87e-4868-898c-8f5375afeeb3" },
			{ "expletive", "cc60cb18-5067-442b-a740-d3b913b2610a" },
			{ "interjection", "d32dff62-4117-4762-a887-96478406769b" },
			{ "noun", "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5" },
			{ "nominal", "085360ef-166c-4324-a5c4-5f4d8eabf75a" },
			{ "gerund", "18b523c8-7dca-4361-93d8-6d039335f26d" },
			{ "proper noun", "a8d31dff-1cdd-4bc7-ab3a-befef67711c2" },
			{ "substantive", "584412e9-3a96-4251-99e2-438f1394432e" },
			{ "participle", "83fdec31-e15b-40e7-bcd2-69134c5a0fcd" },
			{ "particle", "6e758bbc-2b40-427d-b79f-bf14a7c96c6a" },
			{ "nominal particle", "c6853f58-a74c-483e-9494-732002a7ab5b" },
			{ "question particle", "07455e91-118a-4d7d-848c-39bedd355a3d" },
			{ "verbal particle", "12435333-c423-43d7-acf9-6028e3d20a42" },
			{ "prenoun", "f1ac9eab-5f8c-41cf-b234-e53405aaaac5" },
			{ "preverb", "4e676ad2-542d-461d-9d78-1dbcb55ec456" },
			{ "pro-form", "a4fc78d6-7591-4fb3-8edd-82f10ae3739d" },
			{ "interrogative pro-form", "e5e7d0cb-36c5-497d-831c-cb614e283d7c" },
			{ "pro-adjective", "e28bb667-fcaa-4c6e-944b-9b90683a2570" },
			{ "pro-adverb", "d599dd69-b445-4627-a7f3-b9647abdb905" },
			{ "pronoun", "a3274cfd-225f-45fd-8851-a7b1a1e1037a" },
			{ "indefinite pronoun", "b5d9ab85-fa93-4d6a-892b-837efb299ef7" },
			{ "personal pronoun", "2fad3a89-47d7-472a-a8cc-270e7e3e0239" },
			{ "emphatic pronoun", "98f5507f-bf51-43e4-8e08-e066c36c6d6e" },
			{ "possessive pronoun", "64e3b502-90f5-4df0-9212-65f6e5c96c30" },
			{ "reflexive pronoun", "605c54f9-a81f-4bca-8d8c-a6fb08c29676" },
			{ "reciprocal pronoun", "d9a90c6c-3575-4937-bfaf-b3585a1954a9" },
			{ "relative pronoun", "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a" },
			{ "verb", "86ff66f6-0774-407a-a0dc-3eeaf873daf7" },
			{ "copulative verb", "55f2a00e-5f07-4ace-8a44-8794ed1a38a8" },
			{ "ditransitive verb", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "bitransitive verb", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "intransitive verb", "4459ff09-9ee0-4b50-8787-ae40fd76d3b7" },
			{ "transitive verb", "54712931-442f-42d5-8634-f12bd2e310ce" }
		};

		public static readonly Dictionary<string, string> HierarchicalPosGuids = new Dictionary<string, string> { // TODO: Rename this to HierarchicalPosGuidsFromNames
			{ "adjective", "30d07580-5052-4d91-bc24-469b8b2d7df9" },
			{ "adposition", "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5" },
			{ "adposition\ufffcpostposition", "18f1b2b8-0ce3-4889-90e9-003fed6a969f" },
			{ "adposition\ufffcpreposition", "923e5aed-d84a-48b0-9b7a-331c1336864a" },
			{ "adverb", "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9" },
			{ "classifier", "31616603-aadd-47af-a710-cb565fbbbd57" },
			{ "classifier\ufffcnoun classifier", "131209ac-b8f1-44aa-b4f0-9a983e3d5bad" },
			{ "connective", "6e0682a7-efd4-43c9-b083-22c4ce245419" },
			{ "connective\ufffccoordinating connective", "75ac4332-a445-4ce8-918e-b27b04073745" },
			{ "connective\ufffccoordinating connective\ufffccorrelative connective", "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7" },
			{ "connective\ufffcsubordinating connective", "09052f32-bf65-400a-8179-6a1c54ef30c9" },
			{ "connective\ufffcsubordinating connective\ufffcadverbializer", "a0a9906d-d987-42cb-8a65-f549462b16fc" },
			{ "connective\ufffcsubordinating connective\ufffccomplementizer", "8f7ba502-e7c9-4bc4-a881-b0cb1b630466" },
			{ "connective\ufffcsubordinating connective\ufffcrelativizer", "c5f222a3-e1aa-4250-b196-d94f0eb0d47b" },
			{ "determiner", "6df1c8ee-5530-4180-99e8-be2afab0f60d" },
			{ "determiner\ufffcarticle", "af3f65de-b0d5-4243-a196-53b67bd6a4df" },
			{ "determiner\ufffcarticle\ufffcdefinite article", "92ab3e14-e1af-4e7f-8492-e37a1f386e3f" },
			{ "determiner\ufffcarticle\ufffcindefinite article", "d07c625d-ff8b-4c4e-99be-e32b2924626e" },
			{ "determiner\ufffcdemonstrative", "093264d7-06c3-42e1-bc4d-5a965ce63887" },
			{ "determiner\ufffcquantifier", "a4a759b4-5a10-4d7a-80a3-accf5bd840b1" },
			{ "determiner\ufffcquantifier\ufffcnumeral", "e680330e-2b41-4bec-b96b-514743c47cae" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffccardinal numeral", "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcdistributive numeral", "b330eb7d-f43d-4531-846e-5cd5820851ad" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcmultiplicative numeral", "1c030229-affa-4729-9773-878100c1fd28" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcordinal numeral", "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd" },
			{ "determiner\ufffcquantifier\ufffcnumeral\ufffcpartitive numeral", "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04" },
			{ "existential marker", "25b2ef8c-d87e-4868-898c-8f5375afeeb3" },
			{ "expletive", "cc60cb18-5067-442b-a740-d3b913b2610a" },
			{ "interjection", "d32dff62-4117-4762-a887-96478406769b" },
			{ "noun", "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5" },
			{ "noun\ufffcnominal", "085360ef-166c-4324-a5c4-5f4d8eabf75a" },
			{ "noun\ufffcnominal\ufffcgerund", "18b523c8-7dca-4361-93d8-6d039335f26d" },
			{ "noun\ufffcproper noun", "a8d31dff-1cdd-4bc7-ab3a-befef67711c2" },
			{ "noun\ufffcsubstantive", "584412e9-3a96-4251-99e2-438f1394432e" },
			{ "participle", "83fdec31-e15b-40e7-bcd2-69134c5a0fcd" },
			{ "particle", "6e758bbc-2b40-427d-b79f-bf14a7c96c6a" },
			{ "particle\ufffcnominal particle", "c6853f58-a74c-483e-9494-732002a7ab5b" },
			{ "particle\ufffcquestion particle", "07455e91-118a-4d7d-848c-39bedd355a3d" },
			{ "particle\ufffcverbal particle", "12435333-c423-43d7-acf9-6028e3d20a42" },
			{ "prenoun", "f1ac9eab-5f8c-41cf-b234-e53405aaaac5" },
			{ "preverb", "4e676ad2-542d-461d-9d78-1dbcb55ec456" },
			{ "pro-form", "a4fc78d6-7591-4fb3-8edd-82f10ae3739d" },
			{ "pro-form\ufffcinterrogative pro-form", "e5e7d0cb-36c5-497d-831c-cb614e283d7c" },
			{ "pro-form\ufffcpro-adjective", "e28bb667-fcaa-4c6e-944b-9b90683a2570" },
			{ "pro-form\ufffcpro-adverb", "d599dd69-b445-4627-a7f3-b9647abdb905" },
			{ "pro-form\ufffcpronoun", "a3274cfd-225f-45fd-8851-a7b1a1e1037a" },
			{ "pro-form\ufffcpronoun\ufffcindefinite pronoun", "b5d9ab85-fa93-4d6a-892b-837efb299ef7" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun", "2fad3a89-47d7-472a-a8cc-270e7e3e0239" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcemphatic pronoun", "98f5507f-bf51-43e4-8e08-e066c36c6d6e" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcpossessive pronoun", "64e3b502-90f5-4df0-9212-65f6e5c96c30" },
			{ "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcreflexive pronoun", "605c54f9-a81f-4bca-8d8c-a6fb08c29676" },
			{ "pro-form\ufffcpronoun\ufffcreciprocal pronoun", "d9a90c6c-3575-4937-bfaf-b3585a1954a9" },
			{ "pro-form\ufffcpronoun\ufffcrelative pronoun", "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a" },
			{ "verb", "86ff66f6-0774-407a-a0dc-3eeaf873daf7" },
			{ "verb\ufffccopulative verb", "55f2a00e-5f07-4ace-8a44-8794ed1a38a8" },
			{ "verb\ufffcditransitive verb", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "verb\ufffcbitransitive verb", "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc" },
			{ "verb\ufffcintransitive verb", "4459ff09-9ee0-4b50-8787-ae40fd76d3b7" },
			{ "verb\ufffctransitive verb", "54712931-442f-42d5-8634-f12bd2e310ce" }
		};

		public static readonly Dictionary<string, string> FlatPosNames = new Dictionary<string, string> {
			{ "30d07580-5052-4d91-bc24-469b8b2d7df9", "adjective" },
			{ "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5", "adposition" },
			{ "18f1b2b8-0ce3-4889-90e9-003fed6a969f", "postposition" },
			{ "923e5aed-d84a-48b0-9b7a-331c1336864a", "preposition" },
			{ "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9", "adverb" },
			{ "31616603-aadd-47af-a710-cb565fbbbd57", "classifier" },
			{ "131209ac-b8f1-44aa-b4f0-9a983e3d5bad", "noun classifier" },
			{ "6e0682a7-efd4-43c9-b083-22c4ce245419", "connective" },
			{ "75ac4332-a445-4ce8-918e-b27b04073745", "coordinating connective" },
			{ "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7", "correlative connective" },
			{ "09052f32-bf65-400a-8179-6a1c54ef30c9", "subordinating connective" },
			{ "a0a9906d-d987-42cb-8a65-f549462b16fc", "adverbializer" },
			{ "8f7ba502-e7c9-4bc4-a881-b0cb1b630466", "complementizer" },
			{ "c5f222a3-e1aa-4250-b196-d94f0eb0d47b", "relativizer" },
			{ "6df1c8ee-5530-4180-99e8-be2afab0f60d", "determiner" },
			{ "af3f65de-b0d5-4243-a196-53b67bd6a4df", "article" },
			{ "92ab3e14-e1af-4e7f-8492-e37a1f386e3f", "definite article" },
			{ "d07c625d-ff8b-4c4e-99be-e32b2924626e", "indefinite article" },
			{ "093264d7-06c3-42e1-bc4d-5a965ce63887", "demonstrative" },
			{ "a4a759b4-5a10-4d7a-80a3-accf5bd840b1", "quantifier" },
			{ "e680330e-2b41-4bec-b96b-514743c47cae", "numeral" },
			{ "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd", "cardinal numeral" },
			{ "b330eb7d-f43d-4531-846e-5cd5820851ad", "distributive numeral" },
			{ "1c030229-affa-4729-9773-878100c1fd28", "multiplicative numeral" },
			{ "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd", "ordinal numeral" },
			{ "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04", "partitive numeral" },
			{ "25b2ef8c-d87e-4868-898c-8f5375afeeb3", "existential marker" },
			{ "cc60cb18-5067-442b-a740-d3b913b2610a", "expletive" },
			{ "d32dff62-4117-4762-a887-96478406769b", "interjection" },
			{ "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5", "noun" },
			{ "085360ef-166c-4324-a5c4-5f4d8eabf75a", "nominal" },
			{ "18b523c8-7dca-4361-93d8-6d039335f26d", "gerund" },
			{ "a8d31dff-1cdd-4bc7-ab3a-befef67711c2", "proper noun" },
			{ "584412e9-3a96-4251-99e2-438f1394432e", "substantive" },
			{ "83fdec31-e15b-40e7-bcd2-69134c5a0fcd", "participle" },
			{ "6e758bbc-2b40-427d-b79f-bf14a7c96c6a", "particle" },
			{ "c6853f58-a74c-483e-9494-732002a7ab5b", "nominal particle" },
			{ "07455e91-118a-4d7d-848c-39bedd355a3d", "question particle" },
			{ "12435333-c423-43d7-acf9-6028e3d20a42", "verbal particle" },
			{ "f1ac9eab-5f8c-41cf-b234-e53405aaaac5", "prenoun" },
			{ "4e676ad2-542d-461d-9d78-1dbcb55ec456", "preverb" },
			{ "a4fc78d6-7591-4fb3-8edd-82f10ae3739d", "pro-form" },
			{ "e5e7d0cb-36c5-497d-831c-cb614e283d7c", "interrogative pro-form" },
			{ "e28bb667-fcaa-4c6e-944b-9b90683a2570", "pro-adjective" },
			{ "d599dd69-b445-4627-a7f3-b9647abdb905", "pro-adverb" },
			{ "a3274cfd-225f-45fd-8851-a7b1a1e1037a", "pronoun" },
			{ "b5d9ab85-fa93-4d6a-892b-837efb299ef7", "indefinite pronoun" },
			{ "2fad3a89-47d7-472a-a8cc-270e7e3e0239", "personal pronoun" },
			{ "98f5507f-bf51-43e4-8e08-e066c36c6d6e", "emphatic pronoun" },
			{ "64e3b502-90f5-4df0-9212-65f6e5c96c30", "possessive pronoun" },
			{ "605c54f9-a81f-4bca-8d8c-a6fb08c29676", "reflexive pronoun" },
			{ "d9a90c6c-3575-4937-bfaf-b3585a1954a9", "reciprocal pronoun" },
			{ "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a", "relative pronoun" },
			{ "86ff66f6-0774-407a-a0dc-3eeaf873daf7", "verb" },
			{ "55f2a00e-5f07-4ace-8a44-8794ed1a38a8", "copulative verb" },
			{ "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc", "ditransitive verb" },
			{ "4459ff09-9ee0-4b50-8787-ae40fd76d3b7", "intransitive verb" },
			{ "54712931-442f-42d5-8634-f12bd2e310ce", "transitive verb" }
		};

		public static readonly Dictionary<string, string> HierarchicalPosNames = new Dictionary<string, string> {
			{ "30d07580-5052-4d91-bc24-469b8b2d7df9", "adjective" },
			{ "ae115ea8-2cd7-4501-8ae7-dc638e4f17c5", "adposition" },
			{ "18f1b2b8-0ce3-4889-90e9-003fed6a969f", "adposition\ufffcpostposition" },
			{ "923e5aed-d84a-48b0-9b7a-331c1336864a", "adposition\ufffcpreposition" },
			{ "46e4fe08-ffa0-4c8b-bf98-2c56f38904d9", "adverb" },
			{ "31616603-aadd-47af-a710-cb565fbbbd57", "classifier" },
			{ "131209ac-b8f1-44aa-b4f0-9a983e3d5bad", "classifier\ufffcnoun classifier" },
			{ "6e0682a7-efd4-43c9-b083-22c4ce245419", "connective" },
			{ "75ac4332-a445-4ce8-918e-b27b04073745", "connective\ufffccoordinating connective" },
			{ "2d7a5bc6-bbc7-4be9-a036-3d046dbc65f7", "connective\ufffccoordinating connective\ufffccorrelative connective" },
			{ "09052f32-bf65-400a-8179-6a1c54ef30c9", "connective\ufffcsubordinating connective" },
			{ "a0a9906d-d987-42cb-8a65-f549462b16fc", "connective\ufffcsubordinating connective\ufffcadverbializer" },
			{ "8f7ba502-e7c9-4bc4-a881-b0cb1b630466", "connective\ufffcsubordinating connective\ufffccomplementizer" },
			{ "c5f222a3-e1aa-4250-b196-d94f0eb0d47b", "connective\ufffcsubordinating connective\ufffcrelativizer" },
			{ "6df1c8ee-5530-4180-99e8-be2afab0f60d", "determiner" },
			{ "af3f65de-b0d5-4243-a196-53b67bd6a4df", "determiner\ufffcarticle" },
			{ "92ab3e14-e1af-4e7f-8492-e37a1f386e3f", "determiner\ufffcarticle\ufffcdefinite article" },
			{ "d07c625d-ff8b-4c4e-99be-e32b2924626e", "determiner\ufffcarticle\ufffcindefinite article" },
			{ "093264d7-06c3-42e1-bc4d-5a965ce63887", "determiner\ufffcdemonstrative" },
			{ "a4a759b4-5a10-4d7a-80a3-accf5bd840b1", "determiner\ufffcquantifier" },
			{ "e680330e-2b41-4bec-b96b-514743c47cae", "determiner\ufffcquantifier\ufffcnumeral" },
			{ "a5311f3b-ff98-47d2-8ece-b1aca03a8bbd", "determiner\ufffcquantifier\ufffcnumeral\ufffccardinal numeral" },
			{ "b330eb7d-f43d-4531-846e-5cd5820851ad", "determiner\ufffcquantifier\ufffcnumeral\ufffcdistributive numeral" },
			{ "1c030229-affa-4729-9773-878100c1fd28", "determiner\ufffcquantifier\ufffcnumeral\ufffcmultiplicative numeral" },
			{ "3d9d43d6-527c-4e79-be00-82cf2d0fd9bd", "determiner\ufffcquantifier\ufffcnumeral\ufffcordinal numeral" },
			{ "0e652cc3-ef08-4cb1-8b73-a68ebd8e7c04", "determiner\ufffcquantifier\ufffcnumeral\ufffcpartitive numeral" },
			{ "25b2ef8c-d87e-4868-898c-8f5375afeeb3", "existential marker" },
			{ "cc60cb18-5067-442b-a740-d3b913b2610a", "expletive" },
			{ "d32dff62-4117-4762-a887-96478406769b", "interjection" },
			{ "a8e41fd3-e343-4c7c-aa05-01ea3dd5cfb5", "noun" },
			{ "085360ef-166c-4324-a5c4-5f4d8eabf75a", "noun\ufffcnominal" },
			{ "18b523c8-7dca-4361-93d8-6d039335f26d", "noun\ufffcnominal\ufffcgerund" },
			{ "a8d31dff-1cdd-4bc7-ab3a-befef67711c2", "noun\ufffcproper noun" },
			{ "584412e9-3a96-4251-99e2-438f1394432e", "noun\ufffcsubstantive" },
			{ "83fdec31-e15b-40e7-bcd2-69134c5a0fcd", "participle" },
			{ "6e758bbc-2b40-427d-b79f-bf14a7c96c6a", "particle" },
			{ "c6853f58-a74c-483e-9494-732002a7ab5b", "particle\ufffcnominal particle" },
			{ "07455e91-118a-4d7d-848c-39bedd355a3d", "particle\ufffcquestion particle" },
			{ "12435333-c423-43d7-acf9-6028e3d20a42", "particle\ufffcverbal particle" },
			{ "f1ac9eab-5f8c-41cf-b234-e53405aaaac5", "prenoun" },
			{ "4e676ad2-542d-461d-9d78-1dbcb55ec456", "preverb" },
			{ "a4fc78d6-7591-4fb3-8edd-82f10ae3739d", "pro-form" },
			{ "e5e7d0cb-36c5-497d-831c-cb614e283d7c", "pro-form\ufffcinterrogative pro-form" },
			{ "e28bb667-fcaa-4c6e-944b-9b90683a2570", "pro-form\ufffcpro-adjective" },
			{ "d599dd69-b445-4627-a7f3-b9647abdb905", "pro-form\ufffcpro-adverb" },
			{ "a3274cfd-225f-45fd-8851-a7b1a1e1037a", "pro-form\ufffcpronoun" },
			{ "b5d9ab85-fa93-4d6a-892b-837efb299ef7", "pro-form\ufffcpronoun\ufffcindefinite pronoun" },
			{ "2fad3a89-47d7-472a-a8cc-270e7e3e0239", "pro-form\ufffcpronoun\ufffcpersonal pronoun" },
			{ "98f5507f-bf51-43e4-8e08-e066c36c6d6e", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcemphatic pronoun" },
			{ "64e3b502-90f5-4df0-9212-65f6e5c96c30", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcpossessive pronoun" },
			{ "605c54f9-a81f-4bca-8d8c-a6fb08c29676", "pro-form\ufffcpronoun\ufffcpersonal pronoun\ufffcreflexive pronoun" },
			{ "d9a90c6c-3575-4937-bfaf-b3585a1954a9", "pro-form\ufffcpronoun\ufffcreciprocal pronoun" },
			{ "3f9bffe2-da9b-42fa-afbd-d7ca8bca7d4a", "pro-form\ufffcpronoun\ufffcrelative pronoun" },
			{ "86ff66f6-0774-407a-a0dc-3eeaf873daf7", "verb" },
			{ "55f2a00e-5f07-4ace-8a44-8794ed1a38a8", "verb\ufffccopulative verb" },
			{ "efadf1d3-580a-4e4b-a94c-3f1d6e59c5fc", "verb\ufffcditransitive verb" },
			{ "4459ff09-9ee0-4b50-8787-ae40fd76d3b7", "verb\ufffcintransitive verb" },
			{ "54712931-442f-42d5-8634-f12bd2e310ce", "verb\ufffctransitive verb" }
		};
	}
}

