using NAudio.Wave;
using System;

namespace SpeechProcessing.Recorder
{
    class NAudioRecorder : ISpeechRecorder
    {
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;
        private string fileName = string.Empty;

        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRec()
        {
            waveSource = new WaveIn();
            waveSource.WaveFormat = new WaveFormat(16000, 16, 1); // 16bit,16KHz,Mono的录音格式

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveFile = new WaveFileWriter(fileName, waveSource.WaveFormat);

            waveSource.StartRecording();
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRec()
        {
            waveSource.StopRecording();

            // Close Wave(Not needed under synchronous situation)
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }

        /// <summary>
        /// 录音结束后保存的文件路径
        /// </summary>
        /// <param name="fileName">保存wav文件的路径名</param>
        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// 开始录音回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        /// <summary>
        /// 录音结束回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }
        }
    }
}
