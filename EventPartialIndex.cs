using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public class EventPartialIndex {
        public long id { get; set; }
        public long Position { get; set; }
        public bool Excluido { get; set; }
        
        public static readonly int Size = sizeof(long) +
                                          sizeof(long) + 
                                          sizeof(bool) + 1;

    }
}
