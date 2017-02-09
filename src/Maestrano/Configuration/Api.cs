using Newtonsoft.Json.Linq;
using System;
using System.Configuration;

namespace Maestrano.Configuration
{
    public class Api : ConfigurationSection
    {
        private const string ProdDefaultApiHost = "https://api-hub.maestrano.com";

        private string presetName;

        /// <summary>
        /// Load Api configuration into a Api configuration object
        /// </summary>
        /// <returns>A Api configuration object</returns>
        public static Api Load(string preset = "maestrano")
        {
            ConfigurationManager.RefreshSection(preset + "/api");
            var config = ConfigurationManager.GetSection(preset + "/api") as Api;
            if (config == null) config = new Api();
            config.presetName = preset;

            return config;
        }
        /// <summary>
        /// Load Api configuration into a Api configuration object from a Json Object
        /// </summary>
        /// <returns>A Api configuration object</returns>
        public static Api LoadFromJson(string preset, JObject obj)
        {
            var config = new Api();
            config.presetName = preset;
            config.Id = obj["id"].Value<string>();
            config.Key = obj["key"].Value<string>();
            config.Base = obj["base"].Value<string>();
            config.Host = obj["host"].Value<string>();
            return config;
        }

        /// <summary>
        /// Return False (object not read only)
        /// </summary>
        /// <returns></returns>
        public override bool IsReadOnly()
        {
            return false;
        }

        /// <summary>
        /// API Id
        /// </summary>
        [ConfigurationProperty("id", DefaultValue = null, IsRequired = false)]
        public String Id
        {
            get { return (String)this["id"]; }
            set { this["id"] = value; }
        }

        /// <summary>
        /// API Key
        /// </summary>
        [ConfigurationProperty("key", DefaultValue = null, IsRequired = false)]
        public String Key
        {
            get { return (String)this["key"]; }
            set { this["key"] = value; }
        }

        /// <summary>
        /// API Key
        /// </summary>
        [ConfigurationProperty("base", DefaultValue = "/api/v1/", IsRequired = false)]
        public String Base
        {
            get { return (String)this["base"]; }
            set { this["base"] = value; }
        }

        /// <summary>
        /// Verify SSL certs (disabled for now)
        /// </summary>
        [ConfigurationProperty("verifySslCerts", DefaultValue = false, IsRequired = false)]
        public bool VerifySslCerts
        {
            get { return (Boolean)this["verifySslCerts"]; }
            set { this["verifySslCerts"] = value; }
        }

        // Return an identification token ready to
        // be used for HTTP auth
        public string Token { get { return Id + ':' + Key; } }

        // Language used by the API
        public string Lang { get { return "C#"; } }

        // Return the API version
        public string Version { get { return MnoHelper.Version; } }

        // Return the language version
        public string LangVersion { get { return Environment.OSVersion.ToString() + " - " + Environment.Version.ToString(); } }

        /// <summary>
        /// Address of the API Host
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = null, IsRequired = false)]
        public string Host
        {
            get
            {
                var _idp = (String)this["host"];
                if (string.IsNullOrEmpty(_idp))
                {
                    return ProdDefaultApiHost;
                }
                return _idp;
            }

            set { this["host"] = value; }
        }

    }
}
