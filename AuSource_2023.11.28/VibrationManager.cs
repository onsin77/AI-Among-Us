using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VibrationManager : DestroyableSingleton<VibrationManager>
{
	public enum VibrationFalloff
	{
		None,
		Linear,
		InverseLinear
	}

	private class LocalVibration
	{
		public Vector2 intensity;

		public float t;

		public float duration;

		public VibrationFalloff falloff;

		public AudioClip clip;

		public bool loopClip;

		public string name;

		public bool Alive => t <= duration;

		public void Init()
		{
			if ((Object)(object)clip != (Object)null)
			{
				duration = Mathf.Max(duration, clip.length);
			}
		}

		public Vector2 UpdateIntensity(float deltaTime)
		{
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			float num = 1f;
			switch (falloff)
			{
			case VibrationFalloff.Linear:
			{
				float num2 = Mathf.Clamp01(t / duration);
				num = 1f - num2;
				break;
			}
			case VibrationFalloff.InverseLinear:
				num = Mathf.Clamp01(t / duration);
				break;
			}
			if ((Object)(object)clip != (Object)null)
			{
				float num3 = t / clip.length;
				if (num3 > 1f)
				{
					num = 0f;
					t = duration;
				}
				else
				{
					int num4 = (int)(Mathf.Clamp01(num3) * (float)(clip.samples - 1));
					if (clip.GetData(samples, num4))
					{
						num *= Mathf.Abs(samples[0]);
					}
				}
			}
			t += deltaTime;
			if (t >= duration && loopClip)
			{
				t -= duration;
				if (t < 0f)
				{
					t = 0f;
				}
			}
			return intensity * num;
		}
	}

	private class WorldVibration
	{
		public float intensity;

		public Vector2 location;

		public float radius;

		public float t;

		public float duration;

		public VibrationFalloff falloff;

		public AudioClip clip;

		public bool loopClip;

		public string name;

		public bool Alive => t <= duration;

		public void Init()
		{
			if ((Object)(object)clip != (Object)null)
			{
				duration = Mathf.Max(duration, clip.length);
			}
		}

		public Vector2 UpdateIntensity(Vector2 cameraPos, float deltaTime)
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
			float num = 1f;
			switch (falloff)
			{
			case VibrationFalloff.Linear:
			{
				float num2 = Mathf.Clamp01(t / duration);
				num = 1f - num2;
				break;
			}
			case VibrationFalloff.InverseLinear:
				num = Mathf.Clamp01(t / duration);
				break;
			}
			Vector2 val = location - cameraPos;
			float magnitude = val.magnitude;
			val /= magnitude;
			Vector2 val2 = new Vector2(1f - Mathf.Clamp01(val.x), 1f - Mathf.Clamp01(0f - val.x));
			num *= 1f - Mathf.Clamp01(magnitude / radius);
			if ((Object)(object)clip != (Object)null)
			{
				float num3 = t / clip.length;
				if (num3 > 1f)
				{
					num = 0f;
					t = duration;
				}
				else if (clip.channels == 2)
				{
					int num4 = (int)(Mathf.Clamp01(num3) * (float)(clip.samples / 2 - 1)) * 2;
					if (clip.GetData(samples, num4))
					{
						val2.x *= Mathf.Abs(samples[0]);
						val2.y *= Mathf.Abs(samples[1]);
					}
				}
				else
				{
					int num5 = (int)(Mathf.Clamp01(num3) * (float)(clip.samples - 1));
					if (clip.GetData(samples, num5))
					{
						num *= Mathf.Abs(samples[0]);
					}
				}
			}
			t += deltaTime;
			if (t >= duration && loopClip)
			{
				t -= duration;
				if (t < 0f)
				{
					t = 0f;
				}
			}
			return intensity * num * val2;
		}
	}

	public const float SMALL_INTENSITY = 0.3f;

	public const float MEDIUM_INTENSITY = 0.7f;

	public const float FRAME_DURATION = 0.02f;

	private List<LocalVibration> currentLocalVibration = new List<LocalVibration>();

	private List<WorldVibration> currentWorldVibration = new List<WorldVibration>();

	private Vector2 singleFrameVibration = Vector2.zero;

	private bool hasFrameVibration;

	private bool zeroNextFrame;

	public int numVibrationsActive;

	public Vector2 currentVibration;

	private Camera cam;

	private WorldVibration tempSingleFrameWorldVibration = new WorldVibration();

	private WorldVibration tempAmbientSoundVibration = new WorldVibration();

	private bool shouldVibrate = true;

	private static float[] samples = new float[2];

	private void Start()
	{
		cam = Camera.main;
	}

	private void OnEnable()
	{
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
	}

	private void OnDisable()
	{
		SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
	}

	private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		ClearAllVibration();
	}

	public static void ClearAllVibration()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Clear();
		DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Clear();
		DestroyableSingleton<VibrationManager>.Instance.currentVibration = Vector2.zero;
	}

	private void Update()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)cam))
		{
			cam = Camera.main;
		}
		if (!shouldVibrate)
		{
			return;
		}
		if (RaycastAmbientSoundPlayer.players.Count > 0)
		{
			Vector2 cameraPos = Vector2.op_Implicit(((Component)cam).transform.position);
			foreach (RaycastAmbientSoundPlayer player3 in RaycastAmbientSoundPlayer.players)
			{
				if (Object.op_Implicit((Object)(object)player3.AmbientSound))
				{
					player3.t += Time.deltaTime;
					if (player3.t > player3.AmbientSound.length)
					{
						player3.t -= player3.AmbientSound.length;
					}
					if (player3.ambientVolume > 0.01f && player3.ShouldVibrate)
					{
						tempAmbientSoundVibration.clip = player3.AmbientSound;
						tempAmbientSoundVibration.duration = player3.AmbientSound.length;
						tempAmbientSoundVibration.intensity = player3.ambientVolume * 0.5f;
						tempAmbientSoundVibration.location = Vector2.op_Implicit(((Component)player3).transform.position);
						tempAmbientSoundVibration.radius = ((player3.AmbientMaxDist > 0f) ? player3.AmbientMaxDist : 10000f);
						tempAmbientSoundVibration.t = player3.t;
						singleFrameVibration += tempAmbientSoundVibration.UpdateIntensity(cameraPos, 0f);
						hasFrameVibration = true;
					}
				}
			}
		}
		if (currentLocalVibration.Count > 0 || currentWorldVibration.Count > 0 || hasFrameVibration)
		{
			Player player = ReInput.players.GetPlayer(0);
			Vector2 val = singleFrameVibration;
			singleFrameVibration = Vector2.zero;
			for (int num = currentLocalVibration.Count - 1; num >= 0; num--)
			{
				if (currentLocalVibration[num].Alive)
				{
					val += currentLocalVibration[num].UpdateIntensity(Time.deltaTime);
				}
				else
				{
					currentLocalVibration.RemoveAt(num);
				}
			}
			Vector2 cameraPos2 = Vector2.op_Implicit(((Component)cam).transform.position);
			for (int num2 = currentWorldVibration.Count - 1; num2 >= 0; num2--)
			{
				if (currentWorldVibration[num2].Alive)
				{
					val += currentWorldVibration[num2].UpdateIntensity(cameraPos2, Time.deltaTime);
				}
				else
				{
					currentWorldVibration.RemoveAt(num2);
				}
			}
			currentVibration = val;
			val.x = Mathf.Clamp01(val.x);
			val.y = Mathf.Clamp01(val.y);
			if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Joystick)
			{
				player.SetVibration(0, val.x);
				player.SetVibration(1, val.y);
			}
			zeroNextFrame = true;
		}
		else if (zeroNextFrame)
		{
			Player player2 = ReInput.players.GetPlayer(0);
			player2.SetVibration(0, 0f);
			player2.SetVibration(1, 0f);
		}
		numVibrationsActive = currentLocalVibration.Count + currentWorldVibration.Count;
	}

	public static void CancelVibration(AudioClip clipToCancel)
	{
		for (int num = DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration[num].clip == (Object)(object)clipToCancel)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.RemoveAt(num);
				return;
			}
		}
		for (int num2 = DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Count - 1; num2 >= 0; num2--)
		{
			if ((Object)(object)DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration[num2].clip == (Object)(object)clipToCancel)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.RemoveAt(num2);
				break;
			}
		}
	}

	public static void CancelVibration(string name)
	{
		for (int num = DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Count - 1; num >= 0; num--)
		{
			if (DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration[num].name == name)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.RemoveAt(num);
				return;
			}
		}
		for (int num2 = DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Count - 1; num2 >= 0; num2--)
		{
			if (DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration[num2].name == name)
			{
				DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.RemoveAt(num2);
				break;
			}
		}
	}

	public static void Vibrate(float left, float right)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		VibrationManager instance = DestroyableSingleton<VibrationManager>.Instance;
		instance.singleFrameVibration += new Vector2(left, right);
		DestroyableSingleton<VibrationManager>.Instance.hasFrameVibration = true;
	}

	public static void Vibrate(float left, float right, float duration, VibrationFalloff falloffType = VibrationFalloff.None, AudioClip sourceClip = null, bool loopClip = false, string name = "")
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		LocalVibration localVibration = new LocalVibration();
		localVibration.intensity = new Vector2(left, right);
		localVibration.duration = duration;
		localVibration.falloff = falloffType;
		localVibration.t = 0f;
		localVibration.clip = sourceClip;
		localVibration.loopClip = loopClip;
		localVibration.name = name;
		localVibration.Init();
		DestroyableSingleton<VibrationManager>.Instance.currentLocalVibration.Add(localVibration);
	}

	public static void Vibrate(float intensity, Vector2 worldPosition, float radius)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.intensity = intensity;
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.location = worldPosition;
		DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.radius = radius;
		Vector2 cameraPos = Vector2.op_Implicit(((Component)DestroyableSingleton<VibrationManager>.Instance.cam).transform.position);
		VibrationManager instance = DestroyableSingleton<VibrationManager>.Instance;
		instance.singleFrameVibration += DestroyableSingleton<VibrationManager>.Instance.tempSingleFrameWorldVibration.UpdateIntensity(cameraPos, 0f);
		DestroyableSingleton<VibrationManager>.Instance.hasFrameVibration = true;
	}

	public static void Vibrate(float intensity, Vector2 worldPosition, float radius, float duration, VibrationFalloff falloffType = VibrationFalloff.None, AudioClip sourceClip = null, bool loopClip = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		WorldVibration worldVibration = new WorldVibration();
		worldVibration.intensity = intensity;
		worldVibration.location = worldPosition;
		worldVibration.duration = duration;
		worldVibration.falloff = falloffType;
		worldVibration.radius = radius;
		worldVibration.t = 0f;
		worldVibration.clip = sourceClip;
		worldVibration.loopClip = loopClip;
		worldVibration.Init();
		DestroyableSingleton<VibrationManager>.Instance.currentWorldVibration.Add(worldVibration);
	}

	public VibrationManager()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
	}
}
