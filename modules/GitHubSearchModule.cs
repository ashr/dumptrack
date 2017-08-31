using System;
using System.Linq;
using System.Collections.Generic;

using HtmlAgilityPack;

namespace dumptrack
{
	public class GitHubSearchModule : IDumpTrackModule
	{
		private WebUtilities wu = null;
		private System.Net.CookieContainer cookieContainer = null;
		private List<string> blacklist = null;

		public GitHubSearchModule ()
		{
			cookieContainer = new System.Net.CookieContainer ();
			wu = new WebUtilities (cookieContainer);
			blacklist = StorageHelper.ReadLines (StorageHelper.RepoName.GITHUB_BLACKLIST);

			//Log in to GitHub
			//Host: github.com
			//User-Agent: Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0
			//Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8
			//Accept-Language: en-ZA,en-GB;q=0.8,en-US;q=0.5,en;q=0.3
			//Accept-Encoding: gzip, deflate, br
			//Content-Type: application/x-www-form-urlencoded
			//Content-Length: 180
			//Referer: https://github.com/
			//Cookie: logged_in=no; _ga=GA1.2.398293556.1483730913; _octo=GH1.1.2111636044.1483730914; tz=Africa%2FJohannesburg; _gh_sess=eyJzZXNzaW9uX2lkIjoiZjBhNDU1Mzk0NTA5NDJjMGFhZTNkNGZhNDJjYWVjNGYiLCJsYXN0X3JlYWRfZnJvbV9yZXBsaWNhcyI6MTUwNDE2ODIxNDU4MCwic3B5X3JlcG8iOiJnZW50aWxraXdpL21pbWlrYXR6Iiwic3B5X3JlcG9fYXQiOjE1MDQxNjQyMzksImNvbnRleHQiOiIvIiwibGFzdF93cml0ZSI6MTUwNDE2ODIxMDExMSwicmVmZXJyYWxfY29kZSI6Imh0dHBzOi8vZ2l0aHViLmNvbS8iLCJfY3NyZl90b2tlbiI6IlZMOEEzM0ZGQ1pXcGs2dDBrN05rUzNUTm5RanpvQ09BblZhTUI3T2x0aDQ9IiwiZmxhc2giOnsiZGlzY2FyZCI6W10sImZsYXNoZXMiOnsiYW5hbHl0aWNzX2xvY2F0aW9uX3F1ZXJ5X3N0cmlwIjoidHJ1ZSJ9fX0%3D--b1b6f3a10d8050f24468dff5bd7d4da2cc4244ba
			//DNT: 1
			//Connection: keep-alive
			//Upgrade-Insecure-Requests: 1

			//commit=Sign+in&utf8=%E2%9C%93&authenticity_token=rEVnmjAzfMuH4kQ0%2BqAHdQh4Hr3if9GAQcV37Noz%2Fwkyl8Elna%2F2dHqP2HRwnr2bUMV%2BBzT%2B1TFoobLFysoD9Q%3D%3D&login=larry&password=bobarry
				
			/*
			//Get CSRF TOKEN!
			Console.WriteLine("Loading github login page");
			string loginPageHtml = wu.GetResponse("https://github.com/login");

			HtmlDocument loginHtmlDocument = new HtmlDocument ();
			loginHtmlDocument.LoadHtml (loginPageHtml);

			//Strip out token
			//authenticity_token
			HtmlNode authenticityNode = loginHtmlDocument.DocumentNode.SelectSingleNode("//input[@name='authenticity_token']");


			string loginReply = wu.PostData (new Dictionary<string, string> () {
				{"login",Config.getInstance().GitHubUsername},
				{"password",Config.getInstance().GitHubPassword},
				{"authenticity_token",authenticityNode.Attributes["value"].Value},
				{"commit", "Sign in"},
				{"utf8","%E2%9C%93"}
			}, "https://github.com/session");

			//Parse reply, crash if not logged in
			*/
		}

		public List<Offence> FindOffences(){
			List<Offence> offences = new List<Offence> ();
			List<string> offenceSearchTerms = StorageHelper.ReadLines (StorageHelper.RepoName.GITHUB_OFFENCES);

			foreach (string offence in offenceSearchTerms) {
				if (offence.StartsWith ("#"))
					continue;

				Console.WriteLine ("Searching for git offence:" + offence);
				foreach(string url in searchGitUrls(offence)){
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

						ModuleHelper.ReportViolation (url, offence);
					} else
						Console.WriteLine (":No violations found.");

					ModuleHelper.MarkUrlAsProcessed (url);
				}
			}

			return offences;
		}

		private List<string> searchGitUrls(string offence){
			List<string> gitLinks = new List<string> ();
			//Search all commits
			//https://github.com/search?p=6&q=sanlam&type=Commits&utf8=%E2%9C%93
			//This does a query name thing
			//https://github.com/search?utf8=%E2%9C%93&q=QUERY&ref=simplesearch
			string searchUrl = "https://github.com/search?q=" + offence + "&type=Commits&utf8=%E2%9C%93";

			bool paginate = true;
			while (paginate) {
				Console.WriteLine ("SEARCH:" + searchUrl);

				string htmlResult = getHtmlQueryResult (searchUrl);

				if (htmlResult == null || htmlResult.Length == 0)
					return gitLinks;

				HtmlDocument htmlDoc = new HtmlDocument ();
				htmlDoc.LoadHtml (htmlResult);

				//HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes ("//div[@class='repo-list-item']");//"//div[@id='tableSection']");
				//htmlDoc.DocumentNode.SelectNodes("/html[1]/body[1]/div[4]/div[1]/div[2]/div[1]/div[1]/ul[1]/div")
				HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes ("//div[@id='commit_search_results']");
				//htmlDoc.DocumentNode.SelectNodes ("/html[1]/body[1]/div[4]/div[1]/div[2]/div[1]/div[1]/ul[1]/div");
				//nodes[0].ChildNodes[1].ChildNodes[1].ChildNodes[1].Attributes["href"].Value

				if (nodes != null && nodes.Count > 0) {
					foreach (HtmlNode subNode in nodes) {
						foreach (HtmlNode childNodes in subNode.ChildNodes) {
							if (childNodes.ChildNodes.Count == 5) {
								//htmlDoc.DocumentNode.SelectNodes("//div[@id='commit_search_results']")[0].ChildNodes[1].ChildNodes[3].ChildNodes[1].ChildNodes[3].Attributes["href"]
								string href = childNodes.ChildNodes [1].ChildNodes [3].ChildNodes [1].Attributes ["href"].Value;

								bool blacklistedFlag = false;
								foreach (string blacklisted in blacklist) {
									if (href.IndexOf (blacklisted) > -1) {
										blacklistedFlag = true;
										break;
									}
								}
								if (!blacklistedFlag) {
									Console.WriteLine ("\t" + href);
									gitLinks.Add ("https://github.com" + href);
								} else
									Console.WriteLine ("BLACKLISTED:" + href);
							}
						}
					}

					//Check pagination, change page etc
					//<a class="next_page" rel="next" href="/search?p=2&amp;q=sanlam&amp;type=Commits&amp;utf8=%E2%9C%93">Next</a>
					HtmlNode nextPageNode = htmlDoc.DocumentNode.SelectSingleNode ("//a[@class='next_page']");
					if (nextPageNode != null) {
						searchUrl = "https://github.com" + nextPageNode.Attributes ["href"].Value;
					} else {
						paginate = false;
					}
				} else {
					Console.WriteLine ("No results for " + offence);
					paginate = false;
				}
			}

			return gitLinks;
		}

		private string getHtmlQueryResult(string searchUrl){
			string data = "";

			try{
				WebUtilities wu = new WebUtilities (Config.getInstance ().EnableProxy);
				data = wu.GetResponse (searchUrl, true);
			}
			catch(Exception e){
				Console.WriteLine (e.Message);
			}

			return data;
		}
	}
}

