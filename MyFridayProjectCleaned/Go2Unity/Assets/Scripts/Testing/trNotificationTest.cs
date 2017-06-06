using UnityEngine;
using System.Collections;

public class trNotificationTest : MonoBehaviour {

  public void ClickButton(){
    handleLocalNotificationtion(System.DateTime.UtcNow.AddMinutes(1));
  }

  void handleLocalNotificationtion(System.DateTime time){  
    piConnectionManager.Instance.scheduleLocalNotification("Spark","New challenge is available! Go check!", "", time);
    WWLog.logInfo("Sent notification for " + time);
  }
}
