using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TracerLib {
  public static class StringsExtensions {

    /// <summary>
    /// Combines the composite format string with its args. 
    /// Exception is caught and the exception message gets added to format string. 
    /// </summary>
    public static string ReplaceArgs(this string formatString, object[] args) {
      if (args==null || args.Length==0) {
        return formatString;
      }
      try {
        return string.Format(formatString, args);
      } catch (Exception ex) {
        //formatString has illegal format. return origianl format string with exception message
        Tracer.ShowExceptionInDebugger(ex);
        return formatString + " !!! Args conversion error: '" + ex.Message + "' + args: '" + args + "'";
      }
    }
  }
}
