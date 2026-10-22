using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrewmatesKilledTracker : MonoBehaviour
{
	[SerializeField]
	private MultiPrefabInstantiator crewmates;

	[SerializeField]
	private float crewmateSpacing;

	[SerializeField]
	private Vector3 crewmateScale;

	[SerializeField]
	private List<CrewmateTrackerAnim> slashAnimations;

	private PseudoRandomList<CrewmateTrackerAnim> animList = new PseudoRandomList<CrewmateTrackerAnim>();

	private List<CrewmateTrackerEntry> crewmateSprites;

	private void Awake()
	{
		animList.AddRange(slashAnimations);
	}

	public void OnCrewmateKilled()
	{
		if (crewmateSprites == null)
		{
			return;
		}
		int num = PlayerControl.AllPlayerControls.Count((PlayerControl pc) => (Object)(object)pc.Data?.Role != (Object)null && !pc.Data.Role.IsImpostor && pc.Data.IsDead);
		for (int num2 = 0; num2 < crewmateSprites.Count; num2++)
		{
			if (num2 < num)
			{
				if (!crewmateSprites[num2].IsKilled)
				{
					crewmateSprites[num2].SetKilled(animList.PickRandom());
				}
			}
			else
			{
				crewmateSprites[num2].Reset();
			}
		}
	}

	public void OnCrewmateDisconnect()
	{
		int num = PlayerControl.AllPlayerControls.Count((PlayerControl pc) => (Object)(object)pc.Data?.Role != (Object)null && !pc.Data.Role.IsImpostor);
		for (int num2 = crewmates.Count - 1; num2 >= 0; num2--)
		{
			if (num2 >= num - 1)
			{
				crewmates.DestroyInstance(crewmates.GetInstance(num2));
			}
		}
		crewmateSprites = new List<CrewmateTrackerEntry>();
		for (int num3 = 0; num3 < crewmates.Count; num3++)
		{
			crewmateSprites.Add(crewmates.GetInstanceComponent<CrewmateTrackerEntry>(num3));
		}
		((MonoBehaviour)this).StartCoroutine(CoRefreshDelay());
	}

	private IEnumerator CoRefreshDelay()
	{
		yield return (object)new WaitForSeconds(0.1f);
		OnCrewmateKilled();
	}

	private void Start()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		crewmates.DestroyAll();
		crewmateSprites = new List<CrewmateTrackerEntry>(PlayerControl.AllPlayerControls.Count);
		int num = 0;
		foreach (PlayerControl allPlayerControl in PlayerControl.AllPlayerControls)
		{
			if (!((Object)(object)allPlayerControl.Data?.Role == (Object)null) && !allPlayerControl.Data.Role.IsImpostor)
			{
				CrewmateTrackerEntry crewmateTrackerEntry = crewmates.Create<CrewmateTrackerEntry>();
				((Component)crewmateTrackerEntry).transform.localPosition = new Vector3((float)num * crewmateSpacing, 0f, 0f);
				((Component)crewmateTrackerEntry).transform.localScale = crewmateScale;
				crewmateSprites.Add(crewmateTrackerEntry);
				num++;
			}
		}
	}
}
