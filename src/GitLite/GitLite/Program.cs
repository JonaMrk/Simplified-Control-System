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

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}