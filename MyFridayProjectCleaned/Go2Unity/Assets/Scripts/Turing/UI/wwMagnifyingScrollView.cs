using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class wwMagnifyingScrollView : ScrollRect {
  public const float MagnifyingHeight = 200;
  public const float NormalHeight = 90;

  private float index = 0;
  private int roundId = 0;
	
  private float offset = 80;

  protected override void Start(){
    base.Start();
    offset = (this.GetComponent<RectTransform>().GetHeight() - MagnifyingHeight)/2.0f;
  }

  protected override void LateUpdate ()
  {
    base.LateUpdate ();
    if(content == null){
      return;
    }

    float spacingAvg = (content.transform.childCount-1) * content.GetComponent<VerticalLayoutGroup>().spacing/content.transform.childCount;
    index = (content.transform.localPosition.y -(MagnifyingHeight - NormalHeight )- offset)/(NormalHeight+ spacingAvg);
    index = Mathf.Clamp(index, 0, content.transform.childCount - 1);
    roundId = Mathf.RoundToInt(index);

    foreach(Transform child in content.transform){
      if(Mathf.Abs(child.GetSiblingIndex()- index)<1){
        if(child.GetSiblingIndex() == roundId){
          child.GetComponent<LayoutElement>().preferredHeight = Mathf.Lerp( child.GetComponent<LayoutElement>().preferredHeight, MagnifyingHeight, 5 * Time.fixedDeltaTime) ;
        }
        else{
          child.GetComponent<LayoutElement>().preferredHeight = Mathf.Lerp( child.GetComponent<LayoutElement>().preferredHeight, NormalHeight, 5 * Time.fixedDeltaTime) ;
        }
      }
      else{
        child.GetComponent<LayoutElement>().preferredHeight = NormalHeight;
      }
    }

    float snapY = content.transform.localPosition.y;

    if(!Input.GetMouseButton(0)&& velocity .y < 80.0f){      
      snapY = Mathf.Lerp( content.transform.localPosition.y, ((roundId-1) * (NormalHeight+content.GetComponent<VerticalLayoutGroup>().spacing) + MagnifyingHeight+offset) ,  5 * Time.fixedDeltaTime) ;

    }

    snapY = Mathf.Clamp(snapY, (-(NormalHeight+content.GetComponent<VerticalLayoutGroup>().spacing) + MagnifyingHeight + offset) , ((content.transform.childCount-2) * (NormalHeight+content.GetComponent<VerticalLayoutGroup>().spacing) + MagnifyingHeight+offset));

    content.transform.localPosition = new Vector3(content.transform.localPosition.x, snapY, content.transform.localPosition.z) ;

  }
}
