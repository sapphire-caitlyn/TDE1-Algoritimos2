using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public class Product {
        public long ProductId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string Brand { get; set; }

        public void Pad() {
            CategoryId   = CategoryId  .PadRight(StringSizes.CategoryId_STRING_SIZE);
            CategoryCode = CategoryCode.PadRight(StringSizes.CategoryCode_STRING_SIZE);
            Brand        = Brand       .PadRight(StringSizes.Brand_STRING_SIZE);
        }

        public static readonly int Size = sizeof(long) +
                                          StringSizes.CategoryId_STRING_SIZE   +
                                          StringSizes.CategoryCode_STRING_SIZE +
                                          StringSizes.Brand_STRING_SIZE        +
                                          1;

        public override string ToString() {
            return "EVENT START -> " +
                   ProductId    + '\n' +    
                   CategoryId   + '\n' +
                   CategoryCode + '\n' +
                   Brand        + '\n' +
                   "EVENT END \n";
        }
    }
}
