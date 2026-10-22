using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapCountOverlay : MonoBehaviour
{
	public AlphaPulse BackgroundColor;

	public TextMeshPro SabotageText;

	public CounterArea[] CountAreas;

	private readonly Collider2D[] buffer = new Collider2D[45];

	private ContactFilter2D filter;

	private float timer;

	private bool isSab;

	private bool showLivePlayerPosition;

	private bool includeDeadBodies;

	public void SetOptions(bool showLivePlayerPosition, bool includeDeadBodies)
	{
		this.showLivePlayerPosition = showLivePlayerPosition;
		this.includeDeadBodies = includeDeadBodies;
	}

	private void Awake()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		filter.useLayerMask = true;
		filter.layerMask = LayerMask.op_Implicit(Constants.LivingPlayersOnlyMask);
		filter.useTriggers = true;
	}

	private void OnEnable()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		BackgroundColor.SetColor(PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer) ? Palette.DisabledGrey : Color.green);
		timer = 1f;
	}

	private void OnDisable()
	{
		for (int i = 0; i < CountAreas.Length; i++)
		{
			CountAreas[i].UpdateCount(0);
		}
	}

	private void Update()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.deltaTime;
		if (timer < 0.1f)
		{
			return;
		}
		timer = 0f;
		if (!isSab && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			isSab = true;
			BackgroundColor.SetColor(Palette.DisabledGrey);
			((Component)SabotageText).gameObject.SetActive(true);
			return;
		}
		if (isSab && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			isSab = false;
			BackgroundColor.SetColor(Color.green);
			((Component)SabotageText).gameObject.SetActive(false);
		}
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < CountAreas.Length; i++)
		{
			CounterArea counterArea = CountAreas[i];
			if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
			{
				if (ShipStatus.Instance.FastRooms.TryGetValue(counterArea.RoomType, out var value) && Object.op_Implicit((Object)(object)value.roomArea))
				{
					int num = value.roomArea.OverlapCollider(filter, buffer);
					int num2 = 0;
					for (int j = 0; j < num; j++)
					{
						Collider2D val = buffer[j];
						if (((Component)val).CompareTag("DeadBody") && includeDeadBodies)
						{
							DeadBody component = ((Component)val).GetComponent<DeadBody>();
							if ((Object)(object)component != (Object)null && hashSet.Add(component.ParentId))
							{
								num2++;
							}
						}
						else if (!val.isTrigger)
						{
							PlayerControl component2 = ((Component)val).GetComponent<PlayerControl>();
							if (Object.op_Implicit((Object)(object)component2) && component2.Data != null && !component2.Data.Disconnected && !component2.Data.IsDead && (showLivePlayerPosition || !component2.AmOwner) && hashSet.Add(component2.PlayerId))
							{
								num2++;
							}
						}
					}
					counterArea.UpdateCount(num2);
				}
				else
				{
					Debug.LogWarning((object)("Couldn't find counter for:" + counterArea.RoomType));
				}
			}
			else
			{
				counterArea.UpdateCount(0);
			}
		}
	}
}
