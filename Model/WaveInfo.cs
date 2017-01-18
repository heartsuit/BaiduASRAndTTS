namespace SpeechProcessing.Model
{
    /// <summary>
    /// WAV信息结构体
    /// </summary>
    public struct WavInfo
    {
        public string groupid;
        public string rifftype;
        public long filesize;
        public string chunkid;
        public long chunksize;
        public short wformattag; //记录着此声音的格式代号，例如WAVE_FORMAT_PCM，WAVE_F0RAM_ADPCM等等。
        public ushort wchannels; //记录声音的频道数。
        public ulong dwsamplespersec;//记录每秒采样率。 16000
        public ulong dwavgbytespersec;//记录每秒的数据量。
        public ushort wblockalign;//记录区块的对齐单位。
        public ushort wbitspersample;//记录每个取样所需的位元数。 位深16
        public string datachunkid;
        public long datasize;
    }
}
