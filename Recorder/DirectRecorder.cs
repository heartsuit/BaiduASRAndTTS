/*Reference: From Internet
 * Note: When using this as the recorder, you will have to set 
 * Exception Settings--->Managed Debugging Assistants--->LoaderLock(Uncheck this) 
 */

using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace SpeechProcessing.Recorder
{
    public class DirectRecorder : ISpeechRecorder
    {
        // 对DirectSound的支持
        int cNotifyNum = 16;       // 缓冲队列的数目
        int mNextCaptureOffset = 0;      // 该次录音缓冲区的起始点
        int mSampleCount = 0;            // 录制的样本数目
        int mNotifySize = 0;             // 每次通知大小
        int mBufferSize = 0;             // 缓冲队列大小
        string mFileName = string.Empty;     // 文件名
        FileStream mWaveFile = null;         // 文件流
        BinaryWriter mWriter = null;         // 写文件
        Capture mCapDev = null;              // 音频捕捉设备
        CaptureBuffer mRecBuffer = null;     // 缓冲区对象
        Notify mNotify = null;               // 消息通知对象
        WaveFormat mWavFormat;                       // 录音的格式
        Thread mNotifyThread = null;                 // 处理缓冲区消息的线程
        AutoResetEvent mNotificationEvent = null;    // 通知事件

        /// <summary>
        /// 构造函数,设定录音设备,设定录音格式.
        /// </summary>
        public DirectRecorder()
        {
            // 初始化音频捕捉设备
            InitCaptureDevice();
            // 设定录音格式
            mWavFormat = CreateWaveFormat();
        }

        /// <summary>
        /// 设定录音结束后保存的文件,包括路径
        /// </summary>
        /// <param name="fileName">保存wav文件的路径名</param>
        public void SetFileName(string fileName)
        {
            mFileName = fileName;
        }

        /// <summary>
        /// 开始录音
        /// </summary>
        public void StartRec()
        {
            // 创建录音文件
            CreateSoundFile();
            // 创建一个录音缓冲区，并开始录音
            CreateCaptureBuffer();
            // 建立通知消息,当缓冲区满的时候处理方法
            InitNotifications();
            mRecBuffer.Start(true);
        }

        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopRec()
        {
            // 关闭通知消息
            if (null != mNotificationEvent)
                mNotificationEvent.Set();
            // 停止录音
            mRecBuffer.Stop();
            // 写入缓冲区最后的数据
            RecordCapturedData();

            // 回写长度信息
            mWriter.Seek(4, SeekOrigin.Begin);
            mWriter.Write((int)(mSampleCount + 36));   // 写文件长度
            mWriter.Seek(40, SeekOrigin.Begin);
            mWriter.Write(mSampleCount);                // 写数据长度

            mWriter.Close();
            mWaveFile.Close();
            mWriter = null;
            mWaveFile = null;

            // 关闭线程
            if (mNotifyThread != null)
            {
                mNotifyThread.Abort();
            }
        }

        /// <summary>
        /// 初始化录音设备,此处使用主录音设备.
        /// </summary>
        /// <returns>调用成功返回true,否则返回false</returns>
        bool InitCaptureDevice()
        {
            // 获取默认音频捕捉设备
            CaptureDevicesCollection devices = new CaptureDevicesCollection(); // 枚举音频捕捉设备
            Guid deviceGuid = Guid.Empty;                                       // 音频捕捉设备的ID
            if (devices.Count > 0)
                deviceGuid = devices[0].DriverGuid;
            else
            {
                MessageBox.Show("系统中没有音频捕捉设备");
                return false;
            }
            // 用指定的捕捉设备创建Capture对象
            try
            {
                mCapDev = new Capture(deviceGuid);
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 创建录音格式,此处使用16bit,16KHz,Mono的录音格式
        /// </summary>
        /// <returns>WaveFormat结构体</returns>
        private WaveFormat CreateWaveFormat()
        {
            WaveFormat format = new WaveFormat();
            format.FormatTag = WaveFormatTag.Pcm;   // PCM
            format.SamplesPerSecond = 16000;        // 16KHz
            format.BitsPerSample = 16;              // 16Bit
            format.Channels = 1;                    // Mono
            format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
            format.AverageBytesPerSecond = format.BlockAlign * format.SamplesPerSecond;
            return format;
        }

        /// <summary>
        /// 创建录音使用的缓冲区
        /// </summary>
        private void CreateCaptureBuffer()
        {
            // 缓冲区的描述对象
            CaptureBufferDescription bufferdescription = new CaptureBufferDescription();
            if (null != mNotify)
            {
                mNotify.Dispose();
                mNotify = null;
            }
            if (null != mRecBuffer)
            {
                mRecBuffer.Dispose();
                mRecBuffer = null;
            }
            // 设定通知的大小,默认为1s钟
            mNotifySize = (1024 > mWavFormat.AverageBytesPerSecond / 8) ? 1024 : (mWavFormat.AverageBytesPerSecond / 8);
            mNotifySize -= mNotifySize % mWavFormat.BlockAlign;
            // 设定缓冲区大小
            mBufferSize = mNotifySize * cNotifyNum;
            // 创建缓冲区描述           
            bufferdescription.BufferBytes = mBufferSize;
            bufferdescription.Format = mWavFormat;           // 录音格式
            // 创建缓冲区
            mRecBuffer = new CaptureBuffer(bufferdescription, mCapDev);
            mNextCaptureOffset = 0;
        }

        /// <summary>
        /// 初始化通知事件,将原缓冲区分成16个缓冲队列,在每个缓冲队列的结束点设定通知点.
        /// </summary>
        /// <returns>是否成功</returns>
        private bool InitNotifications()
        {
            if (null == mRecBuffer)
            {
                MessageBox.Show("未创建录音缓冲区");
                return false;
            }
            // 创建一个通知事件,当缓冲队列满了就激发该事件.
            mNotificationEvent = new AutoResetEvent(false);
            // 创建一个线程管理缓冲区事件
            if (null == mNotifyThread)
            {
                mNotifyThread = new Thread(new ThreadStart(WaitThread));
                mNotifyThread.Start();
            }
            // 设定通知的位置
            BufferPositionNotify[] PositionNotify = new BufferPositionNotify[cNotifyNum + 1];
            for (int i = 0; i < cNotifyNum; i++)
            {
                PositionNotify[i].Offset = (mNotifySize * i) + mNotifySize - 1;
                PositionNotify[i].EventNotifyHandle = mNotificationEvent.Handle;
            }
            mNotify = new Notify(mRecBuffer);
            mNotify.SetNotificationPositions(PositionNotify, cNotifyNum);
            return true;
        }

        /// <summary>
        /// 将录制的数据写入wav文件
        /// </summary>
        private void RecordCapturedData()
        {
            byte[] CaptureData = null;
            int ReadPos;
            int CapturePos;
            int LockSize;
            mRecBuffer.GetCurrentPosition(out CapturePos, out ReadPos);
            LockSize = ReadPos - mNextCaptureOffset;
            if (LockSize < 0)
                LockSize += mBufferSize;
            // 对齐缓冲区边界,实际上由于开始设定完整,这个操作是多余的.
            LockSize -= (LockSize % mNotifySize);
            if (0 == LockSize)
                return;

            // 读取缓冲区内的数据
            CaptureData = (byte[])mRecBuffer.Read(mNextCaptureOffset, typeof(byte), LockFlag.None, LockSize);
            // 写入Wav文件
            mWriter.Write(CaptureData, 0, CaptureData.Length);
            // 更新已经录制的数据长度.
            mSampleCount += CaptureData.Length;
            // 移动录制数据的起始点,通知消息只负责指示产生消息的位置,并不记录上次录制的位置
            mNextCaptureOffset += CaptureData.Length;
            mNextCaptureOffset %= mBufferSize; // Circular buffer
        }

        /// <summary>
        /// 接收缓冲区满消息的处理线程
        /// </summary>
        private void WaitThread()
        {
            while (true)
            {
                // 等待缓冲区的通知消息
                mNotificationEvent.WaitOne(Timeout.Infinite, true);
                // 录制数据
                RecordCapturedData();
            }
        }

        /// <summary>
        /// 创建保存的波形文件,并写入必要的文件头.
        /// </summary>
        private void CreateSoundFile()
        {
            /**************************************************************************
        Here is where the file will be created. A
        wave file is a RIFF file, which has chunks
        of data that describe what the file contains.
        A wave RIFF file is put together like this:
        The 12 byte RIFF chunk is constructed like this:
        Bytes 0 - 3 : 'R' 'I' 'F' 'F'
        Bytes 4 - 7 : Length of file, minus the first 8 bytes of the RIFF description.
                           (4 bytes for "WAVE" + 24 bytes for format chunk length +
                           8 bytes for data chunk description + actual sample data size.)
        Bytes 8 - 11: 'W' 'A' 'V' 'E'
        The 24 byte FORMAT chunk is constructed like this:
        Bytes 0 - 3 : 'f' 'm' 't' ' '
        Bytes 4 - 7 : The format chunk length. This is always 16.
        Bytes 8 - 9 : File padding. Always 1.
        Bytes 10- 11: Number of channels. Either 1 for mono, or 2 for stereo.
        Bytes 12- 15: Sample rate.
        Bytes 16- 19: Number of bytes per second.
        Bytes 20- 21: Bytes per sample. 1 for 8 bit mono, 2 for 8 bit stereo or
                          16 bit mono, 4 for 16 bit stereo.
        Bytes 22- 23: Number of bits per sample.
        The DATA chunk is constructed like this:
        Bytes 0 - 3 : 'd' 'a' 't' 'a'
        Bytes 4 - 7 : Length of data, in bytes.
        Bytes 8 -...: Actual sample data.
                    ***************************************************************************/
            // Open up the wave file for writing.
            mWaveFile = new FileStream(mFileName, FileMode.Create);
            mWriter = new BinaryWriter(mWaveFile);
            // Set up file with RIFF chunk info.
            char[] ChunkRiff = { 'R', 'I', 'F', 'F' };
            char[] ChunkType = { 'W', 'A', 'V', 'E' };
            char[] ChunkFmt = { 'f', 'm', 't', ' ' };
            char[] ChunkData = { 'd', 'a', 't', 'a' };

            short shPad = 1;                // File padding
            int nFormatChunkLength = 0x10; // Format chunk length.
            int nLength = 0;                // File length, minus first 8 bytes of RIFF description. This will be filled in later.
            short shBytesPerSample = 0;     // Bytes per sample.
            // 一个样本点的字节数目
            if (8 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels)
                shBytesPerSample = 1;
            else if ((8 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels) || (16 == mWavFormat.BitsPerSample && 1 == mWavFormat.Channels))
                shBytesPerSample = 2;
            else if (16 == mWavFormat.BitsPerSample && 2 == mWavFormat.Channels)
                shBytesPerSample = 4;
            // RIFF 块
            mWriter.Write(ChunkRiff);
            mWriter.Write(nLength);
            mWriter.Write(ChunkType);
            // WAVE块
            mWriter.Write(ChunkFmt);
            mWriter.Write(nFormatChunkLength);
            mWriter.Write(shPad);
            mWriter.Write(mWavFormat.Channels);
            mWriter.Write(mWavFormat.SamplesPerSecond);
            mWriter.Write(mWavFormat.AverageBytesPerSecond);
            mWriter.Write(shBytesPerSample);
            mWriter.Write(mWavFormat.BitsPerSample);

            // 数据块
            mWriter.Write(ChunkData);
            mWriter.Write((int)0);   // The sample length will be written in later.
        }
    }
}