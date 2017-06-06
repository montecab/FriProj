using UnityEngine;
using System.Collections;

public delegate float NumericWidgetValueGet();
public delegate void NumericWidgetValueSet(float f);
public delegate string NumericWidgetValueStringify(float f);

public class NumericParamWidget : MonoBehaviour {

	public SuperBasicButton buttonLess;
	public SuperBasicButton buttonMore;
	public UILabel          labelName;
	public UILabel          labelValue;
	public string           paramName = "param";
	
	public float            incrementAmount = 1.0f;
	
	public NumericWidgetValueGet       theGetter;
	public NumericWidgetValueSet       theSetter;
	public NumericWidgetValueStringify theStringifier;
	
	// Use this for initialization
	void Start () {
		buttonLess.onClick = onButtonLess;
		buttonMore.onClick = onButtonMore;
	}
	
	public void initUI() {
		labelName.text = paramName;
		labelValue.text = _safeStringify(_safeGet());
	}
	
	void onButtonLess() {
		_safeSet(_safeGet() - incrementAmount);
		labelValue.text = _safeStringify(_safeGet());
	}
	
	void onButtonMore() {
		_safeSet(_safeGet() + incrementAmount);
		labelValue.text = _safeStringify(_safeGet());
	}
	
	float _safeGet() {
		return theGetter == null ? 0.0f : theGetter();
	}
	
	void _safeSet(float val) {
		if (theSetter != null) {
			theSetter(val);
		}
	}
	
	string _safeStringify(float val) {
		return theStringifier == null ? "??" : theStringifier(val);
	}
}
