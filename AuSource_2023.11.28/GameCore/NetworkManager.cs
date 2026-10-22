namespace GameCore;

public class NetworkManager
{
	public delegate void OnInvitedCallback(string connectionString);

	public OnInvitedCallback OnInvited;

	public bool IsInviteHandled { get; set; } = true;

	public string ConnectionString { get; private set; }

	public char ConnectionStringDeliminator { get; private set; } = ',';
}
