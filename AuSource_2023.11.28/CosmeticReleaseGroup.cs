using System;
using UnityEngine;

[CreateAssetMenu]
public class CosmeticReleaseGroup : AbstractCosmeticGroup
{
	[SerializeField]
	private string date;

	public string DisplayName => ((Object)this).name;

	public string DateString => date;

	public DateTime Date => Constants.ParseDate(date);
}
