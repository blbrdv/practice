using Forth;

Console.WriteLine("Welcome to small subset of Forth!");

var forth = new ForthMachine();
var ongoing = true;

while (ongoing)
{
    var input = Console.ReadLine();
    
    if ("quit".Equals(input))
    {
        ongoing = false;
    }
    else
    {
        forth.Evaluate(input!);
    }
}

Console.WriteLine("Bye!");
