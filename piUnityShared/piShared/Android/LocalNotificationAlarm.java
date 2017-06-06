package com.makewonder;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;

import com.w2.logging.LoggingHelper;

import java.util.Calendar;

/**
 * Created by leisenhuang on 7/28/15.
 */
public class LocalNotificationAlarm {
    private static final String TAG = "LocalNotificationHelper";
    private static final String kTitle = "title";
    private static final String kBody = "body";
    private static final String kAction = "action";


    private Context _context;

    public LocalNotificationAlarm(Context context) {
        _context = context;
    }

    public void sendPushNotification(String title, String body, String jsonAction, long timeInMilliseconds) {
        Calendar updateTime = Calendar.getInstance();
        updateTime.setTimeInMillis(timeInMilliseconds);

        Intent localNotificationIntent = new Intent(this._context, LocalNotificationGenerator.class);
        localNotificationIntent.putExtra(kTitle, title);
        localNotificationIntent.putExtra(kBody, body);
        localNotificationIntent.putExtra(kAction, jsonAction);
        localNotificationIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

        PendingIntent pendingIntent = PendingIntent.getBroadcast(this._context, 0,
                localNotificationIntent, PendingIntent.FLAG_CANCEL_CURRENT);

        AlarmManager alarmManager = (AlarmManager) this._context.getSystemService(Context.ALARM_SERVICE);
        alarmManager.setExact(AlarmManager.RTC_WAKEUP, updateTime.getTimeInMillis(), pendingIntent);

        LoggingHelper.i(TAG, "Set Local Notification for Time: " + updateTime.getTime().toString());
    }
}
