using System;

namespace Rewired.Internal;

public static class ControllerTemplateFactory
{
	private static readonly Type[] _defaultTemplateTypes = new Type[6]
	{
		typeof(GamepadTemplate),
		typeof(RacingWheelTemplate),
		typeof(HOTASTemplate),
		typeof(FlightYokeTemplate),
		typeof(FlightPedalsTemplate),
		typeof(SixDofControllerTemplate)
	};

	private static readonly Type[] _defaultTemplateInterfaceTypes = new Type[6]
	{
		typeof(IGamepadTemplate),
		typeof(IRacingWheelTemplate),
		typeof(IHOTASTemplate),
		typeof(IFlightYokeTemplate),
		typeof(IFlightPedalsTemplate),
		typeof(ISixDofControllerTemplate)
	};

	public static Type[] templateTypes => _defaultTemplateTypes;

	public static Type[] templateInterfaceTypes => _defaultTemplateInterfaceTypes;

	public static IControllerTemplate Create(Guid typeGuid, object payload)
	{
		if (typeGuid == GamepadTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new GamepadTemplate(payload);
		}
		if (typeGuid == RacingWheelTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new RacingWheelTemplate(payload);
		}
		if (typeGuid == HOTASTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new HOTASTemplate(payload);
		}
		if (typeGuid == FlightYokeTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new FlightYokeTemplate(payload);
		}
		if (typeGuid == FlightPedalsTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new FlightPedalsTemplate(payload);
		}
		if (typeGuid == SixDofControllerTemplate.typeGuid)
		{
			return (IControllerTemplate)(object)new SixDofControllerTemplate(payload);
		}
		return null;
	}
}
