namespace TheOtherRoles.Roles.Crewmate;

public sealed class TrapperPlusReport
{
    public enum ReportState
    {
        NoPlayers,
        NotEnoughPlayers,
        Ready
    }

    public ReportState State { get; }
    public IReadOnlyList<RoleId> Roles { get; }

    internal TrapperPlusReport(ReportState state, IEnumerable<RoleId> roles)
    {
        State = state;
        Roles = roles.ToArray();
    }

    public string ToLocalizedText()
    {
        if (State == ReportState.NoPlayers) return GetString("TrapperPlus.Report.NoPlayers");
        if (State == ReportState.NotEnoughPlayers) return GetString("TrapperPlus.Report.NotEnoughPlayers");

        var names = Roles.Select(roleId =>
            RoleInfo.RoleInfoById.TryGetValue(roleId, out var info) ? info.Name : roleId.ToString());
        return string.Format(GetString("TrapperPlus.Report.Roles"), string.Join(", ", names));
    }
}
