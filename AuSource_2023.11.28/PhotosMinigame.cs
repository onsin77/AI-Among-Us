using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rewired;
using UnityEngine;

public class PhotosMinigame : Minigame
{
	public GamePhotoBehaviour[] photos;

	public Sprite[] PhotoContents;

	public Collider2D PoolHitbox;

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	public Rect PolaroidBounds;

	private Controller controller = new Controller();

	private bool prevHadButton;

	private int currentlyGrabbedObject = -1;

	private bool AllowDraggingPhotos => MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Started;

	private List<Sprite> GetUniqueListOfPhotos(int count)
	{
		List<Sprite> list = PhotoContents.ToList();
		List<Sprite> list2 = new List<Sprite>();
		for (int i = 0; i < count; i++)
		{
			Sprite item = list.Random();
			list.Remove(item);
			list2.Add(item);
		}
		return list2;
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		byte[] data = MyNormTask.Data;
		if (data == null || data.Length != 0)
		{
			ReadInitialData();
		}
		else
		{
			List<Sprite> uniqueListOfPhotos = GetUniqueListOfPhotos(photos.Length);
			for (int i = 0; i < photos.Length; i++)
			{
				photos[i].Image.sprite = uniqueListOfPhotos[i];
			}
			WriteInitialData();
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors((Renderer)(object)selectorHand);
		SetupInput(disableCursor: false);
	}

	private void WriteInitialData()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			for (int i = 0; i < photos.Length; i++)
			{
				GamePhotoBehaviour p = photos[i];
				Vector3 localPosition = ((Component)p).transform.localPosition;
				binaryWriter.Write(PhotoContents.IndexOf((Sprite s) => (Object)(object)s == (Object)(object)p.Image.sprite));
				binaryWriter.Write(localPosition.x);
				binaryWriter.Write(localPosition.y);
			}
		}
		MyNormTask.Data = memoryStream.ToArray();
	}

	private void ReadInitialData()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		using MemoryStream input = new MemoryStream(MyNormTask.Data);
		using BinaryReader binaryReader = new BinaryReader(input);
		for (int i = 0; i < photos.Length; i++)
		{
			GamePhotoBehaviour obj = photos[i];
			obj.Image.sprite = PhotoContents[binaryReader.ReadInt32()];
			((Component)obj).transform.localPosition = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), 1f);
		}
	}

	public IEnumerator Start()
	{
		float z = 0f;
		yield return null;
		GamePhotoBehaviour[] array = photos;
		foreach (GamePhotoBehaviour gamePhotoBehaviour in array)
		{
			gamePhotoBehaviour.zOffset = z;
			((Component)gamePhotoBehaviour).transform.SetLocalZ(1f + z);
			z -= 0.01f;
			bool flag = gamePhotoBehaviour.Hitbox.IsTouching(PoolHitbox);
			gamePhotoBehaviour.TargetColor = (flag ? GamePhotoBehaviour.InWaterPink : Color.white);
			gamePhotoBehaviour.Frame.color = gamePhotoBehaviour.TargetColor;
			gamePhotoBehaviour.Image.color = Color.Lerp(gamePhotoBehaviour.TargetColor, Palette.ClearWhite, MyNormTask.TaskTimer / 60f);
		}
	}

	private void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing == CloseState.None)
		{
			controller.Update();
			GamePhotoBehaviour[] array = photos;
			foreach (GamePhotoBehaviour gamePhotoBehaviour in array)
			{
				gamePhotoBehaviour.Frame.color = gamePhotoBehaviour.TargetColor;
				gamePhotoBehaviour.Image.color = Color.Lerp(gamePhotoBehaviour.TargetColor, Palette.ClearWhite, MyNormTask.TaskTimer / 60f);
				clampToBounds(gamePhotoBehaviour);
			}
			if (Controller.currentTouchType == Controller.TouchType.Joystick)
			{
				HandleJoystick();
			}
			else
			{
				HandleMouse();
			}
			if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted && !controller.AnyTouch && photos.All((GamePhotoBehaviour p) => p.Hitbox.IsTouching(PoolHitbox)))
			{
				MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
				((MonoBehaviour)this).StartCoroutine(CoStartClose());
			}
			else if (MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished && !controller.AnyTouch && !amOpening && photos.All((GamePhotoBehaviour p) => !p.Hitbox.IsTouching(PoolHitbox)))
			{
				MyNormTask.NextStep();
				Close();
			}
		}
	}

	private void HandleJoystick()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		bool button = ReInput.players.GetPlayer(0).GetButton(11);
		Vector3 position = ((Component)selectorObject).transform.position;
		position.x = VirtualCursor.currentPosition.x;
		position.y = VirtualCursor.currentPosition.y;
		((Component)selectorObject).transform.position = position;
		if (button && AllowDraggingPhotos)
		{
			if (!prevHadButton)
			{
				float num = 0f;
				currentlyGrabbedObject = -1;
				for (int i = 0; i < photos.Length; i++)
				{
					Vector2 val = Vector2.op_Implicit(((Component)photos[i]).transform.localPosition - ((Component)selectorObject).transform.localPosition);
					float sqrMagnitude = val.sqrMagnitude;
					if (sqrMagnitude <= 1f && (currentlyGrabbedObject == -1 || sqrMagnitude < num))
					{
						num = sqrMagnitude;
						currentlyGrabbedObject = i;
					}
				}
				if (currentlyGrabbedObject != -1)
				{
					GamePhotoBehaviour gamePhotoBehaviour = photos[currentlyGrabbedObject];
					((MonoBehaviour)gamePhotoBehaviour).StopAllCoroutines();
					((MonoBehaviour)gamePhotoBehaviour).StartCoroutine(gamePhotoBehaviour.Pickup());
					FixZ(gamePhotoBehaviour);
				}
			}
			else if (currentlyGrabbedObject != -1)
			{
				GamePhotoBehaviour obj = photos[currentlyGrabbedObject];
				Vector3 position2 = ((Component)obj).transform.position;
				position2.x = position.x;
				position2.y = position.y;
				((Component)obj).transform.position = position2;
			}
			prevHadButton = true;
		}
		else if (AllowDraggingPhotos)
		{
			if (prevHadButton && currentlyGrabbedObject != -1)
			{
				GamePhotoBehaviour obj2 = photos[currentlyGrabbedObject];
				bool inWater = obj2.Hitbox.IsTouching(PoolHitbox);
				((MonoBehaviour)obj2).StopAllCoroutines();
				((MonoBehaviour)obj2).StartCoroutine(obj2.Drop(inWater));
				WriteInitialData();
			}
			prevHadButton = false;
			currentlyGrabbedObject = -1;
		}
		if (currentlyGrabbedObject != -1 && ((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(false);
		}
		else if (currentlyGrabbedObject == -1 && !((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(true);
		}
	}

	private void HandleMouse()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)selectorObject).gameObject.activeSelf)
		{
			((Component)selectorObject).gameObject.SetActive(false);
		}
		if (currentlyGrabbedObject != -1)
		{
			GamePhotoBehaviour obj = photos[currentlyGrabbedObject];
			bool inWater = obj.Hitbox.IsTouching(PoolHitbox);
			((MonoBehaviour)obj).StopAllCoroutines();
			((MonoBehaviour)obj).StartCoroutine(obj.Drop(inWater));
			WriteInitialData();
			currentlyGrabbedObject = -1;
			prevHadButton = false;
		}
		if (!AllowDraggingPhotos)
		{
			return;
		}
		for (int i = 0; i < photos.Length; i++)
		{
			GamePhotoBehaviour gamePhotoBehaviour = photos[i];
			switch (controller.CheckDrag(gamePhotoBehaviour.Hitbox))
			{
			case DragState.TouchStart:
				((MonoBehaviour)gamePhotoBehaviour).StopAllCoroutines();
				((MonoBehaviour)gamePhotoBehaviour).StartCoroutine(gamePhotoBehaviour.Pickup());
				FixZ(gamePhotoBehaviour);
				break;
			case DragState.Dragging:
			{
				Vector3 position = Vector2.op_Implicit(controller.DragPosition);
				position.z = ((Component)gamePhotoBehaviour).transform.position.z;
				((Component)gamePhotoBehaviour).transform.position = position;
				clampToBounds(gamePhotoBehaviour);
				break;
			}
			case DragState.Released:
			{
				bool inWater2 = gamePhotoBehaviour.Hitbox.IsTouching(PoolHitbox);
				((MonoBehaviour)gamePhotoBehaviour).StopAllCoroutines();
				((MonoBehaviour)gamePhotoBehaviour).StartCoroutine(gamePhotoBehaviour.Drop(inWater2));
				WriteInitialData();
				break;
			}
			}
		}
	}

	private void clampToBounds(GamePhotoBehaviour photo)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Component)photo).transform.localPosition;
		localPosition.x = Math.Max(localPosition.x, PolaroidBounds.xMin);
		localPosition.x = Math.Min(localPosition.x, PolaroidBounds.xMax);
		localPosition.y = Math.Max(localPosition.y, PolaroidBounds.yMin);
		localPosition.y = Math.Min(localPosition.y, PolaroidBounds.yMax);
		((Component)photo).transform.localPosition = localPosition;
	}

	private void FixZ(GamePhotoBehaviour current)
	{
		if (current.zOffset == (float)photos.Length / -100f)
		{
			return;
		}
		current.zOffset = (float)photos.Length / -100f;
		GamePhotoBehaviour[] array = photos;
		foreach (GamePhotoBehaviour gamePhotoBehaviour in array)
		{
			if (!((Object)(object)gamePhotoBehaviour == (Object)(object)current))
			{
				gamePhotoBehaviour.zOffset += 0.01f;
				((Component)gamePhotoBehaviour).transform.SetLocalZ(1f + gamePhotoBehaviour.zOffset);
			}
		}
	}
}
