using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class trPickerColumn : MonoBehaviour, IDragHandler{

  public GameObject PickerItemPrefab;
  public VerticalLayoutGroup Grid;
  public ScrollRect ScrollView;

  public delegate void OnSelectionChangedDelegate(trPickerColumn column, int selectedIndex);
  public OnSelectionChangedDelegate OnValueChanged;

  private int selectedIndex = -1;
  private string[] titles;
  private bool shouldSnapToNearestPosition = false;
  private float itemHeight;

  void Update(){
    if (shouldSnapToNearestPosition){
      // find out where to snap to
      float normalizedPosition = 1 - (float)selectedIndex / (float)(titles.Length - 1);

      // this is for scroll inertia!
      ScrollView.verticalNormalizedPosition = Mathf.Lerp(ScrollView.verticalNormalizedPosition, normalizedPosition, 10 * Time.deltaTime); 
      shouldSnapToNearestPosition = !Mathf.Approximately(ScrollView.verticalNormalizedPosition, normalizedPosition);
    }
  }

  public int GetSelectedIndex(){
    return selectedIndex;
  }

  public void AddItems(string[] titles){
    foreach(string title in titles){
      GameObject item = Instantiate(PickerItemPrefab);
      TextMeshProUGUI label = item.GetComponent<TextMeshProUGUI>();
      label.text = title;
      item.transform.SetParent(Grid.transform);
    }
    this.titles = titles;

    //itemHeight = PickerItemPrefab.GetComponent<LayoutElement>().preferredHeight;

    //float height = transform.parent.GetComponent<RectTransform>().GetSize().y;
    //WWLog.logInfo("here is the height for item: " + height);

    // int offset = (int)(height - PickerItemPrefab.GetComponent<LayoutElement>().preferredHeight) / 2;
    // Grid.padding.top = offset;
    // Grid.padding.bottom = offset;
  }

  public void SelectItemAtIndex(int index){
    selectedIndex = index;
    shouldSnapToNearestPosition = true;
  }

  private void updateSelectedIndex(){
    int count = titles.Length - 1;
    int index = Mathf.RoundToInt(Mathf.Clamp((1 - ScrollView.verticalNormalizedPosition) * count, 0, count));

    if (index != selectedIndex){
      selectedIndex = index;
      if (OnValueChanged != null){
        OnValueChanged(this, selectedIndex);
      }
    }
  }

  public void OnPointerUp () {
    updateSelectedIndex();
    shouldSnapToNearestPosition = true;
  }

  public void OnDrag (PointerEventData eventData) {
    updateSelectedIndex();
    shouldSnapToNearestPosition = false;
  }

}
