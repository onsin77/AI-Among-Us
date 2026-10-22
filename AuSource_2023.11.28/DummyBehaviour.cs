using System.Collections;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
	public int PlayerIdToVoteFor = -1;

	private PlayerControl myPlayer;

	private FloatRange voteTime = new FloatRange(3f, 8f);

	private bool voted;

	public void Start()
	{
		myPlayer = ((Component)this).GetComponent<PlayerControl>();
	}

	public void Update()
	{
		GameData.PlayerInfo data = myPlayer.Data;
		if (data == null || data.IsDead)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)MeetingHud.Instance))
		{
			if (!voted)
			{
				voted = true;
				((MonoBehaviour)this).StartCoroutine(DoVote());
			}
		}
		else
		{
			voted = false;
		}
	}

	private IEnumerator DoVote()
	{
		yield return (object)new WaitForSeconds(voteTime.Next());
		byte suspectIdx = 253;
		if (PlayerIdToVoteFor == 253)
		{
			suspectIdx = 253;
		}
		if (PlayerIdToVoteFor >= 0)
		{
			suspectIdx = (byte)PlayerIdToVoteFor;
		}
		else
		{
			for (int i = 0; i < 100; i++)
			{
				int num = IntRange.Next(-1, GameData.Instance.PlayerCount);
				if (num < 0)
				{
					break;
				}
				GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[num];
				if (!playerInfo.IsDead)
				{
					suspectIdx = playerInfo.PlayerId;
					break;
				}
			}
		}
		PlayerIdToVoteFor = -1;
		MeetingHud.Instance.CmdCastVote(myPlayer.PlayerId, suspectIdx);
	}
}
