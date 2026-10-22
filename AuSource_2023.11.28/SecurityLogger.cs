using System.Collections;
using System.Linq;
using UnityEngine;

public class SecurityLogger : MonoBehaviour
{
	private static Collider2D[] hits = new Collider2D[15];

	public SecurityLogBehaviour LogParent;

	public SecurityLogBehaviour.SecurityLogLocations MyLocation;

	public float Cooldown = 5f;

	public SpriteRenderer Image;

	public BoxCollider2D Sensor;

	private float[] Timers = new float[15];

	private ContactFilter2D filter;

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		filter = default;
		filter.useLayerMask = true;
		filter.layerMask = LayerMask.op_Implicit(Constants.PlayersOnlyMask);
	}

	public void FixedUpdate()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Timers.Length; i++)
		{
			Timers[i] -= Time.deltaTime;
		}
		if (Object.op_Implicit((Object)(object)PlayerControl.LocalPlayer) && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			return;
		}
		int num = ((Collider2D)Sensor).OverlapCollider(filter, hits);
		int i2 = 0;
		while (i2 < num)
		{
			PlayerControl playerControl = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl p) => (Object)(object)p.Collider == (Object)(object)hits[i2]);
			if (Object.op_Implicit((Object)(object)playerControl) && playerControl.Data != null && !playerControl.Data.IsDead && Timers[playerControl.PlayerId] < 0f)
			{
				Timers[playerControl.PlayerId] = Cooldown;
				LogParent.LogPlayer(playerControl, MyLocation);
				((MonoBehaviour)this).StopAllCoroutines();
				((MonoBehaviour)this).StartCoroutine(BlinkSensor());
			}
			int num2 = i2 + 1;
			i2 = num2;
		}
	}

	private IEnumerator BlinkSensor()
	{
		yield return Effects.Wait(0.1f);
		Image.color = LogParent.BarColors[(byte)MyLocation];
		yield return Effects.Wait(0.1f);
		Image.color = new Color(1f, 1f, 1f, 0.5f);
	}
}
