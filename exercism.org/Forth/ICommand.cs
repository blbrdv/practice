using System.Collections.ObjectModel;

namespace Forth;

internal interface ICommand
{
    string Id { get; }
}

internal class BuiltinCommand : ICommand
{
    public string Id { get; }
    public Action Action { get; }

    public BuiltinCommand(string id, Action action)
    {
        Id = id;
        Action = action;
    }
}

internal class CustomCommand : ICommand
{
    public string Id { get; }
    
    public IEnumerable<string> Actions { get; }

    public CustomCommand(string id, IEnumerable<string> actions)
    {
        Id = id;
        Actions = actions;
    }
}

internal class WordList : KeyedCollection<string, ICommand>
{
    protected override string GetKeyForItem(ICommand item)
    {
        return item.Id;
    }
}
