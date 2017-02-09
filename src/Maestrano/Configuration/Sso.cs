using Maestrano.Sso;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.SessionState;

namespace Maestrano.Configuration
{
    public class Sso : ConfigurationSection
    {

        private const string ProdIdpHost = "https://api-hub.maestrano.com";
        private const string ProdX509CertFootprint = "2f:57:71:e4:40:19:57:37:a6:2c:f0:c5:82:52:2f:2e:41:b7:9d:7e";
        private const string ProdX509Cert = "-----BEGIN CERTIFICATE-----\nMIIDezCCAuSgAwIBAgIJAPFpcH2rW0pyMA0GCSqGSIb3DQEBBQUAMIGGMQswCQYD\nVQQGEwJBVTEMMAoGA1UECBMDTlNXMQ8wDQYDVQQHEwZTeWRuZXkxGjAYBgNVBAoT\nEU1hZXN0cmFubyBQdHkgTHRkMRYwFAYDVQQDEw1tYWVzdHJhbm8uY29tMSQwIgYJ\nKoZIhvcNAQkBFhVzdXBwb3J0QG1hZXN0cmFuby5jb20wHhcNMTQwMTA0MDUyNDEw\nWhcNMzMxMjMwMDUyNDEwWjCBhjELMAkGA1UEBhMCQVUxDDAKBgNVBAgTA05TVzEP\nMA0GA1UEBxMGU3lkbmV5MRowGAYDVQQKExFNYWVzdHJhbm8gUHR5IEx0ZDEWMBQG\nA1UEAxMNbWFlc3RyYW5vLmNvbTEkMCIGCSqGSIb3DQEJARYVc3VwcG9ydEBtYWVz\ndHJhbm8uY29tMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQD3feNNn2xfEz5/\nQvkBIu2keh9NNhobpre8U4r1qC7h7OeInTldmxGL4cLHw4ZAqKbJVrlFWqNevM5V\nZBkDe4mjuVkK6rYK1ZK7eVk59BicRksVKRmdhXbANk/C5sESUsQv1wLZyrF5Iq8m\na9Oy4oYrIsEF2uHzCouTKM5n+O4DkwIDAQABo4HuMIHrMB0GA1UdDgQWBBSd/X0L\n/Pq+ZkHvItMtLnxMCAMdhjCBuwYDVR0jBIGzMIGwgBSd/X0L/Pq+ZkHvItMtLnxM\nCAMdhqGBjKSBiTCBhjELMAkGA1UEBhMCQVUxDDAKBgNVBAgTA05TVzEPMA0GA1UE\nBxMGU3lkbmV5MRowGAYDVQQKExFNYWVzdHJhbm8gUHR5IEx0ZDEWMBQGA1UEAxMN\nbWFlc3RyYW5vLmNvbTEkMCIGCSqGSIb3DQEJARYVc3VwcG9ydEBtYWVzdHJhbm8u\nY29tggkA8WlwfatbSnIwDAYDVR0TBAUwAwEB/zANBgkqhkiG9w0BAQUFAAOBgQDE\nhe/18oRh8EqIhOl0bPk6BG49AkjhZZezrRJkCFp4dZxaBjwZTddwo8O5KHwkFGdy\nyLiPV326dtvXoKa9RFJvoJiSTQLEn5mO1NzWYnBMLtrDWojOe6Ltvn3x0HVo/iHh\nJShjAn6ZYX43Tjl1YXDd1H9O+7/VgEWAQQ32v8p5lA==\n-----END CERTIFICATE-----";

        private const string IdpDefaultNameId = "urn:oasis:names:tc:SAML:2.0:nameid-format:persistent";
        private const string SamlIdpPath = "/api/v1/auth/saml";
        private const string IdpLogoutPath = "/app_logout";
        private const string IdpUnauthorizedPath = "/app_access_unauthorized";

        private string presetName;

        /// <summary>
        /// Load Sso configuration into a Sso configuration object
        /// </summary>
        /// <returns>A Sso configuration object</returns>
        public static Sso Load(string preset = "maestrano")
        {
            ConfigurationManager.RefreshSection(preset + "/sso");
            var config = ConfigurationManager.GetSection(preset + "/sso") as Sso;
            if (config == null) config = new Sso();
            config.presetName = preset;

            return config;
        }

        /// <summary>
        /// Load configuration into a Sso configuration object from a JObject 
        /// </summary>
        /// <returns>A Sso configuration object</returns>
        public static Sso LoadFromJson(string preset, JObject obj)
        {
            var config = new Sso();
            config.presetName = preset;
            config.InitPath = obj["init_path"].Value<String>();
            config.ConsumePath = obj["consume_path"].Value<String>();
            config.Idm = obj["idm"].Value<String>();
            config.Idp = obj["idp"].Value<String>();
            config.X509Fingerprint = obj["x509_fingerprint"].Value<String>();
            config.X509Certificate = obj["x509_certificate"].Value<String>();
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
        /// Is Single Sign-On enabled - useful for debugging
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled
        {
            get { return (Boolean)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Is Single Logout enabled - useful for debugging
        /// </summary>
        [ConfigurationProperty("sloEnabled", DefaultValue = true, IsRequired = false)]
        public bool SloEnabled
        {
            get { return (Boolean)this["sloEnabled"]; }
            set { this["sloEnabled"] = value; }
        }

        /// <summary>
        /// SSO user creation mode: 'real' or 'virtual'
        /// </summary>
        [ConfigurationProperty("creationMode", DefaultValue = "virtual", IsRequired = false)]
        public string CreationMode
        {
            get { return (String)this["creationMode"]; }
            set { this["creationMode"] = value; }
        }

        /// <summary>
        /// Path to init action
        /// </summary>
        [ConfigurationProperty("initPath", IsRequired = false)]
        public string InitPath
        {
            get { return (String)this["initPath"]; }
            set { this["initPath"] = value; }
        }

        /// <summary>
        /// Path to consume action
        /// </summary>
        [ConfigurationProperty("consumePath", IsRequired = false)]
        public string ConsumePath
        {
            get { return (String)this["consumePath"]; }
            set { this["consumePath"] = value; }
        }

        /// <summary>
        /// Address of the identity manager (for your application)
        /// </summary>
        [ConfigurationProperty("idm", DefaultValue = null, IsRequired = false)]
        public string Idm
        {
            get
            {
                var _idm = (String)this["idm"];
                if (string.IsNullOrEmpty(_idm))
                {
                    if (!string.IsNullOrEmpty(MnoHelper.With(this.presetName).App.Host))
                    {
                        return MnoHelper.App.Host;
                    }
                    return "localhost";
                }
                return _idm;
            }

            set { this["idm"] = value; }
        }

        /// <summary>
        /// Address of the identity provider
        /// </summary>
        [ConfigurationProperty("idp", DefaultValue = null, IsRequired = false)]
        public string Idp
        {
            get
            {
                var _idp = (String)this["idp"];
                if(string.IsNullOrEmpty(_idp)) {
                    return ProdIdpHost;
                }
                return _idp;
            }

            set { this["idp"] = value; }
        }


        /// <summary>
        /// The nameid format for SAML handshake
        /// </summary>
        [ConfigurationProperty("nameIdFormat", DefaultValue = IdpDefaultNameId, IsRequired = false)]
        public string NameIdFormat
        {
            get { return (String)this["nameIdFormat"]; }
            set { this["nameIdFormat"] = value; }
        }


        /// <summary>
        /// Fingerprint of x509 certificate used for SAML
        /// </summary>
        [ConfigurationProperty("x509Fingerprint", DefaultValue = null, IsRequired = false)]
        public string X509Fingerprint
        {
            get
            {
                var _x509fingerprint = (String)this["x509Fingerprint"];
                if(string.IsNullOrEmpty(_x509fingerprint)) {
                    return ProdX509CertFootprint;
                }
                return _x509fingerprint;
            }

            set { this["x509Fingerprint"] = value; }
        }

        // Actual x509 certificate
        [ConfigurationProperty("x509Certificate", DefaultValue = null, IsRequired = false)]
        public string X509Certificate
        {
            get
            {
                var _509certificate = (String)this["x509Certificate"];
                if (string.IsNullOrEmpty(_509certificate))
                {
                    return ProdX509Cert;
                }
                return _509certificate;
            }

            set { this["x509Certificate"] = value; }
        }

        /// <summary>
        /// Return the IDP url to be used for SSO handshake
        /// </summary>
        /// <returns></returns>
        public string IdpUrl()
        {
            return Idp + SamlIdpPath;
        }

        /// <summary>
        /// Return the complete SSO init url for this application
        /// </summary>
        /// <returns></returns>
        public string InitUrl()
        {
            return Idm + InitPath;
        }

        /// <summary>
        /// Return the complete SSO consume url for this application
        /// </summary>
        public string ConsumeUrl()
        {
            return Idm + ConsumePath;
        }

        /// <summary>
        /// return the Maestrano logout url to be used for redirecting a user after logout
        /// </summary>
        [Obsolete("LogoutUrl is deprecated, please use LogoutUrl(userUid) instead.")]
        public string LogoutUrl()
        {
            return Idp + IdpLogoutPath;
        }

        /// <summary>
        /// return the Maestrano logout url to be used for redirecting a user after logout
        /// </summary>
        public string LogoutUrl(Account.User user)
        {
            return LogoutUrl(user.Id);
        }

        /// <summary>
        /// return the Maestrano logout url to be used for redirecting a user after logout
        /// </summary>
        public string LogoutUrl(String  userUid)
        {
            return Idp + IdpLogoutPath + "?user_uid=" + userUid;
        }



        /// <summary>
        /// Return the Maestrano unauthorized page url to
        /// be used when a user is denied access to the application.
        /// Should not need to be used as Maestrano evaluates access
        /// permissions during SSO handshake.
        /// </summary>
        /// <returns></returns>
        public string UnauthorizedUrl()
        {
            return Idp + IdpUnauthorizedPath;
        }

        /// <summary>
        /// Build the url used for user session check
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public string SessionCheckUrl(String uid, String session)
        {
            string url = Idp + SamlIdpPath;
            url += "/" + uid + "?session=" + session;

            return url;
        }

        /// <summary>
        /// Return the SAML Settings to be used by the SAML Request
        /// and Response classes
        /// </summary>
        /// <returns></returns>
        public Saml.Settings SamlSettings()
        {
            Saml.Settings settings = new Saml.Settings();
            settings.AssertionConsumerServiceUrl = ConsumeUrl();
            settings.IdpSsoTargetUrl = IdpUrl();
            settings.IdpCertificate = X509Certificate;
            settings.Issuer = MnoHelper.With(presetName).Api.Id;
            settings.NameIdentifierFormat = NameIdFormat;

            return settings;
        }

        /// <summary>
        /// Build a SAML Request ready to be sent
        /// You can call RedirectUrl() on the returned request
        /// and redirect the user to that url to trigger the Maestrano
        /// SSO handshake
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Saml.Request BuildRequest(NameValueCollection parameters = null)
        {
            return Saml.Request.With(presetName).New(parameters);
        }

        /// <summary>
        /// Build a Saml Response object from a base64 encoded response
        /// </summary>
        /// <param name="samlPostParam">The value of the SAMLResponse POST parameter</param>
        /// <returns></returns>
        public Saml.Response BuildResponse(String samlPostParam)
        {
            var resp = Saml.Response.With(presetName).New();
            resp.LoadXmlFromBase64(samlPostParam);

            return resp;
        }

        /// <summary>
        /// Set the maestrano user in session
        /// </summary>
        public void SetSession(HttpSessionStateBase httpSessionObj, User user)
        {
            var mnoSession = new Session(presetName, httpSessionObj, user);
            mnoSession.Save();
        }

        /// <summary>
        /// Set the maestrano user in session
        /// </summary>
        public void SetSession(HttpSessionState httpSessionObj, User user)
        {
            var mnoSession = new Session(presetName, httpSessionObj, user);
            mnoSession.Save();
        }

        /// <summary>
        /// Clear the maestrano session
        /// </summary>
        /// <param name="httpSessionObj"></param>
        public void ClearSession(HttpSessionStateBase httpSessionObj)
        {
            httpSessionObj.Remove(presetName);
        }

        /// <summary>
        /// Clear the maestrano session
        /// </summary>
        /// <param name="httpSessionObj"></param>
        public void ClearSession(HttpSessionState httpSessionObj)
        {
            httpSessionObj.Remove(presetName);
        }
    }
}
