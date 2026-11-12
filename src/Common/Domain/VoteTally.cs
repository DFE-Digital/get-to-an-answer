namespace Common.Domain;

public class VoteTally
{
    public int Yes { get; set; }

    public int No { get; set; }

    public static string? GetResult(VoteTally? tally) => tally is null ? null : tally.Yes > tally.No ? "Yes" : "No";
}
