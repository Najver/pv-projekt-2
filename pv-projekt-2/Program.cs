using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace pv_projekt_2
{
    public class Program
    {
        /// <summary>
        /// The main entry point of the program. It simulates bank transactions with and without race condition handling.
        /// </summary>
        /// <param name="args">Command-line arguments (not used in this program).</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Zadejte počet účtů (kladné celé číslo):");
            int accountCount = GetValidatedInput();
    
            Console.WriteLine("Zadejte počáteční zůstatek na každém účtu (kladné celé číslo):");
            int initialBalance = GetValidatedInput();
    
            List<int> accounts = InitializeAccounts(accountCount, initialBalance);
    
            Console.WriteLine("\nSimulace bez řešení race condition:");
            SimulateTransactions(accounts, useLock: false);
    
            accounts = InitializeAccounts(accountCount, initialBalance); // Reset účtů
            Console.WriteLine("\nSimulace s řešením race condition (synchronizace):");
            SimulateTransactions(accounts, useLock: true);
            
            Console.WriteLine("\nStiskněte ENTER pro ukončení...");
            Console.ReadLine();
        }

        /// <summary>
        /// Initializes a list of bank accounts with a specified count and initial balance.
        /// </summary>
        /// <param name="count">The number of accounts to create.</param>
        /// <param name="initialBalance">The initial balance for each account.</param>
        /// <returns>A List of integers representing the bank accounts, where each integer is the account balance.</returns>
        public static List<int> InitializeAccounts(int count, int initialBalance)
        {
            var accounts = new List<int>();
            for (int i = 0; i < count; i++)
            {
                accounts.Add(initialBalance);
            }
            return accounts;
        }

        /// <summary>
        /// Simulates a series of bank transactions between accounts, with or without race condition handling.
        /// </summary>
        /// <param name="accounts">A list of integers representing bank account balances.</param>
        /// <param name="useLock">A boolean flag indicating whether to use locking for thread safety. 
        /// If true, transactions are performed with synchronization to prevent race conditions. 
        /// If false, transactions are performed without synchronization.</param>
        /// <remarks>
        /// This method performs 10,000 random transactions between accounts. Each transaction 
        /// transfers a random amount between 1 and 99 from one randomly selected account to another. 
        /// After all transactions are completed, it displays the final balances of all accounts.
        /// </remarks>
        public static void SimulateTransactions(List<int> accounts, bool useLock)
        {
            object lockObject = new object();
            Random random = new Random();
            int transactionCount = 10000;
        
            Parallel.For(0, transactionCount, _ =>
            {
                int fromAccount = random.Next(accounts.Count);
                int toAccount = random.Next(accounts.Count);
                int amount = random.Next(1, 100);
        
                if (useLock)
                {
                    lock (lockObject)
                    {
                        PerformTransaction(accounts, fromAccount, toAccount, amount);
                    }
                }
                else
                {
                    PerformTransaction(accounts, fromAccount, toAccount, amount);
                }
            });
        
            DisplayAccountBalances(accounts);
        }

        /// <summary>
        /// Performs a transaction between two accounts by transferring a specified amount from one account to another.
        /// </summary>
        /// <param name="accounts">A list of integers representing bank account balances.</param>
        /// <param name="from">The index of the account from which the amount is to be deducted.</param>
        /// <param name="to">The index of the account to which the amount is to be added.</param>
        /// <param name="amount">The amount to be transferred between the accounts.</param>
        /// <remarks>
        /// The transaction is only performed if the 'from' and 'to' accounts are different and if the 'from' account has sufficient balance.
        /// </remarks>
        public static void PerformTransaction(List<int> accounts, int from, int to, int amount)
        {
            if (from != to && accounts[from] >= amount)
            {
                accounts[from] -= amount;
                accounts[to] += amount;
            }
        }

        /// <summary>
        /// Displays the balances of all accounts and calculates the total balance.
        /// </summary>
        /// <param name="accounts">A list of integers representing bank account balances.</param>
        /// <remarks>
        /// This method prints the balance of each account to the console, followed by the total balance of all accounts.
        /// Each account is displayed with its index (starting from 1) and its balance in Czech Koruna (Kč).
        /// </remarks>
        public static void DisplayAccountBalances(List<int> accounts)
        {
            Console.WriteLine("Zůstatky na účtech:");
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.WriteLine($"Účet {i + 1}: {accounts[i]} Kč");
            }
        
            int totalBalance = accounts.Sum();
            Console.WriteLine($"Celkový zůstatek: {totalBalance}");
        }

        /// <summary>
        /// Prompts the user for input and validates it to ensure it's a positive integer.
        /// </summary>
        /// <returns>
        /// A positive integer entered by the user.
        /// </returns>
        /// <remarks>
        /// This method continuously prompts the user until a valid positive integer is entered.
        /// If an invalid input is provided, an error message is displayed, and the user is asked to try again.
        /// </remarks>
        public static int GetValidatedInput()
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (int.TryParse(input, out int result) && result > 0)
                {
                    return result;
                }
                else
                {
                    Console.WriteLine("Neplatný vstup. Zadejte prosím kladné celé číslo:");
                }
            }
        }
    }
}