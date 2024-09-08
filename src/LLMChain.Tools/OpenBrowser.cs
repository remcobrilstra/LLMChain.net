using LLMChain.Core.Tools;
using System.Diagnostics;

namespace LLMChain.Tools;

public class OpenBrowser : ITool
{
    public string ID => "CORE.ACTION.BROWSER";
    public string Name => "OpenBrowser";
    public string Description => "Allows you to display a web page to the user";
    public ToolArgs Parameters => new ToolArgs
    {
        ReturnType = "object",
        Properties =
        [
            new("url", "the url of the webpage that you want to display" )
        ]
    };

    public async Task<string> Invoke(Dictionary<string, string> args)
    {

        Uri url = new Uri(args["url"]);

        Process myProcess = new Process();
        myProcess.StartInfo.UseShellExecute = true;
        myProcess.StartInfo.FileName = url.ToString();
        myProcess.Start();
        // Return the weather
        return "[browser was opened]";
    }
}
