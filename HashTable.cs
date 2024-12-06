using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AlgEstruturaDados2 {
    public class HashTable<T> {

        int Capacity;
        public HashTable(int Capacity) {
            for (int i = 0; i < Capacity; i++) {
                this.lstItems.Add(null);
            }
            this.Capacity = Capacity;
        }

        public void Add(T Item, long Value) {
            int Index = (int)(Value % Capacity);
            List<Tuple<long, T>>? L = this.lstItems.ElementAtOrDefault(Index);
            if (L == null) {
                this.lstItems[Index] = new List<Tuple<long, T>>() { new Tuple<long, T>(Value, Item) };
            } else {
                this.lstItems[Index].Add(new Tuple<long, T>(Value, Item));
            }
        }

        public List<T>? GetOrDefault(long Value) {
            int Index = (int)(Value % Capacity);
            List<Tuple<long, T>>? lst = this.lstItems.ElementAtOrDefault(Index);
            return lst == null ? null : lst.Where(p => p.Item1 == Value).Select(p => p.Item2).ToList();
        }

        public List<T> Get(long Value) {
            int Index = (int)(Value % Capacity);
            List<Tuple<long, T>> lst = this.lstItems.ElementAt(Index);
            return lst.Where(p => p.Item1 == Value).Select(p => p.Item2).ToList();
        }

        private int lstItemsSize { get { return this.lstItems.Count(p => p != null); } }

        private List<List<Tuple<long, T>>> lstItems = new();
    }
}
