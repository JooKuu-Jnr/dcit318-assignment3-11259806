using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace InventoryRecordApp
{
    public interface IInventoryEntity
    {
        int Id { get; }
    }

    public record InventoryItem(int Id, string Name, int Quantity, DateTime DateAdded) : IInventoryEntity;

    public class InventoryLogger<T> where T : IInventoryEntity
    {
        private readonly List<T> _log = new();
        private readonly string _filePath;

        public InventoryLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Add(T item)
        {
            _log.Add(item);
        }

        public List<T> GetAll()
        {
            return new List<T>(_log);
        }

        public void SaveToFile()
        {
            try
            {
                string fullPath = Path.GetFullPath(_filePath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
                using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                var opts = new JsonSerializerOptions { WriteIndented = true };
                JsonSerializer.Serialize(fs, _log, opts);
                Console.WriteLine($"Saved {_log.Count} items to: {fullPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Save failed, access denied. {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Save failed, IO error. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed. {ex.Message}");
            }
        }

        public void LoadFromFile()
        {
            try
            {
                string fullPath = Path.GetFullPath(_filePath);
                if (!File.Exists(fullPath))
                {
                    Console.WriteLine($"No file found at: {fullPath}");
                    _log.Clear();
                    return;
                }
                using var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var items = JsonSerializer.Deserialize<List<T>>(fs) ?? new List<T>();
                _log.Clear();
                _log.AddRange(items);
                Console.WriteLine($"Loaded {_log.Count} items from: {fullPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Load failed, access denied. {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Load failed, IO error. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Load failed. {ex.Message}");
            }
        }
    }

    public class InventoryApp
    {
        private readonly InventoryLogger<InventoryItem> _logger;

        public InventoryApp(string filePath)
        {
            _logger = new InventoryLogger<InventoryItem>(filePath);
        }

        public void SeedSampleData()
        {
            _logger.Add(new InventoryItem(1, "LED Bulb", 50, DateTime.Now));
            _logger.Add(new InventoryItem(2, "AA Batteries", 120, DateTime.Now));
            _logger.Add(new InventoryItem(3, "USB Cable", 35, DateTime.Now));
            _logger.Add(new InventoryItem(4, "Notebook", 80, DateTime.Now));
            _logger.Add(new InventoryItem(5, "Stapler", 15, DateTime.Now));
        }

        public void SaveData()
        {
            _logger.SaveToFile();
        }

        public void LoadData()
        {
            _logger.LoadFromFile();
        }

        public void PrintAllItems()
        {
            foreach (var item in _logger.GetAll())
            {
                Console.WriteLine($"Id: {item.Id}, Name: {item.Name}, Qty: {item.Quantity}, DateAdded: {item.DateAdded:g}");
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            string path = args.Length > 0
                ? args[0]
                : Path.Combine(Directory.GetCurrentDirectory(), "inventory.json");

            var app = new InventoryApp(path);

            app.SeedSampleData();
            app.SaveData();

            app.LoadData();
            app.PrintAllItems();
        }
    }
}
