using NAudio.Wave;
using SpeechProcessing.Model;
using System;
using System.IO;

namespace SpeechProcessing
{
    class ClassUtils
    {
        /// <summary>
        /// 将当前时间转为秒
        /// </summary>
        /// <returns>秒数</returns>
        public static long CurrentTime2Second()
        {
            string currentTime = DateTime.Now.ToString();
            DateTime dt = new DateTime(1970, 1, 1);
            TimeSpan d = DateTime.Parse(currentTime) - dt;
            long totalSeconds = d.Ticks / 10000000; // turn current time to seconds

            return totalSeconds;
        }

        /// <summary>
        /// 将.mp3或者其他.wav文件转为16kHz，16bit的.wav（by NAudio）
        /// </summary>
        /// <param name="filePath">转换前音频文件的路径</param>
        /// <returns>转换后音频文件的路径</returns>
        public static string Convert2Wav(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string tempDir = directoryName + "\\temp" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\\";

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            if (filePath.EndsWith(".wav", StringComparison.CurrentCultureIgnoreCase))
            {
                using (var reader = new WaveFileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        WaveFileWriter.CreateWaveFile(tempDir + fileName, conversionStream);
                    }
                }
            }
            else if (filePath.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase))
            {
                using (Mp3FileReader reader = new Mp3FileReader(filePath))
                {
                    var newFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16bit
                    using (var conversionStream = new WaveFormatConversionStream(newFormat, reader))
                    {
                        WaveFileWriter.CreateWaveFile(tempDir + fileName, conversionStream);
                    }
                }
            }

            return tempDir + fileName;
        }

        /// <summary>
        /// 取出WAV头信息
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static WavInfo GetWavInfo(string filePath)
        {
            WavInfo wavInfo = new WavInfo();
            FileInfo fi = new FileInfo(filePath);
            using (System.IO.FileStream fs = fi.OpenRead())
            {
                if (fs.Length >= 44)
                {
                    byte[] bInfo = new byte[44];
                    fs.Read(bInfo, 0, 44);
                    System.Text.Encoding.Default.GetString(bInfo, 0, 4);
                    if (System.Text.Encoding.Default.GetString(bInfo, 0, 4) == "RIFF" && System.Text.Encoding.Default.GetString(bInfo, 8, 4) == "WAVE" && System.Text.Encoding.Default.GetString(bInfo, 12, 4) == "fmt ")
                    {
                        wavInfo.groupid = System.Text.Encoding.Default.GetString(bInfo, 0, 4);
                        System.BitConverter.ToInt32(bInfo, 4);
                        wavInfo.filesize = System.BitConverter.ToInt32(bInfo, 4);
                        //wavInfo.filesize = Convert.ToInt64(System.Text.Encoding.Default.GetString(bInfo,4,4));
                        wavInfo.rifftype = System.Text.Encoding.Default.GetString(bInfo, 8, 4);
                        wavInfo.chunkid = System.Text.Encoding.Default.GetString(bInfo, 12, 4);
                        wavInfo.chunksize = System.BitConverter.ToInt32(bInfo, 16);
                        wavInfo.wformattag = System.BitConverter.ToInt16(bInfo, 20);
                        wavInfo.wchannels = System.BitConverter.ToUInt16(bInfo, 22);
                        wavInfo.dwsamplespersec = System.BitConverter.ToUInt32(bInfo, 24);
                        wavInfo.dwavgbytespersec = System.BitConverter.ToUInt32(bInfo, 28);
                        wavInfo.wblockalign = System.BitConverter.ToUInt16(bInfo, 32);
                        wavInfo.wbitspersample = System.BitConverter.ToUInt16(bInfo, 34);
                        wavInfo.datachunkid = System.Text.Encoding.Default.GetString(bInfo, 36, 4);
                        wavInfo.datasize = System.BitConverter.ToInt32(bInfo, 40);
                    }
                }
            }

            return wavInfo;
        }
    }
}
