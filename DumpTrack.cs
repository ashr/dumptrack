using System;
using System.IO;
using System.Collections.Generic;

namespace dumptrack
{
	public class DumpTrack
	{
		private List<IDumpTrackModule> modules = null;

		public DumpTrack ()
		{
			modules = new List<IDumpTrackModule> ();
			//modules.Add (new GoogleCustomSearchModule ());
			modules.Add (new GitHubSearchModule ());
		}

		public List<Offence> FindDumps(){
			List<Offence> offences = new List<Offence>();
			modules.ForEach (x => offences.AddRange(x.FindOffences ()));
			return offences;
		}

		public void Notify(){
			if (File.Exists("offencelist-" + DateTime.Now.ToString ("yyyy-MM-dd") + ".txt")){
				System.Net.Mail.MailMessage mm = new System.Net.Mail.MailMessage ();
				mm.Subject = "Monitored paste found on the web - " + DateTime.Now.ToString ("yyyy-MM-dd");
				mm.From = new System.Net.Mail.MailAddress(Config.getInstance().FromAddress);
				string toEmailString = Config.getInstance ().ToEmailString;
				string[] emails = toEmailString.Split (';');

				foreach (string email in emails) {
					mm.To.Add (email);
				}

				mm.Attachments.Add (new System.Net.Mail.Attachment ("offencelist-" + DateTime.Now.ToString ("yyyy-MM-dd") + ".txt"));

				System.Net.Mail.SmtpClient sc = new System.Net.Mail.SmtpClient (Config.getInstance().SMTPServer);
				sc.Send (mm);
			}		
		}
	}
}

