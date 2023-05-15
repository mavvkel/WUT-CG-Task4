using Newtonsoft.Json;
using System.Collections.Generic;

namespace CG_Task3
{
    internal interface I2DPrimitive
    {
        //public string Type { get; }  // feels like a hacky solution required for type serialization

        [JsonIgnore]
        public List<System.Drawing.Point> Pixels { get; }

        public List<System.Drawing.Point> HandlePoints { get; set; }

        public System.Drawing.Color Color { get; set; }

    }
}
