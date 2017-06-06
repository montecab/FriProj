using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Turing;
using TMPro;

public class trPickerController : trParameterPanelBase {

  public GameObject PickerColumnPrefab;

  public TextMeshProUGUI PickerHeaderTitleTemplate;
  public GameObject PickerHeaderContainer;

  private List<trPickerColumn> columns = new List<trPickerColumn>();
  public HorizontalLayoutGroup Grid;

  public void SetDataSource(Dictionary <string, List<string>> datasource, int[] selectedIndex){
    foreach(trPickerColumn column in columns){
      column.OnValueChanged -= onColumnValueChanged;
      Destroy(column.gameObject);
    }
    columns.Clear();

    for(int i = 0; i< PickerHeaderContainer.transform.childCount; i++) {
      Destroy(PickerHeaderContainer.transform.GetChild(i).gameObject);
    }

    int index = 0;
    foreach (string key in datasource.Keys){
      TextMeshProUGUI columnHeader = Instantiate(PickerHeaderTitleTemplate, Vector3.zero, Quaternion.identity) as TextMeshProUGUI;
      columnHeader.text = key;
      columnHeader.gameObject.SetActive(true);
      columnHeader.transform.SetParent(PickerHeaderContainer.transform);
      RectTransform rectT = columnHeader.GetComponent<RectTransform>();
      rectT.SetDefaultScale();
      Vector3 position = rectT.localPosition;
      position.z = 0;
      rectT.localPosition = position;

      GameObject column = Instantiate(PickerColumnPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      column.transform.SetParent(Grid.transform);
      trPickerColumn columnController = column.GetComponent<trPickerColumn>();
      columnController.AddItems(datasource[key].ToArray());
      columnController.SelectItemAtIndex(selectedIndex[index]);
      columnController.OnValueChanged += onColumnValueChanged;
      columns.Add(columnController);

      rectT = column.GetComponent<RectTransform>();
      rectT.SetDefaultScale();
      position = rectT.localPosition;
      position.z = 0;
      rectT.localPosition = position;
      index++;
    }
  }

  public void SelectItems(int[] selection){
    for (int i = 0; i < columns.Count; i++){
      columns[i].SelectItemAtIndex(selection[i]);
    }
  }

  public int[] GetSelectedValues(){
    int[] result = new int[columns.Count];
    for(int i = 0; i < columns.Count; i++){
      result[i] = columns[i].GetSelectedIndex();
    }
    return result;
  }

  public override float GetValue () {
    return 0;
  }

  public override void SetUpView (float value) {

  }

  void onColumnValueChanged(trPickerColumn column, int selectedIndex){
    if (OnValueChanged != null){
      OnValueChanged.Invoke(GetValue());
    }
  }
}
