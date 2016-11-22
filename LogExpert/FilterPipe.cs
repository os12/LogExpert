using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace LogExpert
{
  public class FilterPipe
  {
    string fileName;
    FilterParams filterParams;
    IList<int> lastLinesHistoryList = new List<int>();
    StreamWriter writer;
    IList<int> lineMappingList = new List<int>();
    LogWindow logWindow;      // the parent LogWindow
    LogWindow ownLogWindow;    // own window
    bool isStopped;

    public bool IsStopped
    {
      get { return isStopped; }
      set { isStopped = value; }
    }

    public FilterPipe(FilterParams filterParams, LogWindow logWindow)
    {
      this.filterParams = filterParams;
      this.logWindow = logWindow;
      this.IsStopped = false;
      this.fileName = Path.GetTempFileName();

      Logger.logInfo("Created temp file: " + this.FileName);
    }

    public void OpenFile()
    {
      FileStream fStream = new FileStream(this.fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
      this.writer = new StreamWriter(fStream, new UnicodeEncoding(false, false));
    }

    public void CloseFile()
    {
      if (this.writer != null)
      {
        this.writer.Close();
        this.writer = null;
      }
    }

    public bool WriteToPipe(string textLine, int orgLineNum)
    {
      try {
        lock (this.FileName)
        {
          lock (this.lineMappingList)
          {
            try
            {
              this.writer.WriteLine(textLine);
              this.lineMappingList.Add(orgLineNum);
              return true;
            }
            catch (IOException e)
            {
              Logger.logError("writeToPipe(): " + e.ToString());
              return false;
            }
          }
        }
      }
      catch (IOException) 
      {
        Logger.logError("writeToPipe(): file was closed: " + this.fileName);
        return false;
      }
    }

    public string FileName
    {
      get { return fileName; }
    }

    public FilterParams FilterParams
    {
      get { return this.filterParams; }
    }

    public IList<int> LastLinesHistoryList
    {
      get { return this.lastLinesHistoryList; }
    }

    public int GetOriginalLineNum(int lineNum)
    {
      lock (this.lineMappingList)
      {
        if (this.lineMappingList.Count > lineNum)
          return this.lineMappingList[lineNum];
        else
          return -1;
      }
    }

    public LogWindow LogWindow
    {
      get { return this.logWindow; }
    }

    public LogWindow OwnLogWindow
    {
      get { return this.ownLogWindow; }
      set { this.ownLogWindow = value; }
    }

    public void ShiftLineNums(int offset)
    {
      Logger.logDebug("FilterPipe.ShiftLineNums() offset=" + offset);
      List<int> newList = new List<int>();
      lock (this.lineMappingList)
      {
        foreach (int lineNum in this.lineMappingList)
        {
          int line = lineNum - offset;
          if (line >= 0)
          {
            newList.Add(line);
          }
          else
          {
            newList.Add(-1);
          }
        }
        this.lineMappingList = newList;
      }
    }

    public void ClearLineNums()
    {
      Logger.logDebug("FilterPipe.ClearLineNums()");
      lock (this.lineMappingList)
      {
        for (int i = 0; i < this.lineMappingList.Count; ++i)
        {
          this.lineMappingList[i] = -1;
        }
      }
    }

    public void ClearLineList()
    {
      lock (this.lineMappingList)
      {
        this.lineMappingList.Clear();
      }
    }

    public void RecreateTempFile()
    {
      lock (this.lineMappingList)
      {
        this.lineMappingList = new List<int>();
      }
      lock (this.FileName)
      {
        CloseFile();
        // trunc file
        FileStream fStream = new FileStream(this.fileName, FileMode.Truncate, FileAccess.Write, FileShare.Read);
        fStream.SetLength(0);
        fStream.Close();
      }
    }

    public void CloseAndDisconnect()
    {
      ClearLineList();
      OnClosed();
    }


    public delegate void ClosedEventHandler(object sender, EventArgs e);
    public event ClosedEventHandler Closed;
    private void OnClosed()
    {
      if (Closed != null)
        Closed(this, new EventArgs());
    }


  }
}
