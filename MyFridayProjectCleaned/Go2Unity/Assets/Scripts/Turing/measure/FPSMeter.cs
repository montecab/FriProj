using UnityEngine;
using System.Collections;
using System.Text; 
using UnityEngine.UI;

public class FPSMeter : MonoBehaviour {

  [SerializeField]
  private GameObject _canvas;
  [SerializeField]
  private Text _infoLabel;
  [SerializeField]
  private float _updateTime = 0.2f;

  private int _prevFpsValue = 0;
  private float _time = 0;
  private bool _isFPSMeterOn = false;
  private StringBuilder _text;

  private void Start(){
    DontDestroyOnLoad(this);
    _text = new StringBuilder(); 
    _isFPSMeterOn = Turing.trMultivariate.Instance.getOptionValue(Turing.trMultivariate.trAppOption.SHOW_FPS_METER) == Turing.trMultivariate.trAppOptionValue.YES;
    Turing.trMultivariate.Instance.ValueChanged += onOptionValueChanged;
  }

  private void OnDestroy() {
    if (Turing.trMultivariate.Instance != null) {
      Turing.trMultivariate.Instance.ValueChanged -= onOptionValueChanged;
    }
  }

  private void onOptionValueChanged (Turing.trMultivariate.trAppOption option, Turing.trMultivariate.trAppOptionValue newValue) {
    _isFPSMeterOn = Turing.trMultivariate.Instance.getOptionValue(Turing.trMultivariate.trAppOption.SHOW_FPS_METER) == Turing.trMultivariate.trAppOptionValue.YES;
    _canvas.gameObject.SetActive(_isFPSMeterOn);
  }

  private void Update () {
    if(!_isFPSMeterOn || _infoLabel==null){
      return;
    }

    int value = calculateFPS();
    int smoothed = (int)(_prevFpsValue * 0.9f + value * 0.1f);
    _prevFpsValue = value;

    float timeUpdate = Time.time;
    if (timeUpdate - _time > _updateTime){
      uint mUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSize()/1000000;
      uint mHeap = UnityEngine.Profiling.Profiler.GetMonoHeapSize()/1000000;
      uint mAllocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory()/1000000;
      uint mReserved = UnityEngine.Profiling.Profiler.GetTotalReservedMemory()/1000000;
      _text = new StringBuilder();
      _text.AppendFormat("fps: {0}\n",smoothed.ToString("00"));
      _text.AppendFormat("mem: {0}/{1}(mb)\n", mAllocated, mReserved);
      _text.AppendFormat("mono: {0}/{1}(mb)", mUsed, mHeap);
      _infoLabel.text = _text.ToString();
      _time = timeUpdate;
    }
  }

  private int calculateFPS(){
    return (int)(1f / Time.deltaTime);
  }
}
