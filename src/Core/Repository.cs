using System;
using System.IO;
using Storage;

namespace Core
{
  public class Repository 
  {
    public string RepoPath {get; private set; }

    public Repository(string repoPath)
    {
      RepoPath = repoPath;
    }

    public void Init()
    {
      // This is where I created a basic repository structure
      Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite"));
      Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite", "objects"));
      Directory.CreateDirectory(Path.Combine(RepoPath, ".gitlite", "refs"));
    }

    public Commit CreateCommit(string message, string parentcommitId = "")
    {
      string content = message + DateTime.Now.ToString();
      string commmitId = Hasher.ComputeHash(content);

      Commit commit = new Commit(commitId, parentCommitId, message);
      SaveCommit(commit);

      return commit;
    }

    private void Savecommit(Commmit commit)
    {
      string objectsPath = Path.Combine(RepoPath, ".gitlite", "objects");
      string commitFilePath = Path.Combine(objectsPath, commit.CommitId + ".txt");

      string fileContent = 
        "Commit ID: " + commit.CommitId + Environment.NewLine +
        "Parent: " + commit.ParentCommitId + Environment.NewLine +
        "Message: " + commit.Message + Environment.NewLine +
        "TimeStamp: " + commit.Timestamp;

      File.WriteAllText(commitFilePath, fileContent);
      }
  }
}

      
