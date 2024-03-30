using System.Linq;

public static class Reverser
{
    public static string ReverseString(string input)
    {
        return new string(input.Reverse().ToArray());
    }
}

Console.WriteLine($"reverse of 'abcd' is '{Reverser.ReverseString("abcd")}'");
