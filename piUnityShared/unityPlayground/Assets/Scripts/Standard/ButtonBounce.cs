using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ButtonBounce : MonoBehaviour {

		public bool relativeScale = false;
		
		private bool origScaleLatched = false;
		private Vector3 origScale = new Vector3(1f, 1f, 1f);

	void latchScale() {
		if (origScaleLatched) {
			return;
		}
		
		origScale = GetComponent<Transform>().localScale;
		
		origScaleLatched = true;
	}

	void OnClick() {
		latchScale();
	
		if (!enabled)
			return;
		PlayBounceTween();
	}

	public void PlayBounceTween() {
		HOTween.To (this.gameObject.transform, 0.1f, new TweenParms()
		            .Prop ("localScale", origScale * 0.8f)
		            .Ease ( EaseType.EaseOutQuad )
		            );
		HOTween.To (this.gameObject.transform, 0.25f, new TweenParms()
		            .Delay(0.1f)
		            .Prop ("localScale", origScale)
		            .Ease ( EaseType.EaseOutBack )
		            );

	}
}
