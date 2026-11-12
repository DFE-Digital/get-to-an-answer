using Common.Domain;
using FluentAssertions;
using Xunit;

namespace Unit.Tests.Domain;

public class VoteTallyTests
{
    [Fact]
    public void GetResult_Returns_Yes_When_Yes_Count_Is_Greater_Than_No_Count()
    {
        var tally = new VoteTally { Yes = 3, No = 1 };

        VoteTally.GetResult(tally).Should().Be("Yes");
    }

    [Fact]
    public void GetResult_Returns_No_When_No_Count_Is_Greater_Than_Yes_Count()
    {
        var tally = new VoteTally { Yes = 1, No = 3 };

        VoteTally.GetResult(tally).Should().Be("No");
    }

    [Fact]
    public void GetResult_Returns_No_On_Tie()
    {
        var tally = new VoteTally { Yes = 2, No = 2 };

        VoteTally.GetResult(tally).Should().Be("No");
    }

    [Fact]
    public void GetResult_Returns_Null_When_Tally_Is_Null()
    {
        VoteTally.GetResult(null).Should().BeNull();
    }

    [Fact]
    public void GetResult_Returns_No_When_No_Votes_Cast()
    {
        var tally = new VoteTally { Yes = 0, No = 0 };

        VoteTally.GetResult(tally).Should().Be("No");
    }
}
