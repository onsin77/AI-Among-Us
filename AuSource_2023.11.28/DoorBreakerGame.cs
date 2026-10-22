using System.Linq;
using UnityEngine;

public class DoorBreakerGame : Minigame, IDoorMinigame
{
	public OpenableDoor MyDoor;

	public SpriteRenderer[] Buttons;

	public AudioClip FlipSound;

	public void SetDoor(OpenableDoor door)
	{
		MyDoor = door;
	}

	public void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Buttons.Length; i++)
		{
			SpriteRenderer obj = Buttons[i];
			obj.color = Color.gray;
			((Behaviour)((Component)obj).GetComponent<PassiveButton>()).enabled = false;
		}
		int num = 0;
		while (num < 4)
		{
			SpriteRenderer val = Buttons.Random();
			if (!val.flipX)
			{
				val.color = Color.white;
				((Behaviour)((Component)val).GetComponent<PassiveButton>()).enabled = true;
				val.flipX = true;
				num++;
			}
		}
	}

	public void FlipSwitch(SpriteRenderer button)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(FlipSound, loop: false);
		}
		button.color = Color.gray;
		button.flipX = false;
		((Behaviour)((Component)button).GetComponent<PassiveButton>()).enabled = false;
		if (Buttons.All((SpriteRenderer s) => !s.flipX))
		{
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(MyDoor.Id | 0x40));
			MyDoor.SetDoorway(open: true);
			((MonoBehaviour)this).StartCoroutine(CoStartClose(0.4f));
		}
	}
}
