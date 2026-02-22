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

            // Branch references folder
            Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite", "refs", "heads"));

            // I created default branch "main" if it doesn't exist
            string mainRefPath = Path.Combine(RepoPath, ".gitlite", "refs", "heads", "main");
            if (!File.Exists(mainRefPath))
            {
                File.WriteAllText(mainRefPath, "");
            }

            // HEAD will store the current branch name
            File.WriteAllText(Path.Combine(RepoPath, ".gitlite", "HEAD"), "main");
        }

        public Commit CreateCommit(string message)
        {
            string branchName = ReadCurrentBranch();
            string parentCommitId = ReadBranchHeadCommitId(branchName);
            
            string content = message + DateTime.Now.ToString("O");
            string commitId = Hasher.ComputeHash(content);
            
            Commit commit = new Commit(commitId, parentCommitId, message);
            
            SaveCommit(commit);
            WriteBranchHeadCommitId(branchName, commitId);
            
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

        private string ReadHeadsDir()
        {
            return Path.Combine(RepoPath, ".gitlite", "refs", "heads");
        }

        private string ReadCurrentBranch()
        {
            string headPath = GetHeadPath();
            if (!File.Exists(headPath))
                return "main";
            return File.ReadAllText(headPath).Trim();
        }

        private void WriteCurrentBranch(string branchName)
        {
            File.WriteAllText(GetHeadPath(), branchName);
        }
        
        private string GetBranchRefPath(string branchName)
        {
            return Path.Combine(GetHeadsDir(), branchName);
        }
        
        private string ReadBranchHeadCommitId(string branchName)
        {
            string refPath = GetBranchRefPath(branchName);
            if (!File.Exists(refPath))
                return "";
            return File.ReadAllText(refPath).Trim();
        }
        
        private void WriteBranchHeadCommitId(string branchName, string commitId)
        {
            string refPath = GetBranchRefPath(branchName);
            File.WriteAllText(refPath, commitId);
        }

        private Commit LoadCommit(string commitId)
        {
            string path = Path.Combine(RepoPath, ".gitlite", "objects", commitId + ".txt");

            if (!File.Exists(path))
                return null;

            string[] lines = File.ReadAllLines(path);

            string parent = lines[1].Replace("Parent: ", "");
            string message = lines[2].Replace("Message: ", "");

            return new Commit(commitId, parent, message);
        }

        public void PrintLog()
        {
            string currentId = ReadHead();

            if (string.IsNullOrEmpty(currentId))
            {
                Console.WriteLine("No commits found.");
                return;
            }

            while (!string.IsNullOrEmpty(currentId))
            {
                Commit commit = LoadCommit(currentId);

                if (commit == null)
                    break;

                Console.WriteLine("------------");
                Console.WriteLine("Commit ID: " + commit.CommitId);
                Console.WriteLine("Parent: " + commit.ParentCommitId);
                Console.WriteLine("Message: " + commit.Message);
                Console.WriteLine("Timestamp: " + commit.Timestamp);

                currentId = commit.ParentCommitId;
            }
        }
    }
}
