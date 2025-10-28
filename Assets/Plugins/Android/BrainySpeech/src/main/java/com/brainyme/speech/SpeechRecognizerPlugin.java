package com.brainyme.speech;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.speech.RecognitionListener;
import android.speech.RecognizerIntent;
import android.speech.SpeechRecognizer;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;

public class SpeechRecognizerPlugin {
    private static SpeechRecognizerPlugin instance;

    public static SpeechRecognizerPlugin create(Activity activity, String go, String resultMethod, String errorMethod) {
        if (instance == null) instance = new SpeechRecognizerPlugin(activity, go, resultMethod, errorMethod);
        return instance;
    }

    private final Activity activity;
    private SpeechRecognizer recognizer;
    private final String gameObject;
    private final String resultMethod;
    private final String errorMethod;
    private final Handler handler = new Handler(Looper.getMainLooper());

    private SpeechRecognizerPlugin(Activity activity, String go, String result, String error) {
        this.activity = activity;
        this.gameObject = go;
        this.resultMethod = result;
        this.errorMethod = error;

        activity.runOnUiThread(() -> {
            recognizer = SpeechRecognizer.createSpeechRecognizer(activity);
            recognizer.setRecognitionListener(new RecognitionListener() {
                @Override public void onReadyForSpeech(Bundle params) {}
                @Override public void onBeginningOfSpeech() {}
                @Override public void onRmsChanged(float rmsdB) {}
                @Override public void onBufferReceived(byte[] buffer) {}
                @Override public void onEndOfSpeech() {}

                @Override public void onError(int error) {
                    UnityPlayer.UnitySendMessage(gameObject, errorMethod, "Speech error: " + error);
                }

                @Override public void onResults(Bundle results) {
                    ArrayList<String> list = results.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                    String best = (list != null && !list.isEmpty()) ? list.get(0) : "";
                    UnityPlayer.UnitySendMessage(gameObject, resultMethod, best);
                }

                @Override public void onPartialResults(Bundle partialResults) {
                    // optional: send partials if you want
                    // ArrayList<String> list = partialResults.getStringArrayList(SpeechRecognizer.RESULTS_RECOGNITION);
                    // if (list != null && !list.isEmpty()) {
                    //     UnityPlayer.UnitySendMessage(gameObject, resultMethod, list.get(0));
                    // }
                }

                @Override public void onEvent(int eventType, Bundle params) {}
            });
        });
    }

    public void startListening(String lang, int timeoutSeconds) {
        activity.runOnUiThread(() -> {
            if (recognizer == null) return;

            Intent i = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
            i.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
            if (lang != null && !lang.isEmpty()) {
                i.putExtra(RecognizerIntent.EXTRA_LANGUAGE, lang);
                i.putExtra(RecognizerIntent.EXTRA_LANGUAGE_PREFERENCE, lang);
            }
            i.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 3);
            i.putExtra(RecognizerIntent.EXTRA_PARTIAL_RESULTS, true);
            i.putExtra(RecognizerIntent.EXTRA_CALLING_PACKAGE, activity.getPackageName());

            // Endpointer tuning – avoids instant NO_MATCH on many devices
            i.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_MINIMUM_LENGTH_MILLIS, 1200);  // user has at least ~1.2s
            i.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_COMPLETE_SILENCE_LENGTH_MILLIS, 800); // stop after 0.8s silence
            i.putExtra(RecognizerIntent.EXTRA_SPEECH_INPUT_POSSIBLY_COMPLETE_SILENCE_LENGTH_MILLIS, 500);

            // Optional: prefer offline if packs installed (set false if you want online)
            i.putExtra(RecognizerIntent.EXTRA_PREFER_OFFLINE, false);

            try {
                recognizer.startListening(i);
            } catch (Exception e) {
                UnityPlayer.UnitySendMessage(gameObject, errorMethod, "startListening exception: " + e.getMessage());
                return;
            }

            // Safety timeout: stop listening after requested seconds
            int ms = Math.max(1500, timeoutSeconds * 1000);
            handler.removeCallbacksAndMessages(null);
            handler.postDelayed(() -> {
                try { recognizer.stopListening(); } catch (Exception ignored) {}
            }, ms);
        });
    }

    public void stopListening() {
        activity.runOnUiThread(() -> {
            if (recognizer != null) {
                try { recognizer.stopListening(); } catch (Exception ignored) {}
            }
            handler.removeCallbacksAndMessages(null);
        });
    }

    public void release() {
        activity.runOnUiThread(() -> {
            handler.removeCallbacksAndMessages(null);
            if (recognizer != null) {
                try { recognizer.destroy(); } catch (Exception ignored) {}
                recognizer = null;
            }
        });
    }
}
