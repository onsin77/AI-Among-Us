using UnityEngine;

public class HowToPlaySceneMovementPageDummyConsole : MonoBehaviour
{
	public int ConsoleId;

	public HowToPlaySceneMovementPagePlayerAnimator[] Players;

	public float UseDistance;

	[HideInInspector]
	private SpriteRenderer rend;

	public void Start()
	{
		rend = ((Component)this).GetComponent<SpriteRenderer>();
	}

	public void FixedUpdate()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)rend).material.SetColor("_OutlineColor", Color.yellow);
		float num = float.MaxValue;
		for (int i = 0; i < Players.Length; i++)
		{
			HowToPlaySceneMovementPagePlayerAnimator howToPlaySceneMovementPagePlayerAnimator = Players[i];
			Vector2 val = Vector2.op_Implicit(((Component)this).transform.position - ((Component)howToPlaySceneMovementPagePlayerAnimator).transform.position);
			val.y += 0.3636f;
			float magnitude = val.magnitude;
			if (magnitude < num)
			{
				num = magnitude;
			}
			if (magnitude < UseDistance)
			{
				howToPlaySceneMovementPagePlayerAnimator.NearbyConsoles |= 1 << ConsoleId;
			}
			else
			{
				howToPlaySceneMovementPagePlayerAnimator.NearbyConsoles &= ~(1 << ConsoleId);
			}
		}
		if (num < UseDistance * 2f)
		{
			((Renderer)rend).material.SetFloat("_Outline", 1f);
			if (num < UseDistance)
			{
				((Renderer)rend).material.SetColor("_AddColor", Color.yellow);
			}
			else
			{
				((Renderer)rend).material.SetColor("_AddColor", Color.clear);
			}
		}
		else
		{
			((Renderer)rend).material.SetFloat("_Outline", 0f);
			((Renderer)rend).material.SetColor("_AddColor", Color.clear);
		}
	}
}
