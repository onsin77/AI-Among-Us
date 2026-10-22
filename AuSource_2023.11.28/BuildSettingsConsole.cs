using UnityEngine;

[CreateAssetMenu(fileName = "BuildSettingsConsole", menuName = "BuildSettingsConsole")]
public class BuildSettingsConsole : ScriptableObject
{
	[Header("Xbox Options")]
	public int XboxOneGameCoreStreamingInstallLaunchRange;

	public int SeriesXGameCoreStreamingInstallLaunchRange;

	[Header("Switch Options")]
	public string SwitchDescFile;

	public short SwitchReleaseVersion;

	public string SwitchOriginalRomFile;

	public string SwitchPreviousRomFile;

	public bool SwitchCreateRomFile;

	public bool SwitchEnableDebugNpad;

	public bool SwitchEnableHeapInspector;

	public bool SwitchNVNDrawValidation;

	public bool SwitchNVNGraphicsDebugger;

	public bool SwitchNVNShaderDebugging;

	public bool SwitchRedirectWritesToHostMount;

	[Header("PS4 Options")]
	public string PS4PronunciationXmlPath;

	public string PS4PronunciationSigPath;

	public string PS4BackgroundImagePath;

	public string PS4StartupImagePath;

	public string PS4TrophyPackPath;

	public string PS4SonyPasscode;

	public bool PS4CompressPackage;

	[Header("PS4 SIEA Options")]
	public string PS4VersionSiea;

	public string PS4PatchPkgPathSiea;

	public string PS4PatchLatestPkgPathSiea;

	public string PS4PatchChangeInfoPathSiea;

	public string PS4ContentIdSiea;

	public string PS4TitleSecretSiea;

	public string PS4TitleDatPathSiea;

	public int PS4AgeRatingSiea;

	public string PS4ParamSfxPathSiea;

	[Header("PS4 SIEE Options")]
	public string PS4VersionSiee;

	public string PS4PatchPkgPathSiee;

	public string PS4PatchLatestPkgPathSiee;

	public string PS4PatchChangeInfoPathSiee;

	public string PS4ContentIdSiee;

	public string PS4TitleSecretSiee;

	public string PS4TitleDatPathSiee;

	public int PS4AgeRatingSiee;

	public string PS4ParamSfxPathSiee;

	[Header("PS4 SIEJ Options")]
	public string PS4VersionSiej;

	public string PS4PatchPkgPathSiej;

	public string PS4PatchLatestPkgPathSiej;

	public string PS4PatchChangeInfoPathSiej;

	public string PS4ContentIdSiej;

	public string PS4TitleSecretSiej;

	public string PS4TitleDatPathSiej;

	public int PS4AgeRatingSiej;

	public string PS4ParamSfxPathSiej;

	[Header("PS4 SIEAsia Options")]
	public string PS4VersionSieAsia;

	public string PS4PatchPkgPathSieAsia;

	public string PS4PatchLatestPkgPathSieAsia;

	public string PS4PatchChangeInfoPathSieAsia;

	public string PS4ContentIdSieAsia;

	public string PS4TitleSecretSieAsia;

	public string PS4TitleDatPathSieAsia;

	public int PS4AgeRatingSieAsia;

	public string PS4ParamSfxPathSieAsia;

	[Header("PS5 Options")]
	public string PS5BackgroundImagePath;

	public string PS5StartupImagePath;

	public string PS5StartupForegroundImagePath;

	public string PS5StartupBackgroundImagePath;

	public string PS5StartupImagesFolder;

	public string PS5IconImagesFolder;

	public string PS5BgmPath;

	public string PS5SonyPasscode;

	public bool PS5NeedSubmissionMaterials;

	public string PS5UpdateReferencePackage;

	public PS4Settings GetPS4Settings(PSRegion _psRegion)
	{
		PS4Settings result = default;
		switch (_psRegion)
		{
		default:
			result.PS4Version = PS4VersionSiea;
			result.ContentID = PS4ContentIdSiea;
			result.NpTitleSecret = PS4TitleSecretSiea;
			result.NpTitleDatPath = PS4TitleDatPathSiea;
			result.NpAgeRating = PS4AgeRatingSiea;
			result.PS4PatchPkgPath = PS4PatchPkgPathSiea;
			result.PS4PatchLatestPkgPath = PS4PatchLatestPkgPathSiea;
			result.PS4PatchChangeInfoPath = PS4PatchChangeInfoPathSiea;
			result.PS4ParamSfxPath = PS4ParamSfxPathSiea;
			break;
		case PSRegion.SIEE:
			result.PS4Version = PS4VersionSiee;
			result.ContentID = PS4ContentIdSiee;
			result.NpTitleSecret = PS4TitleSecretSiee;
			result.NpTitleDatPath = PS4TitleDatPathSiee;
			result.NpAgeRating = PS4AgeRatingSiee;
			result.PS4PatchPkgPath = PS4PatchPkgPathSiee;
			result.PS4PatchLatestPkgPath = PS4PatchLatestPkgPathSiee;
			result.PS4PatchChangeInfoPath = PS4PatchChangeInfoPathSiee;
			result.PS4ParamSfxPath = PS4ParamSfxPathSiee;
			break;
		case PSRegion.SIEJ:
			result.PS4Version = PS4VersionSiej;
			result.ContentID = PS4ContentIdSiej;
			result.NpTitleSecret = PS4TitleSecretSiej;
			result.NpTitleDatPath = PS4TitleDatPathSiej;
			result.NpAgeRating = PS4AgeRatingSiej;
			result.PS4PatchPkgPath = PS4PatchPkgPathSiej;
			result.PS4PatchLatestPkgPath = PS4PatchLatestPkgPathSiej;
			result.PS4PatchChangeInfoPath = PS4PatchChangeInfoPathSiej;
			result.PS4ParamSfxPath = PS4ParamSfxPathSiej;
			break;
		case PSRegion.SIEAsia:
			result.PS4Version = PS4VersionSieAsia;
			result.ContentID = PS4ContentIdSieAsia;
			result.NpTitleSecret = PS4TitleSecretSieAsia;
			result.NpTitleDatPath = PS4TitleDatPathSieAsia;
			result.NpAgeRating = PS4AgeRatingSieAsia;
			result.PS4PatchPkgPath = PS4PatchPkgPathSieAsia;
			result.PS4PatchLatestPkgPath = PS4PatchLatestPkgPathSieAsia;
			result.PS4PatchChangeInfoPath = PS4PatchChangeInfoPathSieAsia;
			result.PS4ParamSfxPath = PS4ParamSfxPathSieAsia;
			break;
		}
		return result;
	}
}
