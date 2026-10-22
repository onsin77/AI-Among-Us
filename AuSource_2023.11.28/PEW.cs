using System;
using System.Runtime.CompilerServices;

public static class PEW
{
	public static class Hardware
	{
		public enum HardwareType
		{
			PC,
			Switch,
			XboxOne,
			XboxScarlett,
			PS4,
			PS5,
			Stadia,
			Unknown
		}

		private static HardwareType type = GetHardwareType();

		private static int tier = GetHardwareTier();

		public static HardwareType hardwareType
		{
			get
			{
				return type;
			}
			private set
			{
				type = value;
			}
		}

		public static int hardwareTier
		{
			get
			{
				return tier;
			}
			private set
			{
				tier = value;
			}
		}

		private static HardwareType GetHardwareType()
		{
			return HardwareType.Unknown;
		}

		private static int GetHardwareTier()
		{
			return 4;
		}
	}

	[Obsolete("\nDon't reference this class directly, create a class that derives from it! Unity won't properly serialize classes with generics.\n\npublic class ConditionalWhatever : ConditionalValue<Whatever> { }; \n\n(Ignore if you're already doing that)")]
	public class ConditionalValue<T>
	{
		public T PC;

		public T Switch;

		public T XboxOne;

		public T XboxOneX;

		public T PS4;

		public T PS4Pro;

		public T PS5;

		public T Stadia;

		public T Misc;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Select()
		{
			return PC;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(ConditionalValue<T> input)
		{
			return input.Select();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ConditionalValue<T>(T input)
		{
			return new ConditionalValue<T>
			{
				PC = input,
				Switch = input,
				XboxOne = input,
				XboxOneX = input,
				PS4 = input,
				PS4Pro = input,
				PS5 = input,
				Stadia = input,
				Misc = input
			};
		}
	}
}
