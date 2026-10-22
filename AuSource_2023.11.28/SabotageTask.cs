using System.Collections.Generic;
using UnityEngine;

public abstract class SabotageTask : PlayerTask
{
	protected bool didContribute;

	public ArrowBehaviour[] Arrows;

	public void MarkContributed()
	{
		didContribute = true;
	}

	protected void SetupArrows()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Owner) && Owner.AmOwner)
		{
			List<Console> list = FindConsoles();
			for (int i = 0; i < list.Count; i++)
			{
				int consoleId = list[i].ConsoleId;
				Arrows[consoleId].target = ((Component)list[i]).transform.position;
				((Component)Arrows[consoleId]).gameObject.SetActive(true);
			}
		}
		else
		{
			for (int j = 0; j < Arrows.Length; j++)
			{
				((Component)Arrows[j]).gameObject.SetActive(false);
			}
		}
	}
}
