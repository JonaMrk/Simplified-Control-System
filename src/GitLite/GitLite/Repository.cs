using System;
using System.IO;
using Storage;

namespace GitLite
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

            // Create index file for file tracking
            string indexPath = Path.Combine(RepoPath, ".gitlite", "index");
            if (!File.Exists(indexPath))
            {
                File.WriteAllText(indexPath, "");
            }
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

            string[] trackedFiles = ListTrackedFiles();

            string fileContent =
                "Commit ID: " + commit.CommitId + Environment.NewLine +
                "Parent: " + commit.ParentCommitId + Environment.NewLine +
                "Message: " + commit.Message + Environment.NewLine +
                "Timestamp: " + commit.Timestamp + Environment.NewLine +
                "Tracked Files:" + Environment.NewLine;

            foreach (string file in trackedFiles)
            {
                fileContent += file + Environment.NewLine;

                string sourcePath = Path.Combine(RepoPath, file);
                string safeName = SanitizePath(file);
                string destPath = Path.Combine(objectsPath, commit.CommitId + "_" + safeName);

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }

            File.WriteAllText(commitFilePath, fileContent);
        }

        private string GetHeadPath()
        {
            return Path.Combine(RepoPath, ".gitlite", "HEAD");
        }

        private string GetHeadsDir()
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

        public void CreateBranch(string branchName)
        {
            Directory.CreateDirectory(GetHeadsDir());

            string refPath = GetBranchRefPath(branchName);
            if (File.Exists(refPath))
                return;

            string currentBranch = ReadCurrentBranch();
            string currentCommitId = ReadBranchHeadCommitId(currentBranch);

            File.WriteAllText(refPath, currentCommitId);
        }

        public string[] ListBranches()
        {
            string dir = GetHeadsDir();
            if (!Directory.Exists(dir))
                return Array.Empty<string>();

            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
                files[i] = Path.GetFileName(files[i]);

            return files;
        }

        public void CheckoutBranch(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                Console.WriteLine("Invalid branch name.");
                return;
            }

            string refPath = GetBranchRefPath(branchName);
            if (!File.Exists(refPath))
            {
                Console.WriteLine("Branch does not exist.");
                return;
            }

            WriteCurrentBranch(branchName);

            string commitId = ReadBranchHeadCommitId(branchName);

            if (string.IsNullOrEmpty(commitId))
            {
                Console.WriteLine("Switched to branch: " + branchName);
                Console.WriteLine("Branch has no commits yet.");
                return;
            }

            string[] trackedFiles = ListTrackedFiles();
            string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");

            foreach (string file in trackedFiles)
            {
                string safeName = SanitizePath(file);
                string sourcePath = Path.Combine(objectsPath, commitId + "_" + safeName);
                string destPath = Path.Combine(RepoPath, file);

                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine("Missing file in branch commit: " + file);
                    continue;
                }

                File.Copy(sourcePath, destPath, true);
            }

            Console.WriteLine("Switched to branch: " + branchName);
        }

        private string GetIndexPath()
        {
            return Path.Combine(RepoPath, ".gitlite", "index");
        }

        private string SanitizePath(string path)
        {
            return path.Replace("\\", "_").Replace("/", "_");
        }

        public void AddFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                Console.WriteLine("Invalid file name.");
                return;
            }

            string fullPath = Path.Combine(RepoPath, relativePath);

            if (!File.Exists(fullPath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            string indexPath = GetIndexPath();

            if (!File.Exists(indexPath))
                File.WriteAllText(indexPath, "");

            string[] lines = File.ReadAllLines(indexPath);

            foreach (string line in lines)
            {
                if (line.Trim() == relativePath.Trim())
                {
                    Console.WriteLine("File is already tracked.");
                    return;
                }
            }

            File.AppendAllText(indexPath, relativePath.Trim() + Environment.NewLine);
            Console.WriteLine("File added successfully.");
        }

        public string[] ListTrackedFiles()
        {
            string indexPath = GetIndexPath();

            if (!File.Exists(indexPath))
                return Array.Empty<string>();

            string[] lines = File.ReadAllLines(indexPath);

            return Array.FindAll(lines, l => !string.IsNullOrWhiteSpace(l));
        }

        public void PrintLog()
        {
            string branchName = ReadCurrentBranch();
            string currentId = ReadBranchHeadCommitId(branchName);

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

        public string GetFileFromLastCommit(string fileName)
        {
            string branch = ReadCurrentBranch();
            string commitId = ReadBranchHeadCommitId(branch);

            if (string.IsNullOrEmpty(commitId))
                return null;

            string safeName = SanitizePath(fileName);
            string path = Path.Combine(RepoPath, ".gitlite", "objects", commitId + "_" + safeName);

            if (!File.Exists(path))
                return null;

            return path;
        }

        public void DiffFile(string fileName)
        {
            string workingPath = Path.Combine(RepoPath, fileName);

            if (!File.Exists(workingPath))
            {
                Console.WriteLine("File does not exist.");
                return;
            }

            string oldFilePath = GetFileFromLastCommit(fileName);

            if (oldFilePath == null)
            {
                Console.WriteLine("No previous version found.");
                return;
            }

            DiffEngine diff = new DiffEngine();
            var result = diff.Compare(oldFilePath, workingPath);

            foreach (var line in result)
            {
                Console.WriteLine(line);
            }
        }

        public void CheckoutLastCommit()
        {
            string branch = ReadCurrentBranch();
            string commitId = ReadBranchHeadCommitId(branch);

            if (string.IsNullOrEmpty(commitId))
            {
                Console.WriteLine("No commits to checkout.");
                return;
            }

            string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");

            string[] trackedFiles = ListTrackedFiles();

            foreach (string file in trackedFiles)
            {
                string safeName = SanitizePath(file);

                string sourcePath = Path.Combine(objectsPath, commitId + "_" + safeName);
                string destPath = Path.Combine(RepoPath, file);

                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine("Missing file in commit: " + file);
                    continue;
                }

                File.Copy(sourcePath, destPath, true);
                Console.WriteLine("Restored: " + file);
            }

            Console.WriteLine("Checkout complete.");
        }

        public void CheckoutCommit(string commitId)
        {
            if (string.IsNullOrWhiteSpace(commitId))
            {
                Console.WriteLine("Invalid commit ID.");
                return;
            }

            string commitFilePath = Path.Combine(RepoPath, ".gitlite", "objects", commitId + ".txt");

            if (!File.Exists(commitFilePath))
            {
                Console.WriteLine("Commit does not exist.");
                return;
            }

            string[] trackedFiles = ListTrackedFiles();
            string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");

            foreach (string file in trackedFiles)
            {
                string safeName = SanitizePath(file);
                string sourcePath = Path.Combine(objectsPath, commitId + "_" + safeName);
                string destPath = Path.Combine(RepoPath, file);

                if (!File.Exists(sourcePath))
                {
                    Console.WriteLine("Missing file in commit: " + file);
                    continue;
                }

                File.Copy(sourcePath, destPath, true);
                Console.WriteLine("Restored: " + file);
            }

            Console.WriteLine("Checked out commit: " + commitId);
        }
    }
}