using DBPost.Properties;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace DBPost
{
    [Export(typeof(IPlugin))]
    [Export(typeof(ISettings))]
    [ExportMetadata("Guid", "222896DD-9131-4929-A3B0-4B8BB82296D6")]
    [ExportMetadata("Title", "KanColleDbPost")]
    [ExportMetadata("Description", "艦これ統計データベースへ送信するプラグインです。")]
    [ExportMetadata("Version", "1.0")]
    [ExportMetadata("Author", "@about518")]
    public class DBPost : IPlugin, ISettings
    {
        object ISettings.View => new DBPostSettings { DataContext = this, };

        public bool IsSendDb
        {
            get { return Settings.Default.IsSendDb; }
            set
            {
                Settings.Default.IsSendDb = value;
                Settings.Default.Save();
            }
        }

        public string DbAccessKey
        {
            get { return Settings.Default.DbAccessKey; }
            set
            {
                Settings.Default.DbAccessKey = value;
                Settings.Default.Save();
            }
        }

        public ObservableCollection<PostData> postDatas;

        public static readonly string[] endPoints =
        {
            "/kcsapi/api_port/port",
            "/kcsapi/api_get_member/ship2",
            "/kcsapi/api_get_member/ship3",
            "/kcsapi/api_get_member/slot_item",
            "/kcsapi/api_get_member/kdock",
            "/kcsapi/api_get_member/mapinfo",
            "/kcsapi/api_req_hensei/change",
            "/kcsapi/api_req_kousyou/createship",
            "/kcsapi/api_req_kousyou/getship",
            "/kcsapi/api_req_kousyou/createitem",
            "/kcsapi/api_req_map/select_eventmap_rank",
            "/kcsapi/api_req_sortie/battleresult",
            "/kcsapi/api_req_combined_battle/battleresult",
        };
        public void Initialize()
        {
            postDatas = new ObservableCollection<PostData>();
            foreach(var endPoint in endPoints)
            {
                postDatas.Add(new PostData(KanColleClient.Current.Proxy, endPoint));
            }
        }
    }
}
