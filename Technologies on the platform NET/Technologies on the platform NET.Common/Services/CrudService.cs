using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

namespace TechnologiesOnPlatformNET.Common.Services
{
    public class CrudService<T> : ICrudService<T> where T : class
    {
        private readonly Dictionary<Guid, T> _storage = new();

        public void Create(T element)
        {
            var id = (Guid)element.GetType().GetProperty("Id").GetValue(element);
            _storage[id] = element;
        }

        public T Read(Guid id) => _storage.TryGetValue(id, out var value) ? value : null;

        public IEnumerable<T> ReadAll() => _storage.Values;

        public void Update(T element)
        {
            var id = (Guid)element.GetType().GetProperty("Id").GetValue(element);
            _storage[id] = element;
        }

        public void Remove(T element)
        {
            var id = (Guid)element.GetType().GetProperty("Id").GetValue(element);
            _storage.Remove(id);
        }

        // Додаткове завдання
        public void Save(string filePath)
        {
            var json = JsonSerializer.Serialize(_storage.Values);
            File.WriteAllText(filePath, json);
        }

        public void Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var list = JsonSerializer.Deserialize<List<T>>(json);
            _storage.Clear();
            foreach (var item in list)
            {
                var id = (Guid)item.GetType().GetProperty("Id").GetValue(item);
                _storage[id] = item;
            }
        }
    }
}