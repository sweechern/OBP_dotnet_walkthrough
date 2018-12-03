using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
// You will have to add the System.Web to your References if you are using a
// standard console application template.  

namespace ConsoleApplication1
{
    class Program
    {
        static string UrlEncodeCapitalized(string input)
        {
            char[] outputchar = HttpUtility.UrlEncode(input).ToCharArray();
            for (int i = 0; i < outputchar.Length - 2; i++)
            {
                if (outputchar[i] == '%')
                {
                    outputchar[i + 1] = char.ToUpper(outputchar[i + 1]);
                    outputchar[i + 2] = char.ToUpper(outputchar[i + 2]);
                }
            }
            return new string(outputchar);
        }
		
        static void Main(string[] args)
        {
            string oauth_callback = "";  
            // Use your own callback URL
            string oauth_consumer_key = "";
            // Use your own oauth_consumer_key
            string oauth_nonce = "";
            // Use your own oauth_nonce
            string oauth_signature = "";
            // Leave this blank for now
            string oauth_signature_method = "";
            // "HMAC-SHA1" or "HMAC-SHA256"
            string oauth_timestamp = "";
            // Use your own oauth_timestamp
            string oauth_version = "";
            // "1.0" or "1"

            string oauth_consumer_secret = "";
            // Use your own oauth_consumer_secret

            string method = "POST";
            string uri = "https://apisandbox.openbankproject.com/oauth/initiate";

            // Create a list of OAuth parameters
            List<KeyValuePair<string, string>> oauthparameters = new List<KeyValuePair<string, string>>();
            oauthparameters.Add(new KeyValuePair<string, string>("oauth_callback", 
			        UrlEncodeCapitalized(oauth_callback)));
            oauthparameters.Add(new KeyValuePair<string, string>("oauth_consumer_key", oauth_consumer_key));
            oauthparameters.Add(new KeyValuePair<string, string>("oauth_nonce", oauth_nonce));
            oauthparameters.Add(new KeyValuePair<string, string>
                ("oauth_signature_method", oauth_signature_method));
            oauthparameters.Add(new KeyValuePair<string, string>("oauth_timestamp", oauth_timestamp));
            oauthparameters.Add(new KeyValuePair<string, string>("oauth_version", oauth_version));

            // Sort the OAuth parameters on the key
            oauthparameters.Sort((x, y) => x.Key.CompareTo(y.Key));

            // Construct the Base String
            string basestring = method.ToUpper() + "&" + UrlEncodeCapitalized(uri) + "&";
            foreach (KeyValuePair<string, string> pair in oauthparameters)
            {
                if (pair.Key == oauthparameters[oauthparameters.Count - 1].Key)
                {
                    basestring += pair.Key + "%3D" + UrlEncodeCapitalized(pair.Value);
                }
                else
                {
                    basestring += pair.Key + "%3D" + UrlEncodeCapitalized(pair.Value) + "%26";
                }
            }

            // Encrypt with either SHA1 or SHA256, creating the Signature
            var enc = Encoding.ASCII;
            if (oauth_signature_method == "HMAC-SHA1")
            {
                HMACSHA1 hmac = new HMACSHA1(enc.GetBytes(oauth_consumer_secret + "&"));
                hmac.Initialize();
                byte[] buffer = enc.GetBytes(basestring);
                string hmacsha1 = BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "").ToLower();
                byte[] resultantArray = new byte[hmacsha1.Length / 2];
                for (int i = 0; i < resultantArray.Length; i++)
                {
                    resultantArray[i] = Convert.ToByte(hmacsha1.Substring(i * 2, 2), 16);
                }
                string base64 = Convert.ToBase64String(resultantArray);
                oauth_signature = UrlEncodeCapitalized(base64);
            }
            else if (oauth_signature_method == "HMAC-SHA256")
            {
                HMACSHA256 hmac = new HMACSHA256(enc.GetBytes(oauth_consumer_secret + "&"));
                hmac.Initialize();
                byte[] buffer = enc.GetBytes(basestring);
                string hmacsha256 = BitConverter.ToString(hmac.ComputeHash(buffer)).Replace("-", "")
                    .ToLower();
                byte[] resultantArray = new byte[hmacsha256.Length / 2];
                for (int i = 0; i < resultantArray.Length; i++)
                {
                    resultantArray[i] = Convert.ToByte(hmacsha256.Substring(i * 2, 2), 16);
                }
                string base64 = Convert.ToBase64String(resultantArray);
                oauth_signature = UrlEncodeCapitalized(base64);
            }

            // Create the Authorization string for the WebRequest header
            string authorizationstring = "";
            foreach (KeyValuePair<string, string> pair in oauthparameters)
            {
                authorizationstring += pair.Key;
                authorizationstring += "=";
                authorizationstring += pair.Value;
                authorizationstring += ",";
            }
            authorizationstring += "oauth_signature=" + oauth_signature;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.Headers.Add("Authorization", "OAuth " + authorizationstring);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            
            Console.WriteLine(responseFromServer);
            Console.ReadLine(); //Pause
        }
    }
}
