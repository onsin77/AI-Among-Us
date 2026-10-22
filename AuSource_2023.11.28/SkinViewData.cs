using UnityEngine;

public class SkinViewData : ScriptableObject
{
	public Sprite IdleFrame;

	public Sprite EjectFrame;

	public HatData RelatedHat;

	public VisorData RelatedVisor;

	public bool MatchPlayerColor;

	[Header("Idle Animations")]
	public AnimationClip IdleAnim;

	public AnimationClip IdleLeftAnim;

	[Header("Run Animations")]
	public AnimationClip RunAnim;

	public AnimationClip RunLeftAnim;

	[Header("Vent Animations")]
	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public AnimationClip EnterLeftVentAnim;

	public AnimationClip ExitLeftVentAnim;

	[Header("Climb Animations")]
	public AnimationClip ClimbAnim;

	public AnimationClip ClimbDownAnim;

	[Header("Spawn Animations")]
	public AnimationClip SpawnAnim;

	public AnimationClip SpawnLeftAnim;

	[Header("Kill Animations")]
	public AnimationClip KillTongueImpostor;

	public AnimationClip KillTongueVictim;

	public AnimationClip KillShootImpostor;

	public AnimationClip KillShootVictim;

	public AnimationClip KillNeckImpostor;

	public AnimationClip KillNeckVictim;

	public AnimationClip KillStabImpostor;

	public AnimationClip KillStabVictim;

	public AnimationClip KillRHMVictim;

	public int CustomKillAnimID;
}
