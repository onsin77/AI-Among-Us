using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SpawnInMinigame : Minigame
{
	[Serializable]
	public struct SpawnLocation
	{
		public StringNames Name;

		public Sprite Image;

		public AnimationClip Rollover;

		public AudioClip RolloverSfx;

		public Vector3 Location;
	}

	public SpawnLocation[] Locations;

	public PassiveButton[] LocationButtons;

	public TextMeshPro Text;

	public AudioClip DefaultRolloverSound;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	private bool gotButton;

	public override void Begin(PlayerTask task)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected Obj, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		base.Begin(task);
		SpawnLocation[] array = Locations.ToArray();
		array.Shuffle();
		array = array.Take(LocationButtons.Length).OrderBy((SpawnLocation s) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return s.Location.x;
		}).ThenByDescending((SpawnLocation s) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return s.Location.y;
		})
			.ToArray();
		for (int num = 0; num < LocationButtons.Length; num++)
		{
			PassiveButton obj = LocationButtons[num];
			SpawnLocation pt = array[num];
			((UnityEvent)obj.OnClick).AddListener((UnityAction)(() =>
			{
				SpawnAt(pt);
			}));
			((Component)obj).GetComponent<SpriteAnim>().Stop();
			((Component)obj).GetComponent<SpriteRenderer>().sprite = pt.Image;
			((TMP_Text)((Component)obj).GetComponentInChildren<TextMeshPro>()).text = DestroyableSingleton<TranslationController>.Instance.GetString(pt.Name);
			ButtonAnimRolloverHandler component = ((Component)obj).GetComponent<ButtonAnimRolloverHandler>();
			component.StaticOutImage = pt.Image;
			component.RolloverAnim = pt.Rollover;
			component.HoverSound = (Object.op_Implicit((Object)(object)pt.RolloverSfx) ? pt.RolloverSfx : DefaultRolloverSound);
		}
		if ((Object)(object)GameManager.Instance != (Object)null && GameManager.Instance.IsNormal())
		{
			foreach (GameData.PlayerInfo allPlayer in GameData.Instance.AllPlayers)
			{
				if (allPlayer != null && !((Object)(object)allPlayer.Object == (Object)null) && !allPlayer.Disconnected)
				{
					((Component)allPlayer.Object.NetTransform).transform.position = Vector2.op_Implicit(new Vector2(-25f, 40f));
					allPlayer.Object.NetTransform.Halt();
				}
			}
		}
		((MonoBehaviour)this).StartCoroutine(RunTimer());
		ControllerManager.Instance.OpenOverlayMenu(((Object)this).name, null, DefaultButtonSelected, ControllerSelectable);
		PlayerControl.HideCursorTemporarily();
		ConsoleJoystick.SetMode_Menu();
	}

	public override void Close()
	{
		ControllerManager.Instance.CloseOverlayMenu(((Object)this).name);
		if (!gotButton)
		{
			LocationButtons.Random().ReceiveClickUp();
		}
		base.Close();
	}

	private IEnumerator RunTimer()
	{
		for (float time = 10f; time >= 0f; time -= Time.deltaTime)
		{
			((TMP_Text)Text).text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.TimeRemaining, Mathf.CeilToInt(time));
			yield return null;
		}
		LocationButtons.Random().ReceiveClickUp();
	}

	private void SpawnAt(SpawnLocation spawnPoint)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing == CloseState.None)
		{
			Logger.GlobalInstance.Info($"Player selected spawn point {spawnPoint.Name}");
			gotButton = true;
			PlayerControl.LocalPlayer.SetKinematic(b: true);
			PlayerControl.LocalPlayer.NetTransform.SetPaused(isPaused: true);
			PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(Vector2.op_Implicit(spawnPoint.Location));
			DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(CoSpawnAt(PlayerControl.LocalPlayer, spawnPoint));
		}
	}

	private IEnumerator CoSpawnAt(PlayerControl playerControl, SpawnLocation spawnLocation)
	{
		yield return (object)new WaitForFixedUpdate();
		yield return (object)new WaitForFixedUpdate();
		yield return (object)new WaitForFixedUpdate();
		playerControl.SetKinematic(b: false);
		playerControl.NetTransform.SetPaused(isPaused: false);
		Close();
	}

	public IEnumerator WaitForFinish()
	{
		yield return null;
		while (amClosing == CloseState.None)
		{
			yield return null;
		}
	}
}
