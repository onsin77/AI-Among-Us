using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AmongUs.QuickChat;

public class QuickChatMenuPhrasesPageCrewmateButton : AbstractQuickChatMenuPhrasesPageButton
{
	[SerializeField]
	[Header("Crewmate Button")]
	private PoolablePlayer player;

	[SerializeField]
	private SpriteRenderer playerMask;

	[SerializeField]
	private Material playerMaterial;

	[SerializeField]
	private SpriteRenderer[] playerMaterialRenderers;

	[SerializeField]
	private SpriteRenderer[] playerUnmaskedRenderers;

	[SerializeField]
	private SpriteRenderer[] playerColorBackgrounds;

	public override string ButtonType => "Crewmate";

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected Obj, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected Obj, but got Unknown
		Button.OnMouseOver.AddListener((UnityAction)OnButtonMouseOver);
		Button.OnMouseOut.AddListener((UnityAction)OnButtonMouseOut);
	}

	protected override float GetInitialWidth()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Collider.size.x;
	}

	protected override void OnSetup()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected Obj, but got Unknown
		Collider.size = new Vector2(Width, Height);
		((Collider2D)Collider).offset = new Vector2(Width / 2f, (0f - Height) / 2f);
		TextRect.sizeDelta = Collider.size;
		Vector3 localPosition = ((Transform)TextRect).localPosition;
		localPosition.x = ((Collider2D)Collider).offset.x;
		localPosition.y = ((Collider2D)Collider).offset.y;
		((Transform)TextRect).localPosition = localPosition;
		((Renderer)playerMask).material = new Material(((Renderer)playerMask).material);
		((Renderer)playerMask).material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
		GameData.PlayerInfo data = PlayerControl.AllPlayerControls.Where((PlayerControl p) => p.PlayerId == Phrase.PlayerId).First().Data;
		player.UpdateFromEitherPlayerDataOrCache(data, PlayerOutfitType.Default, PlayerMaterial.MaskType.ComplexUI, includePet: false);
		((MonoBehaviour)this).StartCoroutine(CoSetPlayerRendererIndexes());
		playerColorBackgrounds.ForEach((SpriteRenderer x) =>
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			x.color = Phrase.ToPlayerBackgroundColor();
		});
	}

	private IEnumerator CoSetPlayerRendererIndexes()
	{
		playerMaterialRenderers.ForEach((SpriteRenderer x) =>
		{
			((Renderer)x).material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
		});
		playerUnmaskedRenderers.ForEach((SpriteRenderer x) =>
		{
			((Renderer)x).material.SetInt(PlayerMaterial.MaskLayer, 180);
		});
		yield return null;
		playerMaterialRenderers.ForEach((SpriteRenderer x) =>
		{
			((Renderer)x).material.SetInt(PlayerMaterial.MaskLayer, MaskLayer);
		});
		playerUnmaskedRenderers.ForEach((SpriteRenderer x) =>
		{
			((Renderer)x).material.SetInt(PlayerMaterial.MaskLayer, 180);
		});
	}

	private void OnButtonMouseOver()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TextRect).localScale = Vector3.one * 0.9f;
	}

	private void OnButtonMouseOut()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Transform)TextRect).localScale = Vector3.one;
	}
}
