/* BaiDu API ASR-Doc: http://yuyin.baidu.com/docs/asr/
 *  语音识别接口支持 POST 方式
 *  目前 API 仅支持整段语音识别的模式，即需要上传整段语音进行识别
 *  语音数据上传方式有两种：隐式发送和显式发送
 *  原始语音的录音格式目前只支持评测 8k/16k 采样率 16bit 位深的单声道语音
 *  (经测试，转为8k的音频，识别正确率极低！！)
 *  压缩格式支持：pcm（不压缩）、wav、opus、speex、amr、x-flac
 *  系统支持语言种类：中文（zh）、粤语（ct）、英文（en）
 *  Note:
 *  1. 请严格按照文档里描述的参数进行开发，特别请关注原始录音参数以及语音压缩格式的建议，否则会影响识别率，进而影响到产品的用户体验。
 *  2. 目前系统支持的语音时长上限为60s，请不要超过这个长度，否则会返回错误。
*/
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using SpeechProcessing.Model;
using SpeechProcessing.Recorder;

namespace SpeechProcessing
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private string resultStr;

        private bool isRecord = false;
        private string accessToken = null;

        private SpeechModel speechModel = new SpeechModel();
        private AutomaticSpeechRecognition testASR;

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false; // disable maximize

            testASR = new AutomaticSpeechRecognition(speechModel);

            // Bind drag and drop event
            richTextBoxResult.AllowDrop = true;
            richTextBoxResult.DragEnter += new DragEventHandler(richTextBoxResult_DragEnter);
            richTextBoxResult.DragDrop += new DragEventHandler(richTextBoxResult_DragDrop);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // add data to comboBox
            List<KeyValuePair<string, string>> listItems = new List<KeyValuePair<string, string>>();
            listItems.Add(new KeyValuePair<string, string>("English", "en"));
            listItems.Add(new KeyValuePair<string, string>("中文", "zh"));
            listItems.Add(new KeyValuePair<string, string>("粤语", "ct"));
            comboBoxLan.DataSource = listItems;
            comboBoxLan.DisplayMember = "Key";
            comboBoxLan.ValueMember = "Value";
            comboBoxLan.SelectedIndex = 0;

            // binding the event to achieve Asynchronization
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);

            // obtain token from file, avoid too many requests on remote server
            if (!File.Exists(@".\token.dat"))
            {
                accessToken = testASR.GetStrAccess(); // token file does not exist, send a request
            }
            else
            {
                string[] tokenInfo = File.ReadAllLines(@".\token.dat");

                // check if the token has expired
                if (Convert.ToInt32(tokenInfo[1]) > ClassUtils.CurrentTime2Second())
                {
                    accessToken = tokenInfo[0];
                }
                else
                {
                    accessToken = testASR.GetStrAccess(); // expired, request again to refresh
                }
            }

            speechModel.APIAccessToken = accessToken; // update token in model

            // Show tips when mouse hovers
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(buttonRecognize, "Select a file to recognize");
            toolTip.SetToolTip(buttonRecord, "Record/Stop audio");
            toolTip.SetToolTip(buttonRead, "Start reading");
            toolTip.SetToolTip(buttonReadPause, "Pause reading");
            toolTip.SetToolTip(buttonReadStop, "Stop reading");
            toolTip.SetToolTip(textBoxFile, "Click to select a audio file");
            toolTip.SetToolTip(comboBoxLan, "Select the target language");
            toolTip.SetToolTip(richTextBoxResult, "Recognition text or Text to be Read");
        }

        /// <summary>
        /// 选择待识别音频文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxFile_Click(object sender, EventArgs e)
        {
            // select an audio file to recognize
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"D:\";
            openFileDialog.Filter = "|*.wav;*.mp3";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFile.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 启动识别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRecognize_Click(object sender, EventArgs e)
        {
            if (textBoxFile.Text == "")
            {
                labelInfo.ForeColor = Color.HotPink;
                labelInfo.Text = "Error: Please select an audio file!";
                return;
            }
            else
            {
                speechModel.APIRecord = textBoxFile.Text;
                StartRecognize(speechModel.APIRecord);
            }
        }

        private string tempStr = ""; // temporary directory to store the converted audio file

        private void StartRecognize(string apiRecord)
        {
            WavInfo wav = ClassUtils.GetWavInfo(apiRecord);

            //数据量 = (采样频率 × 采样位数 × 声道数 × 时间) / 8
            //if ((double)(wav.datasize * 8) / (wav.dwsamplespersec * wav.wbitspersample * wav.wchannels) > 60)
            //{
            //    labelInfo.ForeColor = Color.HotPink;
            //    labelInfo.Text = "Error: The audio file is too large!";
            //}

            // 非8k/16k, 16bit 位深, 单声道的，进行格式转换
            if (apiRecord.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase)
                || int.Parse(wav.dwsamplespersec.ToString()) != 16000
                || int.Parse(wav.wbitspersample.ToString()) != 16
                || int.Parse(wav.wchannels.ToString()) != 1)
            {
                apiRecord = ClassUtils.Convert2Wav(apiRecord); // convert audio file to 16k，16bit wav
                tempStr = apiRecord;
            }

            labelInfo.ForeColor = Color.SpringGreen;
            labelInfo.Text = "Recognizing...";
            KeyValuePair<string, string> keyVal = (KeyValuePair<string, string>)comboBoxLan.SelectedItem;
            speechModel.APILanguage = keyVal.Value; // fetch the value in comboBox

            if (backgroundWorker.IsBusy != true)
            {
                this.backgroundWorker.RunWorkerAsync(); // do the time consuming task
            }
        }

        #region Asynchronous work
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // indicate that this is recorded audio
            if (isRecord)
            {
                speechModel.APIRecord = System.Environment.CurrentDirectory + @"\record.wav";
                isRecord = false;
            }

            // indicate that conversion work has been done
            if (tempStr != "")
            {
                speechModel.APIRecord = tempStr;
            }
            resultStr = testASR.GetStrText() + "\r\n";
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.richTextBoxResult.Text += resultStr;
            labelInfo.ForeColor = Color.SpringGreen;
            labelInfo.Text = "Recognize completed!";

            // finish recognizing, delete temporary directory
            if (tempStr != "")
            {
                Directory.Delete(Path.GetDirectoryName(tempStr), true); // delete directory or sub-dir recursively
            }
        }
        #endregion

        #region Record audio
        //private ISpeechRecorder recorder = new DirectRecorder(); // under .Net Framework 2.0 or .Net Framework 3.5
        private ISpeechRecorder recorder = new NAudioRecorder();
        private bool switchRecord = true;

        /// <summary>
        /// 开始录音/停止录音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRecord_Click(object sender, EventArgs e)
        {
            if (switchRecord)
            {
                switchRecord = false; // switch the record status
                buttonRecord.BackgroundImage = (Bitmap)Properties.Resources.ResourceManager.GetObject("record_stop");

                if (recorder == null)
                {
                    recorder = new NAudioRecorder();
                }
                recorder.SetFileName("record.wav");
                recorder.StartRec();
                labelInfo.ForeColor = Color.SpringGreen;
                labelInfo.Text = "Record: Recording.";
            }
            else
            {
                switchRecord = true;
                buttonRecord.BackgroundImage = (Bitmap)Properties.Resources.ResourceManager.GetObject("recording");

                if (recorder == null)
                {
                    return;
                }
                isRecord = true;
                recorder.StopRec();
                recorder = null;
                string filePath = Environment.CurrentDirectory + @"\record.wav";
                StartRecognize(filePath);
            }
        }
        #endregion

        #region TTS and Audio Control: Play, Pause and Stop
        private const int NULL = 0, ERROR_SUCCESS = NULL;
        [DllImport("WinMm.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

        private bool isPaused = false;

        private void buttonRead_Click(object sender, EventArgs e)
        {
            string text = richTextBoxResult.Text;

            // avoid blank here in the textbox
            if (text.Trim() == "")
            {
                labelInfo.ForeColor = Color.HotPink;
                labelInfo.Text = "Error: Please input or import some text first!";
                return;
            }

            Random r = new Random();
            do
            {
                //0为女声，1为男声，3为情感合成-度逍遥，4为情感合成-度丫丫
                speechModel.APIPerson = r.Next(0, 5).ToString(); // set the person randomly
            } while (speechModel.APIPerson == "2");

            speechModel.APILanguage = "zh"; // target language is Chinese by default

            //TTS: Text To Speech
            //string requestStr = string.Format("http://tsn.baidu.com/text2audio?tex={0}&lan={1}&per={2}&ctp={3}&cuid={4}&tok={5}&spd={6}&pit={7}&vol={8}",
            //    text, speechModel.APILanguage, speechModel.APIPerson, speechModel.APIClientType, speechModel.APIID, speechModel.APIAccessToken, speechModel.APISpeed, speechModel.APIPitch, speechModel.APIVolume);
            string requestStr = $"http://tsn.baidu.com/text2audio?tex={text}&lan={speechModel.APILanguage}&per={speechModel.APIPerson}&ctp={speechModel.APIClientType}&cuid={speechModel.APIID}&tok={speechModel.APIAccessToken}&spd={speechModel.APISpeed}&pit={speechModel.APIPitch}&vol={speechModel.APIVolume}";

            var resq = WebRequest.Create(requestStr);
            using (var req = resq.GetResponse())
            {
                if (req.ContentType == "audio/mp3")
                {
                    if (isPaused)
                    {
                        mciSendString("resume audio", null, NULL, NULL);
                        labelInfo.ForeColor = Color.SpringGreen;
                        labelInfo.Text = "Read: Resume.";
                    }
                    else
                    {
                        mciSendString("close audio", null, NULL, NULL);

                        string strFileName = Application.StartupPath + "/read.mp3";
                        using (FileStream fs = new FileStream(strFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                            req.GetResponseStream().CopyTo(fs); // Stream.CopyTo() is included from .Net 4.0

                        if (mciSendString(string.Format("open \"{0}\" type mpegvideo alias audio", strFileName), null, NULL, NULL) == ERROR_SUCCESS)
                        {
                            mciSendString("open \"" + strFileName + "\" type mpegvideo alias audio", null, NULL, NULL);
                            mciSendString("play audio", null, NULL, NULL);
                            labelInfo.ForeColor = Color.SpringGreen;
                            labelInfo.Text = "Read: Playing.";
                        }
                    }
                }
                else
                {
                    using (StreamReader strHttpResponse = new StreamReader(req.GetResponseStream(), Encoding.UTF8))
                    {
                        richTextBoxResult.Text = strHttpResponse.ReadToEnd();
                    }
                }
            }
        }

        private void buttonReadPause_Click(object sender, EventArgs e)
        {
            mciSendString("pause audio", null, NULL, NULL);
            isPaused = true;
            labelInfo.ForeColor = Color.SpringGreen;
            labelInfo.Text = "Read: Paused.";
        }

        private void buttonReadStop_Click(object sender, EventArgs e)
        {
            mciSendString("close audio", null, NULL, NULL);
            isPaused = false;
            labelInfo.ForeColor = Color.SpringGreen;
            labelInfo.Text = "Read: Stopped.";
        }
        #endregion

        #region Support drag and drop file
        private void richTextBoxResult_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void richTextBoxResult_DragDrop(object sender, DragEventArgs e)
        {
            Array arrayFileName = (Array)e.Data.GetData(DataFormats.FileDrop);

            string strFileName = arrayFileName.GetValue(0).ToString();

            StreamReader sr = new StreamReader(strFileName, Encoding.UTF8);
            richTextBoxResult.Text = sr.ReadToEnd();
            sr.Close();
        }
        #endregion
    }
}