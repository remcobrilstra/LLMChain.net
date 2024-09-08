using LLMChain.Core.Tools;

namespace LLMChain.Core.Conversations;
public class Agent
{
    public string Name { get; set; }
    public string SystemPrompt { get; set; }

    public string Model { get; set; }
    public string ModelProvider { get; set; }

    public string[] ToolIds { get; set; }


    public void Initialize()
    {
        if (ToolIds != null)
        {
            Tools = new ITool[ToolIds.Length];
            for (int i = 0; i < ToolIds.Length; i++)
            {
                Tools[i] = ToolRepository.Instance[ToolIds[i]];
            }
        }
    }



    public ITool[] Tools { get; set; }

}
