namespace Common.Domain;

public class VoteTally
{
    public int Yes { get; set; }

    public int No { get; set; }

    public static string? GetResult(VoteTally? tally)
    {
        if (tally is null)
        {
            return null;
        }

        return tally.Yes > tally.No ? "Yes Result Message" : "No Result Message";
    }
}
