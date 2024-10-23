using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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
            //CreateDataSet();

            //PesquisaProduct(15200565).Log();

            PesquisaEvent(500).Log();
        }

        //[ 2.1 - 3&4 (Product) ]
        public static Product? PesquisaProduct(long ProductId) {
            FileStream fsProductOutputFile = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            return Pesquisa.SearchProduct(fsProductOutputFile, ProductId);
        }
        //[ 2.1 - 3&4 (Event) ]
        public static Event? PesquisaEvent(long id) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            return Pesquisa.SearchEvent(fsEventOutputFile, id);
        }

        public static void InsertEvent(Event mEvent) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            fsEventOutputFile.Position = fsEventOutputFile.Length - Event.Size;
            Event prevEvent = fsEventOutputFile.ReadEvent();
            mEvent.id = prevEvent.id + 1;
            fsEventOutputFile.WriteEvent(mEvent);
        }

        //[ 3 - Inserção/remoção de dados em um dos arquivos de dados, e reconstrução do índice ]
        public static void RemoveEvent(long id) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.ReadWrite);
            long Position = id * Event.Size - Event.Size + Event.ExcluidoPosition;
            fsEventOutputFile.Position = Position;
            fsEventOutputFile.Write(BitConverter.GetBytes(true));
        }
    }
}