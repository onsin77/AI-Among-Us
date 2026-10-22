namespace AmongUs.QuickChat;

public class QuickChatBreadcrumb
{
	public readonly QuickChatBreadcrumbType Type;

	public readonly QuickChatTag[] Tags;

	public QuickChatBreadcrumb(QuickChatBreadcrumbType type, params QuickChatTag[] tags)
	{
		Type = type;
		Tags = tags;
	}
}
