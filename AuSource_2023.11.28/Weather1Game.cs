using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Weather1Game : Minigame
{
	private static int[] BarrierValidX = new int[10] { -10, -8, -6, -4, -2, 0, 2, 4, 6, 8 };

	private static int[] BarrierValidY = new int[3] { -2, 0, 2 };

	private const int MinX = -10;

	private const int MaxX = 8;

	private const int MinY = -3;

	private const int MaxY = 3;

	public Tilemap BarrierMap;

	public Tile fillTile;

	public Tile controlTile;

	public Tile barrierTile;

	public SpriteRenderer pulseCircle1;

	public SpriteRenderer pulseCircle2;

	public AudioClip NodeMove;

	private Controller control = new Controller();

	private bool inControl;

	private Vector3Int controlTilePos = new Vector3Int(-9, 3, 0);

	private static Vector3Int[] Directions = new Vector3Int[4]
	{
		Vector3Int.up,
		Vector3Int.down,
		Vector3Int.left,
		Vector3Int.right
	};

	private float moveCooldown;

	public void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int val = new Vector3Int(-9, 3, 0);
		HashSet<Vector3Int> hashSet = new HashSet<Vector3Int>();
		hashSet.Add(val);
		SolveMaze(val, hashSet);
		Matrix4x4 val2 = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f));
		new List<Vector3Int>();
		Vector3Int val3 = default;
		val3.x = -10;
		while (val3.x <= 8)
		{
			bool flag = val3.x % 2 == 0;
			val3.y = -3;
			int y;
			while (val3.y <= 3)
			{
				bool flag2 = val3.y % 2 == 0;
				if (PointIsValid(val3) && !hashSet.Contains(val3) && flag == flag2 && BoolRange.Next(0.75f))
				{
					BarrierMap.SetTile(val3, (TileBase)(object)barrierTile);
					if (flag)
					{
						BarrierMap.SetTransformMatrix(val3, val2);
					}
				}
				y = val3.y + 1;
				val3.y = y;
			}
			y = val3.x + 1;
			val3.x = y;
		}
		SetupInput(disableCursor: true);
	}

	private bool SolveMaze(Vector3Int curPos, HashSet<Vector3Int> solution)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (solution.Count > 50)
		{
			return false;
		}
		bool[] array = new bool[4];
		while (Contains(array, item: false))
		{
			int num = array.RandomIdx();
			while (array[num])
			{
				num = (num + 1) % array.Length;
			}
			array[num] = true;
			Vector3Int val = Directions[num] + curPos;
			if (PointIsValid(val) && solution.Add(val))
			{
				if (val.x == 7 && val.y == -3)
				{
					return true;
				}
				if (SolveMaze(val, solution))
				{
					return true;
				}
				solution.Remove(val);
			}
		}
		return false;
	}

	public void Update()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_050b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (amClosing != CloseState.None)
		{
			return;
		}
		control.Update();
		if ((Object)(object)BarrierMap.GetTile(new Vector3Int(7, -3, 0)) == (Object)(object)controlTile)
		{
			((Renderer)pulseCircle1).enabled = false;
			((Renderer)pulseCircle2).enabled = false;
			MyNormTask.NextStep();
			((MonoBehaviour)this).StartCoroutine(CoStartClose());
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player.GetButton(11))
			{
				if (!inControl)
				{
					inControl = true;
				}
				if (moveCooldown > 0f)
				{
					moveCooldown -= Time.deltaTime;
				}
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				Vector2 val = new Vector2(Mathf.Abs(axis2DRaw.x), Mathf.Abs(axis2DRaw.y));
				float num = Mathf.Max(val.x, val.y);
				float num2 = Mathf.Min(val.x, val.y);
				Vector3Int val2 = Vector3Int.zero;
				bool flag = false;
				if (num > 0.7f && num2 < 0.2f)
				{
					if (val.x > val.y)
					{
						val2 = new Vector3Int((int)Mathf.Sign(axis2DRaw.x), 0, 0);
						flag = true;
					}
					else
					{
						val2 = new Vector3Int(0, (int)Mathf.Sign(axis2DRaw.y), 0);
						flag = true;
					}
				}
				else
				{
					moveCooldown = 0f;
				}
				if (flag && moveCooldown <= 0f)
				{
					moveCooldown = 0.15f;
					Vector3Int val3 = controlTilePos + val2;
					if (!Object.op_Implicit((Object)(object)BarrierMap.GetTile(val3)) && PointIsValid(val3) && !AnythingBetween(controlTilePos, val3))
					{
						FillLine(controlTilePos, val3);
						controlTilePos = val3;
						BarrierMap.SetTile(controlTilePos, (TileBase)(object)controlTile);
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySoundImmediate(NodeMove, loop: false);
						}
					}
				}
			}
			else
			{
				moveCooldown = 0f;
				if (inControl)
				{
					inControl = false;
					Vector3Int val4 = default;
					val4.x = -10;
					while (val4.x <= 8)
					{
						val4.y = -3;
						int y;
						while (val4.y <= 3)
						{
							if ((Object)(object)BarrierMap.GetTile(val4) == (Object)(object)fillTile)
							{
								BarrierMap.SetTile(val4, (TileBase)null);
							}
							y = val4.y + 1;
							val4.y = y;
						}
						y = val4.x + 1;
						val4.x = y;
					}
					BarrierMap.SetTile(controlTilePos, (TileBase)null);
					controlTilePos.x = -9;
					controlTilePos.y = 3;
					BarrierMap.SetTile(controlTilePos, (TileBase)(object)controlTile);
				}
			}
		}
		else if (control.AnyTouch)
		{
			for (int i = 0; i < control.Touches.Length; i++)
			{
				Controller.TouchState touch = control.GetTouch(i);
				Vector3Int val5 = ((GridLayout)BarrierMap).WorldToCell(Vector2.op_Implicit(touch.Position));
				TileBase tile = BarrierMap.GetTile(val5);
				if (touch.TouchStart)
				{
					if ((Object)(object)tile == (Object)(object)controlTile)
					{
						inControl = true;
						controlTilePos = val5;
					}
				}
				else if (inControl && !Object.op_Implicit((Object)(object)tile) && PointIsValid(val5) && !AnythingBetween(controlTilePos, val5))
				{
					FillLine(controlTilePos, val5);
					controlTilePos = val5;
					BarrierMap.SetTile(controlTilePos, (TileBase)(object)controlTile);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySoundImmediate(NodeMove, loop: false);
					}
				}
			}
		}
		else if (control.AnyTouchUp)
		{
			for (int j = 0; j < control.Touches.Length; j++)
			{
				control.GetTouch(j);
				if (!inControl)
				{
					continue;
				}
				Vector3Int val6 = default;
				val6.x = -10;
				while (val6.x <= 8)
				{
					val6.y = -3;
					int y;
					while (val6.y <= 3)
					{
						if ((Object)(object)BarrierMap.GetTile(val6) == (Object)(object)fillTile)
						{
							BarrierMap.SetTile(val6, (TileBase)null);
						}
						y = val6.y + 1;
						val6.y = y;
					}
					y = val6.x + 1;
					val6.x = y;
				}
				BarrierMap.SetTile(controlTilePos, (TileBase)null);
				controlTilePos.x = -9;
				controlTilePos.y = 3;
				BarrierMap.SetTile(controlTilePos, (TileBase)(object)controlTile);
			}
		}
		else
		{
			inControl = false;
		}
		((Component)pulseCircle1).transform.position = ((GridLayout)BarrierMap).CellToWorld(controlTilePos) + new Vector3(0.16f, 0.16f, 0f);
	}

	private void FillLine(Vector3Int controlTilePos, Vector3Int touchCellPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int val = controlTilePos;
		BarrierMap.SetTile(val, (TileBase)(object)fillTile);
		if (controlTilePos.x == touchCellPos.x)
		{
			int num = (int)Mathf.Sign((float)(touchCellPos.y - controlTilePos.y));
			while (true)
			{
				int num2 = (val.y += num);
				if (num2 != touchCellPos.y)
				{
					BarrierMap.SetTile(val, (TileBase)(object)fillTile);
					continue;
				}
				break;
			}
		}
		else
		{
			if (controlTilePos.y != touchCellPos.y)
			{
				return;
			}
			int num4 = (int)Mathf.Sign((float)(touchCellPos.x - controlTilePos.x));
			while (true)
			{
				int num2 = (val.x += num4);
				if (num2 != touchCellPos.x)
				{
					BarrierMap.SetTile(val, (TileBase)(object)fillTile);
					continue;
				}
				break;
			}
		}
	}

	private bool AnythingBetween(Vector3Int controlTilePos, Vector3Int touchCellPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int val = controlTilePos;
		if (controlTilePos.x == touchCellPos.x)
		{
			int num = (int)Mathf.Sign((float)(touchCellPos.y - controlTilePos.y));
			while (true)
			{
				int num2 = (val.y += num);
				if (num2 == touchCellPos.y)
				{
					break;
				}
				if (Object.op_Implicit((Object)(object)BarrierMap.GetTile(val)) || !PointIsValid(val))
				{
					return true;
				}
			}
			return false;
		}
		if (controlTilePos.y == touchCellPos.y)
		{
			int num4 = (int)Mathf.Sign((float)(touchCellPos.x - controlTilePos.x));
			while (true)
			{
				int num2 = (val.x += num4);
				if (num2 == touchCellPos.x)
				{
					break;
				}
				if (Object.op_Implicit((Object)(object)BarrierMap.GetTile(val)) || !PointIsValid(val))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private bool PointIsValid(Vector3Int touchCellPos)
	{
		bool flag = touchCellPos.x % 2 == 0;
		bool result = touchCellPos.y % 2 == 0;
		if (touchCellPos.x <= 8 && touchCellPos.x >= -10 && touchCellPos.y <= 3 && touchCellPos.y >= -3)
		{
			if (flag)
			{
				return result;
			}
			return true;
		}
		return false;
	}

	private bool Contains<T>(T[] self, T item) where T : IComparable
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (self[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public Weather1Game()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}

	static Weather1Game()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
	}
}
