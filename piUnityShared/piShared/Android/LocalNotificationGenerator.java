package com.makewonder;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.TaskStackBuilder;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.support.v4.app.NotificationCompat;

import com.makewonder.go2android.MainActivity;
import com.makewonder.go2android.R;
import com.w2.logging.LoggingHelper;

/**
 * Created by leisenhuang on 7/28/15.
 */
public class LocalNotificationGenerator  extends BroadcastReceiver {


    private static final String TAG = "LocalNotificationGenerator";
    private static final String kBlocklyAppPreferences = "BlocklyPreferences";
    private static final String kLastPusNotificationId = "LastNotificationId";

    private static final String kTitle = "title";
    private static final String kBody = "body";
    private static final String kAction = "action";

    @Override
    public void onReceive(Context context, Intent intent) {
        String title = intent.getStringExtra(kTitle);
        String body = intent.getStringExtra(kBody);
        String action = intent.getStringExtra(kAction);
        sendLocalNotification(context, title, body, action);
        LoggingHelper.i(TAG, "Triggered the alarm for showing a push notification.");
    }

    public void sendLocalNotification(Context context, String title, String body, String jsonAction) {

        Intent resultIntent = new Intent(context, MainActivity.class);
        TaskStackBuilder stackBuilder = TaskStackBuilder.create(context);
        stackBuilder.addParentStack(MainActivity.class);
        stackBuilder.addNextIntent(resultIntent);
        PendingIntent resultPendingIntent =
                stackBuilder.getPendingIntent(0, PendingIntent.FLAG_UPDATE_CURRENT);


        NotificationCompat.Builder b = new NotificationCompat.Builder(context);

        Bundle extras = new Bundle();
        if (jsonAction != null && jsonAction.length() > 0) {
            extras.putString(kAction, jsonAction);
        }


        b.setAutoCancel(true)
                .setDefaults(Notification.DEFAULT_ALL)
                .setWhen(System.currentTimeMillis())
                .setSmallIcon(R.drawable.app_icon)
                .setContentTitle(title)
                .setContentText(body)
                .setDefaults(Notification.DEFAULT_LIGHTS | Notification.DEFAULT_SOUND)
                .setContentIntent(resultPendingIntent)
                .addExtras(extras)
                .setAutoCancel(true);


        SharedPreferences preferences = context.getSharedPreferences(kBlocklyAppPreferences, Context.MODE_PRIVATE);
        int lastNotificationId = preferences.getInt(kLastPusNotificationId, 0);



        NotificationManager notificationManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        Notification notification = b.build();


        notificationManager.notify(lastNotificationId + 1, b.build());

        SharedPreferences.Editor editor = preferences.edit();
        editor.putInt(kLastPusNotificationId, lastNotificationId + 1);
        editor.apply();

    }

}
