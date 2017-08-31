using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dumptrack
{
	public class GoogleCustomSearchModule : IDumpTrackModule
	{
		public GoogleCustomSearchModule ()
		{
		}

		public List<Offence> FindOffences (){
			List<Offence> offences = new List<Offence> ();
			//This one is: Paste Search by NetBootCamp (90+ sites)
			//https://cse.google.com/cse/publicurl?cx=013212718322258083429:lha4khxcshs&q=@emaildomain
			List<string> offenceSearchTerms = StorageHelper.ReadLines (StorageHelper.RepoName.EMAIL_OFFENCES);

			foreach (string offence in offenceSearchTerms) {		
				if (offence.StartsWith ("#"))
					continue;

				Console.WriteLine ("Searching for offence:" + offence);
				foreach(string url in searchUrls(offence)){
					Console.Write ("\tProcessing:" + url);
					if (url == null || url == "")
						continue;

					if (ModuleHelper.HasProcessedUrl (url)) {
						Console.WriteLine (":Already processed.");
						continue;
					}

					if (ModuleHelper.IsViolation (url, offence)) {
						Console.WriteLine ("::FOUND OFFENCE::");

						offences.Add (new Offence () {
							Text = offence,
							Type = this.GetType().ToString(),
							Url = url
						});

						ModuleHelper.ReportViolation(url, offence);
					} else
						Console.WriteLine (":No violations found.");

					ModuleHelper.MarkUrlAsProcessed (url);
				}
			}

			return offences;
		}

		private List<string> searchUrls (string offence){
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
			
		private JObject getJsonQueryResult(string query){
			string response = new WebUtilities (Config.getInstance().EnableProxy).GetResponse (
				query,
				true
			);

			if (response != null && response.Length > 0)
				return Newtonsoft.Json.Linq.JObject.Parse (response);

			return null;
		}

		private List<string> addRangeNoDupes(List<string> incoming,List<string> existing){
			foreach (string url in incoming) {
				if (!existing.Contains (url))
					existing.Add (url);
			}
			return existing;
		}

		private List<string> parseUrlsFromResult (JObject jsonResult){
			List<string> urls = new List<string>();

			for (int i = 0; i < jsonResult ["items"].Count(); i++) {
				urls.Add (jsonResult ["items"] [i] ["link"].ToString ());
			}

			return urls;
		}
	}
}

