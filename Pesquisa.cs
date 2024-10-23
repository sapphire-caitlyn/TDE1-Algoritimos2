﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public static class Pesquisa {

        public static Product? SearchProduct(FileStream fsProductOutputFile, long ProductId) {
            long low = 0, high = fsProductOutputFile.Length / Product.Size, mid = (low + high) / 2;

            while (low <= high) {
                fsProductOutputFile.Position = mid * Product.Size;
                Product mProduct = fsProductOutputFile.ReadProduct();
                long id = mProduct.ProductId;

                if (id == ProductId) { return mProduct; } else
                if (id >  ProductId) { high = mid  - 1; } else
                if (id <  ProductId) { low  = mid  + 1; }

                mid = (low + high) / 2;
            }
            return null;
        }

        public static Event? SearchEvent(FileStream fsEventOutputFile, long idEvent) {
            long low = 0, high = fsEventOutputFile.Length / Event.Size, mid = (low + high) / 2;

            while (low <= high) {
                fsEventOutputFile.Position = mid * Event.Size;
                Event mEvent = fsEventOutputFile.ReadEvent();
                long id = mEvent.id;

                if (id == idEvent) { return mEvent ; } else
                if (id  > idEvent) { high = mid - 1; } else
                if (id  < idEvent) { low  = mid + 1; }

                mid = (low + high) / 2;
            }
            return null;
        }
    
        //[ 2.1 - 2 ]
        public static void Log(this Event? mEvent) {
            Console.WriteLine(mEvent == null ? "[ Event é nulo! ]" : mEvent.ToString());
        }
        //[ 2.1 - 2 ]
        public static void Log(this Product? mProduct) {
            Console.WriteLine(mProduct == null ? "[ Product é nulo! ]" : mProduct.ToString());
        }
    }
}