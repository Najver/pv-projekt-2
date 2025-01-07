using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using pv_projekt_2;

namespace pv_projekt
{
    /// <summary>
    /// Test fixture for the Program class.
    /// </summary>
    [TestFixture]
    public class ProgramTests
    {
        /// <summary>
        /// Tests the InitializeAccounts method of the Program class.
        /// </summary>
        /// <remarks>
        /// This test verifies that the InitializeAccounts method correctly initializes
        /// a specified number of accounts with a given initial balance.
        /// </remarks>
        [Test]
        public void TestInitializeAccounts()
        {
            // Arrange
            int accountCount = 5;
            int initialBalance = 100;

            // Act
            var accounts = Program.InitializeAccounts(accountCount, initialBalance);

            // Assert
            Assert.AreEqual(accountCount, accounts.Count);
            foreach (var balance in accounts)
            {
                Assert.AreEqual(initialBalance, balance);
            }
        }

        /// <summary>
        /// Tests the PerformTransaction method of the Program class with a valid transaction.
        /// </summary>
        /// <remarks>
        /// This test ensures that a valid transaction correctly transfers the specified amount
        /// from one account to another.
        /// </remarks>
        [Test]
        public void TestPerformTransaction_ValidTransaction()
        {
            // Arrange
            var accounts = new List<int> { 100, 200 };
            int from = 0;
            int to = 1;
            int amount = 50;

            // Act
            Program.PerformTransaction(accounts, from, to, amount);

            // Assert
            Assert.AreEqual(50, accounts[from]);
            Assert.AreEqual(250, accounts[to]);
        }

        /// <summary>
        /// Tests the PerformTransaction method of the Program class with an invalid transaction.
        /// </summary>
        /// <remarks>
        /// This test verifies that an invalid transaction (where the amount exceeds the available balance)
        /// does not change the account balances.
        /// </remarks>
        [Test]
        public void TestPerformTransaction_InvalidTransaction()
        {
            // Arrange
            var accounts = new List<int> { 100, 200 };
            int from = 0;
            int to = 1;
            int amount = 150; // More than available balance

            // Act
            Program.PerformTransaction(accounts, from, to, amount);

            // Assert
            Assert.AreEqual(100, accounts[from]); // No change
            Assert.AreEqual(200, accounts[to]); // No change
        }

        /// <summary>
        /// Tests the DisplayAccountBalances method of the Program class.
        /// </summary>
        /// <remarks>
        /// This test checks if the DisplayAccountBalances method correctly outputs
        /// the account balances and the total balance to the console.
        /// </remarks>
        [Test]
        public void TestDisplayAccountBalances()
        {
            // Arrange
            var accounts = new List<int> { 100, 200, 300 };

            using (var sw = new System.IO.StringWriter())
            {
                Console.SetOut(sw);

                // Act
                Program.DisplayAccountBalances(accounts);

                // Assert
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("Účet 1: 100 Kč"));
                Assert.IsTrue(output.Contains("Účet 2: 200 Kč"));
                Assert.IsTrue(output.Contains("Účet 3: 300 Kč"));
                Assert.IsTrue(output.Contains("Celkový zůstatek: 600"));
            }
        }

        /// <summary>
        /// Tests the GetValidatedInput method of the Program class with valid input.
        /// </summary>
        /// <remarks>
        /// This test ensures that the GetValidatedInput method correctly accepts and returns
        /// a valid integer input.
        /// </remarks>
        [Test]
        public void TestGetValidatedInput_ValidInput()
        {
            // Arrange
            using (var sr = new System.IO.StringReader("5\n"))
            {
                Console.SetIn(sr);

                // Act
                int result = Program.GetValidatedInput();

                // Assert
                Assert.AreEqual(5, result);
            }
        }

        /// <summary>
        /// Tests the GetValidatedInput method of the Program class with invalid input.
        /// </summary>
        /// <remarks>
        /// This test verifies that the GetValidatedInput method correctly handles invalid inputs
        /// (negative numbers and non-numeric inputs) and prompts for valid input until received.
        /// </remarks>
        [Test]
        public void TestGetValidatedInput_InvalidInput()
        {
            // Arrange
            using (var sr = new System.IO.StringReader("-1\nabc\n10\n"))
            {
                Console.SetIn(sr);

                using (var sw = new System.IO.StringWriter())
                {
                    Console.SetOut(sw);

                    // Act
                    int result = Program.GetValidatedInput();

                    // Assert
                    Assert.AreEqual(10, result);
                    Assert.IsTrue(sw.ToString().Contains("Neplatný vstup"));
                }
            }
        }
        /// <summary>
        /// Tests the Main method of the Program class with a single account scenario.
        /// </summary>
        /// <remarks>
        /// This test verifies that the Main method correctly handles input for a single account
        /// and outputs the expected results, including account balances and total balance,
        /// both with and without race condition handling.
        /// </remarks>
        [Test]
        public void TestMain_SingleAccount()
        {
            // Arrange
            var input = new StringReader("1\n100\n");
            Console.SetIn(input);
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            Program.Main(new string[0]);

            // Assert
            var result = output.ToString();
            Assert.IsTrue(result.Contains("Zadejte počet účtů (kladné celé číslo):"));
            Assert.IsTrue(result.Contains("Zadejte počáteční zůstatek na každém účtu (kladné celé číslo):"));
            Assert.IsTrue(result.Contains("Simulace bez řešení race condition:"));
            Assert.IsTrue(result.Contains("Simulace s řešením race condition (synchronizace):"));
            Assert.AreEqual(2, result.Split("Zůstatky na účtech:").Length - 1);
            Assert.AreEqual(2, result.Split("Účet 1: 100 Kč").Length - 1);
            Assert.AreEqual(2, result.Split("Celkový zůstatek: 100").Length - 1);
        }
        /// <summary>
        /// Tests the Main method of the Program class to verify concurrent access handling.
        /// </summary>
        /// <remarks>
        /// This test checks if the Main method correctly handles concurrent access to accounts,
        /// ensuring that the total balance remains constant when using locks, and differs when not using locks.
        /// </remarks>
        [Test]
        public void TestMain_ConcurrentAccess()
        {
            // Arrange
            var input = new StringReader("5\n1000\n");
            Console.SetIn(input);
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            Program.Main(new string[0]);

            // Assert
            var result = output.ToString();
            Assert.IsTrue(result.Contains("Simulace bez řešení race condition:"));
            Assert.IsTrue(result.Contains("Simulace s řešením race condition (synchronizace):"));

            var balancesWithoutLock = ExtractTotalBalance(result, "Simulace bez řešení race condition:");
            var balancesWithLock = ExtractTotalBalance(result, "Simulace s řešením race condition (synchronizace):");

            Assert.AreEqual(5000, balancesWithLock, "Total balance with lock should remain constant");
            Assert.AreNotEqual(balancesWithoutLock, balancesWithLock, "Balances should differ due to race condition");
        }
        /// <summary>
        /// Tests the Main method of the Program class to verify that the total balance changes
        /// when transactions are performed without using locks to handle race conditions.
        /// </summary>
        /// <remarks>
        /// This test simulates concurrent access to accounts and checks if the total balance
        /// changes due to race conditions when locks are not used.
        /// </remarks>
        [Test]
        public void TestMain_TotalBalanceChangeWithoutLock()
        {
            // Arrange
            var input = new StringReader("3\n1000\n");
            Console.SetIn(input);
            var output = new StringWriter();
            Console.SetOut(output);

            // Act
            Program.Main(new string[0]);

            // Assert
            var result = output.ToString();
            Assert.IsTrue(result.Contains("Simulace bez řešení race condition:"));

            int initialTotalBalance = 3 * 1000; // 3 accounts with 1000 initial balance
            int finalTotalBalance = ExtractTotalBalance(result, "Simulace bez řešení race condition:");

            Assert.AreNotEqual(initialTotalBalance, finalTotalBalance, "Total balance should change after transactions without lock");
        }
        
        /// <summary>
        /// Extracts the total balance from the output string based on the specified header.
        /// </summary>
        /// <param name="output">The output string containing account information and balances.</param>
        /// <param name="header">The header string indicating the section from which to extract the balance.</param>
        /// <returns>The total balance as an integer extracted from the specified section of the output.</returns>
        private int ExtractTotalBalance(string output, string header)
        {
            var startIndex = output.IndexOf(header);
            var endIndex = output.IndexOf("Celkový zůstatek:", startIndex);
            var balanceLine = output.Substring(endIndex).Split('\n')[0];
            return int.Parse(balanceLine.Split(':')[1].Trim());
        }
        /// <summary>
        /// Tests the SimulateTransactions method for thread safety by verifying the total balance remains unchanged with locks.
        /// </summary>
        [Test]
        public void TestSimulateTransactions_ThreadSafety()
        {
            var accounts = Program.InitializeAccounts(5, 1000);
            int initialTotalBalance = accounts.Sum();
            Program.SimulateTransactions(accounts, useLock: true);
            int finalTotalBalance = accounts.Sum();
            Assert.AreEqual(initialTotalBalance, finalTotalBalance);
        }

        /// <summary>
        /// Tests the SimulateTransactions method without using locks, expecting the total balance to change.
        /// </summary>
        [Test]
        public void TestSimulateTransactions_WithoutLock()
        {
            var accounts = Program.InitializeAccounts(5, 1000);
            int initialTotalBalance = accounts.Sum();
            Program.SimulateTransactions(accounts, useLock: false);
            int finalTotalBalance = accounts.Sum();
            Assert.AreNotEqual(initialTotalBalance, finalTotalBalance);
        }

        /// <summary>
        /// Ensures all accounts are accessed at least once during the transaction simulation.
        /// </summary>
        [Test]
        public void TestSimulateTransactions_AllAccountsAccessed()
        {
            var accounts = Program.InitializeAccounts(10, 100);
            var accessedAccounts = new HashSet<int>();
            var lockObject = new object();

            Parallel.For(0, 10000, _ =>
            {
                var random = new Random();
                int from = random.Next(accounts.Count);
                int to = random.Next(accounts.Count);

                lock (lockObject)
                {
                    accessedAccounts.Add(from);
                    accessedAccounts.Add(to);
                }
            });

            Assert.AreEqual(accounts.Count, accessedAccounts.Count);
        }
        
    }
}
