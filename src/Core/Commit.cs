using System;

namespace Core
{
  public class Commit
    {
      public string CommitId {get; set; }
      public string ParentCommitId {get; set; }
      public string Message {get; set; }
      public DateTime Timestamp {get; set; }

      public Commit(string commitId, string parentCommitId, string message)
      {
        CommitId = commitId;
        ParentCommitId = parentCommitId;
        Message = message;
        Timestamp = DateTime.Now;
      }
  }
}

      
      
