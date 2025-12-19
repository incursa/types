using System.Text;
using Bravellian;
using Xunit;

namespace Bravellian.Types.Tests;

public class AdditionalValueObjectTests
{
    [Fact]
    public void VirtualPath_CombineAndEquality()
    {
        var basePath = new VirtualPath("assets/images");
        var combined = basePath.Combine(new VirtualPath("logo.png"));

        Assert.NotNull(combined);
        Assert.Equal("assets/images/logo.png", combined!.ToString());
        Assert.Equal(basePath.DirectorySeperator, combined.Value.DirectorySeperator);
        Assert.Equal(combined, new VirtualPath("assets/images/logo.png"));
    }

    [Fact]
    public void BvFile_FromBase64_CapturesMetadata()
    {
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var base64 = Convert.ToBase64String(bytes);

        var file = BvFile.FromBase64(base64, "greeting.txt", "text/plain");

        Assert.Equal("greeting.txt", file.FileName);
        Assert.Equal("text/plain", file.ContentType);
        Assert.Equal(bytes, file.Data.ToArray());
    }

    [Fact]
    public void Maybe_MatchAndSelect()
    {
        Maybe<int> maybe = new(5);

        var matched = maybe.Match(() => 0, v => v * 2);
        var mapped = maybe.Select(v => v.ToString());
        var orValue = Maybe<int>.None.GetValueOrDefault(10);

        Assert.Equal(10, matched);
        Assert.Equal("5", mapped.Value);
        Assert.Equal(10, orValue);
    }
}
