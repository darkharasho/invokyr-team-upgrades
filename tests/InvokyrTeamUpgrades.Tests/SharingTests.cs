using InvokyrTeamUpgrades;
using Xunit;

public class SharingTests
{
    [Theory]
    [InlineData(0, 3, 5, 3)]
    [InlineData(2, 3, 5, 1)]
    [InlineData(4, 3, 5, 0)]
    [InlineData(0, 7, 5, 5)]
    [InlineData(1, 4, 0, 3)]
    public void LevelsToAdd(int current, int target, int max, int expected) =>
        Assert.Equal(expected, Sharing.LevelsToAdd(current, target, max));

    [Fact]
    public void Highest()
    {
        Assert.Equal(4, Sharing.Highest(new[] { 0, 4, 2 }));
        Assert.Equal(0, Sharing.Highest(new int[0]));
    }

    [Theory]
    [InlineData("up_health", "up_health")]
    [InlineData(" a=b[c] ", "a_b_c_")]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void ConfigKey(string alias, string expected) => Assert.Equal(expected, Sharing.ConfigKey(alias));
}
