using System.Globalization;
using Sprache;

namespace Forth;

internal sealed class ForthMachine
{

    private readonly Stack<int> _stack = new();
    private readonly Dictionary<string, Action> _words = new(StringComparer.OrdinalIgnoreCase);

    public ForthMachine()
    {
        _words.Add("+", () =>
        {
            if (_stack.Count >= 2)
                _stack.Push(_stack.Pop() + _stack.Pop());
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("-", () =>
        {
            if (_stack.Count >= 2)
                _stack.Push(-_stack.Pop() + _stack.Pop());
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("*", () =>
        {
            if (_stack.Count >= 2)
                _stack.Push(_stack.Pop() * _stack.Pop());
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("/", () => 
        {
            if (_stack.Count >= 2)
                _stack.Push(1 / _stack.Pop() * _stack.Pop());
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("DUP", () => 
        {
            if (_stack.Count >= 1)
                _stack.Push(_stack.Peek());
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("DROP", () => 
        {
            if (_stack.Count >= 1)
                _stack.Pop();
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("SWAP", () =>
        {
            if (_stack.Count >= 2)
            {
                var a = _stack.Pop(); 
                var b = _stack.Pop(); 
                _stack.Push(a); 
                _stack.Push(b);
            }
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add("OVER", () =>
        {
            if (_stack.Count >= 2)
            {
                var a = _stack.Pop(); 
                var b = _stack.Pop(); 
                _stack.Push(b); 
                _stack.Push(a); 
                _stack.Push(b);
            }
            else
                Console.WriteLine("Stack underflow");
        });
        _words.Add(".", () =>
        {
            if (_stack.Count >= 1)
                Console.WriteLine(_stack.Pop());
            else
                Console.WriteLine("Stack underflow");
        });
    }

    public void Evaluate(string input)
    {
        var parser = CreateParser();
        var program = parser.Parse(input);
        foreach (var word in program)
        {
            if (_words.TryGetValue(word, out var action))
            {
                action();
            }
            else
            {
                if (int.TryParse(word, out var num))
                {
                    _stack.Push(num);                    
                }
                else
                    Console.WriteLine($"{word} ?");
            }
        }
    }

    private static Parser<IEnumerable<string>> CreateParser()
    {
        var number = Parse.Digit.AtLeastOnce().Text();
        var word = Parse.Letter.Or(Parse.Chars('_', '+', '-', '*', '/', '.')).AtLeastOnce().Text();
        var program = (from def in word.Or(number)
                       select def).Token().Many().End();
        return program;
    }
}
