using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Turing{
  public abstract class trParameterPanelBase : MonoBehaviour {
    public abstract float GetValue();
    public abstract void SetUpView(float value);
    public Text Label;

    public class ParaEvent : UnityEvent<float> {}

    [SerializeField]
    public ParaEvent OnValueChanged = new ParaEvent();

    public virtual void SetRange(wwRange range){

    }
  }
}
