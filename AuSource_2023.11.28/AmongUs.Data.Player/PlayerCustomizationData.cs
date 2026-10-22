using System;
using Newtonsoft.Json;

namespace AmongUs.Data.Player;

[JsonObject(/*Could not decode attribute arguments.*/)]
public class PlayerCustomizationData : AbstractSaveDataGroup
{
	public const string DEFAULT_NAME = "";

	public const byte DEFAULT_COLOR_ID = 0;

	public const string DEFAULT_PET = "";

	public const string DEFAULT_HAT = "";

	public const string DEFAULT_SKIN = "";

	public const string DEFAULT_VISOR = "";

	public const string DEFAULT_NAME_PLATE = "";

	[JsonProperty("name")]
	private string name = "";

	[JsonProperty("colorID")]
	private byte colorID;

	[JsonProperty("pet")]
	private string pet = "";

	[JsonProperty("hat")]
	private string hat = "";

	[JsonProperty("skin")]
	private string skin = "";

	[JsonProperty("visor")]
	private string visor = "";

	[JsonProperty("namePlate")]
	private string namePlate = "";

	public string Name
	{
		get
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				name = DestroyableSingleton<AccountManager>.Instance.GetRandomName();
			}
			if (name.Length > 10)
			{
				name = name.Substring(0, 10);
			}
			return name;
		}
		set
		{
			SetValue(ref name, value, OnNameChanged);
		}
	}

	public byte Color
	{
		get
		{
			return colorID;
		}
		set
		{
			SetValue(ref colorID, value, OnColorChanged);
		}
	}

	public string Pet
	{
		get
		{
			return pet;
		}
		set
		{
			SetValue(ref pet, value, OnPetChanged);
		}
	}

	public string Hat
	{
		get
		{
			return hat;
		}
		set
		{
			SetValue(ref hat, value, OnHatChanged);
		}
	}

	public string Skin
	{
		get
		{
			return skin;
		}
		set
		{
			SetValue(ref skin, value, OnSkinChanged);
		}
	}

	public string Visor
	{
		get
		{
			return visor;
		}
		set
		{
			SetValue(ref visor, value, OnVisorChanged);
		}
	}

	public string NamePlate
	{
		get
		{
			return namePlate;
		}
		set
		{
			SetValue(ref namePlate, value, OnNamePlateChanged);
		}
	}

	public event Action OnNameChanged;

	public event Action OnColorChanged;

	public event Action OnPetChanged;

	public event Action OnHatChanged;

	public event Action OnSkinChanged;

	public event Action OnVisorChanged;

	public event Action OnNamePlateChanged;
}
