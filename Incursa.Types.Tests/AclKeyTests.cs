namespace Incursa.Types.Tests;

public sealed class AclKeyTests
{
    public static TheoryData<string, string> ValidGrantKeys => new()
    {
        { "Minimal resource path (single segment)", "acl:v1:org:org_1:actor:u_1:res:artifacts" },
        { "Multi-segment resource path", "acl:v12:org:org_abc123:actor:u_om_9a8b7c:res:artifacts/prod/nuget:feed_01HXYZ" },
        { "Segment with multiple colon-separated atoms", "acl:v2:org:org-abc.DEF_123:actor:k-ak.DEF_456:res:flow:svc:primary/prod:env:main/inbox:123" },
        { "Dashes underscores and periods across ids/path", "acl:v3:org:Org-1_2.3:actor:Actor_9-9.9:res:m1/e1/bucket-kind:bucket_id.01" },
        { "Deep path", "acl:v1:org:org_123:actor:u_om_123:res:a/b/c/d/e/f/g" },
        { "Long atoms under 128 chars", "acl:v999:org:org_abcdefghijklmnopqrstuvwxyz0123456789._-:actor:actor_abcdefghijklmnopqrstuvwxyz0123456789._-:res:seg_1/seg_2:partA:partB/seg_3" },
        { "Numeric atoms", "acl:v1:org:123:actor:456:res:789/012:345" },
        { "Extra colon subparts in final segment are valid under current grammar", "acl:v1:org:org_1:actor:u_1:res:artifacts:extra:stuff:unexpected" },
    };

    public static TheoryData<string, string> InvalidGrantKeys => new()
    {
        { "Missing acl prefix", "ac1:v1:org:org_1:actor:u_1:res:artifacts" },
        { "Missing org section", "acl:v1:actor:u_1:res:artifacts" },
        { "Wrong field order", "acl:v1:actor:u_1:org:org_1:res:artifacts" },
        { "Missing res section", "acl:v1:org:org_1:actor:u_1:artifacts" },
        { "Version v0 not allowed", "acl:v0:org:org_1:actor:u_1:res:artifacts" },
        { "Version with leading zero not allowed", "acl:v01:org:org_1:actor:u_1:res:artifacts" },
        { "Version must be numeric", "acl:vX:org:org_1:actor:u_1:res:artifacts" },
        { "OrgId cannot contain colon", "acl:v1:org:org:1:actor:u_1:res:artifacts" },
        { "ActorId cannot contain slash", "acl:v1:org:org_1:actor:u/1:res:artifacts" },
        { "ActorId cannot contain space", "acl:v1:org:org_1:actor:u 1:res:artifacts" },
        { "OrgId cannot be empty", "acl:v1:org::actor:u_1:res:artifacts" },
        { "Resource path cannot be empty", "acl:v1:org:org_1:actor:u_1:res:" },
        { "Resource path cannot start with slash", "acl:v1:org:org_1:actor:u_1:res:/artifacts" },
        { "Resource path cannot end with slash", "acl:v1:org:org_1:actor:u_1:res:artifacts/" },
        { "Resource path cannot contain double slash", "acl:v1:org:org_1:actor:u_1:res:artifacts//prod" },
        { "Resource segment cannot end with colon", "acl:v1:org:org_1:actor:u_1:res:nuget:" },
        { "Resource segment cannot contain empty colon subpart", "acl:v1:org:org_1:actor:u_1:res:nuget::feed" },
        { "Resource path cannot be slash only", "acl:v1:org:org_1:actor:u_1:res:/" },
        { "Resource path cannot contain spaces", "acl:v1:org:org_1:actor:u_1:res:artifacts/prod feed" },
        { "Resource segment cannot contain @", "acl:v1:org:org_1:actor:u_1:res:artifacts/@prod" },
        { "Trailing newline rejected", "acl:v1:org:org_1:actor:u_1:res:artifacts\n" },
    };

    [Theory]
    [MemberData(nameof(ValidGrantKeys))]
    public void TryParse_ValidGrantKeys_ReturnsTrue(string caseName, string input)
    {
        AclKey.TryParse(input, out var parsed).ShouldBeTrue(caseName);
        parsed.Value.ShouldBe(input, caseName);
    }

    [Theory]
    [MemberData(nameof(InvalidGrantKeys))]
    public void TryParse_InvalidGrantKeys_ReturnsFalse(string caseName, string input)
    {
        AclKey.TryParse(input, out _).ShouldBeFalse(caseName);
    }

    [Theory]
    [MemberData(nameof(InvalidGrantKeys))]
    public void Parse_InvalidGrantKeys_Throws(string caseName, string input)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AclKey.Parse(input), caseName);
    }
}
