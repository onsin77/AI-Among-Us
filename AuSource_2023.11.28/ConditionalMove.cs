using System;
using UnityEngine;

public class ConditionalMove : MonoBehaviour
{
	[Serializable]
	public struct MoveForPlatforms
	{
		public RuntimePlatform runtimePlatform;

		public Vector3 offset;
	}

	public MoveForPlatforms[] moveForPlatforms = new MoveForPlatforms[0];

	private void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		MoveForPlatforms[] array = this.moveForPlatforms;
		for (int i = 0; i < array.Length; i++)
		{
			MoveForPlatforms moveForPlatforms = array[i];
			if (Application.platform == moveForPlatforms.runtimePlatform)
			{
				Transform transform = ((Component)this).transform;
				transform.position += moveForPlatforms.offset;
			}
		}
	}
}
