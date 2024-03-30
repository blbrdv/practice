public static class CheckForDuplicates 
{
    public static bool HasDuplicates(string str) 
    {
        var chars = new List<char>();
        foreach (char ch in str) 
        {
            if (chars.Contains(ch)) {
                return true;
            }

            chars.Add(ch);
        }

        return false;
    }
}

Console.WriteLine($"'abcd' contains duplicates: {CheckForDuplicates.HasDuplicates("abcd")}");
Console.WriteLine($"'abccd' contains duplicates: {CheckForDuplicates.HasDuplicates("abccd")}");
