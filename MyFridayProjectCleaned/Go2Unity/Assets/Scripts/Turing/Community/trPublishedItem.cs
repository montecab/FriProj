using UnityEngine;
using System.Collections;
using Turing;
using WW.SimpleJSON;
using System.Collections.Generic;

public class trPublishedItem: trDescriptiveItem {
  public string ID = "";
  public Sprite Icon;
  public string IconID = "";
  public string Author = "";
  public string ProgramID = "";
  public int Popularity = 0;
  public List<trPublishedElement> Elements = new List<trPublishedElement>();
  public piRobotType RobotType = piRobotType.UNKNOWN;

  public const string JSON_ID = "id";
  public const string JSON_ICON = "icon";
  public const string JSON_PROGRAM = "program";
  public const string JSON_POPULARITY = "downloads";
  public const string JSON_AUTHOR = "publishedBy";
  public const string JSON_TRIGGERS = "triggers";
  public const string JSON_ELEMENTS = "elements";
  public const string JSON_ROBOTTYPE = "robotType";

  public override void FromJson(JSONClass jsc){
    base.FromJson(jsc);

    if(jsc[JSON_PROGRAM] != null){
      ProgramID = jsc[JSON_PROGRAM];
    }

    if(jsc[JSON_ID] != null){
      ID = jsc[JSON_ID];
    }

    if(jsc[JSON_ICON] != null){
      IconID = jsc[JSON_ICON];
    }

    if(jsc[JSON_AUTHOR] != null){
      Author = jsc[JSON_AUTHOR];
    }

    if(jsc[JSON_POPULARITY] != null){
      Popularity = jsc[JSON_POPULARITY].AsInt;
    }

    if(jsc[JSON_ELEMENTS] != null){
      foreach(JSONClass jst in jsc[JSON_ELEMENTS].AsArray){
        trPublishedElement element = new trPublishedElement();
        element.FromJson(jst);
        Elements.Add(element);
      }
    }

    if(jsc[JSON_ROBOTTYPE] != null){
      int intRobotType = jsc[JSON_ROBOTTYPE].AsInt;
      if (System.Enum.IsDefined(typeof(piRobotType), intRobotType)){
        RobotType = (piRobotType)intRobotType;
      }
    }
  }
}

public class trPublishedElement{
  public ElementType Type = ElementType.CUE;
  public trTriggerType TriggerType;
  public trBehaviorType BehaviorType;

  public const string JSON_TYPE = "type";
  public const string JSON_NAME = "name";
  public void FromJson(JSONClass jsc){
    piStringUtil.ParseStringToEnum<ElementType>(jsc[JSON_TYPE].Value, out Type);

    if(Type == ElementType.CUE){
      piStringUtil.ParseStringToEnum<trTriggerType>(jsc[JSON_NAME].Value, out TriggerType);
    }
    else if(Type == ElementType.BEHAVIOR){
      piStringUtil.ParseStringToEnum<trBehaviorType>(jsc[JSON_NAME].Value, out BehaviorType);
    }
  }

}

public enum ElementType{
  CUE,
  BEHAVIOR
}

public class trDescriptiveItem: trSharedItemBase{
  public string Description= "";
  public string Name = "";

  public const string JSON_NAME = "name";
  public const string JSON_DESCRIPTION = "description";

  public override void FromJson(JSONClass jsc){
    base.FromJson(jsc);

    if(jsc[JSON_NAME] != null){
      Name = jsc[JSON_NAME];
    }
    if(jsc[JSON_DESCRIPTION] != null){
      Description = jsc[JSON_DESCRIPTION];
    }
  }
}

public class trPrivateSharedItem: trSharedItemBase{
  public string token = "";
  public const string JSON_TOKEN = "token";
  public override void FromJson(JSONClass jsc){
    if(jsc[JSON_TOKEN] != null){
      token = jsc[JSON_TOKEN];
    }
  }
}

public class trSharedItemBase{
  public trProgram Program;

  public virtual void FromJson(JSONClass jsc){  }
}

public enum CommunityCategory{
  Picks,
  All,
  Popular
}
