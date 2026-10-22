using System;
using UnityEngine;

[Serializable]
public class StoreTabButton : TabButton
{
	public float ItemListStartY = 2f;

	public FloatRange XRange = new FloatRange(-1f, 1f);

	public int NumPerRow = 4;

	public StoreTab storeTab;

	public GameObject NewItemIcon;
}
