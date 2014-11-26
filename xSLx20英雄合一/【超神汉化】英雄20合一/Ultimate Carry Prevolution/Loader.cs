using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using LeagueSharp;

namespace Ultimate_Carry_Prevolution
{
	class Loader
	{
		public static bool IsBetaTester;
		public const string VersionNumber = "1.6";

		public Loader()
		{
			try
			{
				const string address = "https://raw.githubusercontent.com/xSLx/LeagueSharp/master/Ultimate%20Carry%20Prevolution/Ultimate%20Carry%20Prevolution/Keys.txt";
				var httpWebRequest = (HttpWebRequest)WebRequest.Create(address);
				httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.111 Safari/537.36";
				httpWebRequest.Headers[HttpRequestHeader.AcceptEncoding] = "*/*";
				httpWebRequest.Headers[HttpRequestHeader.AcceptLanguage] = "de-de,de;q=0.8,en-us;q=0.5,en;q=0.3";
				httpWebRequest.Headers[HttpRequestHeader.AcceptCharset] = "ISO-8859-1,utf-8;q=0.7,*;q=0.7";
				var stream = httpWebRequest.GetResponse().GetResponseStream();
				if(stream != null)
				{
					var sr = new StreamReader(stream);
					var allKeys = sr.ReadToEnd();
					if(allKeys.Contains(MyKey()))
					{
						IsBetaTester = true;
						Game.PrintChat("BetaTester enabled.");
					}
				}
			}
			catch (Exception)
			{
				Game.PrintChat("BetaTests currently not available.");
			}
			Chat.WellCome();
		}

		private string MyKey()
		{
			var fingerPrint = Identifier("Win32_BIOS", "SerialNumber") +
							Identifier("Win32_DiskDrive", "Model") +
							Identifier("Win32_DiskDrive", "Manufacturer") +
							Identifier("Win32_DiskDrive", "Signature") +
							Identifier("Win32_DiskDrive", "TotalHeads") +
							Identifier("Win32_BaseBoard", "Model") +
							Identifier("Win32_BaseBoard", "Manufacturer") +
							Identifier("Win32_BaseBoard", "Name") +
							Identifier("Win32_BaseBoard", "SerialNumber") +
							Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
			return Encode(fingerPrint);
		}

		public string Encode(string str)
		{
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
		}

		private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
		{
			string[] result = { "" };
			var mc = new ManagementClass(wmiClass);
			var moc = mc.GetInstances();
			foreach(var mo in from ManagementBaseObject mo in moc
							  where mo[wmiMustBeTrue].ToString() == "True"
							  where result[0] == ""
							  select mo)
			{
				try
				{
					result[0] = mo[wmiProperty].ToString();
					break;
				}
				catch(Exception)
				{
					result[0] = "";
				}
			}
			return result[0];
		}

		private static string Identifier(string wmiClass, string wmiProperty)
		{
			string[] result = { "" };
			var mc = new ManagementClass(wmiClass);
			var moc = mc.GetInstances();
			foreach(var mo in moc.Cast<ManagementBaseObject>().Where(mo => result[0] == ""))
			{
				try
				{
					result[0] = mo[wmiProperty].ToString();
					break;
				}
				catch(Exception)
				{
					result[0] = "";
				}
			}
			return result[0];
		}


	}
}
