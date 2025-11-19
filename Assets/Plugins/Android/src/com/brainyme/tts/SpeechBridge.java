package com.brainyme.tts;

import android.app.Activity;
import android.content.Intent;

public class SpeechBridge {

    public static void StartListening(Activity unityActivity) {
        Intent intent = new Intent(unityActivity, SpeechActivity.class);
        unityActivity.startActivity(intent);
    }
}
