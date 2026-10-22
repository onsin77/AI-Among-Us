using System;
using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using UnityEngine;

public class KillOverlay : MonoBehaviour
{
	public SpriteRenderer background;

	public GameObject flameParent;

	public OverlayKillAnimation[] KillAnims;

	public OverlayKillAnimation[] HorseWrangleAnims;

	public OverlayKillAnimation[] CustomKillAnimations;

	private Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();

	private Coroutine showAll;

	private Coroutine showOne;

	public bool IsOpen
	{
		get
		{
			if (showAll == null)
			{
				return queue.Count > 0;
			}
			return true;
		}
	}

	public IEnumerator WaitForFinish()
	{
		while (showAll != null || queue.Count > 0)
		{
			yield return null;
		}
	}

	public void ShowKillAnimation(OverlayKillAnimation killAnimation, GameData.PlayerInfo killer, GameData.PlayerInfo victim)
	{
		queue.Enqueue(() =>
		{
			OverlayKillAnimation overlayKillAnimation = Object.Instantiate<OverlayKillAnimation>(killAnimation, ((Component)this).transform);
			overlayKillAnimation.Initialize(killer, victim);
			((Component)overlayKillAnimation).gameObject.SetActive(false);
			return CoShowOne(overlayKillAnimation);
		});
		if (showAll == null)
		{
			showAll = ((MonoBehaviour)this).StartCoroutine(ShowAll());
		}
	}

	public void ShowKillAnimation(GameData.PlayerInfo killer, GameData.PlayerInfo victim)
	{
		IEnumerable<OverlayKillAnimation> self = KillAnims;
		if (Object.op_Implicit((Object)(object)killer.Object))
		{
			OverlayKillAnimation[] killAnimations = killer.Object.MyPhysics.Animations.GetKillAnimations();
			if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek && Constants.ShouldHorseAround())
			{
				self = HorseWrangleAnims;
			}
			else if (killAnimations.Length != 0)
			{
				self = killAnimations;
			}
			else
			{
				SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(killer.Object.CurrentOutfit.SkinId);
				if (Object.op_Implicit((Object)(object)skin) && skin.CustomKillAnimID > 0)
				{
					self = new OverlayKillAnimation[1] { CustomKillAnimations[skin.CustomKillAnimID - 1] };
				}
			}
		}
		OverlayKillAnimation killAnimation = self.Random();
		ShowKillAnimation(killAnimation, killer, victim);
	}

	public void ShowMeeting(MeetingCalledAnimation prefab, GameData.PlayerInfo playerInfo)
	{
		queue.Enqueue(() =>
		{
			MeetingCalledAnimation meetingCalledAnimation = Object.Instantiate<MeetingCalledAnimation>(prefab, ((Component)this).transform);
			meetingCalledAnimation.Initialize(playerInfo);
			((Component)meetingCalledAnimation).gameObject.SetActive(false);
			return CoShowOne(meetingCalledAnimation);
		});
		if (showAll == null)
		{
			showAll = ((MonoBehaviour)this).StartCoroutine(ShowAll());
		}
	}

	private IEnumerator ShowAll()
	{
		while (queue.Count > 0 || showOne != null)
		{
			if (showOne == null)
			{
				showOne = ((MonoBehaviour)this).StartCoroutine(queue.Dequeue()());
			}
			yield return null;
		}
		showAll = null;
	}

	private IEnumerator CoShowOne(OverlayAnimation anim)
	{
		yield return anim.CoShow(this);
		Object.Destroy((Object)(object)((Component)anim).gameObject);
		showOne = null;
	}
}
