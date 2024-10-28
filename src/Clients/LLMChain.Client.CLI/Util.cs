using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.Client.CLI;
internal static class Util
{

    public static IConfiguration LoadConfiguration()
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

        return builder.Build();
    }
}
