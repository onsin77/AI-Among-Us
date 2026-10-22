using System;
using UnityEngine;

public class Tilemap2 : MonoBehaviour
{
	public Sprite[] sprites;

	private Tile2[] tileData;

	public int Width = 1;

	public int Height = 1;

	internal void SetTile(Vector3Int vec, int tileId)
	{
		int num = vec.x + vec.y * Width;
		tileData[num].SpriteId = tileId;
	}

	internal void SetTransformMatrix(Vector3Int vec, Matrix4x4 rot90)
	{
	}

	internal MonoBehaviour GetTile(Vector3Int touchCellPos)
	{
		throw new NotImplementedException();
	}

	internal Vector3Int WorldToCell(Vector2 worldPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = worldPos - Vector2.op_Implicit(((Component)((Component)this).transform).transform.position);
		return new Vector3Int(Mathf.RoundToInt(val.x / (float)Width), Mathf.RoundToInt(val.y / (float)Height), 0);
	}
}
