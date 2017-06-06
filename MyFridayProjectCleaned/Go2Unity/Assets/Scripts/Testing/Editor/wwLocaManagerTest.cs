using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Turing;

[TestFixture]
public class wwLocaManagerTest{

  private static readonly string PATH_PO_FOLDER = "Strings/wonder";
  private const string PO_SAVED_FOLDER_NAME = "poFiles";
  private const string APP_NAME = "wonder";
  private wwLocaManager _locaManager;

  [SetUp]
  public void SetUp() {
    _locaManager = new wwLocaManager();
    _locaManager.Init(APP_NAME, BuildInfo.AppVersion, getPOSavedPath());
    _locaManager.SetLanguage(wwLocaManager.getSystemTextLanguage(), PATH_PO_FOLDER, getPOSavedPath());
  }

  [Test]
  public void LookupTextInPrefabs(){
    List<TMPTextExtractor.POString> searchPrefabResults = TMPTextExtractor.GetPOStringsFromPrefabs();
    Assert.Greater(searchPrefabResults.Count, 0);
    foreach(TMPTextExtractor.POString poString in searchPrefabResults){
      bool isFound = false;
      string text = poString.content.Replace("\\n", System.Environment.NewLine);
      _locaManager.Format(text, out isFound);
      Assert.True(isFound);
    }
  }

  [TestCase("Assets/Scenes/Turing/Start.unity")]
  [TestCase("Assets/Scenes/Turing/Lobby.unity")]
  [TestCase("Assets/Scenes/Turing/Map.unity")]
  [TestCase("Assets/Scenes/Turing/Main.unity")]
  [TestCase("Assets/Scenes/Turing/Vault.unity")]
  [TestCase("Assets/Scenes/Turing/Community.unity")]
  [TestCase("Assets/Scenes/Turing/Profiles.unity")]
  [TestCase("Assets/Scenes/Turing/RemoteControl.unity")]
  [TestCase("Assets/Scenes/Turing/Options.unity")]
  public void LookupTextInScene(string scenePath){
    List<TMPTextExtractor.POString> searchSceneResults = TMPTextExtractor.GetPOStringsFromScene(scenePath);
    foreach(TMPTextExtractor.POString poString in searchSceneResults){
      bool isFound = false;
      string text = poString.content.Replace("\\n", System.Environment.NewLine);
      _locaManager.Format(text, out isFound);
      Assert.True(isFound);
    }
  }

  private string getPOSavedPath(){
    return Application.persistentDataPath + "/" + PO_SAVED_FOLDER_NAME;
  }

}
