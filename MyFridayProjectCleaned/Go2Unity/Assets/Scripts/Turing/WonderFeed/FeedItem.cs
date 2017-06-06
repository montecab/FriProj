using UnityEngine;
using System.Collections;
using WW.SimpleJSON;

public class FeedItem {
  public FeedType Type;
  public string ID; 
  public string Description;
  public string Link;
  public Sprite Image;
  public string ImageUrl;

  public const string JSON_ID = "id";
  public const string JSON_TYPE = "type";
  public const string JSON_IMAGE = "image";
  public const string JSON_DESCRIPTION = "description";
  public const string JSON_LINK = "link";
  public const string JSON_ELEMENTS = "elements";

  public void FromJson(JSONClass jsc){

    if(jsc[JSON_TYPE] != null){
      piStringUtil.ParseStringToEnum<FeedType>(jsc[JSON_TYPE].Value, out Type);
    }

    if(jsc[JSON_ID] != null){
      ID = jsc[JSON_ID];
    }

    if(jsc[JSON_IMAGE] != null){
      ImageUrl = jsc[JSON_IMAGE];
    }

    if(jsc[JSON_LINK] != null){
      Link = jsc[JSON_LINK];
    }

    if(jsc[JSON_DESCRIPTION] != null){
      Description = jsc[JSON_DESCRIPTION];
    }
  }

}

public enum FeedType{
  TEXT,
  THUMBNAIL
}
