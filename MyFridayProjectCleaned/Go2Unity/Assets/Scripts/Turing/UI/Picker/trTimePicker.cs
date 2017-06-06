using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class trTimePicker : trPickerController {


  public enum trTimePickerMode {
    SHORT_TIME = 0,
    LONG_TIME
  }

  private trTimePickerMode timerMode = trTimePickerMode.LONG_TIME;

  public trTimePickerMode TimerMode {
    get {
      return timerMode;
    }
    set {
      this.timerMode = value;
      this.Start();
    }
  }
  Dictionary <string, List<string>> datasource;
  private int[] longTimerSelection = new int[]{4, 10, 5};
  private int[] shortTimerSelection = new int[]{2, 0};

  private string[] longHeaderTitles = new string[] {"@!@Hrs@!@", "@!@Mins@!@", "@!@Secs@!@"};  
  private string[] shortHeaderTitles = new string[] {"@!@Seconds@!@" , ""};
	// Use this for initialization
	void Start () {

    datasource = new Dictionary <string, List<string>>();
	  //datasource = new List<List<string>>();
    int [] timerSelection = longTimerSelection;

    if (this.TimerMode == trTimePickerMode.LONG_TIME) { 
      timerSelection = longTimerSelection;

     
      List<string> hoursList   = new List<string>();
      List<string> minutesList = new List<string>();
      List<string> secondsList = new List<string>();

      datasource.Add(longHeaderTitles[0], hoursList  );
      datasource.Add(longHeaderTitles[1], minutesList);
      datasource.Add(longHeaderTitles[2], secondsList);

      for (int i = 0; i < 25; i++){
        hoursList.Add(i.ToString());
      }

      for (int i = 0; i < 60; i++){
        minutesList.Add(i.ToString());
      }

      for (int i = 0; i < 60; i += 5){
        secondsList.Add(i.ToString());
      }
    }
    else if (this.TimerMode == trTimePickerMode.SHORT_TIME) {
      timerSelection = shortTimerSelection;

      List<string> deciSecondsList = new List<string>();
      List<string> secondsList = new List<string>();
      
      datasource.Add(shortHeaderTitles[0], secondsList);
      datasource.Add(shortHeaderTitles[1], deciSecondsList);



      for (int i = 0; i < 11; i++){
        secondsList.Add(i.ToString());
      }
      
      for (float i = 0.0f; i < 1; i += 0.1f){
        deciSecondsList.Add(i.ToString(".00"));
      }
    }
    
    SetDataSource(datasource, timerSelection);
	}
	
  public override float GetValue () {
    float result = 0;
    int[] rawValues = GetSelectedValues();
    if (this.TimerMode == trTimePickerMode.LONG_TIME) {
      result = 60.0f * (60 * rawValues[0] + rawValues[1]) + 5.0f * rawValues[2];
      result = float.Parse(datasource[longHeaderTitles[0]][rawValues[0]]) * 60f * 60f + 
               float.Parse(datasource[longHeaderTitles[1]][rawValues[1]]) * 60f + 
               float.Parse(datasource[longHeaderTitles[2]][rawValues[2]]);
    }
    else {
      result = float.Parse(datasource[shortHeaderTitles[0]][rawValues[0]]) +
               float.Parse(datasource[shortHeaderTitles[1]][rawValues[1]]);
    }
    return result;
  }

  public override void SetUpView (float value) {
    int [] selection = null;
    switch (this.TimerMode) {
      case trTimePickerMode.LONG_TIME:
        int minutes = (int)value/60;
        int seconds = (int)value % 60;
        int hours = minutes/60;
        int remainingMinutes = minutes % 60;
        selection = new int[]{hours, remainingMinutes, (int)(seconds / 5f)};
        longTimerSelection = selection;
        break;
      case trTimePickerMode.SHORT_TIME:
        int shortTimeSeconds = (int)value;
        int shortTimeDeciSeconds = Mathf.RoundToInt((value - shortTimeSeconds) * 10f);
        selection = new int[]{shortTimeSeconds, shortTimeDeciSeconds };
        shortTimerSelection = selection;
        break;  
    }

    if (datasource != null){
      SelectItems(selection);
    }
  }
}
