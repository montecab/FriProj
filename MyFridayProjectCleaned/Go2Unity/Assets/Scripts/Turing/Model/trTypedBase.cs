using UnityEngine;
using System.Collections;
using WW.SimpleJSON;
using System;

// trTypedBase.
// this is an abstract sub-class of trBase which includes a public member variable named "Type".
// the advantage of inheriting from this templatized base class is that you don't have to serialize the Type member.


namespace Turing {
  
  public abstract class trTypedBase <T> : trBase where T: struct, IComparable, IFormattable, IConvertible {
  
    public T Type;
    
    public trTypedBase() {}
    
    public trTypedBase(T typeVal) : this() {
      Type = typeVal;
    }
  
    #region serialization
    protected override void IntoJson(JSONClass jsc) {
      jsc[TOKENS.TYPE] = Type.ToString();
      base.IntoJson(jsc);
    }
    
    protected override void OutOfJson(JSONClass jsc) {
      base.OutOfJson(jsc);
      piStringUtil.ParseStringToEnum<T>(jsc[TOKENS.TYPE], out Type);
    }
    #endregion serialization

  }    
}
