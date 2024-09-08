using System.Xml.Linq;

namespace LLMChain.Core.Tools;
public class ToolRepository
{
    public static ToolRepository Instance { get; } = new ToolRepository();

    private List<ITool> _tools = new List<ITool>();


    public ITool this[string ID]
    {
        get { return _tools.FirstOrDefault(t => t.ID == ID); }
    }

    private ToolRepository() { }


    public void AddTool(ITool tool)
    {
        _tools.Add(tool);
    }

    public ITool GetTool(string name)
    {
        return _tools.FirstOrDefault(t => t.Name == name);
    }

    public List<ITool> GetTools()
    {
        return new List<ITool>(_tools);
    }

}
