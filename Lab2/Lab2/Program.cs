using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Lab2AsyncCrud
{
    // Entity interface to enforce Id property
    public interface IEntity
    {
        Guid Id { get; set; }
    }

    // Generic asynchronous CRUD service with thread-safety, pagination, and file persistence
    public interface ICrudServiceAsync<T> : IEnumerable<T> where T : IEntity
    {
        Task<bool> CreateAsync(T element);
        Task<T> ReadAsync(Guid id);
        Task<IEnumerable<T>> ReadAllAsync();
        Task<IEnumerable<T>> ReadAllAsync(int page, int amount);
        Task<bool> UpdateAsync(T element);
        Task<bool> RemoveAsync(T element);
        Task<bool> SaveAsync();
    }

    public class CrudServiceAsync<T> : ICrudServiceAsync<T> where T : IEntity
    {
        private readonly List<T> _items = new List<T>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly string _filePath;

        public CrudServiceAsync(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<bool> CreateAsync(T element)
        {
            _lock.EnterWriteLock();
            try
            {
                _items.Add(element);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task<T> ReadAsync(Guid id)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.FirstOrDefault(e => e.Id == id);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<IEnumerable<T>> ReadAllAsync()
        {
            _lock.EnterReadLock();
            try
            {
                return _items.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<IEnumerable<T>> ReadAllAsync(int page, int amount)
        {
            _lock.EnterReadLock();
            try
            {
                return _items.Skip((page - 1) * amount).Take(amount).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<bool> UpdateAsync(T element)
        {
            _lock.EnterWriteLock();
            try
            {
                var index = _items.FindIndex(e => e.Id == element.Id);
                if (index < 0) return false;
                _items[index] = element;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task<bool> RemoveAsync(T element)
        {
            _lock.EnterWriteLock();
            try
            {
                return _items.RemoveAll(e => e.Id == element.Id) > 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        // SaveAsync: snapshot under lock, then write outside to avoid lock held across await
        public async Task<bool> SaveAsync()
        {
            List<T> snapshot;
            _lock.EnterReadLock();
            try
            {
                snapshot = _items.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }

            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(json);
            }

            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            _lock.EnterReadLock();
            try
            {
                return _items.ToList().GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Sample entity: Bus with generated data
    public class Bus : IEntity
    {
        private static readonly string[] Models = { "Volvo", "Mercedes", "Scania", "MAN", "Ikarus" };
        private static readonly Random Rnd = new Random();

        public Guid Id { get; set; }
        public string Model { get; set; }
        public int Capacity { get; set; }
        public double Price { get; set; }

        public static Bus CreateNew()
        {
            return new Bus
            {
                Id = Guid.NewGuid(),
                Model = Models[Rnd.Next(Models.Length)],
                Capacity = Rnd.Next(20, 61),
                Price = Math.Round(Rnd.NextDouble() * 200_000 + 50_000, 2)
            };
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var service = new CrudServiceAsync<Bus>("buses.json");

            // Use thread-safe collection for tasks
            var tasks = new ConcurrentBag<Task<bool>>();
            Parallel.For(0, 1000, i =>
            {
                var bus = Bus.CreateNew();
                tasks.Add(service.CreateAsync(bus));
            });
            await Task.WhenAll(tasks);

            // Compute min, max, average for Capacity and Price
            var all = await service.ReadAllAsync();
            Console.WriteLine($"Capacity: Min = {all.Min(b => b.Capacity)}, Max = {all.Max(b => b.Capacity)}, Avg = {all.Average(b => b.Capacity):F2}");
            Console.WriteLine($"Price:    Min = {all.Min(b => b.Price):C}, Max = {all.Max(b => b.Price):C}, Avg = {all.Average(b => b.Price):C}");

            // Save to file
            await service.SaveAsync();
            Console.WriteLine("Data saved to buses.json");

            // Synchronization primitives examples
            object lockObj = new object();
            SemaphoreSlim semaphore = new SemaphoreSlim(2);
            AutoResetEvent autoReset = new AutoResetEvent(false);

            // Lock example
            lock (lockObj)
            {
                Console.WriteLine("Lock acquired");
            }

            // Semaphore example
            await semaphore.WaitAsync();
            try
            {
                Console.WriteLine("Inside semaphore slot");
            }
            finally
            {
                semaphore.Release();
            }

            // AutoResetEvent example
            Task.Run(() =>
            {
                Console.WriteLine("Waiting for signal...");
                autoReset.WaitOne();
                Console.WriteLine("AutoResetEvent signaled");
            });
            Thread.Sleep(500);
            autoReset.Set();
        }
    }
}
