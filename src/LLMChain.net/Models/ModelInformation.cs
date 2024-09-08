namespace LLMChain.Core.Models;

/// <summary>
/// As some very relevant data is generally not exposed through the api's we'll need to keep track of those ourselves
/// </summary>
public class ModelInformation
{
    public string Provider { get; set; }
    public string ModelId { get; set; }
    public string ModelName { get; set; }
    public ulong ContextWindowSize { get; set; }
    public float InputTokenCost1M { get; set; }
    public float OutputTokenCost1M { get; set; }
}
