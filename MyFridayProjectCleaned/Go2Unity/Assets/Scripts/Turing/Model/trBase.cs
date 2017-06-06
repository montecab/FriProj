using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing {
  
  public abstract class trBase {
    public string UUID;
    
    public const string Indent = "  ";
    
    protected trBase() {
      UUID = wwUID.getUID();
    }
    
    #region serialization    
    // this is the only public ToJson method for all Base objects.
    public JSONClass ToJson() {
      JSONClass js = new JSONClass();
      IntoJson(js);
      return js;
    }
    
    public static T FromJson<T>(JSONClass jsc) where T:trBase, new()  {
      T ret = new T();
      ret.OutOfJson(jsc);
      return ret;
    }
    
    protected virtual void IntoJson(JSONClass jsc) {
      jsc[TOKENS.ID] = UUID;
    }
    
    protected virtual void OutOfJson(JSONClass jsc) {
      UUID = jsc[TOKENS.ID];
    }
    
    #endregion serialization    
  }
}

