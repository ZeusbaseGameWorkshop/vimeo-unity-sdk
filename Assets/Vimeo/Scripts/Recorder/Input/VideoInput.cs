#if UNITY_2017_3_OR_NEWER

using UnityEngine;

namespace Vimeo.Recorder
{
    public class VideoInput : MonoBehaviour
    {
        public VimeoRecorder recorder;
        
        public int outputWidth { get; protected set; }
        public int outputHeight { get; protected set; }

        public virtual Texture2D GetFrame() { return new Texture2D(1, 1); }
        public virtual void BeginRecording() { }
    }
}

#endif