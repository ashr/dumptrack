using System;
using System.IO;

namespace dumptrack
{
	public class ModuleHelper
	{
		public ModuleHelper ()
		{
		}

		public static bool HasProcessedUrl(string url){
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

		public static bool IsViolation(string url, string offence){
			string fixedUrl = url.StartsWith ("http") ? url : "http://" + url;
			string urlData = new WebUtilities (Config.getInstance().EnableProxy).GetResponse (fixedUrl, true);

			if (urlData != null && urlData.Length > 0) {
				SaveData (url, urlData, offence);
				return urlData.IndexOf (offence) > -1;
			}

			return false;
		}

		public static void MarkUrlAsProcessed(string url){
			using (StreamWriter writer = new StreamWriter (Config.getInstance().ProcessedUrlsPath,true)) {
				writer.WriteLine (url);
				writer.Close ();
			}
		}

		public static void SaveData (string url, string urlData, string offence){
			using (StreamWriter writer = new StreamWriter("dumps-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt",true)){
				writer.WriteLine(offence + ":" + url);
				writer.WriteLine(urlData);
			}
		}

		public static void ReportViolation(string url, string offence){
			string filename = "offencelist-" + DateTime.Now.ToString ("yyyy-MM-dd") + ".txt";

			using (StreamWriter writer = new StreamWriter (filename,true)) {
				writer.WriteLine (offence + ":" + url);
				writer.Close ();
			}
		}
	}
}

