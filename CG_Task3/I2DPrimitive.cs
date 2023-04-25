using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CG_Task3
{
    internal interface I2DPrimitive
    {
        public StylusPointCollection Pixels { get; }

        public StylusPointCollection HandlesPoints { get; set; }
    }
}
