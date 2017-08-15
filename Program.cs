using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using slackerchat;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dumptrack
{
	class MainClass
	{
		private static void saveData (string url, string urlData, string offence){
			using (StreamWriter writer = new StreamWriter("dumps-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt",true)){
				writer.WriteLine(offence + ":" + url);
				writer.WriteLine(urlData);
			}
		}

		private static bool hasResults(JObject json){
			return json["searchInformation"]["totalResults"].ToString() != "0";
		}

		private static bool getNextResult(out string url, JObject searchResult, int resultIndex){			
			url = "";
			return false;
		}

		private static bool hasProcessedUrl(string url){
			if (!File.Exists (Config.getInstance().ProcessedUrlsPath)) {
				File.Create (Config.getInstance().ProcessedUrlsPath);
				//Little delay otherwise locks the next read on mono which is weird but hey
				System.Threading.Thread.Sleep (2000);
			}

			string data = "";

			using (StreamReader reader = new StreamReader (Config.getInstance().ProcessedUrlsPath)) {
				data = reader.ReadToEnd ();
			}	

			return data.IndexOf (url) != -1;
		}

		private static bool IsViolation(string url, string offence){
			string fixedUrl = url.StartsWith ("http") ? url : "http://" + url;
			string urlData = new WebUtilities (true).GetResponse (fixedUrl, true);

			if (urlData != null && urlData.Length > 0) {
				saveData (url, urlData, offence);
				return urlData.IndexOf (offence) > -1;
			}

			return false;
		}

		private static void markUrlAsProcessed(string url){
			using (StreamWriter writer = new StreamWriter (Config.getInstance().ProcessedUrlsPath,true)) {
				writer.WriteLine (url);
				writer.Close ();
			}
		}

		private static void reportViolation(string url, string offence){
			string filename = "offencelist-" + DateTime.Now.ToString ("yyyy-MM-dd") + ".txt";

			using (StreamWriter writer = new StreamWriter (filename,true)) {
				writer.WriteLine (offence + ":" + url);
				writer.Close ();
			}
		}

		private static JObject getJsonQueryResult(string query){
			string response = new WebUtilities (true).GetResponse (
				query,
				true
			);

			if (response != null && response.Length > 0)
				return Newtonsoft.Json.Linq.JObject.Parse (response);

			return null;
		}

		private static List<string> searchUrls (string offence){
			List<string> urls = new List<string> ();
			string searchUrl = Config.getInstance ().CustomSearchEndpoint +
               "?key=" +
               Config.getInstance ().APIKey +
               "&cx=" +
               Config.getInstance ().CustomSearchIdentifier +
               "&q=" +
               offence;

			Console.WriteLine ("SEARCH:" + searchUrl);

			JObject jsonResult = getJsonQueryResult (searchUrl);

			if (jsonResult == null)
				return urls;
			
			if (jsonResult ["searchInformation"] ["totalResults"].ToString () != "0") {
				int startIndex = -1;
				urls = addRangeNoDupes(parseUrlsFromResult (jsonResult),urls);

				while(JObject.Parse(jsonResult["queries"].ToString()).SelectToken("nextPage") != null){
					startIndex = int.Parse (JObject.Parse ((jsonResult ["queries"] ["nextPage"]) [0].ToString ()) ["startIndex"].ToString ());
					searchUrl = Config.getInstance ().CustomSearchEndpoint +
						"?key=" +
						Config.getInstance ().APIKey +
						"&cx=" +
						Config.getInstance ().CustomSearchIdentifier +
						"&q=" +
						offence + "&start=" + startIndex.ToString ();
					
					Console.WriteLine ("SEARCH:" + searchUrl);
					jsonResult = getJsonQueryResult (searchUrl);

					if (jsonResult == null) {
						return urls;
					}

					urls = addRangeNoDupes(parseUrlsFromResult (jsonResult),urls);
				}
			}

			return urls;
		}

		private static List<string> addRangeNoDupes(List<string> incoming,List<string> existing){
			foreach (string url in incoming) {
				if (!existing.Contains (url))
					existing.Add (url);
			}
			return existing;
		}

		private static List<string> parseUrlsFromResult (JObject jsonResult){
			List<string> urls = new List<string>();

			for (int i = 0; i < jsonResult ["items"].Count(); i++) {
				urls.Add (jsonResult ["items"] [i] ["link"].ToString ());
			}

			return urls;
		}

		public static void Main (string[] args)
		{
			//This one is: Paste Search by NetBootCamp (90+ sites)
			//https://cse.google.com/cse/publicurl?cx=013212718322258083429:lha4khxcshs&q=@emaildomain
			List<string> offences = StorageHelper.ReadLines (StorageHelper.RepoName.EMAIL_OFFENCES);

			foreach (string offence in offences) {		
				if (offence.StartsWith ("#"))
					continue;
				
				Console.WriteLine ("Searching for offence:" + offence);
				foreach(string url in searchUrls(offence)){
					Console.Write ("\tProcessing:" + url);
					if (url == null || url == "")
						continue;
			
					if (hasProcessedUrl (url)) {
						Console.WriteLine (":Already processed.");
						continue;
					}

					if (IsViolation (url, offence)) {
						Console.WriteLine ("::FOUND OFFENCE::");
						reportViolation (url, offence);
					} else
						Console.WriteLine (":No violations found.");

					markUrlAsProcessed (url);
				}
			}

			return;
		}
	}
}
