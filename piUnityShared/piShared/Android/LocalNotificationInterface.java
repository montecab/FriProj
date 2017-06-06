package com.makewonder;

/**
 * Created by leisenhuang on 7/28/15.
 */
public interface LocalNotificationInterface {
    public void scheduleNotification(String title, String body, String jsonParams, long millisecondsSince1970);
}
