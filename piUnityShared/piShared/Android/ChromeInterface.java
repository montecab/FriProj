package com.makewonder;

/**
 * Created by leisenhuang on 1/23/15.
 */
public interface ChromeInterface {
    public void openChrome();
    public void showChromeButton();
    public void hideChromeButton();
    public void openVoiceRecording();
    public void openPrivacyDialog();
    public void showConnectToRobotDialog(int robotType);
    public void showSystemDialog(String title, String message, String buttonText);
    public void startDownloadLocaFiles(String appName, String version, boolean zip, String fileName, String path );
}
