﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MORT
{
    class TransManager
    {
        private static TransManager instance;
        public static TransManager Instace
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransManager();
                }
                return instance;
            }
        }

        public class NaverKeyData
        {

            public enum eState
            {
                Normal, Error, Limit,
            }
            public string id;
            public string secret;
            public eState eNMTstate = NaverKeyData.eState.Normal;

            public NaverKeyData(string id, string secret)
            {
                this.id = id;
                this.secret = secret;
            }

            public void SetState(NaverKeyData.eState state, string apiType)
            {
                this.eNMTstate = state;

            }
        }

        public const int MAX_NAVER = 15;
        public GSTrans.Sheets sheets;

        public string googleKey;

        public List<string> transCodeList = new List<string>();             //얀덱스
        public List<string> resultCodeList = new List<string>();

        public int currentNaverIndex;
        public List<NaverKeyData> naverKeyList = new List<NaverKeyData>();
        public List<string> naverTransCodeList = new List<string>();
        public List<string> naverResultCodeList = new List<string>();

        public List<string> googleTransCodeList = new List<string>();
        public List<string> googleResultCodeList = new List<string>();

        public Dictionary<string, string> formerResultDic = new Dictionary<string, string>();

        /// <summary>
        /// 이전에 기록한 결과 제거.
        /// </summary>
        public void ClearFormerDic()
        {
            formerResultDic.Clear();
        }

        public void InitGtrans(string sheetID, string clientID, string secretKey, string source, string result)
        {
            if (sheets == null)
            {
                sheets = new GSTrans.Sheets();
            }
            googleKey = sheetID;
            sheets.spreadsheetId = @sheetID;// @"1k4dlDiXjuJnIS0K1EYuMxB40f_cZP3t0sGtS5cv3J3I";


            bool isComplete = sheets.Init(@sheetID, clientID, secretKey);

            //sheets.RowCount = 30;
            //초기화 - 반드시 해줘야 함 - 시트가 제대로 준비되었는지 확인하고, 준비되지 않았을 경우 셋팅합니다.
            //sheets.Initialize();

            //초기화 후 언제나 변경 가능한 변수 설정
            sheets.source = source;
            sheets.target = result;

            if (!isComplete && clientID != "")
            {
                SettingManager.isErrorEmptyGoogleToken = true;
            }
        }

        public void InitGTransToken()
        {
            if (sheets != null)
            {
                sheets.InitToken();
            }
        }

        public void DeleteAllGsTransToken()
        {
            if (sheets != null)
            {
                sheets.DeleteToken();
            }
        }

        public async Task<string> StartTrans(string text, SettingManager.TransType trasType)
        {
            if (text == "")
            {
                return "";
            }

            Task<string> task1 = Task<string>.Run(() => GetTrans2(text, trasType));
            string result = await task1;

            return result;

        }


        public async Task<string> GetTrans2(string text, SettingManager.TransType trasType)
        {
            try
            {
                bool isError = false;

                bool isContain = false;

                if (trasType != SettingManager.TransType.db)
                {
                    //text = text.Replace(System.Environment.NewLine, " ");
                 
                    isContain = formerResultDic.ContainsKey(text);
                }

                string result = "";

                if (!isContain)
                {
                    //trasType = SettingManager.TransType.google;
                    if (trasType == SettingManager.TransType.db)
                    {
                
                        StringBuilder sb = new StringBuilder(text, 8192);
                        StringBuilder sb2 = new StringBuilder(8192);
                        Form1.ProcessGetDBText(sb, sb2);
                        result = sb2.ToString();
                    }
                    else
                    {
                   
                        /*
                         * 2020 년 8월 15일 이후로 지원하지 않음.
                        if (trasType == SettingManager.TransType.yandex)
                        {
                            result = YandexAPI.instance.GetResult(text, ref isError);
                        }
                        */
                        if (trasType == SettingManager.TransType.naver)
                        {
                            result = NaverTranslateAPI.instance.GetResult(text, ref isError);
                            result = result.Replace("\r\n ", System.Environment.NewLine);
                        }
                        else if (trasType == SettingManager.TransType.google)
                        {
                            result = sheets.Translate(text, ref isError);
                            result = result.Replace("\r\n ", System.Environment.NewLine);
                        }
                        else if (trasType == SettingManager.TransType.google_url)
                        {
                            result = GoogleBasicTranslateAPI.instance.GetResult(text, ref isError);
                        }
                    }
                 

                    if (!isError && trasType != SettingManager.TransType.db)
                    {
                        formerResultDic.Add(text, result);

                        if (formerResultDic.Count > 5000)
                        {
                            formerResultDic.Clear();
                        }
                    }
                }
                else
                {
                    result = formerResultDic[text];
                }





                return result;
            }
            catch (Exception e)
            {
                return "Error " + e;
            }
        }

        public static bool GetIsRemain()
        {
            bool isRemain = true;

            if (instance == null)
            {
                isRemain = false;
            }

            return isRemain;
        }

        public void InitTransCode()
        {
            //TODO : 코드와 콤보박스 모두 설정할 수 있도록 변경해야 한다.
            transCodeList.Add("en");
            transCodeList.Add("ja");
            transCodeList.Add("zh-CHS");
            transCodeList.Add("zh-CHT");
            transCodeList.Add("ko");
            transCodeList.Add("ru");
            transCodeList.Add("de");
            transCodeList.Add("pt");
            transCodeList.Add("es");
            transCodeList.Add("fr");
            transCodeList.Add("vi");
            transCodeList.Add("th");
     

            resultCodeList.Add("ko");
            resultCodeList.Add("en");
            resultCodeList.Add("ja");
            resultCodeList.Add("zh-CHS");
            resultCodeList.Add("zh-CHT");
            resultCodeList.Add("ru");
            resultCodeList.Add("de");
            resultCodeList.Add("pt");
            resultCodeList.Add("es");
            resultCodeList.Add("fr");
            resultCodeList.Add("vi");
            resultCodeList.Add("th");
        

            naverTransCodeList.Add("en");
            naverTransCodeList.Add("ja");
            naverTransCodeList.Add("zh-CN");
            naverTransCodeList.Add("zh-TW");
            naverTransCodeList.Add("es");
            naverTransCodeList.Add("fr");
            naverTransCodeList.Add("vi");
            naverTransCodeList.Add("th");
            naverTransCodeList.Add("id");
            naverTransCodeList.Add("ko");



            naverResultCodeList.Add("ko");
            naverResultCodeList.Add("en");


            googleTransCodeList.Add("en");
            googleTransCodeList.Add("ja");
            googleTransCodeList.Add("zh-CN");
            googleTransCodeList.Add("zh-TW");
            googleTransCodeList.Add("ko");
            googleTransCodeList.Add("ru");
            googleTransCodeList.Add("de");
            googleTransCodeList.Add("pt-BR");
            googleTransCodeList.Add("pt-PT");
            googleTransCodeList.Add("es");
            googleTransCodeList.Add("fr");
            googleTransCodeList.Add("vi");
            googleTransCodeList.Add("th");

            googleResultCodeList.Add("ko");
            googleResultCodeList.Add("en");
            googleResultCodeList.Add("ja");
            googleResultCodeList.Add("zh-CN");
            googleResultCodeList.Add("zh-TW");
            googleResultCodeList.Add("ru");
            googleResultCodeList.Add("de");
            googleResultCodeList.Add("pt-BR");
            googleResultCodeList.Add("pt-PT");
            googleResultCodeList.Add("es");
            googleResultCodeList.Add("fr");
            googleResultCodeList.Add("vi");
            googleResultCodeList.Add("th");
        }


        public void SetState(NaverKeyData.eState state)
        {

            if (naverKeyList.Count == 0)
            {

            }
            else
            {

                if (naverKeyList.Count > currentNaverIndex)
                {
                    naverKeyList[currentNaverIndex].SetState(state, NaverTranslateAPI.instance.GetAPIType());
                }

            }
        }

        public NaverKeyData GetNextNaverKey()
        {
            NaverKeyData data = null;

            if (naverKeyList.Count == 0)
            {
                data = new NaverKeyData("", "");
            }
            else
            {
                currentNaverIndex++;
                if (currentNaverIndex >= naverKeyList.Count || currentNaverIndex == MAX_NAVER + 1)
                {
                    currentNaverIndex = 0;
                }

                data = naverKeyList[currentNaverIndex];
            }

            return data;
        }

        public void OpenNaverKeyFile()
        {
            currentNaverIndex = 0;
            try
            {
                StreamReader r = new StreamReader(GlobalDefine.NAVER_ACCOUNT_FILE);

                string line;

                Dictionary<string, NaverKeyData> dataDic = new Dictionary<string, NaverKeyData>();

                for (int i = 0; i < naverKeyList.Count; i++)
                {
                    if (dataDic.ContainsKey(naverKeyList[i].id))
                    {
                        dataDic.Add(naverKeyList[i].id, naverKeyList[i]);
                    }
                }
                naverKeyList.Clear();
                while ((line = r.ReadLine()) != null)
                {
                    string id = line;
                    string secret = "";
                    line = r.ReadLine();
                    if (line != null)
                    {
                        secret = line;


                        if (dataDic.ContainsKey(id) && dataDic[id].secret == secret)
                        {
                            naverKeyList.Add(dataDic[id]);
                        }
                        else
                        {
                            NaverKeyData data = new NaverKeyData(id, secret);
                            naverKeyList.Add(data);
                        }

                    }
                    else
                    {
                        break;
                    }
                }


                for (int i = 0; i < naverKeyList.Count; i++)
                {
                    Util.ShowLog("id : " + naverKeyList[i].id + " / secret : " + naverKeyList[i].secret);
                }

                r.Close();
                r.Dispose();

            }
            catch (FileNotFoundException)
            {
                using (System.IO.FileStream fs = System.IO.File.Create(GlobalDefine.NAVER_ACCOUNT_FILE))
                {
                    fs.Close();
                    fs.Dispose();

                }
            }
        }


        public void SaveNaverKeyFile(string id, string secret)
        {

            id = id.Replace(" ", "");
            secret = secret.Replace(" ", "");
            try
            {
                using (StreamWriter newTask = new StreamWriter(GlobalDefine.NAVER_ACCOUNT_FILE, false))
                {

                    newTask.WriteLine(id);
                    newTask.WriteLine(secret);

                    //첫 번째는 넘김.
                    for (int i = 1; i < naverKeyList.Count; i++)
                    {
                        newTask.WriteLine(naverKeyList[i].id);
                        newTask.WriteLine(naverKeyList[i].secret);
                    }

                    if (naverKeyList.Count > 0)
                    {
                        naverKeyList[0].id = id;
                        naverKeyList[0].secret = secret;
                    }
                    else
                    {
                        naverKeyList.Add(new NaverKeyData(id, secret));
                    }
                    newTask.Close();
                }


            }
            catch (FileNotFoundException)
            {
                using (System.IO.FileStream fs = System.IO.File.Create(GlobalDefine.NAVER_ACCOUNT_FILE))
                {
                    fs.Close();
                    fs.Dispose();
                    using (StreamWriter newTask = new StreamWriter(GlobalDefine.NAVER_ACCOUNT_FILE, false))
                    {
                        newTask.WriteLine(id);
                        newTask.WriteLine(secret);

                        for (int i = 1; i < naverKeyList.Count; i++)
                        {
                            newTask.WriteLine(naverKeyList[i].id);
                            newTask.WriteLine(naverKeyList[i].secret);
                        }

                        if (naverKeyList.Count > 0)
                        {
                            naverKeyList[0].id = id;
                            naverKeyList[0].secret = secret;
                        }
                        else
                        {
                            naverKeyList.Add(new NaverKeyData(id, secret));
                        }

                        newTask.Close();
                    }
                }
            }
        }

        public NaverKeyData GetNaverKey()
        {
            NaverKeyData data = null;

            if (naverKeyList.Count > 0)
            {
                data = naverKeyList[0];
            }
            else
            {
                data = new NaverKeyData("", "");
            }

            return data;
        }


    }
}
