using Microsoft.Extensions.Configuration;

namespace LLMChain.Client.CLI.SampleLogic;
internal interface ITestLogic
{
    void Configure(IConfiguration configuration);
    Task Run(IConfiguration configuration);
}