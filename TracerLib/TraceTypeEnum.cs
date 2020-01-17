//==========================================================================================================================================
// Copyright: Peter Huber, Singapore, 2014
// This code is contributed to the Public Domain. You might use it freely for any purpose, commercial or non-commercial. It is provided 
// "as-is." The author gives no warranty of any kind whatsoever. It is up to you to ensure that there are no defects, the code is 
// fit for your purpose and does not infringe on other copyrights. Use this code only if you agree with these conditions. The entire risk of 
// using it lays with you :-)
//==========================================================================================================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace TracerLib {

  
  public enum TraceTypeEnum {
    undef = 0,
    Trace,
    Warning,
    Error,
    Exception
  }


  public static class TraceTypeEnumExtension {
    public static string ShortString(this TraceTypeEnum tracerSource) {
      switch (tracerSource) {
      case TraceTypeEnum.Trace:
        return "Trc";
      case TraceTypeEnum.Warning:
        return "War";
      case TraceTypeEnum.Error:
        return "Err";
      case TraceTypeEnum.Exception:
        return "Exc";
      default:
        return tracerSource.ToString();
      }
    }
  }

}