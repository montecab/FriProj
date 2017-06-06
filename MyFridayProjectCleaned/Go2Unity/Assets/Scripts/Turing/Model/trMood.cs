using UnityEngine;
using System.Collections;

namespace Turing {

  // IMPORTANT: these enums are baked into the filenames of the animations on the robot.
  //            do not change them without understanding the full implications for SW and FW !  
  //            notably, the numbers here MUST MATCH those used in the conversion script 'json2robotWmood.py',
  //            which is in the ArtPipelineTools repository.
  public enum trMoodType : int {
    NO_CHANGE  = 0,  // used to indicate that this state does not alter mood
    HAPPY      = 1,  // IMPORTANT: READ COMMENT ABOVE BEFORE CHANGING.
    CAUTIOUS   = 2,  // IMPORTANT: READ COMMENT ABOVE BEFORE CHANGING.
    CURIOUS    = 3,  // IMPORTANT: READ COMMENT ABOVE BEFORE CHANGING.
    FRUSTRATED = 4,  // IMPORTANT: READ COMMENT ABOVE BEFORE CHANGING.
    SILLY      = 5,  // IMPORTANT: READ COMMENT ABOVE BEFORE CHANGING.
//    BRAVE      = 6, These two moods are removed for the first version of the app
//    SURPRISED  = 7,
  }
  
  public static class trMood {
    public const trMoodType DefaultMood = trMoodType.HAPPY;

    public static string MoodString(trMoodType moodType){
      switch (moodType) {
        case trMoodType.HAPPY:
          return "happy";
//        case trMoodType.BRAVE:
//          return "brave";
//        case trMoodType.SURPRISED:
//          return "surprised";
        case trMoodType.CAUTIOUS:
          return "cautious";
        case trMoodType.CURIOUS:
           return "curious";
        case trMoodType.FRUSTRATED:
           return "frustrated";
        case trMoodType.SILLY:
          return "silly";
        case trMoodType.NO_CHANGE:
        default:
          return MoodString(trMood.DefaultMood);
      }
    }
  }
}
