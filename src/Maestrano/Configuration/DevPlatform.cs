using System;
using System.Configuration;

namespace Maestrano.Configuration
{
    public class DevPlatform : ConfigurationSection
    {
        private const string ProdDefaultHost = "https://developer.maestrano.com";
        private const string ProdDefaultApiPath = "/api/config/v1";


        /// <summary>
        /// Load Api configuration into a DevPlatform configuration object
        /// </summary>
        /// <returns>A DevPlatform configuration object</returns>
        public static DevPlatform Load()
        {
            ConfigurationManager.RefreshSection("maestranoDevPlatform");
            var config = ConfigurationManager.GetSection("maestranoDevPlatform") as DevPlatform;
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
        // The host to the developer platform API
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = ProdDefaultHost, IsRequired = false)]
        public String Host
        {
            get { return (String)this["host"]; }
            set { this["host"] = value; }
        }

        /// <summary>
        // The hostt to the developer platform API
        /// </summary>
        [ConfigurationProperty("apiPath", DefaultValue = ProdDefaultApiPath, IsRequired = false)]
        public String ApiPath
        {
            get { return (String)this["apiPath"]; }
            set { this["apiPath"] = value; }
        }

        /// <summary>
        // The hostt to the developer platform API
        /// </summary>
        [ConfigurationProperty("environment", DefaultValue = null, IsRequired = false)]
        public DevPlatformEnvironment Environment
        {
            get { return (DevPlatformEnvironment)this["environment"]; }
            set { this["environment"] = value; }
        }

    }
}
