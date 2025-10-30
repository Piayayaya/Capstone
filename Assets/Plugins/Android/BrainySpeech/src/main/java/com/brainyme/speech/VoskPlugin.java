// Assets/Plugins/Android/BrainySpeech/src/main/java/com/brainyme/speech/VoskPlugin.java
package com.brainyme.speech;

import android.media.AudioFormat;
import android.media.AudioRecord;
import android.media.MediaRecorder;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import org.vosk.LibVosk;
import org.vosk.LogLevel;
import org.vosk.Model;
import org.vosk.Recognizer;

import java.io.File;
import java.io.IOException;

public class VoskPlugin {

    private static final String TAG = "VoskPlugin";
    private static final int SAMPLE_RATE = 16000;

    // Unity callback targets (from C#)
    private final String goName;
    private final String resultMethod;
    private final String errorMethod;

    // Vosk state
    private Model model;
    private Recognizer recognizer;

    // Audio state
    private AudioRecord recorder;
    private Thread audioThread;
    private volatile boolean running = false;

    static {
        // Vosk 0.3.70 uses enum LogLevel, not int
        LibVosk.setLogLevel(LogLevel.ERROR);
    }

    private VoskPlugin(String goName, String resultMethod, String errorMethod) {
        this.goName = goName;
        this.resultMethod = resultMethod;
        this.errorMethod = errorMethod;
    }

    // ---- Factory to match your C# bridge ----
    public static VoskPlugin create(android.app.Activity activity,
                                    String goName,
                                    String resultMethod,
                                    String errorMethod) {
        // 'activity' not used directly here, but keeping the signature your C# expects
        return new VoskPlugin(goName, resultMethod, errorMethod);
    }

    // ---- API expected by your C# ----
    public void loadModel(String modelPath) throws IOException {
        if (model != null) return;

        if (modelPath == null || modelPath.trim().isEmpty())
            throw new IOException("Model path is empty");

        File dir = new File(modelPath);
        if (!dir.exists() || !dir.isDirectory())
            throw new IOException("Model path not found: " + modelPath);

        model = new Model(modelPath);
        recognizer = new Recognizer(model, SAMPLE_RATE);
        Log.i(TAG, "Vosk model loaded: " + modelPath);
    }

    public void startListening() {
        if (running) return;
        if (recognizer == null) {
            sendError("Vosk not initialized. Call loadModel(path) first.");
            return;
        }

        int min = AudioRecord.getMinBufferSize(
                SAMPLE_RATE, AudioFormat.CHANNEL_IN_MONO, AudioFormat.ENCODING_PCM_16BIT);
        int bufSize = Math.max(min, 4096) * 3;

        // Prefer VOICE_RECOGNITION; fall back to MIC if needed
        recorder = new AudioRecord(
                MediaRecorder.AudioSource.VOICE_RECOGNITION,
                SAMPLE_RATE,
                AudioFormat.CHANNEL_IN_MONO,
                AudioFormat.ENCODING_PCM_16BIT,
                bufSize);

        if (recorder.getState() != AudioRecord.STATE_INITIALIZED) {
            try { recorder.release(); } catch (Throwable ignored) {}
            recorder = new AudioRecord(
                    MediaRecorder.AudioSource.MIC,
                    SAMPLE_RATE,
                    AudioFormat.CHANNEL_IN_MONO,
                    AudioFormat.ENCODING_PCM_16BIT,
                    bufSize);
            if (recorder.getState() != AudioRecord.STATE_INITIALIZED) {
                sendError("AudioRecord init failed");
                try { recorder.release(); } catch (Throwable ignored) {}
                recorder = null;
                return;
            }
        }

        running = true;
        recorder.startRecording();

        audioThread = new Thread(this::readLoop, "VoskMicThread");
        audioThread.setDaemon(true);
        audioThread.start();
    }

    public void stopListening() {
        running = false;

        if (audioThread != null) {
            try { audioThread.join(500); } catch (InterruptedException ignored) {}
            audioThread = null;
        }
        if (recorder != null) {
            try { recorder.stop(); } catch (Throwable ignored) {}
            try { recorder.release(); } catch (Throwable ignored) {}
            recorder = null;
        }
    }

    public void release() {
        stopListening();
        try { if (recognizer != null) recognizer.close(); } catch (Throwable ignored) {}
        try { if (model != null) model.close(); } catch (Throwable ignored) {}
        recognizer = null;
        model = null;
    }

    // ---- Internals ----
    private void readLoop() {
        short[] buf = new short[2048];
        while (running && recorder != null) {
            int n;
            try {
                n = recorder.read(buf, 0, buf.length);
            } catch (Throwable t) {
                sendError("recorder.read failed: " + t.getMessage());
                break;
            }
            if (n <= 0) continue;

            try {
                if (recognizer.acceptWaveForm(buf, n)) {
                    sendResult(recognizer.getResult());        // final utterance JSON
                } else {
                    sendResult(recognizer.getPartialResult()); // partial JSON
                }
            } catch (Throwable t) {
                sendError("Vosk error: " + t.getMessage());
            }
        }
    }

    private void sendResult(String json) {
        if (json == null) json = "{}";
        try {
            UnityPlayer.UnitySendMessage(goName, resultMethod, json);
        } catch (Throwable ignored) {}
    }

    private void sendError(String message) {
        try {
            UnityPlayer.UnitySendMessage(goName, errorMethod, message == null ? "" : message);
        } catch (Throwable ignored) {}
        Log.e(TAG, message == null ? "error" : message);
    }
}
