using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AmongUs.GameOptions;

[Flags]
[JsonConverter(typeof(StringEnumConverter))]
public enum GameKeywords : uint
{
	All = 0u,
	English = 0x100u,
	SpanishLA = 2u,
	Brazilian = 0x800u,
	Portuguese = 0x10u,
	Korean = 4u,
	Russian = 8u,
	Dutch = 0x1000u,
	Filipino = 0x40u,
	French = 0x2000u,
	German = 0x4000u,
	Italian = 0x8000u,
	Japanese = 0x200u,
	SpanishEU = 0x400u,
	Arabic = 0x20u,
	Polish = 0x80u,
	SChinese = 0x10000u,
	TChinese = 0x20000u,
	Irish = 0x40000u,
	Other = 1u
}
