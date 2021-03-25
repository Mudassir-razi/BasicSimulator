using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grid
{
    //Errors that can take place in circuit
    class CircuitError
    {
        public string Message { get; private set; }

        public CircuitError(string m)
        {
            Message = m;
        }
    }

    class CircuitWarning
    {
        public string Message { get; private set; }
        public CircuitWarning(string m)
        {
            Message = m;
        }
    }

}
