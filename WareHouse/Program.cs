using System;
using System.Collections.Generic;

namespace WarehouseAppNS
{
    public interface IInventoryItem
    {
        int Id { get; }
        string Name { get; }
        int Quantity { get; set; }
    }

    public class ElectronicItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public string Brand { get; }
        public int WarrantyMonths { get; }

        public ElectronicItem(int id, string name, int quantity, string brand, int warrantyMonths)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            Brand = brand;
            WarrantyMonths = warrantyMonths;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Qty: {Quantity}, Brand: {Brand}, Warranty: {WarrantyMonths} months";
        }
    }

    public class GroceryItem : IInventoryItem
    {
        public int Id { get; }
        public string Name { get; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; }

        public GroceryItem(int id, string name, int quantity, DateTime expiryDate)
        {
            Id = id;
            Name = name;
            Quantity = quantity;
            ExpiryDate = expiryDate;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Qty: {Quantity}, Expiry: {ExpiryDate:d}";
        }
    }

    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string message) : base(message) { }
    }

    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(string message) : base(message) { }
    }

    public class InvalidQuantityException : Exception
    {
        public InvalidQuantityException(string message) : base(message) { }
    }

    public class InventoryRepository<T> where T : IInventoryItem
    {
        private readonly Dictionary<int, T> _items = new Dictionary<int, T>();

        public void AddItem(T item)
        {
            if (_items.ContainsKey(item.Id))
                throw new DuplicateItemException($"Item with Id {item.Id} already exists");
            _items[item.Id] = item;
        }

        public T GetItemById(int id)
        {
            if (!_items.TryGetValue(id, out var item))
                throw new ItemNotFoundException($"Item with Id {id} not found");
            return item;
        }

        public void RemoveItem(int id)
        {
            if (!_items.Remove(id))
                throw new ItemNotFoundException($"Item with Id {id} not found");
        }

        public List<T> GetAllItems()
        {
            return new List<T>(_items.Values);
        }

        public void UpdateQuantity(int id, int newQuantity)
        {
            if (newQuantity < 0)
                throw new InvalidQuantityException("Quantity cannot be negative");
            var item = GetItemById(id);
            item.Quantity = newQuantity;
        }
    }

    public class WareHouseManager
    {
        private readonly InventoryRepository<ElectronicItem> _electronics = new InventoryRepository<ElectronicItem>();
        private readonly InventoryRepository<GroceryItem> _groceries = new InventoryRepository<GroceryItem>();

        public void SeedData()
        {
            _electronics.AddItem(new ElectronicItem(101, "Smartphone", 20, "Samsung", 24));
            _electronics.AddItem(new ElectronicItem(102, "Laptop", 10, "Dell", 12));
            _electronics.AddItem(new ElectronicItem(103, "Headphones", 30, "Sony", 6));

            _groceries.AddItem(new GroceryItem(201, "Milk", 50, DateTime.Today.AddDays(7)));
            _groceries.AddItem(new GroceryItem(202, "Rice", 100, DateTime.Today.AddMonths(12)));
            _groceries.AddItem(new GroceryItem(203, "Eggs", 200, DateTime.Today.AddDays(14)));
        }

        public void PrintAllItems<T>(InventoryRepository<T> repo) where T : IInventoryItem
        {
            foreach (var item in repo.GetAllItems())
                Console.WriteLine(item);
        }

        public void IncreaseStock<T>(InventoryRepository<T> repo, int id, int quantity) where T : IInventoryItem
        {
            try
            {
                var item = repo.GetItemById(id);
                int newQty = item.Quantity + quantity;
                repo.UpdateQuantity(id, newQty);
                Console.WriteLine($"Updated Id {id} to Qty {newQty}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed. {ex.Message}");
            }
        }

        public void RemoveItemById<T>(InventoryRepository<T> repo, int id) where T : IInventoryItem
        {
            try
            {
                repo.RemoveItem(id);
                Console.WriteLine($"Removed Id {id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed. {ex.Message}");
            }
        }

        public void DemoInvalidQuantity<T>(InventoryRepository<T> repo, int id, int newQuantity) where T : IInventoryItem
        {
            try
            {
                repo.UpdateQuantity(id, newQuantity);
                Console.WriteLine($"Updated Id {id} to Qty {newQuantity}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed. {ex.Message}");
            }
        }

        public InventoryRepository<ElectronicItem> ElectronicsRepo => _electronics;
        public InventoryRepository<GroceryItem> GroceriesRepo => _groceries;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var mgr = new WareHouseManager();
            mgr.SeedData();

            Console.WriteLine("Grocery items:");
            mgr.PrintAllItems(mgr.GroceriesRepo);
            Console.WriteLine();

            Console.WriteLine("Electronic items:");
            mgr.PrintAllItems(mgr.ElectronicsRepo);
            Console.WriteLine();

            Console.WriteLine("Try duplicate add:");
            try
            {
                mgr.ElectronicsRepo.AddItem(new ElectronicItem(101, "Tablet", 5, "Apple", 12));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed. {ex.Message}");
            }

            Console.WriteLine("Try remove non existent:");
            mgr.RemoveItemById(mgr.GroceriesRepo, 999);

            Console.WriteLine("Try invalid quantity update:");
            mgr.DemoInvalidQuantity(mgr.ElectronicsRepo, 102, -5);

            Console.WriteLine("Increase stock:");
            mgr.IncreaseStock(mgr.GroceriesRepo, 202, 25);
        }
    }
}
