using UnityEngine;
using System.Collections;

public class SuperBasicButton : MonoBehaviour {
	
	public delegate void FunctionDelegate();
	public FunctionDelegate onClick;
	public FunctionDelegate onPress;
	public FunctionDelegate onRelease;
	public FunctionDelegate onDrag;

	void OnClick() {
		if( onClick != null ) {
			onClick();
		}
	}

	void OnPress(bool isDown) {
		if(isDown) {
			if(onPress != null) {
				onPress();
			}
		}
		else {
			if (onRelease != null) {
				onRelease();
			}
		}
	}
	
	void OnDrag(Vector2 delta) {
		if (onDrag != null) {
			onDrag();
		}
	}
}