using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Lab2AsyncCrud;

namespace Tests
{
    public class CrudServiceAsyncTests : IDisposable
    {
        private const string TestFile = "test_buses.json";
        private readonly CrudServiceAsync<Bus> _service;

        public CrudServiceAsyncTests()
        {
            // Перед кожним тестом видаляємо старий файл
            if (File.Exists(TestFile))
                File.Delete(TestFile);

            _service = new CrudServiceAsync<Bus>(TestFile);
        }

        public void Dispose()
        {
            // Після кожного тесту чистимо файл
            if (File.Exists(TestFile))
                File.Delete(TestFile);
        }

        [Fact]
        public async Task CreateReadUpdateRemove_Test()
        {
            var bus = Bus.CreateNew();

            // Create
            Assert.True(await _service.CreateAsync(bus));

            // Read
            var read = await _service.ReadAsync(bus.Id);
            Assert.NotNull(read);
            Assert.Equal(bus.Id, read.Id);

            // Update
            read.Capacity = 123;
            Assert.True(await _service.UpdateAsync(read));
            var updated = await _service.ReadAsync(bus.Id);
            Assert.Equal(123, updated.Capacity);

            // Remove
            Assert.True(await _service.RemoveAsync(updated));
            Assert.Null(await _service.ReadAsync(bus.Id));
        }

        [Fact]
        public async Task ReadAll_Returns_All_Items()
        {
            // Додаємо кілька елементів
            var buses = Enumerable.Range(0, 5).Select(_ => Bus.CreateNew()).ToList();
            foreach (var b in buses)
                await _service.CreateAsync(b);

            var all = (await _service.ReadAllAsync()).ToList();
            Assert.Equal(5, all.Count);
            // Перевіряємо, що всі ID присутні
            foreach (var b in buses)
                Assert.Contains(all, x => x.Id == b.Id);
        }

        [Fact]
        public async Task Pagination_Test()
        {
            // Додаємо 25 елементів
            for (int i = 0; i < 25; i++)
                await _service.CreateAsync(Bus.CreateNew());

            // Перші 10
            var page1 = (await _service.ReadAllAsync(1, 10)).ToList();
            Assert.Equal(10, page1.Count);

            // Друга сторінка (елементи 11–20)
            var page2 = (await _service.ReadAllAsync(2, 10)).ToList();
            Assert.Equal(10, page2.Count);
            Assert.DoesNotContain(page2[0].Id, page1.Select(x => x.Id));

            // Третя сторінка (елементи 21–25)
            var page3 = (await _service.ReadAllAsync(3, 10)).ToList();
            Assert.Equal(5, page3.Count);
        }

        [Fact]
        public async Task SaveAsync_Creates_File_With_Correct_Content()
        {
            // Додаємо 3 елементи
            var sample = Enumerable.Range(0, 3).Select(_ => Bus.CreateNew()).ToList();
            foreach (var b in sample)
                await _service.CreateAsync(b);

            // Save
            Assert.True(await _service.SaveAsync());
            Assert.True(File.Exists(TestFile));
            // Перечитуємо файл і перевіряємо JSON (синхронно)
            var json = File.ReadAllText(TestFile);

            Assert.Contains(sample[0].Id.ToString(), json);
            Assert.Contains(sample[1].Id.ToString(), json);
            Assert.Contains(sample[2].Id.ToString(), json);
        }


        [Fact]
        public void Enumerator_Yields_All_Items()
        {
            // Додаємо елементи синхронно (не async)
            var buses = Enumerable.Range(0, 4).Select(_ => Bus.CreateNew()).ToList();
            foreach (var b in buses)
                _service.CreateAsync(b).GetAwaiter().GetResult();

            // Використовуємо foreach
            var seen = new System.Collections.Generic.List<Guid>();
            foreach (var b in _service)
                seen.Add(b.Id);

            Assert.Equal(4, seen.Count);
            foreach (var b in buses)
                Assert.Contains(b.Id, seen);
        }
    }
}
