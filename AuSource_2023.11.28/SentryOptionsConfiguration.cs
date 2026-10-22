using Sentry;
using Sentry.Unity;
using UnityEngine;

[CreateAssetMenu(fileName = "Assets/Resources/Sentry/SentryOptionsConfiguration.cs", menuName = "Sentry/SentryOptionsConfiguration", order = 999)]
public class SentryOptionsConfiguration : ScriptableOptionsConfiguration
{
	private const bool SetEnvironment = true;

	public override void Configure(SentryUnityOptions options)
	{
		((SentryOptions)options).Environment = BuildEnvironment.Production.ToString().ToLower();
	}
}
