namespace Forth;

internal interface IEntity {};

internal sealed class Number : IEntity
{

    public int Value { get; private set; }

    public Number(string value) {
        this.Value = int.Parse(value.ToString());
    }

}

internal interface IWord : IEntity 
{
    string Id { get; }
}

internal sealed class Word : IWord
{

    public string Id { get; }

    public Word(string id) {
        Id = id;
    }

}

internal sealed class WordDefinition : IWord
{

    public IEnumerable<string> Definition { get; private set; }

    public string Id { get; }

    public WordDefinition(string id, IEnumerable<string> def) {
        Id = id;
        Definition = def;
    }

}
