using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace TDE_1 {
    public static class StringSizes {
        public static readonly int EventTime_STRING_SIZE    = 23;
        public static readonly int CategoryId_STRING_SIZE   = 38;
        public static readonly int CategoryCode_STRING_SIZE = 38;
        public static readonly int Brand_STRING_SIZE        = 32;
        public static readonly int UserSession_STRING_SIZE  = 36;
    }

    static class Program {

        public static void Main(string[] args) {
            CreateDataSet();
        }

        public static void CreateDataSet() {
            new DataSetFactory();

            FileStream fs = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);

            Console.WriteLine(fs.ReadProduct());
            Console.WriteLine(fs.ReadProduct());
            Console.WriteLine(fs.ReadProduct());
        }
    }
}