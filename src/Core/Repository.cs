using System;
using System.IO;
using Storage;

namespace Core
{
    public class Repository
    {
        public string RepoPath { get; private set; }

        public Repository(string repoPath)
        {
            RepoPath = repoPath;
        }

        public void Init()
        {
            // I Created the basic repository structure
            Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite"));
            Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite", "objects"));
            Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite", "refs"));
        }

        public Commit CreateCommit(string message)
        {
            string parentCommitId = ReadHead();
            
            string content = message + DateTime.Now.ToString("O");
            string commitId = Hasher.ComputeHash(content);
            
            Commit commit = new Commit(commitId, parentCommitId, message);
            
            SaveCommit(commit);
            WriteHead(commitId);
            
            return commit;
        }

        private void SaveCommit(Commit commit)
        {
            string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");
            Directory.CreateDirectory(objectsPath);

            string commitFilePath = Path.Combine(objectsPath, commit.CommitId + ".txt");

            string fileContent =
                "Commit ID: " + commit.CommitId + Environment.NewLine +
                "Parent: " + commit.ParentCommitId + Environment.NewLine +
                "Message: " + commit.Message + Environment.NewLine +
                "Timestamp: " + commit.Timestamp;

            File.WriteAllText(commitFilePath, fileContent);
        }
        private string GetHeadPath()
        {
            return Path.Combine(RepoPath, ".gitlite", "HEAD");
        }

        private string ReadHead()
        {
            string headPath = GetHeadPath();
            if (!File.Exists(headPath))
                return "";
            return File.ReadAllText(headPath);
        }


        private void WriteHead(string commitId)
        {
            string headPath = GetHeadPath();
            File.WriteAllText(headPath, commitId);
        }

        public void PrintLog()
        {
            string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");

            if (!Directory.Exists(objectsPath))
            {
                Console.WriteLine("No commits found.");
                return;
            }

            string[] commitFiles = Directory.GetFiles(objectsPath, "*.txt");

            foreach (string file in commitFiles)
            {
                Console.WriteLine("------------");
                Console.WriteLine(File.ReadAllText(file));
            }
        }
    }
}
