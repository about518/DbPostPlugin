using Grabacr07.KanColleWrapper;
using Livet;
using Nekoxy;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace DBPost
{
    public class SvDataRaw
    {
        public string RequestBody;
        public string ResponseBody;
        public string EndPoint;
        public SvDataRaw(string reqBody, string resBody, string endPoint)
        {
            RequestBody = reqBody;
            ResponseBody = resBody;
            EndPoint = endPoint;
        }

        public static SvDataRaw Parse(Session session)
        {
            var reqBody = session.Response.BodyAsString.Replace("svdata=", "");
            var result = new SvDataRaw(session.Request.BodyAsString, reqBody, session.Request.PathAndQuery);
            return result;
        }

        public static bool TryParse(Session session, out SvDataRaw result)
        {
            try
            {
                result = Parse(session);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                result = null;
                return false;
            }

            return true;
        }

        public void SendDb()
        {
            if( !Properties.Settings.Default.IsSendDb || string.IsNullOrEmpty(Properties.Settings.Default.DbAccessKey) )
            {
                return;
            }
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                NameValueCollection post = new NameValueCollection();
                post.Add("token", Properties.Settings.Default.DbAccessKey);
                post.Add("agent", "byCEC6DSrXrTrKcADwT3");  // このクライアントのエージェントキー
                post.Add("url", EndPoint);
                string requestBody = System.Text.RegularExpressions.Regex.Replace(RequestBody, @"&api(_|%5F)token=[0-9a-f]+|api(_|%5F)token=[0-9a-f]+&?", "");  // api_tokenを送信しないように削除
                post.Add("requestbody", requestBody);
                post.Add("responsebody", ResponseBody);
                wc.UploadValuesAsync(new Uri("http://api.kancolle-db.net/2/"), post);
#if DEBUG
                Debug.WriteLine("==================================================");
                Debug.WriteLine("Send to KanColle statistics database");
                Debug.WriteLine("url: " + EndPoint);
                Debug.WriteLine("reqBody: " + requestBody);
                Debug.WriteLine("resBody: " + ResponseBody);
                Debug.WriteLine("==================================================");
#endif
            }
        }
    }

    public static class KanColleProxyExtensions
    {
        /// <summary>
        /// Nekoxy でフックした <see cref="Session" /> オブジェクトの <see cref="Session.Response" /> データを
        /// <see cref="SvDataRaw" /> 型にパースします。
        /// </summary>
        public static IObservable<SvDataRaw> Parse(this IObservable<Session> source)
        {
            Func<Session, SvDataRaw> converter = session =>
            {
                SvDataRaw result;
                return SvDataRaw.TryParse(session, out result) ? result : null;
            };

            return source.Select(converter).Do(s => s.SendDb());
        }
    }


    public class PostData : NotificationObject
    {
        public PostData(KanColleProxy proxy, string endPoint)
        {
            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == endPoint).Parse().Subscribe();
        }
    }
}
