using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class piStringUtil {

	// shorten the input string so that it takes up at most maxChars, by removing characters from the middle.
	public static string abbreviateMiddle(string value, int maxChars) {
		if (value.Length <= maxChars) {
			// yer good
			return value;
		}
		
		const string ellipsis = "...";
		const int padding = 5;
		
		if (maxChars < (ellipsis.Length + padding)) {
			return value.Substring(0, maxChars);
		}
		
		int lengthL = Mathf.FloorToInt(maxChars / 2) - Mathf.FloorToInt(ellipsis.Length / 2);
		int lengthR = maxChars - (lengthL + ellipsis.Length);
		string sL = value.Substring(0, lengthL);
		string sR = value.Substring(value.Length - lengthR);
		
		return sL + ellipsis + sR;
	}
	
	public static string byteArrayToString(byte[] ba)
	{
		StringBuilder hex = new StringBuilder(ba.Length * 2);
		foreach (byte b in ba)
			hex.AppendFormat("{0:x2}", b);
		return hex.ToString();
	}
  
  public static string byteArrayToString2(byte[] ba) {
    return byteArrayToString2(ba, 0);
  }
    
  public static string byteArrayToString2(byte[] ba, int initialByteCount) {
    int bytesThisLine = 0;
    int byteNum = initialByteCount;

    StringBuilder hex = new StringBuilder(ba.Length * 2);
    foreach (byte b in ba) {
      if (bytesThisLine == 0) {
        hex.Append(byteNum.ToString("0000"));
        hex.Append(" 0x");
        hex.AppendFormat("{0:x4}", byteNum);
        hex.Append("  ");
      }
      if (bytesThisLine > 0 && bytesThisLine % 4 == 0) {
        hex.Append(" ");
      }
      hex.AppendFormat("{0:x2} ", b);
      
      bytesThisLine++;
      
      if (bytesThisLine >= 8) {
        hex.Append("\n");
        bytesThisLine = 0;
      }
      
      byteNum += 1;
    }
    return hex.ToString();
  }
  
  public static bool ParseStringToEnum<T>(string s, out T enumValue) where T: struct, IComparable, IFormattable, IConvertible {
    foreach (T enumCandidate in System.Enum.GetValues(typeof(T))) {
      if (s == enumCandidate.ToString()) {
        enumValue = enumCandidate;
        return true;
      }
    }
    
    enumValue = default(T);
    
    return false;
  }

  public static string UpperCaseWords(string value) {
    char[] array = value.ToCharArray();
    if (array.Length >= 1) {
      array[0] = char.ToUpper(array[0]);
    }

    for (int i = 1; i < array.Length; i++) {
      if (array[i - 1] == ' ') {
        array[i] = char.ToUpper(array[i]);
      }
    }
    return new string(array);
  }
  
  public static bool ParseStringToEnum<T>(string s, out T enumValue, T defaultValue) where T: struct, IComparable, IFormattable, IConvertible {
    bool parsed = ParseStringToEnum<T>(s, out enumValue);
    if (!parsed) {
      enumValue = defaultValue;
    }
    
    return parsed;
  }

  public static Vector2 ParseStringToVector2(string s)
  {
    s = s.Replace("(", "");
    s = s.Replace(")", "");
    string[] temp = s.Split(',');
    float x = float.Parse(temp[0]);
    float y =float.Parse(temp[1]);
    Vector2 ret = new Vector2(x,y);
    return ret;
  }

  
  public static string dictionaryToString<T1, T2>(Dictionary<T1, T2> dict) {
    if (dict == null) {
      return "{}";
    }

    string ret = "";
    ret += "{ ";
    string delim = "";
    foreach (T1 key in dict.Keys) {
      ret += delim;
      ret += "{\"" + key.ToString() + "\": \"" + dict[key].ToString() + "\"}";
      delim = " ";
    }
    ret += " }";
    
    return ret;
  }
    
    
  
	private static bool test_abbreviateMiddle(string value, int maxChars, string expectedResult) {
		string actualResult = abbreviateMiddle(value, maxChars);
		if (actualResult == expectedResult) {
			return true;
		}
		
		Debug.LogError("test failed. value:" + value + " maxChars:0x" + maxChars + " expected:" + expectedResult + " actual:" + actualResult);
		return false;
	}
  
	public static bool test() {
		int countPass = 0;
		int countFail = 0;
		string val = "0123456789ABCDEFG";

		int passVal;
		
		passVal = test_abbreviateMiddle(val, 17, "0123456789ABCDEFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 16, "0123456...BCDEFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 15, "012345...BCDEFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 14, "012345...CDEFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 10, "0123...EFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 11, "0123...DEFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 9, "012...EFG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 8, "012...FG") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 7, "0123456") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 6, "012345") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 5, "01234") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 4, "0123") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 3, "012") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 2, "01") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 1, "0") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		passVal = test_abbreviateMiddle(val, 0, "") ? 1 : 0;
		countPass += passVal;
		countFail += (1 - passVal);
		
		Debug.Log("tests. pass:" + countPass + " fail:" + countFail + " total:" + (countPass + countFail));
		
		return (countFail == 0);
	}
}
