using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public enum FTUEType{ // Chronological order
  CHROME = 0,
  LOBBY_SCROLLQUEST,
  MAP_AREA_BUTTON,
  MAIN_PLAY_BUTTON,
  LOBBY_FREEPLAY,
  FREEPLAY_PAGE,
  FREEPLAY_BACK_TO_LOBBY,
}

public class FTUEManager : Singleton<FTUEManager> {
  
  public static readonly string FTUE_MODELS_PATH = "FTUESettings/";
  public static readonly string FTUE_KEY = "FTUE_Progress";
  public static readonly Dictionary<FTUEType, string> FTUE_STRING_TABLE = new Dictionary<FTUEType, string>(){
    {FTUEType.CHROME, "@!@Hello, my name is Eli!\n\nLet's connect your robot.\nTap on the + button.@!@"},
    {FTUEType.LOBBY_SCROLLQUEST, "@!@{0} is ready to go!\nLet's start our first challenge.\n\nTap on Scroll Quest.@!@"},
    {FTUEType.MAP_AREA_BUTTON,"@!@Great!\n\nTap on Wonder Workshop.@!@"},
    {FTUEType.MAIN_PLAY_BUTTON,"@!@Press Play\nand Watch {0}!@!@"},
    {FTUEType.LOBBY_FREEPLAY, "@!@Wow, you really are good at this!\nLet me introduce you to Free Play.\n\nTap on Free Play below.@!@"},
    {FTUEType.FREEPLAY_PAGE, "@!@Free Play allows you to create any program you want.\n\nTry creating one.@!@"},
    {FTUEType.FREEPLAY_BACK_TO_LOBBY, "@!@Awesome!\n\nLet's go back to the lobby.@!@"}
  };

  private int _currentFTUE = -1;
  public int currentFTUE{
    get{
      if(_currentFTUE == -1){
        _currentFTUE = PlayerPrefs.GetInt(FTUE_KEY, 0);
      }
      return _currentFTUE;
    }
    private set{
      _currentFTUE = value;
      PlayerPrefs.SetInt(FTUE_KEY, _currentFTUE);
    }
  }

  public bool IsInFTUE(){
    return currentFTUE < Enum.GetValues(typeof(FTUEType)).Length;
  }

  public bool ShouldDisplayFTUE(FTUEType type){
    return currentFTUE == (int)type;
  }

  public void MoveToNextFTUE(){
    currentFTUE++;
  }

  public void SkipFTUE(){
    currentFTUE = Enum.GetValues(typeof(FTUEType)).Length;
  }

}
