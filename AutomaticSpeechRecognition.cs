/*
 * Reference: 
 *      ASR: http://www.cnblogs.com/bfyx/p/3915297.html
 *           http://www.sufeinet.com/thread-9254-1-1.html
 *      TTS: http://www.cnblogs.com/geovindu/p/4995463.html
 *           http://www.th7.cn/Program/net/201507/508107.shtml
 */
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using SpeechProcessing.Model;

namespace SpeechProcessing
{
    class AutomaticSpeechRecognition
    {
        private SpeechModel speechModel;

        //ASR: Automatic Speech Recognition
        public AutomaticSpeechRecognition(SpeechModel speechModel)
        {
            this.speechModel = speechModel;
        }

        /// <summary>
        /// 获取百度认证口令码
        /// </summary>
        /// <returns>百度认证口令码: accessToken</returns>
        public string GetStrAccess()
        {
            string accessHtml = null;
            string accessToken = null;
            string[] accessTokenInfo = new string[2];

            // string getAccessUrl = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials" +
            //"&client_id=" + speechModel.APIKey + "&client_secret=" + speechModel.APISecretKey;
            string getAccessUrl = $"https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={speechModel.APIKey}&client_secret={speechModel.APISecretKey}";

            try
            {
                HttpWebRequest getAccessRequest = WebRequest.Create(getAccessUrl) as HttpWebRequest;
                getAccessRequest.ContentType = "multipart/form-data";
                getAccessRequest.Accept = "*/*";
                getAccessRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
                getAccessRequest.Timeout = 30000;   // timeout after 30s
                getAccessRequest.Method = "post";

                HttpWebResponse response = getAccessRequest.GetResponse() as HttpWebResponse;
                using (StreamReader strHttpResponse = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    accessHtml = strHttpResponse.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.ToString());
            }

            JObject jo = JObject.Parse(accessHtml);
            accessToken = jo["access_token"].ToString();   // parse to get token
            int expiresIn = 2592000; // expire in: 2592000, one month

            // record the token request time            
            long totalSeconds = ClassUtils.CurrentTime2Second();

            accessTokenInfo[0] = accessToken;
            accessTokenInfo[1] = (totalSeconds + expiresIn).ToString(); // the expired time

            // write the token information into file
            File.WriteAllLines(@".\token.dat", accessTokenInfo);
            return accessToken;

            /* JObject after parsing:
             * {{
                  "access_token": "24.fd8c2088ac28b2722403c1acc36797e9.2592000.1487243775.282335-8317833",
                  "session_key": "9mzdCSCKQicpJZhQpgi/4cz7biI1uBSCE5PlgR4wdEq4NErxkOJQA3uJq2sTjY7SSKK8J0rsxOD18B5ugOj7QClCxwDt",
                  "scope": "public audio_voice_assistant_get audio_tts_post wise_adapt lebo_resource_base lightservice_public hetu_basic lightcms_map_poi kaidian_kaidian",
                  "refresh_token": "25.68c6dc99cb375b786b030d156d51cccb.315360000.1782910269.282335-6432116",
                  "session_secret": "443304340f3b40e766006aa319732096",
                  "expires_in": 2592000
               }}
             */
        }

        /// <summary>
        /// 获取识别后的文本
        /// </summary>
        /// <returns>若正确：语音对应的文本；若错误：错误信息</returns>
        public string GetStrText()
        {
            //string getTextUrl = "http://vop.baidu.com/server_api?lan=" + speechModel.APILanguage + "&cuid=" + speechModel.APIID + "&token=" + speechModel.APIAccessToken;
            string getTextUrl = $"http://vop.baidu.com/server_api?lan={speechModel.APILanguage}&cuid={speechModel.APIID}&token={speechModel.APIAccessToken}";

            HttpWebRequest getTextRequst = WebRequest.Create(getTextUrl) as HttpWebRequest;

            getTextRequst.ContentType = "audio /" + speechModel.APIFormat + ";rate=" + speechModel.APIFrequency;
            getTextRequst.ContentLength = new FileInfo(speechModel.APIRecord).Length;
            getTextRequst.Method = "post";
            getTextRequst.Accept = "*/*";
            getTextRequst.KeepAlive = true;
            getTextRequst.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
            getTextRequst.Timeout = 30000;  // timeout after 30s

            FileStream fs = new FileStream(speechModel.APIRecord, FileMode.Open);
            byte[] voice = new byte[fs.Length];
            fs.Read(voice, 0, voice.Length);
            fs.Close();

            using (Stream writeStream = getTextRequst.GetRequestStream())
            {
                writeStream.Write(voice, 0, voice.Length);
            }

            HttpWebResponse getTextResponse;
            try
            {
                getTextResponse = getTextRequst.GetResponse() as HttpWebResponse;
            }
            catch (Exception)
            {
                return "Error: Audio file is too large!";
            }

            string strJSON = "";

            using (StreamReader strHttpText = new StreamReader(getTextResponse.GetResponseStream(), Encoding.UTF8))
            {
                strJSON = strHttpText.ReadToEnd();
            }

            JObject jsons = JObject.Parse(strJSON); //parse JSON string

            if (jsons["err_msg"].Value<string>() == "success.")
            {
                return jsons["result"][0].ToString();
            }
            else
            {
                string error = null;
                int errNum = jsons["err_no"].Value<int>();

                // Error message
                switch (errNum)
                {
                    case 3300:
                        error = "输入参数不正确！";
                        break;
                    case 3301:
                        error = "识别错误！";
                        break;
                    case 3302:
                        error = "验证失败！";
                        break;
                    case 3303:
                        error = "语音服务器后端问题！";
                        break;
                    case 3304:
                        error = "请求 GPS 过大，超过限额！";
                        break;
                    case 3305:
                        error = "产品线当前日请求数超过限额！";
                        break;
                    default:
                        error = "发生未知错误！";
                        break;
                }
                //error = jsons["err_no"].Value<string>() + jsons["err_msg"].Value<string>();
                return error;
            }
        }
    }
}
