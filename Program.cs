using AlgEstruturaDados2;
using BTree;
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

        //public static void Main(string[] args) {
        //    //new DataSetFactory().CreateDataSet();
        //    //PartialIndexFactory.CreateEventPartialIndex();
        //    //InsertEvent(new Event() { Brand = "samsung", CategoryCode = "15151", CategoryId = "515151", EventTime = DateTime.Now.ToString(), EventType = EventType.View, Price = 55.5, ProductId = 2121212, UserId = 51515151, UserSession = "55s1d5as5d15df1" });
        //    //PesquisaEventUsingPartialIndex(750).Log();
        //    //RemoveEvent(750);
        //    //PesquisaEventUsingPartialIndex(750).Log();
        //}

        //TDE 2
        public static void Main(string[] args) {
            //BtreeTest();
            HashTest();
        }
        public static void BtreeTest() {
            Console.WriteLine($"INICIANDO GERAÇÂO BTREE as {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            BTreeDictionary<long, long> BTree = GeraBtreeEvento();
            Console.WriteLine($"GERAÇÂO BTREE FINALIZOU as {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            PesquisaEventById(BTree, 10);
            PesquisaEventById(BTree, 20);
            PesquisaEventById(BTree, 30);
            PesquisaEventById(BTree, 40);
            PesquisaEventById(BTree, 50);
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Inserindo Evento e modificando BTree");
            long id = 
            InsertEvent(new Event() {
                Brand = "samsung",
                CategoryCode = "12398120371203",
                CategoryId = "1as515s1d51asd5",
                EventTime = DateTime.Now.ToString(),
                EventType = EventType.View,
                Price = 99999,
                ProductId = 155665533,
                UserId = 897788799,
                UserSession = Guid.NewGuid().ToString(),
            }, BTree);
            sw.Stop();
            Console.WriteLine("Inserindo Evento e modificando BTree (Recriando Index Parcial)");
            PartialIndexFactory.CreateEventPartialIndex();

            Console.WriteLine($"Inserindo Evento e modificando BTree: {sw.ElapsedMilliseconds}ms");
            PesquisaEventById(BTree, 10);
            PesquisaEventById(BTree, 20);
            PesquisaEventById(BTree, 30);
            PesquisaEventById(BTree, 40);
            PesquisaEventById(BTree, id);
        }

        public static void HashTest() {
            Console.WriteLine($"INICIANDO GERAÇÂO HASH as {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            HashTable<long> hastTable = GeraHashEvento(219901453);
            Console.WriteLine($"GERAÇÂO HASH FINALIZOU as {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
            PesquisaProductEventsById(hastTable, 1002532);
            PesquisaProductEventsById(hastTable, 1002628);
            PesquisaProductEventsById(hastTable, 1002634);
            PesquisaProductEventsById(hastTable, 1002544);
            PesquisaProductEventsById(hastTable, 1002532);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Inserindo Produto e modificando HASHTable");
            long id =
            InsertProduct(new Product() {
                Brand        = "samsung",
                CategoryCode = "12398120371203",
                ProductId    = 555555555,
                CategoryId   = "sdasdada"
            }, hastTable);
            sw.Stop();

            PesquisaProductEventsById(hastTable, 1002532);
            PesquisaProductEventsById(hastTable, 1002628);
            PesquisaProductEventsById(hastTable, 1002634);
            PesquisaProductEventsById(hastTable, 1002544);
            PesquisaProductEventsById(hastTable, id);
        }

        public static void PesquisaEventById(BTreeDictionary<long, long> BTree, long id) {
            Console.WriteLine($"Pesquisando id = {id}");
            Console.WriteLine($"Pesquisando id = {id}, PartialIndex");
            
            Stopwatch sw = Stopwatch.StartNew();
            
            Event mEvent = PesquisaEventUsingPartialIndex(id)!;
            mEvent.Log();
            Console.WriteLine($"Pesquisa com PartialIndex levou {sw.ElapsedMilliseconds}ms \n\n");
            sw.Stop();

            Console.WriteLine($"Pesquisando id = {id}, BTree");
            sw = Stopwatch.StartNew();
            FileStream fsEventFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            long Position;
            BTree.TryGetValue(id, out Position);
            fsEventFile.Position = Position;
            mEvent = fsEventFile.ReadEvent();
            mEvent.Log();
            fsEventFile.Close();
            Console.WriteLine($"Pesquisa com BTree levou {sw.ElapsedMilliseconds}ms \n\n\n");
            sw.Stop();
        }

        public static void PesquisaProductEventsById(HashTable<long> hashTable, long idProduto) {
            Console.WriteLine($"Pesquisando id = {idProduto}");
            Console.WriteLine($"Pesquisando id = {idProduto}, Hash");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            FileStream fsEventFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            List<long> lstPositions = hashTable.Get(idProduto);
            List<Tuple<long, List<long>>> lstIds = new();
            foreach (long eventPosition in lstPositions){
                fsEventFile.Position = eventPosition;
                Event mEvent = fsEventFile.ReadEvent();

                if (lstIds.Any(p => p.Item1 == mEvent.ProductId)) {
                    lstIds.First(p => p.Item1 == mEvent.ProductId).Item2.Add(mEvent.id);
                } else {
                    lstIds.Add(new Tuple<long, List<long>>(mEvent.ProductId, new()));
                }
            }
            Console.WriteLine("Found [" + lstPositions.Count + "] Events");
            Console.WriteLine($"Pesquisa com Hash levou {sw.ElapsedMilliseconds}ms \n\n\n");
            sw.Start();

            Console.WriteLine($"Pesquisando id = {idProduto}, INDEX");
            FileStream fsProductFile = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            List<long> lstEventId = new();
            foreach (Tuple<long, List<long>> tProduct in lstIds) {
                Product? mProduct = PesquisaProduct(tProduct.Item1);
                foreach (long idEvent in tProduct.Item2) {
                    Event? mEvent = PesquisaEvent(tProduct.Item1);
                }
            }
            Console.WriteLine("Found [" + lstPositions.Count + "] Events");
            fsEventFile.Close();
            fsProductFile.Close();
            Console.WriteLine($"Pesquisa com INDEX levou {sw.ElapsedMilliseconds}ms \n\n\n");
            sw.Stop();
        }

        #region TDE 2 
        //[ TDE 2 ]
        public static BTreeDictionary<long, long> GeraBtreeEvento() {
            BTreeDictionary<long, long> BTree = new();
            
            FileStream fsEventFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);

            while (fsEventFile.Position < fsEventFile.Length) {
                long Position = fsEventFile.Position;
                Event mProduct = fsEventFile.ReadEvent();
                BTree.Add(mProduct.id, Position);
                long Pos;
                BTree.TryGetValue(mProduct.id, out Pos);
                decimal Percentage = ((decimal)fsEventFile.Position / (decimal)fsEventFile.Length) * 100M;
                Console.WriteLine(Percentage.ToString("n2") + "%");
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            fsEventFile.Close();

            return BTree;
        }
        
        public static HashTable<long> GeraHashEvento(int HashSize) {
            HashTable<long> lstHash = new(HashSize);
            FileStream fsEventFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            
            while (fsEventFile.Position < fsEventFile.Length) {
                long Position = fsEventFile.Position;
                Event mEvent = fsEventFile.ReadEvent();
                lstHash.Add(Position, mEvent.ProductId);

                decimal Percentage = ((decimal)fsEventFile.Position / (decimal)fsEventFile.Length) * 100M;
                Console.WriteLine(Percentage.ToString("n2") + "%");
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            fsEventFile.Close();
            return lstHash;
        }

        //[ TDE 2 ]
        #endregion TDE 2 
        //[ 2.1 - 3&4 (Product) ]
        public static Product? PesquisaProduct(long ProductId) {
            FileStream fsProductOutputFile = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            Product? mProduct = Pesquisa.SearchProduct(fsProductOutputFile, ProductId);
            fsProductOutputFile.Close();
            return mProduct;
        }
        //[ 2.1 - 3&4 (Event) ]
        public static Event? PesquisaEvent(long id) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.Read);
            Event? mEvent = Pesquisa.SearchEvent(fsEventOutputFile, id);
            fsEventOutputFile.Close();
            return mEvent;
        }
        public static Event? PesquisaEventUsingPartialIndex(long id) {
            FileStream fsEventFile      = new(Paths.PATH_EVENT_OUTPUT_FILE  , FileMode.Open, FileAccess.Read);
            FileStream fsEventIndexFile = new(Paths.PATH_EVENT_PARTIAL_INDEX, FileMode.Open, FileAccess.Read);
            Event? mEvent = Pesquisa.SearchEventUsingPartialIndex(fsEventFile, fsEventIndexFile, id);
            fsEventFile.Close();
            fsEventIndexFile.Close();
            return mEvent;
        }

        public static void InsertEvent(Event mEvent) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.ReadWrite);
            fsEventOutputFile.Position = fsEventOutputFile.Length - Event.Size;
            Event prevEvent = fsEventOutputFile.ReadEvent();
            mEvent.id = prevEvent.id + 1;
            fsEventOutputFile.WriteEvent(mEvent);

            fsEventOutputFile.Close();
            PartialIndexFactory.CreateEventPartialIndex();
        }

        public static long InsertProduct(Product mProduct, HashTable<long> hashTable) {
            FileStream fsProductOutputFile = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Open, FileAccess.ReadWrite);
            fsProductOutputFile.Position = fsProductOutputFile.Length;
            fsProductOutputFile.WriteProduct(mProduct);

            fsProductOutputFile.Close();
            hashTable.Add(mProduct.ProductId, mProduct.ProductId);
            return mProduct.ProductId;
        }

        public static long InsertEvent(Event mEvent, BTreeDictionary<long, long> BTree) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.ReadWrite);
            fsEventOutputFile.Position = fsEventOutputFile.Length - Event.Size;
            Event prevEvent = fsEventOutputFile.ReadEvent();
            mEvent.id = prevEvent.id + 1;
            BTree.Add(mEvent.id, fsEventOutputFile.Position);
            fsEventOutputFile.WriteEvent(mEvent);

            fsEventOutputFile.Close();
            PartialIndexFactory.CreateEventPartialIndex();
            return mEvent.id;
        }

        //[ 3 - Inserção/remoção de dados em um dos arquivos de dados, e reconstrução do índice ]
        public static void RemoveEvent(long id) {
            FileStream fsEventOutputFile = new(Paths.PATH_EVENT_OUTPUT_FILE, FileMode.Open, FileAccess.ReadWrite);
            fsEventOutputFile.Position = id * Event.Size;
            Event mEvent = fsEventOutputFile.ReadEvent();
            mEvent.Excluido = true;
            fsEventOutputFile.Position -= Event.Size;
            fsEventOutputFile.WriteEvent(mEvent);
            fsEventOutputFile.Close();
        }
    }
}