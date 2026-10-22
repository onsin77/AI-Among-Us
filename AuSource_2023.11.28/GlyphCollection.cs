using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

[CreateAssetMenu(menuName = "PEW/GlyphCollection")]
public class GlyphCollection : ScriptableObject
{
	[Serializable]
	public class GlyphMap
	{
		public string elementIdentifier;

		public string alternateElementIdentifier;

		public Sprite glyph;
	}

	public enum ErrorCode
	{
		None,
		NoController,
		NoGlyphFound,
		NoElementsBoundToAction
	}

	public string controllerType;

	public List<GlyphMap> glyphMaps = new List<GlyphMap>();

	public Dictionary<string, GlyphMap> glyphDict;

	private static List<ActionElementMap> mapResults = new List<ActionElementMap>();

	private static GlyphCollection defaultGlyphCollection;

	private static Dictionary<string, GlyphCollection> otherGlyphCollections = new Dictionary<string, GlyphCollection>();

	private static HashSet<string> controllersWithNoValidGlyphCollection = new HashSet<string>();

	private static string GlyphPath => "Glyphs/XboxGlyphs";

	public void Initialize()
	{
		glyphDict = new Dictionary<string, GlyphMap>();
		foreach (GlyphMap glyphMap in glyphMaps)
		{
			glyphDict[glyphMap.elementIdentifier.ToLower()] = glyphMap;
			if (!string.IsNullOrEmpty(glyphMap.alternateElementIdentifier))
			{
				glyphDict[glyphMap.alternateElementIdentifier.ToLower()] = glyphMap;
			}
		}
	}

	public static Sprite FindGlyph(int actionName, out ErrorCode error)
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)defaultGlyphCollection))
		{
			defaultGlyphCollection = Resources.Load<GlyphCollection>(GlyphPath);
			if (!Object.op_Implicit((Object)(object)defaultGlyphCollection))
			{
				error = ErrorCode.NoGlyphFound;
				return null;
			}
			defaultGlyphCollection.Initialize();
		}
		Player player = ReInput.players.GetPlayer(0);
		Controller val = player.controllers.GetLastActiveController();
		if (val == null)
		{
			foreach (Controller controller in player.controllers.Controllers)
			{
				if (controller is Joystick)
				{
					val = controller;
					break;
				}
			}
		}
		if (val != null)
		{
			GlyphCollection glyphCollection = defaultGlyphCollection;
			if (otherGlyphCollections.TryGetValue(val.name, out var value))
			{
				glyphCollection = value;
			}
			else if (!controllersWithNoValidGlyphCollection.Contains(val.name))
			{
				value = GlyphControllerMapCollection.Instance.TryGetGlyphCollectionForController(val.name);
				if (Object.op_Implicit((Object)(object)value))
				{
					glyphCollection = value;
					otherGlyphCollections.Add(val.name, value);
					Debug.Log((object)("Found valid glyph collection for " + val.name));
				}
				else
				{
					controllersWithNoValidGlyphCollection.Add(val.name);
					Debug.Log((object)("No valid glyph collection for " + val.name + ", using default"));
				}
			}
			int elementMapsWithAction = player.controllers.maps.GetElementMapsWithAction(actionName, false, mapResults);
			if (elementMapsWithAction > 0)
			{
				ActionElementMap val2 = mapResults[0];
				if (elementMapsWithAction > 1)
				{
					for (int i = 1; i < elementMapsWithAction; i++)
					{
						if ((int)mapResults[i].elementType == 0)
						{
							val2 = mapResults[i];
						}
					}
				}
				GlyphMap value2 = null;
				if (glyphCollection.glyphDict.TryGetValue(val2.elementIdentifierName.ToLower(), out value2))
				{
					error = ErrorCode.None;
					return value2.glyph;
				}
				Debug.LogWarning((object)("GlyphCollection.FindGlyph: GlyphCollection didn't have a glyph for element " + val2.elementIdentifierName + " on controller" + val.name));
				error = ErrorCode.NoGlyphFound;
				return null;
			}
			RewiredConstsEnum.Action action = (RewiredConstsEnum.Action)actionName;
			Debug.LogWarning((object)("GlyphCollection.FindGlyph: No elements bound to action " + action));
			error = ErrorCode.NoElementsBoundToAction;
			return null;
		}
		error = ErrorCode.NoController;
		return null;
	}
}
