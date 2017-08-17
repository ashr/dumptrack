using System;
using System.IO;

using System.Net;
using System.Net.Security;

using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace dumptrack
{
	public class WebUtilities
	{
		private bool EnableProxy = false;
		private string HTTP_PROXY_ADDR = "";

		private CookieContainer cookies = new CookieContainer();

		public WebUtilities(){
			EnableProxy = Config.getInstance ().EnableProxy;
			HTTP_PROXY_ADDR = Config.getInstance ().ProxyAddress;
		}

		public WebUtilities(CookieContainer Cookies) : this()
		{
			this.cookies = Cookies;
		}

		public WebUtilities(bool EnableProxy = false)
		{
			this.EnableProxy = EnableProxy;
		}

		public string Base64Encode(string data)
		{
			try
			{
				byte[] bytesToEncode = Encoding.UTF8.GetBytes(data);
				return Convert.ToBase64String(bytesToEncode);
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}
			return "";
		}

		public string Base64Decode(string data)
		{
			try
			{
				byte[] decodedBytes = Convert.FromBase64String(data);
				string decodedText = Encoding.UTF8.GetString(decodedBytes);
				return decodedText;
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}
			return "";
		}

		public string URLEncode(string URL)
		{
			return System.Web.HttpUtility.UrlEncode(URL);
		}

		public string URLDecode(string encodedURL)
		{
			return System.Web.HttpUtility.UrlDecode(encodedURL);
		}

		public string HTMLEncode(string HTML)
		{
			return System.Web.HttpUtility.HtmlEncode(HTML);
		}

		public string HTMLDecode(string encodedHTML)
		{
			return System.Web.HttpUtility.HtmlDecode(encodedHTML);
		}

		public string GenerateMD5(string data)
		{
			MD5 hasher = MD5.Create();
			char[] charData = data.ToCharArray();
			byte[] byteCharData = new byte[charData.Length];
			for (int i = 0; i < charData.Length; i++)
			{
				byteCharData[i] = (byte)charData[i];
			}
			byte[] hashed = hasher.ComputeHash(byteCharData);
			StringBuilder sb = new StringBuilder();
			foreach (byte b in hashed) sb.Append(b.ToString());
			return sb.ToString();
		}


		public string GetResponse(string url,bool autoredirect=true)
		{
			WebProxy proxyObject = null;
			ServicePointManager.ServerCertificateValidationCallback +=
				delegate(
					object sender, 
					System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
					System.Security.Cryptography.X509Certificates.X509Chain
					chain, SslPolicyErrors sslPolicyErrors) {
				return true;
			};
				
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
			request.CookieContainer = cookies;
			request.AllowAutoRedirect = autoredirect;
			//request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
			request.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:18.0) Gecko/20100101 Firefox/18.0";
			//Ignore shitty certificates

			request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
				System.Security.Cryptography.X509Certificates.X509Chain chain,
				System.Net.Security.SslPolicyErrors sslPolicyErrors)
			{
				return true; // **** Always accept
			};

			if (EnableProxy) { 
				proxyObject = new WebProxy (Config.getInstance ().ProxyAddress);
				request.Proxy = proxyObject;
			}

			try
			{
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
					{
						string data = streamReader.ReadToEnd();
						return data;
					}
				}
				else
				{
				}
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}

			return null;
		}

		public string PostJSONData(string data, string URL)
		{
			try
			{
				WebProxy proxyObject = null;
				System.Net.ServicePointManager.Expect100Continue = false;

				ServicePointManager.ServerCertificateValidationCallback +=
					delegate(
						object sender, 
						System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
						System.Security.Cryptography.X509Certificates.X509Chain
						chain, SslPolicyErrors sslPolicyErrors) {
					return true;
				};


				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
				request.Method = "POST";
				request.ContentType = "application/json; charset=UTF-8";
				request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
				request.CookieContainer = cookies;
				request.AllowAutoRedirect = true;
				request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
				request.Headers.Add("Accept-Language","en-US,en;q=0.5");
				request.Headers.Add("Accept-Encoding","gzip, deflate");
				request.Headers.Add("X-Requested-With","XMLHttpRequest");

				//Ignore shitty certificates
				request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors)
				{
					return true; // **** Always accept
				};

				if (EnableProxy){
					proxyObject = new WebProxy(Config.getInstance().ProxyAddress);
					request.Proxy = proxyObject;
				}

				StringBuilder postData = new StringBuilder();
				postData.Append(data);

				byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());
				request.ContentLength = byteArray.Length;

				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
					{
						string returnData = streamReader.ReadToEnd();
						return returnData;
					}
				}
				else
				{
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return null;
		}

		public string PostData(Dictionary<string, string> data, string URL)
		{
			try
			{
				WebProxy proxyObject = new WebProxy(HTTP_PROXY_ADDR);
				ServicePointManager.ServerCertificateValidationCallback +=
					delegate(
						object sender, 
						System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
						System.Security.Cryptography.X509Certificates.X509Chain
						chain, SslPolicyErrors sslPolicyErrors) {
					return true;
				};


				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET4.0C; .NET4.0E)";
				request.CookieContainer = cookies;
				request.AllowAutoRedirect = true;

				request.ServerCertificateValidationCallback = delegate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
					System.Security.Cryptography.X509Certificates.X509Chain chain,
					System.Net.Security.SslPolicyErrors sslPolicyErrors)
				{
					return true; // **** Always accept
				};

				if (EnableProxy)
					request.Proxy = proxyObject;
				
				StringBuilder postData = new StringBuilder();
				foreach (KeyValuePair<string, string> kv in data)
				{
					if (postData.Length == 0)
						postData.Append(kv.Key + "=" + kv.Value);
					else
						postData.Append("&" + kv.Key + "=" + kv.Value);
				}

				byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());
				request.ContentLength = byteArray.Length;

				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();

				HttpWebResponse response = (HttpWebResponse)request.GetResponse();

				if (response.StatusCode == HttpStatusCode.OK)
				{
					using (StreamReader streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
					{
						string returnData = streamReader.ReadToEnd();
						return returnData;
					}
				}
				else
				{
				}
			}
			catch (Exception e)
			{
				Console.Write (e.Message);
			}

			return null;
		}

		public CookieContainer GetCookies { get { return cookies; } set { cookies = value; } }

		public dynamic ParseHTTPNameValuePairResponseIntoDynamic(string webresponse)
		{
			throw new NotImplementedException();
		}

		public static string StringToBinary(string data)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char c in data.ToCharArray())
			{
				sb.Append(Convert.ToString(c, 2).PadLeft(8,'0'));
			}
			return sb.ToString();
		}

		public static string BinaryToString(string data)
		{
			List<Byte> byteList = new List<Byte>();

			for (int i = 0; i < data.Length; i += 8)
			{
				byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
			}

			return Encoding.ASCII.GetString(byteList.ToArray());
		}
	}
}