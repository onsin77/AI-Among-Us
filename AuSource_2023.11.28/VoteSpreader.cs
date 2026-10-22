using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoteSpreader : MonoBehaviour
{
	public List<SpriteRenderer> Votes = new List<SpriteRenderer>();

	public FloatRange VoteRange = new FloatRange(-0.5f, 1.15f);

	public int maxVotesBeforeSmoosh = 7;

	public float CounterY = -0.16f;

	public float adjustRate = 4f;

	private void Update()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		int num = Votes.Count((SpriteRenderer v) =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Vector3 localScale = ((Component)v).transform.localScale;
			return localScale.magnitude > 0.0001f;
		});
		for (int num2 = 0; num2 < num; num2++)
		{
			SpriteRenderer val = Votes[num2];
			Vector2 val2 = Vector2.op_Implicit(new Vector3(0f, CounterY));
			val2.x = VoteRange.SpreadToEdges(num2, Mathf.Max(num, maxVotesBeforeSmoosh));
			Vector3 localPosition = ((Component)val).transform.localPosition;
			localPosition = Vector2.op_Implicit(Vector2.Lerp(Vector2.op_Implicit(localPosition), val2, Time.deltaTime * adjustRate));
			localPosition.z = (float)(-num2) / 50f;
			((Component)val).transform.localPosition = localPosition;
		}
	}

	public void AddVote(SpriteRenderer newVote)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((Component)newVote).transform.localPosition = new Vector3(VoteRange.max, CounterY, 0f);
		Votes.Add(newVote);
	}
}
