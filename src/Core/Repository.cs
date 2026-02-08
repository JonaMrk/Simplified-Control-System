using System.IO;

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
  }
}

      
