using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public static class Paths{
        public static readonly string PATH_INPUT_FILE_1 = Path.Combine(Directory.GetCurrentDirectory(), "input.csv");
        //public static readonly string PATH_INPUT_FILE_1 = Path.Combine(Directory.GetCurrentDirectory(), "test.csv");
        public static readonly string PATH_INPUT_FILE_2 = Path.Combine(Directory.GetCurrentDirectory(), "2019-Nov.csv");
        
        public static readonly string PATH_EVENT_OUTPUT_FILE   = Path.Combine(Directory.GetCurrentDirectory(), "EventOutput.bin");
        public static readonly string PATH_PRODUCT_OUTPUT_FILE = Path.Combine(Directory.GetCurrentDirectory(), "ProductOutput.bin");
        public static readonly string PATH_EVENT_PARTIAL_INDEX = Path.Combine(Directory.GetCurrentDirectory(), "EventPartialIndex.bin");
    }
    
    //[ 2.1 - 1 ]
    public class DataSetFactory {
        
        StreamReader srFile1 = new(Paths.PATH_INPUT_FILE_1);
        //StreamReader srFile2 = new(Paths.PATH_INPUT_FILE_2);
        FileStream   fsEventOutputFile   = new(Paths.PATH_EVENT_OUTPUT_FILE  , FileMode.Create, FileAccess.ReadWrite);
        FileStream   fsProductOutputFile = new(Paths.PATH_PRODUCT_OUTPUT_FILE, FileMode.Create, FileAccess.ReadWrite);

        public long EventIndex   = 0;

        public void CreateDataSet() {
            string log = "Creating data set!";
            Console.WriteLine(log);
            Console.WriteLine($"Processing File -> {Paths.PATH_INPUT_FILE_1}");
            ProcessFile(srFile1);
            Console.WriteLine($"Processing Products -> ");
            ProcessProducts();

            srFile1.Close();
            fsEventOutputFile.Close();
            fsProductOutputFile.Close();
        }

        /// <summary>
        /// T1 = ProdutctId, T2 = Position  
        /// </summary>
        List<Tuple<long, long>> lstidProductPosition = new();
        
        public void ProcessFile(StreamReader Stream) {
            _ = Stream.ReadLine();
            while (!Stream.EndOfStream) {
                Event? mEvent = Stream.ReadEvent();
                if (mEvent == null) { continue; }
                mEvent.id = EventIndex;

                Product mProduct = mEvent.GetProduct();
                if (!lstidProductPosition.Any(p => p.Item1 == mProduct.ProductId)) {
                    lstidProductPosition.Add(new Tuple<long, long>(mProduct.ProductId, fsEventOutputFile.Position));
                }
                fsEventOutputFile.WriteEvent(mEvent);

                decimal Percentage = ((decimal)Stream.BaseStream.Position / (decimal)Stream.BaseStream.Length) * 100M;
                Console.WriteLine(Percentage.ToString("n2") + "%");
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                EventIndex++;
            }
        }

        public void ProcessProducts() {
            lstidProductPosition = lstidProductPosition.OrderBy(p => p.Item1).ToList();

            for(int i = 0; i < lstidProductPosition.Count; i++) {
                Tuple<long, long> prodPos = lstidProductPosition[i];
                fsEventOutputFile.Position = prodPos.Item2;

                Event mEvent = fsEventOutputFile.ReadEvent();
                Product mProduct = mEvent.GetProduct();

                decimal Percentage = ((decimal)i / (decimal)lstidProductPosition.Count() - 1) * 100M;
                Console.WriteLine(Percentage.ToString("n2") + "%");
                Console.SetCursorPosition(0, Console.CursorTop - 1);

                fsProductOutputFile.WriteProduct(mProduct);
            }
        }
    }

    public static class Extensions {
        #region [ Event ]
        public static Event? ReadEvent(this StreamReader sr) {
            string? Row = sr.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(Row)) { return null; }
            Event mEvent = new();

            string[] lstFields = Row.Split(',');
            mEvent.EventTime    = lstFields[0];
            switch (lstFields[1]) {
                case "view":
                    mEvent.EventType = EventType.View;
                    break;
                case "cart":
                    mEvent.EventType = EventType.Cart;
                    break;
                case "remove_from_cart":
                    mEvent.EventType = EventType.RemoveFromCart;
                    break;
                case "purchase":
                    mEvent.EventType = EventType.Purchase;
                    break;
                default:
                    break;
            }
            mEvent.ProductId    = long.Parse(lstFields[2]);
            mEvent.CategoryId   = lstFields[3];
            mEvent.CategoryCode = lstFields[4];
            mEvent.Brand        = lstFields[5];
            mEvent.Price        = double.Parse(lstFields[6]);
            mEvent.UserId       = long.Parse(lstFields[7]);
            mEvent.UserSession  = lstFields[8];

            return mEvent;
        }

        public static void WriteEvent(this FileStream fs, Event mEvent) {
            mEvent.Pad();
            byte[] bufferId= BitConverter.GetBytes(mEvent.id);
            fs.Write(bufferId);
            byte[] bufferEventTime = Encoding.ASCII.GetBytes(mEvent.EventTime);
            fs.Write(bufferEventTime);
            byte[] bufferEventType  = BitConverter.GetBytes((int)mEvent.EventType);
            fs.Write(bufferEventType);
            byte[] bufferProductId  = BitConverter.GetBytes(mEvent.ProductId);
            fs.Write(bufferProductId);
            byte[] bufferCategoryId = Encoding.ASCII.GetBytes(mEvent.CategoryId);
            fs.Write(bufferCategoryId);
            byte[] bufferCategoryCode = Encoding.ASCII.GetBytes(mEvent.CategoryCode);
            fs.Write(bufferCategoryCode);
            byte[] bufferBrand        = Encoding.ASCII.GetBytes(mEvent.Brand);
            fs.Write(bufferBrand);
            byte[] bufferPrice        = BitConverter.GetBytes(mEvent.Price);
            fs.Write(bufferPrice);
            byte[] bufferUserId       = BitConverter.GetBytes(mEvent.UserId);
            fs.Write(bufferUserId);
            byte[] bufferUserSession  = Encoding.ASCII.GetBytes(mEvent.UserSession);
            fs.Write(bufferUserSession);
            byte[] bufferExcluido     = BitConverter.GetBytes(mEvent.Excluido);
            fs.Write(bufferExcluido);

            fs.Write(Encoding.ASCII.GetBytes("\n"));
        }

        public static Event ReadEvent(this FileStream fs) {
            byte[] bytes = fs.ReadBytes(Event.Size);

            Event mEvent = new();

            int Skip = 0;
            byte[] bufferid           = bytes.Skip(Skip).Take(sizeof(long)).ToArray();
            Skip += sizeof(long);
            byte[] bufferEventTime    = bytes.Skip(Skip).Take(StringSizes.EventTime_STRING_SIZE).ToArray();
            Skip += StringSizes.EventTime_STRING_SIZE;
            byte[] bufferEventType    = bytes.Skip(Skip).Take(sizeof(int)).ToArray();
            Skip += sizeof(int);
            byte[] bufferProductId    = bytes.Skip(Skip).Take(sizeof(long)).ToArray();       
            Skip += sizeof(long);
            byte[] bufferCategoryId   = bytes.Skip(Skip).Take(StringSizes.CategoryId_STRING_SIZE).ToArray();        
            Skip += StringSizes.CategoryId_STRING_SIZE;
            byte[] bufferCategoryCode = bytes.Skip(Skip).Take(StringSizes.CategoryCode_STRING_SIZE).ToArray();          
            Skip += StringSizes.CategoryCode_STRING_SIZE;
            byte[] bufferBrand        = bytes.Skip(Skip).Take(StringSizes.Brand_STRING_SIZE).ToArray();   
            Skip += StringSizes.Brand_STRING_SIZE;
            byte[] bufferPrice        = bytes.Skip(Skip).Take(sizeof(double)).ToArray();   
            Skip += sizeof(double);
            byte[] bufferUserId       = bytes.Skip(Skip).Take(sizeof(long)).ToArray();    
            Skip += sizeof(long);
            byte[] bufferUserSession  = bytes.Skip(Skip).Take(StringSizes.UserSession_STRING_SIZE).ToArray();
            Skip += StringSizes.UserSession_STRING_SIZE;
            byte[] bufferExcluido     = bytes.Skip(Skip).Take(sizeof(bool)).ToArray();

            mEvent.id           = BitConverter.ToInt64(bufferid);
            mEvent.EventTime    = Encoding.Default.GetString(bufferEventTime).Trim();
            mEvent.EventType    = (EventType)BitConverter.ToInt32(bufferEventType);
            mEvent.ProductId    = BitConverter.ToInt64(bufferProductId);
            mEvent.CategoryId   = Encoding.Default.GetString(bufferCategoryId).Trim();
            mEvent.CategoryCode = Encoding.Default.GetString(bufferCategoryCode).Trim();
            mEvent.Brand        = Encoding.Default.GetString(bufferBrand).Trim();
            mEvent.Price        = BitConverter.ToDouble(bufferPrice);
            mEvent.UserId       = BitConverter.ToInt64(bufferUserId);
            mEvent.UserSession  = Encoding.Default.GetString(bufferUserSession).Trim();
            mEvent.Excluido     = BitConverter.ToBoolean(bufferExcluido);

            return mEvent;
        }
        #endregion [ Event ]
        
        public static void WriteProduct(this FileStream fs, Product mProduct) {
            mProduct.Pad();
            byte[] bufferProductId  = BitConverter.GetBytes(mProduct.ProductId);
            fs.Write(bufferProductId);
            byte[] bufferCategoryId = Encoding.ASCII.GetBytes(mProduct.CategoryId);
            fs.Write(bufferCategoryId);
            byte[] bufferCategoryCode = Encoding.ASCII.GetBytes(mProduct.CategoryCode);
            fs.Write(bufferCategoryCode);
            byte[] bufferBrand        = Encoding.ASCII.GetBytes(mProduct.Brand);
            fs.Write(bufferBrand);
            fs.Write(Encoding.ASCII.GetBytes("\n"));
        }

        public static Product ReadProduct(this FileStream fs) {
            byte[] bytes = fs.ReadBytes(Product.Size);

            Product mProduct = new();

            int Skip = 0;
            byte[] bufferProductId    = bytes.Skip(Skip).Take(sizeof(long)).ToArray();
            Skip += sizeof(long);
            byte[] bufferCategoryId   = bytes.Skip(Skip).Take(StringSizes.CategoryId_STRING_SIZE).ToArray();
            Skip += StringSizes.CategoryId_STRING_SIZE;
            byte[] bufferCategoryCode = bytes.Skip(Skip).Take(StringSizes.CategoryCode_STRING_SIZE).ToArray();
            Skip += StringSizes.CategoryCode_STRING_SIZE;
            byte[] bufferBrand        = bytes.Skip(Skip).Take(StringSizes.Brand_STRING_SIZE).ToArray();

            mProduct.ProductId    = BitConverter.ToInt64(bufferProductId);
            mProduct.CategoryId   = Encoding.Default.GetString(bufferCategoryId).Trim();
            mProduct.CategoryCode = Encoding.Default.GetString(bufferCategoryCode).Trim();
            mProduct.Brand        = Encoding.Default.GetString(bufferBrand).Trim();
            return mProduct;
        }

        public static Product GetProduct(this Event mEvent) {
            Product mProduct = new() {
                ProductId    = mEvent.ProductId,
                Brand        = mEvent.Brand,
                CategoryId   = mEvent.CategoryId,
                CategoryCode = mEvent.CategoryCode,
            };
            return mProduct;
        }

        public static byte[] ReadBytes(this FileStream Fs, int nBytes) {
            byte[] bytes = new byte[nBytes];
            for (int i = 0; i < nBytes; i++) {
                bytes[i] = (byte)Fs.ReadByte();
            }
            return bytes;
        }
    }
}
