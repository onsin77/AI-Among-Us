using System.Collections;
using UnityEngine;

public class HowToPlaySceneMovementPage : AbstractHowToPlayScenePage
{
	public HowToPlaySceneMovementPagePlayerAnimator[] players;

	public HowToPlaySceneMovementPageDummyConsole[] Consoles;

	public Vector2[] WayPoints;

	public Camera backupCam;

	public void OnDrawGizmos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < WayPoints.Length; i++)
		{
			Vector2 val = WayPoints[i];
			Vector2 val2 = WayPoints[(i + 1) % WayPoints.Length];
			Gizmos.DrawLine(Vector2.op_Implicit(val), Vector2.op_Implicit(val2));
		}
	}

	public void OnEnable()
	{
		backupCam.cullingMask = 0;
		((MonoBehaviour)this).StartCoroutine(RunPlayer(0));
		if (players.Length > 1)
		{
			((MonoBehaviour)this).StartCoroutine(RunPlayer(1));
		}
	}

	public void OnDisable()
	{
		backupCam.cullingMask = 0x7FFFFFFF ^ LayerMask.GetMask(new string[1] { "UI" });
	}

	private IEnumerator RunPlayer(int idx)
	{
		HowToPlaySceneMovementPagePlayerAnimator myPlayer = players[idx];
		while (true)
		{
			int i = 0;
			while (i < WayPoints.Length)
			{
				bool willInterrupt = i == 2 || i == 5;
				yield return myPlayer.WalkPlayerTo(WayPoints[i], willInterrupt, 0.1f);
				if (willInterrupt)
				{
					yield return DoUse(idx, (i != 2) ? 1 : 0);
				}
				int num = i + 1;
				i = num;
			}
		}
	}

	private IEnumerator DoUse(int idx, int consoleid)
	{
		HowToPlaySceneMovementPagePlayerAnimator myPlayer = players[idx];
		yield return WaitForSeconds(0.2f);
		if (idx == 0)
		{
			yield return myPlayer.finger.MoveTo(Vector2.op_Implicit(((Component)myPlayer.UseButton).transform.position), 0.75f);
		}
		else
		{
			yield return myPlayer.finger.MoveTo(Vector2.op_Implicit(((Component)Consoles[consoleid]).transform.position), 0.75f);
		}
		yield return WaitForSeconds(0.2f);
		yield return myPlayer.finger.DoClick(0.4f);
		yield return WaitForSeconds(0.2f);
		if (!(myPlayer.joystick is HowToPlaySceneMovementPageDemoKeyboardStick))
		{
			yield return myPlayer.finger.MoveTo(Vector2.op_Implicit(((Component)myPlayer.joystick).transform.position), 0.75f);
		}
		else
		{
			yield return WaitForSeconds(0.75f);
		}
	}

	public static IEnumerator WaitForSeconds(float duration)
	{
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			yield return null;
		}
	}
}
