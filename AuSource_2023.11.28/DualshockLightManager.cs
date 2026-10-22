using System.Collections.Generic;
using Rewired;
using Rewired.ControllerExtensions;
using UnityEngine;

public class DualshockLightManager : DestroyableSingleton<DualshockLightManager>
{
	public class LightOverlayHandle
	{
		public Color color;

		public float intensity = 1f;

		public void Dispose()
		{
			DestroyableSingleton<DualshockLightManager>.Instance.overlays.Remove(this);
		}
	}

	public class LightOverlayFlash
	{
		public LightOverlayHandle handle;

		public AudioClip clip;

		public float t;

		public float duration;

		private static float[] samples = new float[2];

		public bool Alive => t <= duration;

		public void Init()
		{
			if ((Object)(object)clip != (Object)null)
			{
				duration = Mathf.Max(duration, clip.length);
			}
		}

		public void Dispose()
		{
			if (handle != null)
			{
				handle.Dispose();
				handle = null;
			}
		}

		public void Update(float deltaTime)
		{
			float num = 1f;
			if ((Object)(object)clip != (Object)null)
			{
				float num2 = t / clip.length;
				if (num2 > 1f)
				{
					num = 0f;
					t = duration;
				}
				else
				{
					int num3 = (int)(Mathf.Clamp01(num2) * (float)(clip.samples - 1));
					if (clip.GetData(samples, num3))
					{
						num *= Mathf.Abs(samples[0]);
					}
				}
			}
			t += deltaTime;
			handle.color.a = Mathf.Clamp01(num * 10f);
		}
	}

	private const float lightIntensity = 0.5f;

	private Color baseColor = new Color(0f, 0.5f, 1f, 1f);

	private Color oldColor = Color.white;

	private List<LightOverlayHandle> overlays = new List<LightOverlayHandle>();

	private List<LightOverlayFlash> lightOverlayFlashes = new List<LightOverlayFlash>();

	public Color BaseColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return baseColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			baseColor = value;
		}
	}

	public LightOverlayHandle AllocateLight()
	{
		LightOverlayHandle lightOverlayHandle = new LightOverlayHandle();
		overlays.Add(lightOverlayHandle);
		return lightOverlayHandle;
	}

	private float GetExternalBrightnessFromElectrical()
	{
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance) && ShipStatus.Instance.Systems.Count > 7)
		{
			float num = (float)(int)((SwitchSystem)ShipStatus.Instance.Systems[SystemTypes.Electrical]).Value / 255f;
			return Mathf.Lerp(0.05f, 1f, num);
		}
		return 1f;
	}

	private void Update()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		Player player = ReInput.players.GetPlayer(0);
		if (player.controllers.joystickCount <= 0)
		{
			return;
		}
		IDualShock4Extension extension = ((Controller)player.controllers.Joysticks[0]).GetExtension<IDualShock4Extension>();
		if (extension == null)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)ShipStatus.Instance) || Object.op_Implicit((Object)(object)LobbyBehaviour.Instance))
		{
			Color val = baseColor;
			val.a *= GetExternalBrightnessFromElectrical() * 0.5f;
			for (int num = lightOverlayFlashes.Count - 1; num >= 0; num--)
			{
				if (lightOverlayFlashes[num].Alive)
				{
					lightOverlayFlashes[num].Update(Time.deltaTime);
				}
				else
				{
					lightOverlayFlashes[num].Dispose();
					lightOverlayFlashes.RemoveAt(num);
				}
			}
			foreach (LightOverlayHandle overlay in overlays)
			{
				Color color = overlay.color;
				color.a = overlay.intensity;
				val = Color.Lerp(val, color, overlay.color.a);
			}
			if (oldColor != val)
			{
				oldColor = val;
				if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
				{
					extension.SetLightColor(val);
				}
			}
		}
		else if (oldColor != Palette.Blue)
		{
			oldColor = Palette.Blue;
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
			{
				extension.SetLightColor(Palette.Blue);
			}
		}
	}

	public static void Flash(Color c, float intensity, AudioClip clip)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		LightOverlayFlash lightOverlayFlash = new LightOverlayFlash();
		lightOverlayFlash.handle = DestroyableSingleton<DualshockLightManager>.Instance.AllocateLight();
		lightOverlayFlash.handle.color = c;
		lightOverlayFlash.handle.intensity = intensity;
		lightOverlayFlash.clip = clip;
		lightOverlayFlash.Init();
		DestroyableSingleton<DualshockLightManager>.Instance.lightOverlayFlashes.Add(lightOverlayFlash);
	}

	public DualshockLightManager()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
	}
}
