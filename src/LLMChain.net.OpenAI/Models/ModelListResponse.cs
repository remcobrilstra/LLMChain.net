using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMChain.OpenAI.Models;

internal class ModelListResponse
{
    public string _object { get; set; }
    public Model[] data { get; set; }


    public class Model
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string owned_by { get; set; }
    }

}
