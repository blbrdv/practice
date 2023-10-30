using shortid;
using shortid.Configuration;

namespace Common;

public static class Id
{
    public static string New()
    {
        return ShortId.Generate(new GenerationOptions(length: 8));
    }
}
