using CS2Toolkit.Models;
using CS2Toolkit.Models.Abstractions;

namespace CS2Toolkit.Models.Tests;

public sealed class PreferredBoneParserTests
{
    [Theory]
    [InlineData("head", PreferredAimBone.Head)]
    [InlineData("neck", PreferredAimBone.Neck)]
    [InlineData("body", PreferredAimBone.Body)]
    [InlineData("chest", PreferredAimBone.Body)]
    [InlineData(null, PreferredAimBone.Head)]
    public void Parse_maps_values(string? input, PreferredAimBone expected) =>
        Assert.Equal(expected, PreferredBoneParser.Parse(input));

    [Theory]
    [InlineData(PreferredAimBone.Head, "H")]
    [InlineData(PreferredAimBone.Neck, "N")]
    [InlineData(PreferredAimBone.Body, "B")]
    public void ToLabel_returns_short_label(PreferredAimBone bone, string expected) =>
        Assert.Equal(expected, PreferredBoneParser.ToLabel(bone));

    [Fact]
    public void GetPreferenceOrder_head_prefers_head_first()
    {
        var order = PreferredBoneParser.GetPreferenceOrder(PreferredAimBone.Head).ToList();
        Assert.Equal([BoneId.Head, BoneId.Neck, BoneId.Chest], order);
    }

    [Fact]
    public void GetPreferenceOrder_body_prefers_chest_first()
    {
        var order = PreferredBoneParser.GetPreferenceOrder(PreferredAimBone.Body).ToList();
        Assert.Equal([BoneId.Chest, BoneId.Neck, BoneId.Head], order);
    }
}
