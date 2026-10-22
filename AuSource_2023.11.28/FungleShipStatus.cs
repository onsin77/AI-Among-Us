using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Innersloth.Assets;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FungleShipStatus : ShipStatus
{
	private readonly Dictionary<int, Mushroom> sporeMushrooms = new Dictionary<int, Mushroom>();

	[SerializeField]
	private MushroomMixupSabotageSystem specialSabotage;

	[SerializeField]
	private GameObject[] startAMBSounds;

	[SerializeField]
	private AssetReference staticWavesAsset;

	[SerializeField]
	private AssetReference animatedWavesAsset;

	public ZiplineBehaviour Zipline { get; private set; }

	public Vector2 LastBinocularPos
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	protected override void OnEnable()
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		if (Systems == null)
		{
			((Renderer)DestroyableSingleton<HudManager>.Instance.ShadowQuad).material.SetInt("_Mask", 7);
			Zipline = ((Component)this).GetComponentInChildren<ZiplineBehaviour>();
			Systems = new Dictionary<SystemTypes, ISystemType>(SystemTypeComparer.Instance)
			{
				{
					SystemTypes.Ventilation,
					new VentilationSystem()
				},
				{
					SystemTypes.Comms,
					new HqHudSystemType()
				},
				{
					SystemTypes.Reactor,
					new ReactorSystemType(60f, SystemTypes.Reactor)
				},
				{
					SystemTypes.Doors,
					new DoorsSystemType()
				},
				{
					SystemTypes.MushroomMixupSabotage,
					specialSabotage
				}
			};
			Systems.Add(SystemTypes.Sabotage, new SabotageSystemType(Systems.Values.Where((ISystemType i) => i is IActivatable).Cast<IActivatable>().ToArray()));
			Camera main = Camera.main;
			main.backgroundColor = CameraColor;
			FollowerCamera component = ((Component)main).GetComponent<FollowerCamera>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.shakeAmount = 0f;
				component.shakePeriod = 0f;
			}
			sporeMushrooms.Clear();
			Mushroom[] componentsInChildren = ((Component)this).GetComponentsInChildren<Mushroom>();
			Mushroom[] array = componentsInChildren;
			foreach (Mushroom mushroom in array)
			{
				sporeMushrooms[mushroom.Id] = mushroom;
			}
			if (sporeMushrooms.Count != componentsInChildren.Length)
			{
				logger.Error("Spore Mushroom IDs are not all unique. Please use Fix Spore Screen Mushroom IDs to assign unique IDs, otherwise they will not work properly in networked games.");
			}
			LastBinocularPos = Vector2.zero;
			LoadWavesAsync(animatedWavesAsset);
		}
	}

	private void LoadWavesAsync(AssetReference waveAsset)
	{
		((MonoBehaviour)this).StartCoroutine(((MonoBehaviour)(object)this).CoLoadAssetAsync<GameObject>(waveAsset, (Action<GameObject>)((GameObject prefab) =>
		{
			Object.Instantiate<GameObject>(prefab, ((Component)this).transform);
		})));
	}

	public override void StartSFX()
	{
		GameObject[] array = startAMBSounds;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
	}

	public override void OnMeetingCalled()
	{
		Zipline.OnMeetingCalled();
	}

	public void TriggerSporesFromMushroom(int mushroomId)
	{
		if (sporeMushrooms.TryGetValue(mushroomId, out var value))
		{
			value.TriggerSpores();
		}
		else
		{
			logger.Error($"Failed to trigger spore mushroom {mushroomId} - no mushroom exists");
		}
	}

	public Mushroom GetMushroomFromId(int mushroomId)
	{
		if (sporeMushrooms.TryGetValue(mushroomId, out var value))
		{
			return value;
		}
		logger.Error($"Failed to check spore mushroom {mushroomId} - no mushroom exists");
		return null;
	}

	public bool IsMushroomMixupActive()
	{
		return specialSabotage.IsActive;
	}
}
