using System.Collections.Generic;
using UnityEngine;

public class Cosmicube : MonoBehaviour
{
	[SerializeField]
	private CosmicubeData data;

	private CosmicubeCell startCell;

	public CosmicubeData Data => data;

	public void SetData(CosmicubeData d)
	{
		data = d;
	}

	public void SetMenu(CosmicubeMenu menu)
	{
		CosmicubeCell[] componentsInChildren = ((Component)this).GetComponentsInChildren<CosmicubeCell>();
		foreach (CosmicubeCell cosmicubeCell in componentsInChildren)
		{
			if ((Object)(object)startCell == (Object)null)
			{
				startCell = cosmicubeCell;
			}
			cosmicubeCell.SetMenu(menu);
		}
	}

	public PassiveButton GetFirstCell()
	{
		return ((Component)startCell).GetComponentInChildren<PassiveButton>();
	}

	public IEnumerable<PassiveButton> GetAllButtons()
	{
		return ((Component)this).GetComponentsInChildren<PassiveButton>();
	}
}
