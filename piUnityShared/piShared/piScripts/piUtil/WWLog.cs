using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// logging utility class.
// this composes a line containing the log level, the milliseconds since the app started, and the calling method,
// and ships it off to a native logger (if present), or to the usual unity logger if not present.
// note, DEBUG-level log lines are only respected if "Development Build" is checked,
//       except in the unity editor, which seems to ignore that setting.

public class WWLog
{
	// standard unix SysLog levels
	public enum logLevel : uint {
		EMERGENCY = 0,      // Something is very wrong.
		CRITICAL  = 1,      // Something is very wrong. 
		ALERT     = 2,      // Something is very wrong.
		ERROR     = 3,      // Something is definitely wrong. This should never happen. Investigate!
		WARNING   = 4,      // Something is probably wrong. An engineer should investigate this.
		NOTICE    = 5,      // Unusual, but not necessarily a problem.
		INFO      = 6,      // Informative and useful in production. nothing is wrong.
		DEBUG     = 7,      // Not useful during production: should not even appear in production logs.
	}
	
	static Dictionary<logLevel, string> logLevelStrings = new Dictionary<logLevel, string>()
	{
		{logLevel.EMERGENCY, "EMERG "},
		{logLevel.CRITICAL , "CRIT  "},
		{logLevel.ALERT    , "ALERT "},
		{logLevel.ERROR    , "ERROR "},
		{logLevel.WARNING  , "WARN  "},
		{logLevel.NOTICE   , "NOTICE"},
		{logLevel.INFO     , "INFO  "},
		{logLevel.DEBUG    , "DEBUG "},
	};
	
	public static int methodFieldWidth = 20;	// number of characters to pad the method name to
	
	private static bool _warnOnSkipDebug = true;
		
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	private static void _log(logLevel level, string line, int lookBack) {
		if ((level == logLevel.DEBUG) && (!Debug.isDebugBuild)) {
			if (_warnOnSkipDebug) {
				_warnOnSkipDebug = false;
				// note: recursive by one.
				_log(logLevel.INFO, "skipping all DEBUG level logging.", lookBack + 1);
			}
			return;
		}

		System.Reflection.MethodBase callingMethod = null;
		string callingTypeName = "unknown";
		string callingMethodName = "unknown";
		// note: simple empirical measurements on the cost of getting the stackframe(1):
		// unity player, macbook pro: .0116 seconds per 1000 calls (0.01ms per call)
		// ipad mini                : .0455 seconds per 1000 calls (0.05ms per call)
		// by comparison, the act of actually printing to the log is much more expensive:
		// unity player, macbook pro: .7817 seconds per 1000 calls (0.78ms per call)
		// ipad mini                : .7930 seconds per 1000 calls (0.79ms per call)
		System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(lookBack);
		if (sf != null) {
			callingMethod = sf.GetMethod();
			if (callingMethod != null) {
				callingMethodName = callingMethod.Name;
				if (callingMethod.DeclaringType != null) {
					callingTypeName = callingMethod.DeclaringType.Name;
				}
			}
		}
		
		string sMethod    = callingTypeName + ":" + callingMethodName + "()";
		string sTimestamp = piUnityUtils.SessionWallTime.ToString("t:0000.000s");	// covers about 3.7 hours
		string sLevel     = logLevelStrings[level];
		string newLine    = sLevel + " " + sTimestamp + " mthd:" + sMethod.PadRight(methodFieldWidth) + " msg:" + line;
		PIBInterface.Actions.log(level, newLine);
	}
	
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void log(logLevel level, string line) {
		_log(level, line, 2);
	}
	
	// conveniences:
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void logDebug(string line) { _log(logLevel.DEBUG  , line, 2); }
	
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void logInfo (string line) { _log(logLevel.INFO   , line, 2); }
	
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void logWarn (string line) { _log(logLevel.WARNING, line, 2); }
	
//	[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	public static void logError(string line) { _log(logLevel.ERROR  , line, 2); }
}
