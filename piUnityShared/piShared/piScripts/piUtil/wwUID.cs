using UnityEngine;
using System.Collections;
using System;

public abstract class wwUID {

  public static bool ShortMode = true;
  
  private static int prevSecond = SecondsSince2015;
  private static int nextIndex  = 0;
  
  public static string getUID() {  
		

    if (ShortMode) {
      int second = SecondsSince2015;
      if (second == prevSecond) {
        nextIndex += 1;
      }
      else {
        nextIndex = 1;
      }
      prevSecond = second;
      
      return second.ToString() + "_" + nextIndex.ToString();
    }
    else {
      return System.Guid.NewGuid().ToString();
    }	
  }
  
  private static int SecondsSince2015 {
    get {
      TimeSpan ts = DateTime.UtcNow - new DateTime(2015, 1, 1);
      int dt = (int)ts.TotalSeconds;
      return dt;
    }
  }
}
