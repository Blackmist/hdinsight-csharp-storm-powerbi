using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.SCP;
using Microsoft.SCP.Rpc.Generated;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net;
using PowerBIExtensionMethods;

namespace WordCount
{
    public class PowerBiBolt : ISCPBolt
    {
        //Uri for Power BI datasets
        private string datasetsUri { get; set; }

        //Used to authenticate to PowerBi
        private string username { get; set; }
        private string password { get; set; }
        private string clientId { get; set; }
        private static AuthenticationContext authContext = null;
        private static string token = String.Empty;
        //Create an instance of TokenCache to cache the access token
        private TokenCache TC;
        private UserCredential user;

        //Used when connecting/working with datasets
        private string datasetName { get; set; }
        private string tableName { get; set; }
        private string resourceUri { get; set; }

        //Holds the ID of the dataset
        private string datasetId { get; set; }

        Context context;

        public PowerBiBolt(Context context)
        {
            this.context = context;

            //Get the default datasets URI
            datasetsUri = Properties.Settings.Default.DatasetUri;

            //Get the username/password/clientID from appsettings
            username = Properties.Settings.Default.PowerBiUsername;
            password = Properties.Settings.Default.PowerBiPassword;
            clientId = Properties.Settings.Default.PowerBiClientId;
            string authority = Properties.Settings.Default.Authority;
            TC = new TokenCache();
            user = new UserCredential(this.username, this.password);
            authContext = new AuthenticationContext(authority, TC);

            //Get the dataset name to use
            datasetName = Properties.Settings.Default.DatasetName;

            //Get the table name
            tableName = Properties.Settings.Default.TableName;

            //Get the resource URI
            resourceUri = Properties.Settings.Default.ResourceUri;

            //Get the correct DatasetsUri to use
            datasetsUri = TestConnection();
            
            //Create the dataset if it doesn't exist
            CreateDataset();

            //Declare Input and Output schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            //The input contains a tuple containing a string field (the word,) and the count
            inputSchema.Add("default", new List<Type>() { typeof(string), typeof(int) });
            this.context.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public void Execute(SCPTuple tuple)
        {
            //TODO:
            // add logic to batch rows vs. sending one at a time.

            //In a production application, use more specific exception handling. 
            try
            {
                //Create the request
                HttpWebRequest request = DatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, tableName), "POST", AccessToken());

                //Create a row
                List<WordCount> words = new List<WordCount>
                {
                    new WordCount{Word=tuple.GetString(0), Count=tuple.GetInteger(1)}
                };
                PostRequest(request, words.ToJson(JavaScriptConverter<WordCount>.GetSerializer()));

            }
            catch (Exception ex)
            {
                throw ex;
            } 
        }

        // Get a new instance
        public static PowerBiBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new PowerBiBolt(ctx);
        }

        string AccessToken()
        {
            //TODO: Should probably add try/catch to handle
            //transient communications problems with auth service

            //Call AcquireToken to get an Azure token from Azure Active Directory token issuance endpoint
            //If token in the cache is valid, no call will be made and the token will just be returned
            //from cache
            token = authContext.AcquireToken(resourceUri, clientId, user).AccessToken.ToString();

            return token;
        }

        private string TestConnection()
        {
            // Check the connection for redirects
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", AccessToken()));
            request.AllowAutoRedirect = false;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //If there is a redirect, return that as the new Uri
            if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                return response.Headers["Location"];
            }
            return datasetsUri;
        }

        void CreateDataset()
        {
            //In a production application, use more specific exception handling.           
            try
            {
                //Get datasets by name
                var datasets = GetAllDatasets().Datasets(datasetName);
                
                if (datasets.Count() == 0)
                {
                    //If no dataset exists, create one by
                    //POSTing a request
                    HttpWebRequest request = DatasetRequest(datasetsUri, "POST", AccessToken());
                    //The request body is the schema of the WordCount object/table
                    //The result will contain the ID value starting at position 7
                    datasetId = PostRequest(request, new WordCount().ToJsonSchema(datasetName)).Substring(7,36);
                }
                else
                {
                    //If the ataset exits, get dataset id
                    datasetId = datasets.First()["id"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetResponse(HttpWebRequest request)
        {
            string response = string.Empty;
            //Get the response
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    //Read the response
                    response = reader.ReadToEnd();
                }
            }
            return response;
        }

        private string PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
            return GetResponse(request);
        }

        private HttpWebRequest DatasetRequest(string datasetsUri, string method, string accessToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", accessToken));

            return request;
        }

        List<Object> GetAllDatasets()
        {
            List<Object> datasets = null;

            //In a production application, use more specific exception handling.
            try
            {
                //Create a GET web request to list all datasets
                HttpWebRequest request = DatasetRequest(datasetsUri, "GET", AccessToken());

                //Get HttpWebResponse from GET request
                string responseContent = GetResponse(request);

                //Get list from response
                datasets = responseContent.ToObject<List<Object>>();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return datasets;
        }
    }
}