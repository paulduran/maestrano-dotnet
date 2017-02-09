using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Maestrano.Configuration
{
    public class Preset
    {
        public string Name { get; set; }

        public Sso Sso { get; private set; }
        public App App { get; private set; }
        public Api Api { get; private set; }
        public Connec Connec { get; private set; }
        public Webhook Webhook { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The preset name (e.g.: maestrano)</param>
        public Preset(string name)
        {
            Name = name;
            App = App.Load(name);
            Api = Api.Load(name);
            Connec = Connec.Load(name);
            Webhook = Webhook.Load(name);
            Sso = Sso.Load(name);
        }

        public Preset(JObject obj)
        {
            Name = obj["marketplace"].Value<string>();
            App = App.LoadFromJson(Name, obj["app"].Value<JObject>());
            Api = Api.LoadFromJson(Name, obj["api"].Value<JObject>());
            Connec = Connec.LoadFromJson(Name, obj["connec"].Value<JObject>());
            Webhook = Webhook.LoadFromJson(Name, obj["webhooks"].Value<JObject>());
            Sso = Sso.LoadFromJson(Name, obj["sso"].Value<JObject>());
        }


        /// <summary>
        /// App environment: 'test' or 'production'
        /// </summary>
        public string Environment {
            get { return App.Environment; }
            set { App.Environment = value; }
        }

        /// <summary>
        /// Check that ID and Key passed in argument match
        /// the ones configured at the API level.
        /// Used for remote authentication from Maestrano.
        /// </summary>
        /// <param name="apiId">An application ID</param>
        /// <param name="apiKey">An API Key</param>
        /// <returns>true if the authentication is successful, false otherwise</returns>
        public bool Authenticate(string apiId, string apiKey)
        {
            return Api.Id == apiId && Api.Key == apiKey;
        }

        /// <summary>
        /// Authenticate a request from Maestrano using HTTP Basic Authentication
        /// </summary>
        /// <param name="request">An HttpRequest object</param>
        /// <returns>true if the authentication is successful, false otherwise</returns>
        public bool Authenticate(System.Web.HttpRequest request)
        {
            bool authenticated = false;
            var authHeader = request.Headers["Authorization"];
            if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic",
                        StringComparison.OrdinalIgnoreCase) &&
                    authHeaderVal.Parameter != null)
                {
                    var credentials = authHeaderVal.Parameter;
                    var encoding = Encoding.GetEncoding("iso-8859-1");
                    credentials = encoding.GetString(Convert.FromBase64String(credentials));

                    int separator = credentials.IndexOf(':');
                    string apiId = credentials.Substring(0, separator);
                    string apiKey = credentials.Substring(separator + 1);
                    authenticated = Authenticate(apiId,apiKey);
                }
            }

            return authenticated;
        }

        /// <summary>
        /// Take a user uid (either real like 'usr-1' or virtual like
        /// 'usr-1.cld-3') and return the real uid part
        /// </summary>
        /// <param name="userUid">A real or virtual uid</param>
        /// <returns>Real user uid</returns>
        public string UnmaskUser(string userUid)
        {
            string[] words = userUid.Split('.');
            if (words.Length > 0)
            {
                return words.First();
            }
            return userUid;
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
        public string MaskUser(string userUid, string groupUid)
        {
            string sanitizedUserUid = UnmaskUser(userUid);

            if (Sso.CreationMode == "virtual")
            {
                return sanitizedUserUid + '.' + groupUid;
            }
            else
            {
                return sanitizedUserUid;
            }
        }

        /// <summary>
        /// Return whether the environment is production like (production or production sandbox)
        /// </summary>
        /// <returns>true for production and production-sandbox</returns>
        /// 
        [Obsolete("isProduction is deprecated")]
        public Boolean isProduction()
        {
            return Environment.Equals("production", StringComparison.InvariantCultureIgnoreCase)
                || Environment.Equals("production-sandbox", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Return whether the environment is production like (production or production sandbox)
        /// </summary>
        /// <returns>true for production and production-sandbox</returns>
        [Obsolete("isDevelopment is deprecated")]
        public Boolean isDevelopment()
        {
            return !isProduction();
        }

        /// <summary>
        /// Return a json serializable object describing the current 
        /// Maestrano configuration. The metadata will be fetched remotely
        /// by Maestrano. Note that the metadata exclude any info
        /// including an API Key.
        /// </summary>
        /// <returns>JObject which can be converted to JSON using ToString()</returns>
        public JObject ToMetadata()
        {
            JObject metadata = new JObject(
                new JProperty("environment", Environment),
                new JProperty("app", new JObject(new JProperty("host", App.Host))),
                new JProperty("api", new JObject(
                    new JProperty("id", Api.Id),
                    new JProperty("lang", Api.Lang),
                    new JProperty("version", Api.Version),
                    new JProperty("lang_version", Api.LangVersion))),
                new JProperty("sso", new JObject(
                    new JProperty("enabled", Sso.Enabled),
                    new JProperty("creation_mode", Sso.CreationMode),
                    new JProperty("init_path", Sso.InitPath),
                    new JProperty("consume_path", Sso.ConsumePath),
                    new JProperty("idm", Sso.Idm),
                    new JProperty("idp", Sso.Idp),
                    new JProperty("name_id_format", Sso.NameIdFormat),
                    new JProperty("x509_fingerprint", Sso.X509Fingerprint),
                    new JProperty("x509_certificate", Sso.X509Certificate))),
                new JProperty("webhook", new JObject(
                    new JProperty("account", new JObject(
                        new JProperty("groups_path", Webhook.Account.GroupsPath),
                        new JProperty("group_users_path", Webhook.Account.GroupUsersPath)
                        )),
                    new JProperty("connec", new JObject(
                        new JProperty("notifications_path", Webhook.Connec.NotificationsPath),
                        new JProperty("subscriptions", new JObject(
                                new JProperty("accounts", Webhook.Connec.Subscriptions.Accounts),
                                new JProperty("company", Webhook.Connec.Subscriptions.Company),
                                new JProperty("invoices", Webhook.Connec.Subscriptions.Invoices),
                                new JProperty("sales_orders", Webhook.Connec.Subscriptions.SalesOrders),
                                new JProperty("purchase_orders", Webhook.Connec.Subscriptions.PurchaseOrders),
                                new JProperty("quotes", Webhook.Connec.Subscriptions.Quotes),
                                new JProperty("payments", Webhook.Connec.Subscriptions.Payments),
                                new JProperty("journals", Webhook.Connec.Subscriptions.Journals),
                                new JProperty("items", Webhook.Connec.Subscriptions.Items),
                                new JProperty("organizations", Webhook.Connec.Subscriptions.Organizations),
                                new JProperty("people", Webhook.Connec.Subscriptions.People),
                                new JProperty("projects", Webhook.Connec.Subscriptions.Projects),
                                new JProperty("tax_codes", Webhook.Connec.Subscriptions.TaxCodes),
                                new JProperty("tax_rates", Webhook.Connec.Subscriptions.TaxRates),
                                new JProperty("events", Webhook.Connec.Subscriptions.Events),
                                new JProperty("venues", Webhook.Connec.Subscriptions.Venues),
                                new JProperty("event_orders", Webhook.Connec.Subscriptions.EventOrders),
                                new JProperty("work_locations", Webhook.Connec.Subscriptions.WorkLocations),
                                new JProperty("pay_items", Webhook.Connec.Subscriptions.PayItems),
                                new JProperty("employees", Webhook.Connec.Subscriptions.Employees),
                                new JProperty("pay_schedules", Webhook.Connec.Subscriptions.PaySchedules),
                                new JProperty("time_sheets", Webhook.Connec.Subscriptions.TimeSheets),
                                new JProperty("time_activities", Webhook.Connec.Subscriptions.TimeActivities),
                                new JProperty("pay_runs", Webhook.Connec.Subscriptions.PayRuns),
                                new JProperty("pay_stubs", Webhook.Connec.Subscriptions.PayStubs)
                            ))
                        ))))
            );

            return metadata;
        }
    }
}
