using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class trStepIndicator : MonoBehaviour {

  [SerializeField]
  private Image _progressImage;
  [SerializeField]
  private Color _defaultColor = new Color(0, 0, 0, 63f/225f);
  [SerializeField]
  private Color _currentStepColor = new Color(1f, 175f/225f, 36f/225f, 1f);
  [SerializeField]
  private Color _pastStepColor = new Color(74f/255f, 170f/255f, 92f/255f, 1f);

  public enum ProgressState{
    DEFAULT,
    CURRENT,
    PAST
  }

  public void SetProgressState (ProgressState state){
    switch (state) {
      case ProgressState.CURRENT:
        _progressImage.color = _currentStepColor;
        break;
      case ProgressState.DEFAULT:
        _progressImage.color = _defaultColor;
        break;
      case ProgressState.PAST:
        _progressImage.color = _pastStepColor;
        break;
    }
  }

}
