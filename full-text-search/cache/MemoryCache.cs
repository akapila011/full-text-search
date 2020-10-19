using System;
using System.Collections.Generic;

namespace full_text_search.cache {
    
    public class MemoryCache<K, V> {
        
        public uint MaxEntries { get; private set; }
        public string Alias { get; private set; }
        public DateTime DateCreated { get; private set; }  // UTC
        
        private Dictionary<K, V> entries;  // TODO: need to make these 3 thread safe for future
        private Queue<K> entriesQueue;  // used to determine which entries to remove first
        public uint EntriesOccupied { get; private set; }

        public MemoryCache(uint maxEntries, string alias) {
            this.MaxEntries = maxEntries;
            this.Alias = alias;
            DateTime dateTime = DateTime.Now;
            this.DateCreated = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
            
            this.entries = new Dictionary<K, V>();
            this.entriesQueue = new Queue<K>((int)this.MaxEntries);
            this.EntriesOccupied = 0;
        }

        public V get(K key) {
            this.entries.TryGetValue(key, out var value);
            return value;
        }
        
        public (K, V) set(K key, V value, bool forceOverwrite = true) {  // TODO: force overwrite option
            if (this.EntriesOccupied == MaxEntries) {
                var removedQueueKey = this.entriesQueue.Dequeue();
                var removedCount = this.remove(removedQueueKey);
            }

            if (forceOverwrite) {
                this.entries[key] = value;
            }
            else {
                this.entries.Add(key, value);
            }
            this.entriesQueue.Enqueue(key);
            this.EntriesOccupied += 1;
            return (key, value);
        }

        public int remove(K key) {
            var removed = this.entries.Remove(key);
            if (!removed) {
                return 0;
            }
            this.EntriesOccupied -= 1;
            return 1;
        }
        
        // TODO: flush
    }
}