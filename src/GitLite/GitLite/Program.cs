using System;
using System.IO;

namespace GitLite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string repoPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            Console.WriteLine("Repository path: " + repoPath);
            Repository repo = new Repository(repoPath);

            Console.WriteLine("GitLite Version Control System");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Enter command:");

            string command = Console.ReadLine();

            switch (command)
            {
                case "init":
                    repo.Init();
                    Console.WriteLine("Repository initialized.");
                    break;

                case "add":

                    Console.WriteLine("Enter file name to track:");
                    string fileName = Console.ReadLine();

                    repo.AddFile(fileName);

                    Console.WriteLine("File added to index.");
                    break;

                case "commit":

                    Console.WriteLine("Enter commit message:");
                    string message = Console.ReadLine();

                    repo.CreateCommit(message);

                    Console.WriteLine("Commit created.");
                    break;

                case "log":
                    repo.PrintLog();
                    break;

                case "diff":
                    Console.WriteLine("Enter file name:");
                    string file = Console.ReadLine();

                    repo.DiffFile(file);
                    break;

                case "checkout":
                    repo.CheckoutLastCommit();
                    break;

                case "checkout-commit":
                    Console.WriteLine("Enter commit ID:");
                    string commitId = Console.ReadLine();

                    repo.CheckoutCommit(commitId);
                    break;

                case "branch":
                    Console.WriteLine("Enter branch name:");
                    string branchName = Console.ReadLine();
                    repo.CreateBranch(branchName);
                    Console.WriteLine("Branch created.");
                    break;

                case "checkout-branch":
                    Console.WriteLine("Enter branch name:");
                    string branchToCheckout = Console.ReadLine();
                    repo.CheckoutBranch(branchToCheckout);
                    break;

                case "branches":
                    var branches = repo.ListBranches();
                    foreach (var b in branches)
                    {
                        Console.WriteLine(b);
                    }
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}