using UnityEngine;

public static class Palette
{
	public static readonly Color AcceptedGreen = Color32.op_Implicit(new Color32((byte)0, byte.MaxValue, (byte)42, byte.MaxValue));

	public static readonly Color DisabledGrey = new Color(0.3f, 0.3f, 0.3f, 1f);

	public static readonly Color DisabledClear = new Color(1f, 1f, 1f, 0.3f);

	public static readonly Color EnabledColor = new Color(1f, 1f, 1f, 1f);

	public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

	public static readonly Color ClearWhite = new Color(1f, 1f, 1f, 0f);

	public static readonly Color HalfWhite = new Color(1f, 1f, 1f, 0.5f);

	public static readonly Color White_75Alpha = new Color(1f, 1f, 1f, 0.75f);

	public static readonly Color White = new Color(1f, 1f, 1f, 1f);

	public static readonly Color LightBlue = new Color(0.5f, 0.5f, 1f);

	public static readonly Color Blue = new Color(0.2f, 0.2f, 1f);

	public static readonly Color Orange = new Color(1f, 0.6f, 0.005f);

	public static readonly Color Purple = new Color(0.6f, 0.1f, 0.6f);

	public static readonly Color Brown = new Color(0.72f, 0.43f, 0.11f);

	public static readonly Color CrewmateBlue = Color32.op_Implicit(new Color32((byte)140, byte.MaxValue, byte.MaxValue, byte.MaxValue));

	public static readonly Color ImpostorRed = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)25, (byte)25, byte.MaxValue));

	public static readonly Color CosmicubeCellUnlockedColor = new Color(6f / 255f, 200f / 255f, 100f / 255f);

	public static readonly Color CosmicubeCellLockedColor = new Color(179f / 255f, 171f / 255f, 183f / 255f);

	public static readonly Color CosmicubeQuality_NamePlate = Color32.op_Implicit(new Color32((byte)73, (byte)174, (byte)217, byte.MaxValue));

	public static readonly Color CosmicubeQuality_Hat = Color32.op_Implicit(new Color32((byte)225, (byte)222, (byte)0, byte.MaxValue));

	public static readonly Color CosmicubeQuality_Visor = Color32.op_Implicit(new Color32((byte)12, (byte)22, (byte)221, byte.MaxValue));

	public static readonly Color CosmicubeQuality_Skin = Color32.op_Implicit(new Color32((byte)239, (byte)53, (byte)230, byte.MaxValue));

	public static readonly Color CosmicubeQuality_Pet = Color32.op_Implicit(new Color32((byte)222, (byte)28, (byte)28, byte.MaxValue));

	public static readonly Color CosmicubeQuality_Kill = Color32.op_Implicit(new Color32((byte)0, byte.MaxValue, (byte)179, byte.MaxValue));

	public static readonly Color InventorySelectedTextColor = new Color(0f, 201f / 255f, 106f / 255f, 1f);

	public static readonly Color InventoryTextColor = new Color(161f / 255f, 0.8f, 222f / 255f, 1f);

	public static readonly Color LogSuccessColor = Color32.op_Implicit(new Color32((byte)149, (byte)240, (byte)53, byte.MaxValue));

	public static readonly Color LogWarningColor = Color32.op_Implicit(new Color32((byte)240, (byte)174, (byte)53, byte.MaxValue));

	public static readonly Color LogErrorColor = Color32.op_Implicit(new Color32((byte)240, (byte)75, (byte)53, byte.MaxValue));

	public static readonly StringNames[] ColorNames = new StringNames[18]
	{
		StringNames.ColorRed,
		StringNames.ColorBlue,
		StringNames.ColorGreen,
		StringNames.ColorPink,
		StringNames.ColorOrange,
		StringNames.ColorYellow,
		StringNames.ColorBlack,
		StringNames.ColorWhite,
		StringNames.ColorPurple,
		StringNames.ColorBrown,
		StringNames.ColorCyan,
		StringNames.ColorLime,
		StringNames.ColorMaroon,
		StringNames.ColorRose,
		StringNames.ColorBanana,
		StringNames.ColorGray,
		StringNames.ColorTan,
		StringNames.ColorCoral
	};

	public const int PreviewGreenColorId = 2;

	public static readonly Color32[] PlayerColors = new Color32[18]
	{
		new Color32((byte)198, (byte)17, (byte)17, byte.MaxValue),
		new Color32((byte)19, (byte)46, (byte)210, byte.MaxValue),
		new Color32((byte)17, (byte)128, (byte)45, byte.MaxValue),
		new Color32((byte)238, (byte)84, (byte)187, byte.MaxValue),
		new Color32((byte)240, (byte)125, (byte)13, byte.MaxValue),
		new Color32((byte)246, (byte)246, (byte)87, byte.MaxValue),
		new Color32((byte)63, (byte)71, (byte)78, byte.MaxValue),
		new Color32((byte)215, (byte)225, (byte)241, byte.MaxValue),
		new Color32((byte)107, (byte)47, (byte)188, byte.MaxValue),
		new Color32((byte)113, (byte)73, (byte)30, byte.MaxValue),
		new Color32((byte)56, byte.MaxValue, (byte)221, byte.MaxValue),
		new Color32((byte)80, (byte)240, (byte)57, byte.MaxValue),
		FromHex(6233390),
		FromHex(15515859),
		FromHex(15787944),
		FromHex(7701907),
		FromHex(9537655),
		FromHex(14115940)
	};

	public static readonly Color32[] ShadowColors = new Color32[18]
	{
		new Color32((byte)122, (byte)8, (byte)56, byte.MaxValue),
		new Color32((byte)9, (byte)21, (byte)142, byte.MaxValue),
		new Color32((byte)10, (byte)77, (byte)46, byte.MaxValue),
		new Color32((byte)172, (byte)43, (byte)174, byte.MaxValue),
		new Color32((byte)180, (byte)62, (byte)21, byte.MaxValue),
		new Color32((byte)195, (byte)136, (byte)34, byte.MaxValue),
		new Color32((byte)30, (byte)31, (byte)38, byte.MaxValue),
		new Color32((byte)132, (byte)149, (byte)192, byte.MaxValue),
		new Color32((byte)59, (byte)23, (byte)124, byte.MaxValue),
		new Color32((byte)94, (byte)38, (byte)21, byte.MaxValue),
		new Color32((byte)36, (byte)169, (byte)191, byte.MaxValue),
		new Color32((byte)21, (byte)168, (byte)66, byte.MaxValue),
		FromHex(4263706),
		FromHex(14586547),
		FromHex(13810825),
		FromHex(4609636),
		FromHex(5325118),
		FromHex(11813730)
	};

	public static readonly Color32 VisorColor = new Color32((byte)149, (byte)202, (byte)220, byte.MaxValue);

	private static Color32 FromHex(int v)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new Color32((byte)(v >> 16), (byte)(v >> 8), (byte)v, byte.MaxValue);
	}

	public static string GetColorName(int colorId)
	{
		if (colorId < 0 || colorId >= ColorNames.Length)
		{
			Debug.LogError((object)$"Color ID {colorId}, but Palette Length of {ColorNames.Length}");
			return "???";
		}
		return DestroyableSingleton<TranslationController>.Instance.GetString(ColorNames[colorId]);
	}

	static Palette()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0510: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0551: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_059f: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_067f: Unknown result type (might be due to invalid IL or missing references)
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0691: Unknown result type (might be due to invalid IL or missing references)
		//IL_069e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
	}
}
