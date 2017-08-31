using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace dumptrack
{
	public class Config
	{
		private static Config instance = null;
		private static String locker = "1";
		private Config()
		{
			APIKey = ConfigurationManager.AppSettings["api.key"];
			EnableProxy = ConfigurationManager.AppSettings["enable.proxy"] == "1" ? true : false;
			ProxyAddress = ConfigurationManager.AppSettings["proxy.address"];
			OffenceWatchlistPath = ConfigurationManager.AppSettings["offence.watchlist.path"];
			ProcessedUrlsPath = ConfigurationManager.AppSettings["processedurls.path"];
			CustomSearchEndpoint = ConfigurationManager.AppSettings["customsearch.endpoint"];
			CustomSearchIdentifier = ConfigurationManager.AppSettings["customsearch.identifier"];
			FromAddress = ConfigurationManager.AppSettings ["notification.FromAddress"];
			ToEmailString = ConfigurationManager.AppSettings ["notification.ToEmailString"];
			SMTPServer = ConfigurationManager.AppSettings ["notification.SMTPServer"];
			EnableGitHubSearch = ConfigurationManager.AppSettings["enable.githubsearch"] == "1" ? true : false;
			GitHubUsername = ConfigurationManager.AppSettings ["github.username"];
			GitHubPassword = ConfigurationManager.AppSettings ["github.password"];
			Debug = ConfigurationManager.AppSettings ["debug"] == "1" ? true : false;
		}

		public static Config getInstance()
		{
			if (instance == null)
			{
				lock (locker)
				{
					instance = new Config();
				}
			}
			return instance;
		}

		public string APIKey { get; set; }
		public bool EnableProxy { get; set; }
		public string ProxyAddress{ get;set; }
		public string OffenceWatchlistPath { get; set; }
		public string ProcessedUrlsPath { get; set; }
		public string CustomSearchEndpoint { get; set; }
		public string CustomSearchIdentifier { get; set; }
		public string FromAddress { get;set; }
		public string ToEmailString { get;set; }
		public string SMTPServer { get;set; }
		public bool EnableGitHubSearch { get; set; }
		public string GitHubUsername {get;set;}
		public string GitHubPassword { get; set; }
		public bool Debug{ get; set; }
    }
}
