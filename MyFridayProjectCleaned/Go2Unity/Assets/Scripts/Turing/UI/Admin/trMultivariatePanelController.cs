using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Turing {
  public class trMultivariatePanelController : MonoBehaviour {

    public Text TitleText;
    public GridLayoutGroup ParametersGrid;
    public Scrollbar ScrollBar;
    public GameObject ItemPrefab;
    public Button CloseButton;

    private ViewModelRepresentation currentModel;

    void Awake(){
      CloseButton.onClick.AddListener(() => goBack());
    }

  	// Use this for initialization
  	void Start () {
      setupViewForModel(generateOptionsModel());
  	}

    public void goBack(){
      gameObject.SetActive(false);
      setupViewForModel(generateOptionsModel());
    }

    private ViewModelRepresentation generateOptionsModel(){

      ViewModelRepresentation result = new ViewModelRepresentation();
      result.Title = "App Options";

      foreach (trMultivariate.trAppOption item in Enum.GetValues(typeof(trMultivariate.trAppOption))){
        if (trMultivariate.Instance.isUsedOption(item)) {
          result.parameter[item.ToString()] = trMultivariate.Instance.getOptionValue(item).ToString();
        }
      }

      result.handler = handleCategoryItemClicked;

      return result;
    }

    private ViewModelRepresentation generateOptionValuesModes(trMultivariate.trAppOption option){
      trMultivariate instance = trMultivariate.Instance;
      ViewModelRepresentation result = new ViewModelRepresentation();
      result.activeOption = option;
      result.Title = string.Format("{0} : {1}", option, instance.getOptionValue(option));

      foreach (trMultivariate.trAppOptionValue item in instance.getPossibleValues(option)){
        result.parameter[item.ToString()] = "";
      }

      result.handler = handleParameterItemClicked;

      return result;
    }

    private void setupViewForModel(ViewModelRepresentation viewModel){
      currentModel = viewModel;
      TitleText.text = viewModel.Title;

      var children = new List<GameObject>();
      foreach (Transform child in ParametersGrid.transform) {
        children.Add(child.gameObject);
        trListItemControl control = child.GetComponent<trListItemControl>();
        if (control != null){
          control.onItemClicked.RemoveAllListeners();
        }
      }
      children.ForEach(child => Destroy(child));


      foreach (KeyValuePair<string, string> entry in viewModel.parameter){
        GameObject itemObject = Instantiate(ItemPrefab) as GameObject;
        trListItemControl control = itemObject.GetComponent<trListItemControl>();
        control.NameText.text = entry.Key;
        control.ValueText.text = entry.Value;
        control.onItemClicked.AddListener(viewModel.handler);
      
        itemObject.transform.SetParent(ParametersGrid.transform);
        itemObject.GetComponent<RectTransform>().SetDefaultScale();
      }
    }

    private void handleCategoryItemClicked(trListItemControl item){
      trMultivariate.trAppOption option;
      piStringUtil.ParseStringToEnum<trMultivariate.trAppOption>(item.NameText.text, out option);
      setupViewForModel(generateOptionValuesModes(option));
    }

    private void handleParameterItemClicked(trListItemControl item){
      trMultivariate.trAppOptionValue optionValue;
      piStringUtil.ParseStringToEnum<trMultivariate.trAppOptionValue>(item.NameText.text, out optionValue);
      trMultivariate.Instance.setOptionValue(currentModel.activeOption, optionValue);
      setupViewForModel(generateOptionsModel());
    }

    private class ViewModelRepresentation {
      public string Title;
      public Dictionary<string, string> parameter;
      public UnityEngine.Events.UnityAction<trListItemControl> handler;
      public trMultivariate.trAppOption activeOption;

      public ViewModelRepresentation () {
        parameter = new Dictionary<string,string>();
      }

      public ViewModelRepresentation (string title, Dictionary<string, string> parameter){
        this.Title = title;
        this.parameter = parameter;
      }
      
    }
  }
}
