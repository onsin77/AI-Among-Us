using PowerTools;
using UnityEngine;

public class ReactorShipRoom : SkeldShipRoom, IReactorShipRoom
{
	public Sprite normalManifolds;

	public Sprite meltdownManifolds;

	public SpriteRenderer Manifolds;

	public AnimationClip normalReactor;

	public AnimationClip meltdownReactor;

	public SpriteAnim Reactor;

	public AnimationClip normalHighFloor;

	public AnimationClip meltdownHighFloor;

	public SpriteAnim HighFloor;

	public AnimationClip normalMidFloor;

	public AnimationClip meltdownMidFloor;

	public SpriteAnim MidFloor1;

	public SpriteAnim MidFloor2;

	public AnimationClip normalLowFloor;

	public AnimationClip meltdownLowFloor;

	public SpriteAnim LowFloor;

	public AnimationClip[] normalPipes;

	public AnimationClip[] meltdownPipes;

	public SpriteAnim[] Pipes;

	public SupressorBehaviour[] Supressors;

	public MeshRenderer Orb;

	public Rotater OrbGlass;

	public ChainBehaviour[] Arms;

	public void StartMeltdown()
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Manifolds))
		{
			Manifolds.sprite = meltdownManifolds;
		}
		if (Object.op_Implicit((Object)(object)Reactor))
		{
			Reactor.Play(meltdownReactor);
		}
		if (Object.op_Implicit((Object)(object)HighFloor))
		{
			HighFloor.Play(meltdownHighFloor);
		}
		if (Object.op_Implicit((Object)(object)MidFloor1))
		{
			MidFloor1.Play(meltdownMidFloor);
		}
		if (Object.op_Implicit((Object)(object)MidFloor2))
		{
			MidFloor2.Play(meltdownMidFloor);
		}
		if (Object.op_Implicit((Object)(object)LowFloor))
		{
			LowFloor.Play(meltdownLowFloor);
		}
		for (int i = 0; i < Pipes.Length; i++)
		{
			Pipes[i].Play(meltdownPipes[i]);
		}
		for (int j = 0; j < Supressors.Length; j++)
		{
			Supressors[j].Deactivate();
		}
		if (Object.op_Implicit((Object)(object)Orb))
		{
			((Renderer)Orb).material.SetColor("_Color", Color.red);
			((Renderer)Orb).material.SetFloat("_Speed", 3f);
		}
		if (Object.op_Implicit((Object)(object)OrbGlass))
		{
			OrbGlass.DegreesPerSecond = 1440f;
		}
		for (int k = 0; k < Arms.Length; k++)
		{
			Arms[k].SwingRange.min = -0.75f;
			Arms[k].SwingRange.max = 0.75f;
		}
	}

	public void StopMeltdown()
	{
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Manifolds))
		{
			Manifolds.sprite = normalManifolds;
		}
		if (Object.op_Implicit((Object)(object)Reactor))
		{
			Reactor.Play(normalReactor);
		}
		if (Object.op_Implicit((Object)(object)HighFloor))
		{
			HighFloor.Play(normalHighFloor);
		}
		if (Object.op_Implicit((Object)(object)MidFloor1))
		{
			MidFloor1.Play(normalMidFloor);
		}
		if (Object.op_Implicit((Object)(object)MidFloor2))
		{
			MidFloor2.Play(normalMidFloor);
		}
		if (Object.op_Implicit((Object)(object)LowFloor))
		{
			LowFloor.Play(normalLowFloor);
		}
		for (int i = 0; i < Pipes.Length; i++)
		{
			Pipes[i].Play(normalPipes[i]);
		}
		for (int j = 0; j < Supressors.Length; j++)
		{
			Supressors[j].Activate();
		}
		if (Object.op_Implicit((Object)(object)Orb))
		{
			((Renderer)Orb).material.SetColor("_Color", Color.white);
			((Renderer)Orb).material.SetFloat("_Speed", 1f);
		}
		if (Object.op_Implicit((Object)(object)OrbGlass))
		{
			OrbGlass.DegreesPerSecond = 720f;
		}
		for (int k = 0; k < Arms.Length; k++)
		{
			Arms[k].SwingRange.min = -0.15f;
			Arms[k].SwingRange.max = 0.15f;
		}
	}
}
