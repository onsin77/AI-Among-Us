using TMPro;
using UnityEngine;

public class SecurityLogGame : Minigame
{
	public float EntryHeight = 0.4f;

	public ObjectPoolBehavior EntryPool;

	public Sprite[] LocationBgs;

	public TextMeshPro SabText;

	public float ScreenHeight = 4f;

	public Scroller scroller;

	private SecurityLogBehaviour Logger;

	public void Update()
	{
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			if (((Behaviour)SabText).isActiveAndEnabled)
			{
				((Component)SabText).gameObject.SetActive(false);
				RefreshScreen();
			}
			else if (Logger.HasNew)
			{
				Logger.HasNew = false;
				RefreshScreen();
			}
		}
		else if (!((Behaviour)SabText).isActiveAndEnabled)
		{
			EntryPool.ReclaimAll();
			((Component)SabText).gameObject.SetActive(true);
		}
	}

	private void Awake()
	{
		SetupInput(disableCursor: false);
		Logger = ((Component)ShipStatus.Instance).GetComponent<SecurityLogBehaviour>();
		if (!PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			RefreshScreen();
		}
		else
		{
			((Component)SabText).gameObject.SetActive(true);
		}
	}

	private void RefreshScreen()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		EntryPool.ReclaimAll();
		for (int i = 0; i < Logger.LogEntries.Count; i++)
		{
			SecurityLogBehaviour.SecurityLogEntry entry = Logger.LogEntries[i];
			LogEntryBubble logEntryBubble = EntryPool.Get<LogEntryBubble>();
			logEntryBubble.SetMaskLayer(50 + i);
			logEntryBubble.DecorateLog(entry, LocationBgs[(byte)entry.Location]);
			((Component)logEntryBubble).transform.localPosition = new Vector3(0f, (float)i * (0f - EntryHeight), 0f);
		}
		float max = Mathf.Max(0f, (float)Logger.LogEntries.Count * EntryHeight - ScreenHeight);
		float scrollPercY = scroller.GetScrollPercY();
		scroller.SetBounds(new FloatRange(0f, max), null);
		if (scrollPercY > 0.95f)
		{
			scroller.ScrollPercentY(1f);
		}
	}
}
