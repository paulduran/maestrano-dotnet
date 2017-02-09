using Newtonsoft.Json.Linq;
using System;
using System.Configuration;

namespace Maestrano.Configuration
{
    public class Connec : ConfigurationSection
    {
        private const string ProdDefaultApiHost = "https://api-connec.maestrano.com";
        private const string ProdDefaultBasePath = "/api/v2";

        private string presetName;

        /// <summary>
        /// Load configuration into a Connec configuration object
        /// </summary>
        /// <returns>A Connec configuration object</returns>
        public static Connec Load(string preset = "maestrano")
        {
            ConfigurationManager.RefreshSection(preset + "/connec");
            var config = ConfigurationManager.GetSection(preset + "/connec") as Connec;
            if (config == null) config = new Connec();
            config.presetName = preset;

            return config;
        }

        /// <summary>
        /// Load configuration into a Connec configuration object from a JObject 
        /// </summary>
        /// <returns>A Connec configuration object</returns>
        public static Connec LoadFromJson(string preset, JObject obj)
        {
            var config = new Connec();
            config.presetName = preset;
            config.Host = obj["host"].ToString();
            config.BasePath = obj["base_path"].ToString();

            return config;
        }

        /// <summary>
        /// Return the Connec! API Host
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = null, IsRequired = false)]
        public string Host
        {
            get
            {
                var _host = (String)this["host"];
                if (string.IsNullOrEmpty(_host))
                {
                    return ProdDefaultApiHost;
                }
                return _host;
            }

            set { this["host"] = value; }
        }

        /// <summary>
        /// Return the Connec! API Base Path
        /// </summary>
        [ConfigurationProperty("base-path", DefaultValue = null, IsRequired = false)]
        public string BasePath
        {
            get
            {
                var _path = (String)this["base-path"];
                if (string.IsNullOrEmpty(_path))
                {
                    return ProdDefaultBasePath;
                }
                return _path;
            }

            set { this["base-path"] = value; }
        }
    }
}
