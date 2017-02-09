using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Maestrano.Configuration;
using Maestrano.Net;
using RestSharp;

namespace Maestrano
{

    public class AutoConfigureException : Exception
    {
        public AutoConfigureException(string message) : base(message)
        {
        }

        public AutoConfigureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public static class MnoHelper
    {
        // VERSION
        public static string Version { get { return "1.1.0"; } }

        private static Dictionary<string, Preset> presetDict;
        private static Preset defaultPreset;
        [Obsolete("use MnoHelper.With(markpetlace).Sso instead")]
        public static Configuration.Sso Sso { get; private set; }
        [Obsolete("use MnoHelper.With(markpetlace).App instead")]
        public static Configuration.App App { get; private set; }
        [Obsolete("use MnoHelper.With(markpetlace).Api instead")]
        public static Configuration.Api Api { get; private set; }
        [Obsolete("use MnoHelper.With(markpetlace).Connec instead")]
        public static Configuration.Connec Connec { get; private set; }
        [Obsolete("use MnoHelper.With(markpetlace).Webhook instead")]
        public static Configuration.Webhook Webhook { get; private set; }

        static MnoHelper()
        {
            // Initialize preset list
            presetDict = new Dictionary<string, Preset>();

            // Load "maestrano" preset by default
            defaultPreset = new Preset("maestrano");

            // Emulate old configuration behaviour for backward 
            // compatibility
            App = defaultPreset.App;
            Api = defaultPreset.Api;
            Connec = defaultPreset.Connec;
            Webhook = defaultPreset.Webhook;
            Sso = defaultPreset.Sso;
        }

        public static Dictionary<string, Preset> Presets()
        {
            return new Dictionary<string, Preset>(presetDict);
        }

        /// <summary>
        /// Autoconfigure Maestrano using the Developer platform.
        /// Parameters are taken from Environment variable names, or from the Configuration if not found
        /// Environment name: 
        /// MNO_DEVPL_HOST=<developer platform host>
        /// MNO_DEVPL_API_PATH=<developer platform host>
        /// MNO_DEVPL_ENV_NAME=<your environment nid>
        /// MNO_DEVPL_ENV_KEY=<your environment key>
        /// MNO_DEVPL_ENV_SECRET=<your environment secret>
        /// </summary>
        ///  <exception cref="AutoConfigureException">If the developer platform could not be reached.</exception>
        public static void AutoConfigure()
        {
            var config = DevPlatform.Load();
            var host = System.Environment.GetEnvironmentVariable("MNO_DEVPL_HOST") ?? (config == null ? null: config.Host);
            var path = System.Environment.GetEnvironmentVariable("MNO_DEVPL_API_PATH") ?? (config == null ? null : config.ApiPath);

            var key = System.Environment.GetEnvironmentVariable("MNO_DEVPL_ENV_KEY") ?? (config == null ? null : config.Environment.ApiKey);
            var secret = System.Environment.GetEnvironmentVariable("MNO_DEVPL_ENV_SECRET") ?? (config == null ? null : config.Environment.ApiSecret);
            AutoConfigure(host, path, key, secret);
        }

        /// <summary>
        /// Autoconfigure Maestrano using the Developer platform
        /// </summary>
        /// <param name="host"> developer platform host.</param>
        /// <param name="path"> developer platform api path</param>
        /// <param name="key"> environment key.</param>
        /// <param name="secret"> environment secret.</param>
        public static void AutoConfigure(string host, string path, string key, string secret)
        {
            if (String.IsNullOrEmpty(host)){
                throw new AutoConfigureException("Developper Platform Host was not provided.");
            }
            if (String.IsNullOrEmpty(path))
            {
                throw new AutoConfigureException("Developper Api Path Host was not provided.");
            }
            if (String.IsNullOrEmpty(key))
            {
                throw new AutoConfigureException("Developper Environment Key was not provided.");
            }
            if (String.IsNullOrEmpty(secret))
            {
                throw new AutoConfigureException("Developper Environment Secret was not provided.");
            }

            var client = new JsonClient(host, path, key, secret);
            RestResponse response = client.Get("marketplaces");
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AutoConfigureException("Could not authenticate. Check your key and secret");
            }
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                throw new AutoConfigureException(message, response.ErrorException);
            }
            if (String.IsNullOrEmpty(response.Content))
            {
                throw new AutoConfigureException("Could not retrieve any result from the developer platform.");
            }
            var content = JObject.Parse(response.Content);
            if (content["marketplaces"] == null)
            {
                throw new AutoConfigureException("Could not find marketplaces in the json response");
            }
            var marketplaces = content["marketplaces"].Value<JArray>();
            foreach (var marketplace in marketplaces)
            {
                var preset = new Preset(marketplace.Value<JObject>());
                presetDict.Add(preset.Name, preset);
            }
        }

        /// <summary>
        /// Scope the MnoHelper to a specific configuration preset
        /// </summary>
        public static Preset With(string presetName = "maestrano")
        {
            if (presetName == null)
            {
                presetName = "maestrano";
            }

            if (!presetDict.ContainsKey(presetName))
            {
                presetDict.Add(presetName, new Preset(presetName));
            }

            return presetDict[presetName];
        }

        /// <summary>
        /// App environment: 'test' or 'production'
        /// </summary>
        [Obsolete("use MnoHelper.With(markpetlace).Environment instead")]
        public static string Environment
        {
            get { return defaultPreset.App.Environment; }
            set { defaultPreset.App.Environment = value; }
        }

        /// <summary>
        /// Check that ID and Key passed in argument match
        /// the ones configured at the API level.
        /// Used for remote authentication from Maestrano.
        /// </summary>
        /// <param name="apiId">An application ID</param>
        /// <param name="apiKey">An API Key</param>
        /// <returns>true if the authentication is successful, false otherwise</returns>
        [Obsolete("use MnoHelper.With(markpetlace).Authenticate instead")]
        public static bool Authenticate(string apiId, string apiKey)
        {
            return defaultPreset.Authenticate(apiId, apiKey);
        }

        /// <summary>
        /// Authenticate a request from Maestrano using HTTP Basic Authentication
        /// </summary>
        /// <param name="request">An HttpRequest object</param>
        /// <returns>true if the authentication is successful, false otherwise</returns>
        [Obsolete("use MnoHelper.With(markpetlace).Authenticate instead")]
        public static bool Authenticate(System.Web.HttpRequest request)
        {
            return defaultPreset.Authenticate(request);
        }

        /// <summary>
        /// Take a user uid (either real like 'usr-1' or virtual like
        /// 'usr-1.cld-3') and return the real uid part
        /// </summary>
        /// <param name="userUid">A real or virtual uid</param>
        /// <returns>Real user uid</returns>
        [Obsolete("use MnoHelper.With(markpetlace).UnmaskUser instead")]
        public static string UnmaskUser(string userUid)
        {
            return defaultPreset.UnmaskUser(userUid);
        }

        /// <summary>
        /// Take a user uid (either real or virtual) and a group uid
        /// and return the user uid that should be used within the app
        /// based on the Sso.CreationMode parameter
        /// </summary>
        /// <param name="userUid">a real or virtual user uid</param>
        /// <param name="groupUid">a group uid</param>
        /// <returns>
        /// The real user uid (usr-1) if Sso.CreationMode is set to "real"
        /// The virtual user uid (user-1.cld-2) if Sso.CreationMode is set to "virtual"
        /// </returns>
        [Obsolete("use MnoHelper.With(markpetlace).MaskUser instead")]
        public static string MaskUser(string userUid, string groupUid)
        {
            return defaultPreset.MaskUser(userUid, groupUid);
        }

        /// <summary>
        /// Return whether the environment is production like (production or production sandbox)
        /// </summary>
        /// <returns>true for production and production-sandbox</returns>
        [Obsolete("isProduction is deprecated")]
        public static Boolean isProduction()
        {
            return defaultPreset.isProduction();
        }

        /// <summary>
        /// Return whether the environment is production like (production or production sandbox)
        /// </summary>
        /// <returns>true for production and production-sandbox</returns>
        [Obsolete("isDevelopment is deprecated")]
        public static Boolean isDevelopment()
        {
            return defaultPreset.isDevelopment();
        }

        /// <summary>
        /// Return a json serializable object describing the current 
        /// Maestrano configuration. The metadata will be fetched remotely
        /// by Maestrano. Note that the metadata exclude any info
        /// including an API Key.
        /// </summary>
        /// <returns>JObject which can be converted to JSON using ToString()</returns>
        [Obsolete("use MnoHelper.With(markpetlace).ToMetadata instead")]
        public static JObject ToMetadata()
        {
            return defaultPreset.ToMetadata();
        }

        /// <summary>
        /// Clear a preset configuration
        /// </summary>
        /// <param name="presetName">name of preset to clear</param>
        public static void ClearPreset(string presetName)
        {
            if (presetDict.ContainsKey(presetName))
            {
                presetDict.Remove(presetName);
            }

            if (presetName.Equals("maestrano"))
            {
                defaultPreset = new Preset("maestrano");
                presetDict.Add("maestrano", defaultPreset);

                // Emulate old configuration behaviour for backward 
                // compatibility
                App = defaultPreset.App;
                Api = defaultPreset.Api;
                Connec = defaultPreset.Connec;
                Webhook = defaultPreset.Webhook;
                Sso = defaultPreset.Sso;
            }
        }
    }
}
