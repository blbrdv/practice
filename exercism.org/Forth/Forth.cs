using System.Globalization;
using Sprache;

namespace Forth;

internal sealed class ForthMachine
{

    private readonly Stack<int> _stack = new();
    private readonly WordList _words = new();

    public ForthMachine()
    {
        _words.Add(
            new BuiltinCommand("+", () =>
            {
                if (_stack.Count >= 2)
                    _stack.Push(_stack.Pop() + _stack.Pop());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("-", () =>
            {
                if (_stack.Count >= 2)
                    _stack.Push(_stack.Pop() - _stack.Pop());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("*", () =>
            {
                if (_stack.Count >= 2)
                    _stack.Push(_stack.Pop() * _stack.Pop());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("/", () =>
            {
                if (_stack.Count >= 2)
                    _stack.Push(_stack.Pop() / _stack.Pop());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("DUP", () =>
            {
                if (_stack.Count >= 1)
                    _stack.Push(_stack.Peek());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("DROP", () =>
            {
                if (_stack.Count >= 1)
                    _stack.Pop();
                else
                    Console.WriteLine("Stack underflow");
            })
        );
        _words.Add(
            new BuiltinCommand("SWAP", () =>
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
            })
        );
        _words.Add(
            new BuiltinCommand("OVER", () =>
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
            })
        );
        _words.Add(
            new BuiltinCommand(".", () =>
            {
                if (_stack.Count >= 1)
                    Console.WriteLine(_stack.Pop());
                else
                    Console.WriteLine("Stack underflow");
            })
        );
    }

    public void Evaluate(string input)
    {
        var parser = CreateParser();
        var program = parser.Parse(input);
        foreach (var word in program)
        {
            switch (word!) 
            {
                case Word w:
                    if (_words.Any(e => e.Id == w.Id))
                    {
                        var def = _words[w.Id];
                        switch (def)
                        {
                            case BuiltinCommand bc:
                                bc.Action();
                                break;
                            case CustomCommand cc:
                                Evaluate(string.Join(" ", cc.Actions));
                                break;
                        }
                    }
                    else
                        Console.WriteLine($"{w.Id} ?");
                    break;
                case WordDefinition wd:
                    _words.Add(new CustomCommand(wd.Id, wd.Definition));
                    break;
                case Number n:
                    _stack.Push(n.Value);
                    break;
            }
        }
    }

    private static Parser<IEnumerable<IEntity>> CreateParser()
    {
        var numDef =
            from neg in Parse.Char('-').Optional()
            from num in Parse.Digit.AtLeastOnce().Text()
            select $"{(neg.IsDefined ? '-' : ' ')}{num}";
        var specChars = Parse.Chars(
            '_', '+', '-', '*', 
            '/', '.', '!', '~', 
            '#', '$', '%', '&', 
            '<', '>', '=', '?', 
            '@', '^', '|');
        var wordDef = Parse.Letter.Or(specChars).AtLeastOnce().Text();
        
        Parser<IEntity> number = from num in numDef select new Number(num);
        Parser<IEntity> word = from w in wordDef select new Word(w);
        Parser<IEntity> definition =
            from open in Parse.Char(':')
            from gap1 in Parse.WhiteSpace.Many()
            from name in wordDef
            from gap2 in Parse.WhiteSpace.Many()
            from def in numDef.Or(wordDef).DelimitedBy(Parse.WhiteSpace)
            from gap3 in Parse.WhiteSpace.Many()
            from close in Parse.Char(';')
            select new WordDefinition(name, def);
        
        return from d in (number
                    .Or(word)
                    .Or(definition)
                ).DelimitedBy(Parse.WhiteSpace)
            select d.ToArray();
    }
}
