using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TDE_1 {
    public class Event {
        public long id { get; set; }
        public string EventTime { get; set; }
        public EventType EventType { get; set; }
        public long ProductId { get; set; }
        public string CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string Brand { get; set; }
        public double Price { get; set; }
        public long UserId { get; set; }
        public string UserSession { get; set; }
        public bool Excluido { get; set; } = false;
        public void Pad() {
            EventTime    = EventTime   .PadRight(StringSizes.EventTime_STRING_SIZE);
            CategoryId   = CategoryId  .PadRight(StringSizes.CategoryId_STRING_SIZE);
            CategoryCode = CategoryCode.PadRight(StringSizes.CategoryCode_STRING_SIZE);
            Brand        = Brand       .PadRight(StringSizes.Brand_STRING_SIZE);
            UserSession  = UserSession .PadRight(StringSizes.UserSession_STRING_SIZE);
        }

        public static readonly int Size = sizeof(long) +
                                          StringSizes.EventTime_STRING_SIZE +
                                          sizeof(int) +
                                          sizeof(long) +
                                          StringSizes.CategoryId_STRING_SIZE +
                                          StringSizes.CategoryCode_STRING_SIZE +
                                          StringSizes.Brand_STRING_SIZE +
                                          sizeof(double) +
                                          sizeof(long) +
                                          StringSizes.UserSession_STRING_SIZE +
                                          sizeof(bool) +
                                          1;

        public static readonly int ExcluidoPosition = Size - 1 - sizeof(bool);

        public override string ToString() {
            return "EVENT START -> " +
                   id + '\n' +
                   EventTime    + '\n' +
                   EventType    + '\n' +    
                   ProductId    + '\n' +    
                   CategoryId   + '\n' +
                   CategoryCode + '\n' +
                   Brand        + '\n' +
                   Price        + '\n' +
                   UserId       + '\n' +
                   UserSession  + '\n' +
                   Excluido     + '\n' +
                   "EVENT END \n";
        }
    }

    public enum EventType {
        View           = 0, 
        Cart           = 1, 
        RemoveFromCart = 2, 
        Purchase       = 3
    }

}
