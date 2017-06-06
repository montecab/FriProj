using UnityEngine;
using System.Collections.Generic;

namespace Turing {
  
  
  public class trExpressions : Singleton<trExpressions> {
    public Dictionary<trBehaviorType, List<trExpression>> categories = new Dictionary<trBehaviorType, List<trExpression>>();
    public Dictionary<trBehaviorType, string>             categoryNames  = new Dictionary<trBehaviorType, string>();
    
    bool initialized = false;

    #region list management
    void init() {
      if (initialized) {
        return;
      }
      
      initialized = true;
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=1901035233
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      setCategoryName(trBehaviorType.EXPRESSION_CATEGORY_1, "@!@Cautious@!@");
      setCategoryName(trBehaviorType.EXPRESSION_CATEGORY_2, "@!@Curious@!@");
      setCategoryName(trBehaviorType.EXPRESSION_CATEGORY_3, "@!@Frustrated@!@");
      setCategoryName(trBehaviorType.EXPRESSION_CATEGORY_4, "@!@Joyful@!@");
      setCategoryName(trBehaviorType.EXPRESSION_CATEGORY_5, "@!@Silly@!@");
      
      // this code is GENERATED from this spreadsheet: https://docs.google.com/spreadsheets/d/1gccOs1yIe2lUmDRtA9-zyCjHAi_aCFkYPwXHsqK9djM/edit#gid=0
      // DO NOT EDIT HERE. edit the spreadsheet, then copy-paste the code.
      addExpression(1000, trBehaviorType.EXPRESSION_CATEGORY_1, "Help Help !", "dash_cautious_spark_expression_helpHelp");
      addExpression(1001, trBehaviorType.EXPRESSION_CATEGORY_2, "Let's Do It !", "dash_confidence_spark_expression_letsDoIt");
      addExpression(1002, trBehaviorType.EXPRESSION_CATEGORY_3, "Awwww", "dash_frustration_spark_expression_awwwHeadShake");
      addExpression(1003, trBehaviorType.EXPRESSION_CATEGORY_4, "Way 2 Go !", "dash_happy_spark_expression_wayToGo");
      addExpression(1004, trBehaviorType.EXPRESSION_CATEGORY_5, "YAH!", "dash_happy_spark_expression_YAH");
    }
    
    public trExpression GetExpression(trState state){
      uint expressionId = (uint)state.BehaviorParameterValue;
      return GetExpression(expressionId);
    }
    
    public int GetUserFacingIndex(trState state){

      trExpression expression = GetExpression(state); // because no mater dash or dot they should have the same index
      return(GetUserFacingIndex(expression));
    }
    
    public int GetUserFacingIndex(trExpression expression){
      init();
      if (categories[expression.category].Contains(expression)) {
        return categories[expression.category].IndexOf(expression) + 1;
      }


      WWLog.logError("Cannot find expression " + expression.userFacingName);
      return -1;
    }
    
    private void setCategoryName(trBehaviorType category, string name) {
      categoryNames[category] = name;
    }
    
    public string getCategoryName(trBehaviorType category) {
      if (!categoryNames.ContainsKey(category)) {
        WWLog.logError("no name for category: " + category.ToString());
        return category.ToString();
      }
      
      return categoryNames[category];
    }
    
   
   
    private void addExpression(int id,trBehaviorType category,  string userFacingName, string filename) {
      init();
      addExpression((uint)id, filename, category, userFacingName);
    }
    
    private void addExpression(uint id, string filename, trBehaviorType category, string userFacingName) {
      init();
      
      if (string.IsNullOrEmpty(filename)) {
        WWLog.logError("anim file name missing: " + id);
        return;
      }

      if (GetExpression(id) != null) {
        WWLog.logError("duplicate expression id: " + id);
        return;
      }
      
      if (!string.IsNullOrEmpty(filename)) {
        addExpression(id, filename, category, userFacingName, categories);
      }
      

    }
    
    private void addExpression(uint id, string filename, trBehaviorType category, string userFacingName, Dictionary<trBehaviorType, List<trExpression>> categoriesDict) {
      init();
      if (!categoriesDict.ContainsKey(category)) {
        categoriesDict[category] = new List<trExpression>();
      }
      
      trExpression expression = new trExpression();
      expression.id             = id;
      expression.filename       = filename;
      expression.category       = category;
      expression.userFacingName = userFacingName;
      
      categoriesDict[category].Add(expression);
    }

    public trExpression GetExpression(uint id) {
      init ();
      if (categories == null) {
        return null;
      }
      
      foreach (List<trExpression> expressions in categories.Values) {
        foreach (trExpression expression in expressions) {
          if (expression.id == id) {
            return expression;
          }
        }
      }
      
      return null;
    }

    public List<trExpression> GetCategory(trBehaviorType type) {
      init ();
      return categories[type];
    }
    
    public List<trBehaviorType> GetCategories() {
      init ();
      List<trBehaviorType> ret = new List<trBehaviorType>();
      foreach (trBehaviorType trBT in categoryNames.Keys) {
        ret.Add (trBT);
      }
      return ret;
    }

    #endregion list management
  }
  
  
  public class trExpression {
    public const uint           INVALID_ID      = 0;
    public uint                 id              = INVALID_ID;
    public string               filename        = ""; // filename on robot
    public trBehaviorType       category        = trBehaviorType.EXPRESSION_CATEGORY_1;
    public string               userFacingName  = "";
  }
}
