using LLMChain.Core.Models;

namespace LLMChain.Core;

/// <summary>
/// As some very relevant data is generally not exposed through the api's we'll need to keep track of those ourselves
/// </summary>
public class ModelInformationRepository
{
    public static ModelInformationRepository Instance { get; } = new ModelInformationRepository();

    private List<ModelInformation> _models = new List<ModelInformation>();


    public ModelInformation this[string ID]
    {
        get { return _models.FirstOrDefault(t => t.ModelId == ID); }
    }

    private ModelInformationRepository() { }

    public void AddModels(ModelInformation[]? models)
    {
        if (models != null)
        {
            _models.AddRange(models);
        }
    }
}
