using System.Diagnostics;

namespace Ume_Chat_External_General;

[DebuggerDisplay("{ToString().TrimEnd(':')}")]
public class ProgressString(int index, int total)
{
    public int Index { get; } = index;
    public int Total { get; } = total;

    public override string ToString()
    {
        var length = Total.ToString().Length;

        var output = Index.ToString();

        while (output.Length != length)
            output = output.Insert(0, "0");

        return $"[{output}/{Total}]:";
    }
}