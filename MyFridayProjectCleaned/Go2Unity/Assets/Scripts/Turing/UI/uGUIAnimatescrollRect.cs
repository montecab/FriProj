using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW;

namespace WW.UGUI{
	[RequireComponent(typeof(ScrollRect))]
	public class uGUIAnimatescrollRect : MonoBehaviour {
		
		public ScrollRect MScrollRect;
		public RectTransform content;
		public RectTransform TargetPosition;
		
		
		public void ScrollToElement(Transform element){
			Vector3 dir = element.transform.position - TargetPosition.transform.position;
			if(!MScrollRect.horizontal){
				dir = new Vector3(0, dir.y, 0);
			}
			if(!MScrollRect.vertical){
				dir = new Vector3(dir.x, 0, 0);
			}
			content.transform.position = content.transform.position - dir;
			
		}
		
		void updateCachedReferences(){
			if(content == null){
				content = MScrollRect.content;
			}
		}
		
		
	}
}
