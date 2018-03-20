﻿#if UNITY_2017_3_OR_NEWER 

using UnityEditor.Media;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
    using Unity.Collections;
#else
    using UnityEngine.Collections;
#endif

using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Vimeo.Recorder {

    public class RecorderController : MonoBehaviour
    {
        [HideInInspector] public string outputPath = Path.GetTempPath();
        [HideInInspector] public VimeoRecorder recorder;
        [HideInInspector] public string encodedFilePath;
        [HideInInspector] public bool isRecording = false;

        private VideoTrackAttributes videoAttrs;
        private AudioTrackAttributes audioAttrs;
        // private int sampleFramesPerVideoFrame;

        [HideInInspector] public int currentFrame = 0;

        private Material matCopy;
        private Shader shaderCopy;
        private Mesh fullscreenQuad;

        private MediaEncoder encoder;
        private NativeArray<float> audioBuffer;
        private RenderTexture renderBuffer;
        private CommandBuffer commandBuffer;

        private VideoInput videoInput;

        public void BeginRecording()
        {
            Debug.Log("RecorderController: BeginRecording()");
            isRecording = true;

            InitVideoInput();
            videoInput.BeginRecording();

            encodedFilePath = Path.Combine(outputPath, GetFileName());
            Debug.Log(encodedFilePath);

            if (recorder.realTime) {
                Application.targetFrameRate = recorder.frameRate;
            }
            else {
                Time.captureFramerate = recorder.frameRate;
            }

            // Configure encoder
            videoAttrs = new VideoTrackAttributes
            {
                frameRate = new MediaRational(recorder.frameRate),
                width  = (uint)videoInput.outputWidth,
                height = (uint)videoInput.outputHeight,
                includeAlpha = false
            };

            audioAttrs = new AudioTrackAttributes
            {
                sampleRate = new MediaRational(48000),
                channelCount = 2,
                language = "en"
            };

            encoder = new UnityEditor.Media.MediaEncoder(encodedFilePath, videoAttrs);
        }

        public string GetFileName()
        {
            string name = recorder.videoName;
    
            if (name.Contains("%R")) {
                name = name.Replace("%R", videoInput.outputWidth + "x" + videoInput.outputHeight);
            }

            if (name.Contains("%TS")) {
                name = name.Replace("%TS", String.Format("{0:yyyy.MM.dd H.mm.ss}", System.DateTime.Now));
            }

            return name + ".mp4";
        }

        public void EndRecording()
        {
            if (encoder != null) {
                encoder.Dispose();
                encoder = null;
            }

            if (videoInput != null) {
                videoInput.EndRecording();
            }

            Destroy(videoInput);

            Time.captureFramerate = 0;

            if (isRecording) {
                Debug.Log("RecorderController.EndRecording()");
            }

            currentFrame = 0;
            isRecording = false;
        }

        public void DeleteVideoFile()
        {
            File.Delete(encodedFilePath);
        }

        IEnumerator RecordFrame()
        {
            yield return new WaitForEndOfFrame();
            if (encoder != null && isRecording) {
                encoder.AddFrame(videoInput.GetFrame());
                videoInput.EndFrame();
            }
        }

        public void LateUpdate()
        {
            if (encoder != null && isRecording) {
                if (recorder.recordMode == RecordMode.Duration) {
                    if (currentFrame > recorder.frameRate * recorder.recordDuration) {
                        recorder.EndRecording();
                    }
                    else {
                        StartCoroutine(RecordFrame());
                    }
                }
                currentFrame++;
            }
        }
        
        private void InitVideoInput()
        {
            if (videoInput != null) {
                Destroy(videoInput);
            }

            switch(recorder.defaultVideoInput) {
                case VideoInputType.Screen:
                    videoInput = gameObject.AddComponent<ScreenInput>();
                    break;
                
                case VideoInputType.Camera360:
                case VideoInputType.Camera:
                    videoInput = gameObject.AddComponent<CameraInput>();
                    break;
            }

            videoInput.recorder = recorder;
        }

        public void OnDestroy()
        {
            Destroy(videoInput);
        }
    }
}
#endif