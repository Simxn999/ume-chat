using System.Diagnostics;

namespace Ume_Chat_External_General;

/// <summary>
///     String representation of batch progress.
///     Example: [01/50]
/// </summary>
/// <param name="index">Current index of item in batch</param>
/// <param name="total">Total number of items in batch</param>
[DebuggerDisplay("{ToString().TrimEnd(':')}")]
public class ProgressString(int index, int total)
{
    public int Index { get; } = index;
    public int Total { get; } = total;

    /// <summary>
    ///     Retrieve progress as string
    /// </summary>
    /// <returns>String of progress</returns>
    public override string ToString()
    {
        var length = Total.ToString().Length;

        var output = Index.ToString();

        while (output.Length != length)
            output = output.Insert(0, "0");

        return $"[{output}/{Total}]:";
    }
}
