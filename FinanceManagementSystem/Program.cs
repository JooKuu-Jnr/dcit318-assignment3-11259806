using System;
using System.Collections.Generic;

namespace FinanceManagementSystem
{
    public record Transaction(int Id, DateTime Date, decimal Amount, string Category);

    public interface ITransactionProcessor
    {
        void Process(Transaction transaction);
    }

    public class BankTransferProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[BankTransfer] Processing {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class MobileMoneyProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[MobileMoney] Processing {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class CryptoWalletProcessor : ITransactionProcessor
    {
        public void Process(Transaction transaction)
        {
            Console.WriteLine($"[CryptoWallet] Processing {transaction.Amount:C} for {transaction.Category}");
        }
    }

    public class Account
    {
        public string AccountNumber { get; }
        public decimal Balance { get; protected set; }

        public Account(string accountNumber, decimal initialBalance)
        {
            AccountNumber = accountNumber;
            Balance = initialBalance;
        }

        public virtual void ApplyTransaction(Transaction transaction)
        {
            Balance -= transaction.Amount;
            Console.WriteLine($"[Account] Deducted {transaction.Amount:C}. New balance: {Balance:C}");
        }
    }

    public sealed class SavingsAccount : Account
    {
        public SavingsAccount(string accountNumber, decimal initialBalance)
            : base(accountNumber, initialBalance) { }

        public override void ApplyTransaction(Transaction transaction)
        {
            if (transaction.Amount > Balance)
            {
                Console.WriteLine("Insufficient funds");
                return;
            }

            Balance -= transaction.Amount;
            Console.WriteLine($"[SavingsAccount] Deducted {transaction.Amount:C}. Updated balance: {Balance:C}");
        }
    }

    public class FinanceApp
    {
        private readonly List<Transaction> _transactions = new();

        public void Run()
        {
            var account = new SavingsAccount("ACC-001", 1000m);
            Console.WriteLine($"Created SavingsAccount {account.AccountNumber} with balance {account.Balance:C}");

            var t1 = new Transaction(1, DateTime.Now, 150m, "Groceries");
            var t2 = new Transaction(2, DateTime.Now, 300m, "Utilities");
            var t3 = new Transaction(3, DateTime.Now, 700m, "Entertainment");

            ITransactionProcessor p1 = new MobileMoneyProcessor();
            ITransactionProcessor p2 = new BankTransferProcessor();
            ITransactionProcessor p3 = new CryptoWalletProcessor();

            p1.Process(t1);
            p2.Process(t2);
            p3.Process(t3);

            account.ApplyTransaction(t1);
            account.ApplyTransaction(t2);
            account.ApplyTransaction(t3);

            _transactions.Add(t1);
            _transactions.Add(t2);
            _transactions.Add(t3);

            Console.WriteLine("All transactions recorded.");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new FinanceApp();
            app.Run();
        }
    }
}
