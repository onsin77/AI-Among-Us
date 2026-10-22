using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PEW/GlyphControllerMapCollection")]
public class GlyphControllerMapCollection : ScriptableObject
{
	[Serializable]
	public class GlyphControllerMap
	{
		public string controllerName;

		public string glyphCollectionPath;
	}

	public List<GlyphControllerMap> nameToGlyphCollectionList = new List<GlyphControllerMap>();

	private Dictionary<string, string> nameToGlyphCollectionDict;

	private static GlyphControllerMapCollection _instance;

	public static GlyphControllerMapCollection Instance
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)_instance))
			{
				_instance = Resources.Load<GlyphControllerMapCollection>("ControllerGlyphMapAsset");
				_instance.Initialize();
			}
			return _instance;
		}
	}

	public void Initialize()
	{
		nameToGlyphCollectionDict = new Dictionary<string, string>();
		foreach (GlyphControllerMap nameToGlyphCollection in nameToGlyphCollectionList)
		{
			nameToGlyphCollectionDict.Add(nameToGlyphCollection.controllerName, nameToGlyphCollection.glyphCollectionPath);
		}
	}

	public GlyphCollection TryGetGlyphCollectionForController(string controllerName)
	{
		if (nameToGlyphCollectionDict.TryGetValue(controllerName, out var value))
		{
			GlyphCollection glyphCollection = Resources.Load<GlyphCollection>(value);
			if (Object.op_Implicit((Object)(object)glyphCollection))
			{
				glyphCollection.Initialize();
				return glyphCollection;
			}
		}
		return null;
	}
}
