using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Diagnostics;

namespace Forumbee
{

    
    [DataContract]
    public class UrlBuilder
    {
        private const string host = "forumbee.com";
        private const string endpoint  = "/sso/1/login";


 		    //Example usage       
        static void Main(string[] args)
        {

            UrlBuilder builder = new UrlBuilder()
            {
                key     = "test1",
                email   = "test@example.com",
                name    = "test user",
                avatar  = "https://secure.gravatar.com/avatar/849853effb2c1143c8f65e3fe41e6714",
                domain  = "your-subdomain",
                secret  = "your-secret-key"
            };

            string url = builder.ToUrl();

            Debug.WriteLine(url);
        }
		    //End example
		
        
        public UrlBuilder()
        {
            //Date milliseconds since 1970
            DateTime epoch = new DateTime(1970, 1, 1);
            DateTime now   = DateTime.Now.ToUniversalTime();
            long d = (long)(now - epoch).TotalMilliseconds;
            date = d.ToString();
            uri = endpoint;
        }

        //Unique identifier for this user from your system (alphanumeric)
        [DataMember(IsRequired=true)]
        public String key;


        //Validated user email address
        [DataMember(IsRequired = true)]
        public String email;

        
        //User display name
        [DataMember(IsRequired = true)]
        public String name;

        
        //Optional. User avatar image url. Requires http://, https:// or //:
        [DataMember(EmitDefaultValue=false)]
        public String avatar;

        
        //Optional. User role
        [DataMember(EmitDefaultValue=false)]
        public String role;

        
        //Optional. Comma separated list of forum URL names to grant access
        [DataMember(EmitDefaultValue = false)]
        public String forums;


        //Optional. Redirect user to specific url e.g. /community/forum
        [DataMember(EmitDefaultValue = false)]
        public String redirect;

        
        //Your site sub-domain
        [DataMember(IsRequired = true)]
        public String domain;


        //The SSO endpoint URI (/sso/1/login)
        [DataMember(IsRequired = true)]
        public String uri;

        
        //The current time stamp milliseconds since 1970 UTC
        [DataMember(IsRequired = true)]
        public String date;


        public String secret;


        private String ToJson()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(UrlBuilder));

                js.WriteObject(ms, this);

                ms.Position = 0;
                StreamReader sr = new StreamReader(ms);
                string json = sr.ReadToEnd();
                sr.Close();
                return json;
            }
        }


        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }


        private static string SHA1Hash(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);

            string hex = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            return hex;
        }

        public string ToUrl()
        {

            string json     = ToJson();
            string base64   = EncodeTo64(json);
            string concat   = secret + base64;
            string digest   = SHA1Hash(concat);

            string query    = "?digest=" + WebUtility.UrlEncode(digest) + "&data=" + WebUtility.UrlEncode(base64);
            string url      = "https://" + domain + "." + host + endpoint + query;

            return url;
        }



    }
}
