using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Turing;

public class ProfilerController : MonoBehaviour {

  [SerializeField]
  private Button _buttonScenesLoad;
  [SerializeField]
  private Button _buttonProgramPerf;
  [SerializeField]
  private Button _buttonStartScene;
  [SerializeField]
  private InputField _textScenesLoad;
  [SerializeField]
  private InputField _textProgramPerf;

  private string _buttonScenesLoadName;
  private string _buttonProgramPerfName;
  private string _buttonStartSceneName;
  private string _textScenesLoadName;
  private string _textProgramProgramPerfName;

  private List<string> _sceneNames = new List<string>(){"Lobby", "Map", "Main", "Community", "Profiles", "RemoteControl"};
  private const string SCENE_PROFILER = "Profiler";
  private int _sceneIndex;
  private string _scenesLoadRessult;
  private Stopwatch _scenesLoadStopwatch;
  private bool _isTestingScenesLoad;


  private void Awake() {
    if (Application.isPlaying) {
      DontDestroyOnLoad(transform.gameObject);
    }
    //Cache names so we can re-link references
    _buttonScenesLoadName = _buttonScenesLoad.name;
    _buttonProgramPerfName = _buttonProgramPerf.name;
    _buttonStartSceneName = _buttonStartScene.name;
    _textScenesLoadName = _textScenesLoad.name;
    _textProgramProgramPerfName = _textProgramPerf.name;
    RelinkReferences();
  }

  private void Start(){
    piConnectionManager.Instance.showChromeButton();
    trDataManager.Instance.Init();
  }

  private void RelinkReferences(){
    //Re-link references
    if(_buttonScenesLoad==null){
      _buttonScenesLoad = GameObject.Find(_buttonScenesLoadName).GetComponent<Button>();
    }
    if(_buttonProgramPerf==null){
      _buttonProgramPerf = GameObject.Find(_buttonProgramPerfName).GetComponent<Button>();
    }
    if(_buttonStartScene==null){
      _buttonStartScene = GameObject.Find(_buttonStartSceneName).GetComponent<Button>();
    }
    if(_textScenesLoad==null){
      _textScenesLoad = GameObject.Find(_textScenesLoadName).GetComponent<InputField>();
    }
    if(_textProgramPerf==null){
      _textProgramPerf = GameObject.Find(_textProgramProgramPerfName).GetComponent<InputField>();
    }
    _buttonScenesLoad.onClick.RemoveAllListeners();
    _buttonProgramPerf.onClick.RemoveAllListeners();
    _buttonStartScene.onClick.RemoveAllListeners();

    _buttonScenesLoad.onClick.AddListener(TestScenesLoad);
    _buttonProgramPerf.onClick.AddListener(TestProgramPerformance);
    _buttonStartScene.onClick.AddListener(BackToStartScene);
  }

  private void TestScenesLoad(){
//    Profiler.logFile = Application.persistentDataPath + "profilerLog.txt";
//    Profiler.enabled = true;
    StopAllCoroutines();
    _sceneIndex = 0;
    _scenesLoadRessult = BuildInfo.Summary+"\nDevice:"+SystemInfo.deviceModel+"\n";
    trNavigationRouter.Instance.onSceneLoaded += OnSceneLoaded;
    _isTestingScenesLoad = true;
    _scenesLoadStopwatch = new Stopwatch();
    _scenesLoadStopwatch.Reset();
    _scenesLoadStopwatch.Start();
    trNavigationRouter.Instance.ShowSceneWithName(_sceneNames[_sceneIndex]);
  }

  private void OnSceneLoaded(){
    if(_isTestingScenesLoad){
      _scenesLoadStopwatch.Stop();
      _scenesLoadRessult += _sceneNames[_sceneIndex]+" loaded, took: "+_scenesLoadStopwatch.ElapsedMilliseconds+" ms\n";
      StartCoroutine(LoadNextScene());
    }
    else{
      trNavigationRouter.Instance.onSceneLoaded -= OnSceneLoaded;
      foreach (var profilerCtrl in FindObjectsOfType<ProfilerController>()){
        if(profilerCtrl!=this){
          Destroy(profilerCtrl.gameObject);
        }
      }
      RelinkReferences();
      //Print result
      _textScenesLoad.text = _scenesLoadRessult;
    }
  }

  private IEnumerator LoadNextScene(){
    yield return new WaitForSeconds(3f);
    _sceneIndex ++;
    if(_sceneIndex<_sceneNames.Count){
      _scenesLoadStopwatch.Reset();
      _scenesLoadStopwatch.Start();
      trNavigationRouter.Instance.ShowSceneWithName(_sceneNames[_sceneIndex]);
    }
    else{
      _isTestingScenesLoad = false;
      trNavigationRouter.Instance.ShowSceneWithName(SCENE_PROFILER);
    }
  }

  private void TestProgramPerformance(){
    
  }

  private void BackToStartScene(){
    trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.START);
  }

}
