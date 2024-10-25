using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public static class PartialIndexFactory {

        public static int TotalAdressesPerIndex = 100;
        
        public static void CreateEventPartialIndex() {
            FileStream fsEventFile             = new(Paths.PATH_EVENT_OUTPUT_FILE  , FileMode.Open  , FileAccess.Read);
            FileStream fsEventPartialIndexFile = new(Paths.PATH_EVENT_PARTIAL_INDEX, FileMode.Create, FileAccess.ReadWrite);

            Console.WriteLine("Creating Partial Index");
            fsEventFile.Position += (Event.Size * TotalAdressesPerIndex); 

            while(true) {
                long Position = fsEventFile.Position;
                Event mEvent  = fsEventFile.ReadEvent();

                if(mEvent.id < 0) {
                    if(((int)fsEventFile.Length / Event.Size) % TotalAdressesPerIndex == 0) {
                        break;
                    } else {
                        fsEventFile.Position = (fsEventFile.Length / Event.Size) - 1 * Event.Size - 1;
                        mEvent = fsEventFile.ReadEvent();
                        Write(Position, mEvent, fsEventFile, fsEventPartialIndexFile);
                        break;
                    }
                }

                Write(Position, mEvent, fsEventFile, fsEventPartialIndexFile);
            }
        }

        private static void Write(long Position, Event mEvent, FileStream fsEventFile, FileStream fsEventPartialIndexFile) {
            EventPartialIndex mIndex = new() { id = mEvent.id, Position = Position, Excluido = mEvent.Excluido };
            fsEventPartialIndexFile.WriteEventIndex(mIndex);

            decimal Percentage = ((decimal)fsEventFile.Position / (decimal)fsEventFile.Length) * 100M;
            Console.WriteLine(Percentage.ToString("n2") + "%");
            Console.SetCursorPosition(0, Console.CursorTop - 1);

            fsEventFile.Position += (Event.Size * (TotalAdressesPerIndex - 1));
        }

        public static EventPartialIndex ReadEventIndex(this FileStream fs) {
            byte[] bytes = fs.ReadBytes(EventPartialIndex.Size);

            EventPartialIndex mIndex = new();

            int Skip = 0;
            byte[] bufferId       = bytes.Skip(Skip).Take(sizeof(long)).ToArray();
            Skip += sizeof(long);
            byte[] bufferPosition = bytes.Skip(Skip).Take(sizeof(long)).ToArray();
            Skip += sizeof(long);
            byte[] bufferDelete   = bytes.Skip(Skip).Take(sizeof(bool)).ToArray();

            mIndex.id       = BitConverter.ToInt64(bufferId);
            mIndex.Position = BitConverter.ToInt64(bufferPosition);
            mIndex.Excluido = BitConverter.ToBoolean(bufferDelete);
            return mIndex;
        }

        public static void WriteEventIndex(this FileStream fs, EventPartialIndex mIndex) {
            byte[] bufferId           = BitConverter.GetBytes(mIndex.id);
            byte[] bufferPosition     = BitConverter.GetBytes(mIndex.Position);
            byte[] bufferExcluido     = BitConverter.GetBytes(mIndex.Excluido);
            fs.Write(bufferId);
            fs.Write(bufferPosition);
            fs.Write(bufferExcluido);
            fs.Write(Encoding.ASCII.GetBytes("\n"));
        }


    }
}
