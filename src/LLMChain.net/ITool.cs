namespace LLMChain.Core;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    ToolArgs Parameters { get; }
    Task<string> Invoke(Dictionary<string, string> args);
}

public class ToolArgs
{
    public string ReturnType { get; set; }

    public string[] Required { get; set; }

    public PropertyDescription[] Properties { get; set; }

}

public enum propType
{
    Object,
    String,
    Enum,
    Boolean
}

public class PropertyDescription
{
    public PropertyDescription(string name, string description, propType type = propType.String)
    {
        Name = name;
        Description = description;
        Type = type;
    }
    public string Name { get; set; }
    public propType Type { get; set; }
    public string Description { get; set; }
}
