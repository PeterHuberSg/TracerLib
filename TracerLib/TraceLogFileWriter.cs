using System;
using System.Threading;


namespace TracerLib {

  
  /// <summary>
  /// The TraceLogFileWriter supports writing tracing information to a tracing file.
  /// 
  /// If the max size of the file is reached, a new file gets created. If there are too many files, the
  /// oldest gets deleted.
  /// </summary>
  public class TraceLogFileWriter: IDisposable {


    #region Properties
    //      ----------

    /// <summary>
    /// File parameters like file name and location, size limitations, etc.
    /// </summary>
    public FileParameterStruct FileParameter { get { return logFileWriter.FileParameter; } }


    /// <summary>
    /// Get the full path and name of the current file
    /// </summary>
    public string FullName { get { return logFileWriter.FullName; } }


    /// <summary>
    /// Text added to header line
    /// </summary>
    public string HeaderText { get; set; }


    private string getHeaderText() {
      return (string.IsNullOrEmpty(HeaderText) ? "" : " - " + HeaderText);
    }


    /// <summary>
    /// newFileCreated is called when existing file is full and a new file gets created. This action is called on the 
    /// MessageTracker thread
    /// </summary>
    Action newFileCreated;
    #endregion


    #region Constructor
    //      -----------

    const string traceLogFileWriterMarker = "==================";
    const string fileExtension    = "txt";
    string fileName;


    /// <summary>
    /// Setup TraceLogFileWriter with directoryPath, fileName, max file size and number of files to be used
    /// </summary>
    public TraceLogFileWriter(
      string directoryPath, 
      string newFileName, 
      long maxFileByteCount, 
      int maxFileCount, 
      Action newFileCreated):
      this(directoryPath, newFileName, maxFileByteCount, maxFileCount)
    {
      this.newFileCreated = newFileCreated;
    }


    LogFileWriter logFileWriter;
    Func<TraceMessage, bool> filter;


    /// <summary>
    /// Setup TraceLogFileWriter with directoryPath, fileName, max file size and number of files to be used
    /// </summary>
    public TraceLogFileWriter(
      string directoryPath,
      string fileName,
      long maxFileByteCount,
      int maxFileCount,
      int logFileWriterTimerInitialDelay = 10,
      int logFileWriterTimerInterval = 10000) {

      //lock (lineQueueLock) {
      //setup logFileWriter
      HeaderText = "";
      this.fileName = fileName;
      logFileWriter = new LogFileWriter(
        new FileParameterStruct(directoryPath, fileName, fileExtension, maxFileByteCount, maxFileCount),
        logFileWriter_GetNewFileHeader,
        logFileWriter_GetNewDayHeader,
        logFileWriterTimerInitialDelay : logFileWriterTimerInitialDelay,
        logFileWriterTimerInterval : logFileWriterTimerInterval);

      //write separator line
      if (logFileWriter.Length>0) {
        //existing file, write empty line
        logFileWriter.WriteMessage(getSeparatorLine(""));
      }
      logFileWriter.WriteMessage(getSeparatorLine(traceLogFileWriterMarker + " Start Application " + AppDomain.CurrentDomain.FriendlyName +
          getHeaderText() + " " + traceLogFileWriterMarker));

      TraceMessage[] existingMessages = Tracer.GetTrace(writeMessages);
      writeMessages(existingMessages);
    }


    public void Dispose() {
      if (logFileWriter!=null) {
        logFileWriter.Dispose();
        logFileWriter = null;
      }
    }


    void writeMessages(TraceMessage[] TraceMessages) {
      if (logFileWriter==null) return; //logFileWriter is disposed

      lock(logFileWriter){
        foreach (TraceMessage traceMessage in TraceMessages) {
          if (filter!=null && filter(traceMessage)) continue;

          logFileWriter.WriteMessage(traceMessage.ToString());
        }
      }
    }



    string logFileWriter_GetNewFileHeader() {
      if (newFileCreated!=null) {
        newFileCreated();
      }
      return getSeparatorLine(traceLogFileWriterMarker + " " + DateTime.Now.ToShortDateString() + "@#MachineFile#@" + getHeaderText() + " " + traceLogFileWriterMarker);
    }


    string logFileWriter_GetNewDayHeader() {
      return getSeparatorLine(traceLogFileWriterMarker + " " + DateTime.Now.ToShortDateString() + getHeaderText() + " " + traceLogFileWriterMarker);
    }


    public void FlushAndCloseLogFileWriter() {
      LogFileWriter tmpLogFileWriter = null;
      tmpLogFileWriter = Interlocked.Exchange(ref logFileWriter, tmpLogFileWriter);
      if (tmpLogFileWriter!=null) {
        tmpLogFileWriter.Dispose();
      }
    }


    /// <summary>
    /// Release some resources. Used for Unit testing
    /// </summary>
    public void Reset() {
      if (logFileWriter!=null) {
        logFileWriter = null;
        logFileWriter.Dispose();
      }
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// get default parameter for constructor
    /// </summary>
    public static void GetDefaultParameters(string applicationDataDirectory,
      out string directoryPath, out string fileName, out long maxFileByteCount, out int maxFileCount) {
      directoryPath = applicationDataDirectory + @"\Trace";
      fileName = "LineTrace";
      maxFileByteCount = 10*(1<<20);//10 MBytes
      maxFileCount = 20;
    }


    /// <summary>
    /// Check if file parameters are valid
    /// </summary>
    public static bool ValidateConstructorParameters(
      string directoryPath, 
      string fileName, 
      long maxFileByteCount, 
      int maxFileCount,
      out string problem) //
    {
      FileParameterStruct validateFileParameterStruct = new FileParameterStruct(directoryPath, fileName, "txt", maxFileByteCount, maxFileCount);
      return validateFileParameterStruct.ValidateConstructorParameters(true, out problem);
    }

    
    /// <summary>
    /// Supports the changing of file name, size, etc.
    /// </summary>
    public void ChangeProperties(string newDirectoryPath, long newMaxFileByteCount, int newMaxFileCount){
      logFileWriter.ChangeFileProperties(
        new FileParameterStruct(newDirectoryPath, fileName, fileExtension, newMaxFileByteCount, newMaxFileCount));
    }
    
    
    ///// <summary>
    ///// Adds an empty line to the trace, without time or any other information.
    ///// </summary>
    //public static void AddEmptyLine() {
    //  doTrace("");
    //}


    ///// <summary>
    ///// Trace an information. Time will be automatically added and
    ///// Trace Type character will be *
    ///// </summary>
    //public static void Trace(string messageString) {
    //  DateTime traceDateTime = DateTime.Now;
    //  Trace(traceDateTime, messageString);
    //}


    ///// <summary>
    ///// Trace an information. Time will be automatically added and
    ///// Trace Type character will be *
    ///// </summary>
    //public static void Trace(string messageString, params object[] args) {
    //  Trace(messageString.ReplaceArgs(args));
    //}


    ///// <summary>
    ///// Trace an information. Time will be automatically added 
    ///// </summary>
    //public static void Trace(char traceTypeChar, string messageString) {
    //  DateTime traceDateTime = DateTime.Now;
    //  Trace(traceDateTime, traceTypeChar, messageString);
    //}


    ///// <summary>
    ///// Trace an information. Time will be automatically added and
    ///// Trace Type character will be *
    ///// </summary>
    //public static void Trace(char traceTypeChar, string messageString, params object[] args) {
    //  Trace(traceTypeChar, messageString.ReplaceArgs(args));
    //}


    ///// <summary>
    ///// Trace an information. Trace Type character will be *
    ///// </summary>
    //public static void Trace(DateTime traceDateTime, string messageString) {
    //  Trace(traceDateTime, '*', messageString);
    //}


    ///// <summary>
    ///// Trace an information. Trace Type character will be *
    ///// </summary>
    //public static void Trace(DateTime traceDateTime, string messageString, params object[] args) {
    //  Trace(traceDateTime, messageString.ReplaceArgs(args));
    //}


    ///// <summary>
    ///// Trace an information
    ///// </summary>
    //public static void Trace(DateTime traceDateTime, char traceTypeChar, string messageString, params object[] args) {
    //  Trace(traceDateTime, traceTypeChar, messageString.ReplaceArgs(args));
    //}


    ///// <summary>
    ///// Trace an information
    ///// </summary>
    //public static void Trace(DateTime traceDateTime, char traceTypeChar, string messageString) {
    //  string tabMessageString = messageString.Replace(Environment.NewLine, Environment.NewLine + "        \t\t");
    //  string traceString = 
    //    traceDateTime.ToString("hh:mm:ss.fff") + "\t" + traceTypeChar + 
    //    "\t" + tabMessageString;

    //  doTrace(traceString);
    //}


    //private static void doTrace(string traceString) {
    //  if (System.Diagnostics.Debugger.IsAttached) {
    //    //write trace to output window in debugger
    //    System.Diagnostics.Debug.WriteLine(traceString);
    //  }

    //  if (maxLineQueueCount>0) {
    //    //store lines in lineQueue, providing access to the earlier lines
    //    lock (lineQueueLock) {
    //      if (lineQueue.Count>=maxLineQueueCount) {
    //        lineQueue.Dequeue();
    //      }
    //      lineQueue.Enqueue(traceString);
    //    }
    //  }

    //  if (traceLogFileWriter!=null) {
    //    //TraceLogFileWriter is constructed.
    //    //write the message to the file
    //    traceLogFileWriter.logFileWriter.WriteMessage(traceString);
    //  }
    //}


    static string getSeparatorLine(string messageString) {
      DateTime traceDateTime = DateTime.Now;
      return traceDateTime.ToString("T") + traceDateTime.ToString(".fff\t=\t")+ messageString;
    }


    ///// <summary>
    ///// Returns the latest lineQueueCount number of trace lines. If lineQueueCount==0, null gets returned.
    ///// </summary>
    //public static string[] GetLatestTraceLines() {
    //  if (lineQueue==null) return null;

    //  lock (lineQueueLock) {
    //    string[] returnStrings = lineQueue.ToArray();
    //    return returnStrings;
    //  }
    //}
    #endregion
  }
}