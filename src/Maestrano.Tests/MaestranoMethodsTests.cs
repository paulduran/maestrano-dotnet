using System;
using Maestrano.Saml;
//using NUnit.Framework;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Web;
using Maestrano.Sso;
using System.Configuration;
using System.Collections.Specialized;
using System.Web.SessionState;

namespace Maestrano.Tests
{
    public class MaestranoMethodsTests
    {

        public MaestranoMethodsTests()
        {
            Helpers.destroyMnoSession();
        }

        [Test]
        public void Authenticate_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            MnoHelper.Api.Id = "app-1";
            MnoHelper.Api.Key = "somekey";

            Assert.AreEqual(true, MnoHelper.Authenticate(MnoHelper.Api.Id, MnoHelper.Api.Key));
            Assert.AreEqual(false, MnoHelper.Authenticate(MnoHelper.Api.Id, "someotherrandomkey"));
        }

        [Test]
        public void isProduction_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            Assert.IsTrue(MnoHelper.isProduction());
            MnoHelper.Environment = "production-sandbox";
            Assert.IsTrue(MnoHelper.isProduction());
            MnoHelper.Environment = "development";
            Assert.IsFalse(MnoHelper.isProduction());
            MnoHelper.Environment = "test";
            Assert.IsFalse(MnoHelper.isProduction());
        }

        [TestCase]
        public void isDevelopment_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            Assert.IsFalse(MnoHelper.isDevelopment());
            MnoHelper.Environment = "production-sandbox";
            Assert.IsFalse(MnoHelper.isDevelopment());
            MnoHelper.Environment = "development";
            Assert.IsTrue(MnoHelper.isDevelopment());
            MnoHelper.Environment = "test";
            Assert.IsTrue(MnoHelper.isDevelopment());
        }

        [TestCase]
        public void UnmaskUser_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            Assert.AreEqual("usr-1", MnoHelper.UnmaskUser("usr-1"));
            Assert.AreEqual("usr-1", MnoHelper.UnmaskUser("usr-1.cld-4"));
        }

        [TestCase]
        public void MaskUser_WhenRealMode_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            MnoHelper.Sso.CreationMode = "real";

            Assert.AreEqual("usr-1", MnoHelper.MaskUser("usr-1", "cld-1"));
        }

        [TestCase]
        public void MaskUser_WhenVirtualMode_ItReturnsTheRightValues()
        {
            MnoHelper.Environment = "production";
            MnoHelper.Sso.CreationMode = "virtual";

            Assert.AreEqual("usr-1.cld-1", MnoHelper.MaskUser("usr-1", "cld-1"));
        }

        [Test]
        public void ToMetadata_ItReturnsTheRightObject()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";
            MnoHelper.Api.Id = "app-1";
            MnoHelper.Api.Key = "somekey";

            JObject expected = new JObject(
                new JProperty("environment", MnoHelper.Environment),
                new JProperty("app", new JObject(new JProperty("host", MnoHelper.App.Host))),
                new JProperty("api", new JObject(
                    new JProperty("id", MnoHelper.Api.Id),
                    new JProperty("lang", MnoHelper.Api.Lang),
                    new JProperty("version", MnoHelper.Api.Version),
                    new JProperty("lang_version", MnoHelper.Api.LangVersion))),
                new JProperty("sso", new JObject(
                    new JProperty("enabled", MnoHelper.Sso.Enabled),
                    new JProperty("creation_mode", MnoHelper.Sso.CreationMode),
                    new JProperty("init_path", MnoHelper.Sso.InitPath),
                    new JProperty("consume_path", MnoHelper.Sso.ConsumePath),
                    new JProperty("idm", MnoHelper.Sso.Idm),
                    new JProperty("idp", MnoHelper.Sso.Idp),
                    new JProperty("name_id_format", MnoHelper.Sso.NameIdFormat),
                    new JProperty("x509_fingerprint", MnoHelper.Sso.X509Fingerprint),
                    new JProperty("x509_certificate", MnoHelper.Sso.X509Certificate))),
                new JProperty("webhook", new JObject(
                    new JProperty("account", new JObject(
                        new JProperty("groups_path", MnoHelper.Webhook.Account.GroupsPath),
                        new JProperty("group_users_path", MnoHelper.Webhook.Account.GroupUsersPath)
                        )),
                    new JProperty("connec", new JObject(
                        new JProperty("notifications_path", MnoHelper.Webhook.Connec.NotificationsPath),
                        new JProperty("subscriptions", new JObject(
                                new JProperty("accounts", MnoHelper.Webhook.Connec.Subscriptions.Accounts),
                                new JProperty("company", MnoHelper.Webhook.Connec.Subscriptions.Company),
                                new JProperty("invoices", MnoHelper.Webhook.Connec.Subscriptions.Invoices),
                                new JProperty("sales_orders", MnoHelper.Webhook.Connec.Subscriptions.SalesOrders),
                                new JProperty("purchase_orders", MnoHelper.Webhook.Connec.Subscriptions.PurchaseOrders),
                                new JProperty("quotes", MnoHelper.Webhook.Connec.Subscriptions.Quotes),
                                new JProperty("payments", MnoHelper.Webhook.Connec.Subscriptions.Payments),
                                new JProperty("journals", MnoHelper.Webhook.Connec.Subscriptions.Journals),
                                new JProperty("items", MnoHelper.Webhook.Connec.Subscriptions.Items),
                                new JProperty("organizations", MnoHelper.Webhook.Connec.Subscriptions.Organizations),
                                new JProperty("people", MnoHelper.Webhook.Connec.Subscriptions.People),
                                new JProperty("projects", MnoHelper.Webhook.Connec.Subscriptions.Projects),
                                new JProperty("tax_codes", MnoHelper.Webhook.Connec.Subscriptions.TaxCodes),
                                new JProperty("tax_rates", MnoHelper.Webhook.Connec.Subscriptions.TaxRates),
                                new JProperty("events", MnoHelper.Webhook.Connec.Subscriptions.Events),
                                new JProperty("venues", MnoHelper.Webhook.Connec.Subscriptions.Venues),
                                new JProperty("event_orders", MnoHelper.Webhook.Connec.Subscriptions.EventOrders),
                                new JProperty("work_locations", MnoHelper.Webhook.Connec.Subscriptions.WorkLocations),
                                new JProperty("pay_items", MnoHelper.Webhook.Connec.Subscriptions.PayItems),
                                new JProperty("employees", MnoHelper.Webhook.Connec.Subscriptions.Employees),
                                new JProperty("pay_schedules", MnoHelper.Webhook.Connec.Subscriptions.PaySchedules),
                                new JProperty("time_sheets", MnoHelper.Webhook.Connec.Subscriptions.TimeSheets),
                                new JProperty("time_activities", MnoHelper.Webhook.Connec.Subscriptions.TimeActivities),
                                new JProperty("pay_runs", MnoHelper.Webhook.Connec.Subscriptions.PayRuns),
                                new JProperty("pay_stubs", MnoHelper.Webhook.Connec.Subscriptions.PayStubs)
                            ))
                        ))))
            );

            Assert.AreEqual(expected.ToString(), MnoHelper.ToMetadata().ToString());
        }

        [Test]
        public void Sso_SessionCheckUrl_ItsReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";

            Assert.AreEqual(MnoHelper.Sso.Idp + "/api/v1/auth/saml/usr-1?session=somesessiontoken", MnoHelper.Sso.SessionCheckUrl("usr-1", "somesessiontoken"));
        }

        [Test]
        public void Sso_IdpUrl_ItsReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";

            Assert.AreEqual(MnoHelper.Sso.Idp + "/api/v1/auth/saml", MnoHelper.Sso.IdpUrl());
        }

        [Test]
        public void Sso_InitUrl_ItReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";

            Assert.AreEqual(MnoHelper.Sso.Idm + MnoHelper.Sso.InitPath, MnoHelper.Sso.InitUrl());
        }


        [Test]
        public void Sso_ConsumeUrl_ItReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";

            Assert.AreEqual(MnoHelper.Sso.Idm + MnoHelper.Sso.ConsumePath, MnoHelper.Sso.ConsumeUrl());
        }

        [Test]
        public void Sso_LogoutUrl_ItReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";

            Assert.AreEqual(MnoHelper.Sso.Idp + "/app_logout", MnoHelper.Sso.LogoutUrl());
        }

        [Test]
        public void Sso_UnauthorizedUrl_ItReturnsTheRightUrl()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";

            Assert.AreEqual(MnoHelper.Sso.Idp + "/app_access_unauthorized", MnoHelper.Sso.UnauthorizedUrl());
        }

        [Test]
        public void Sso_SamlSettings_ItReturnsTheRightSamlSettings()
        {
            MnoHelper.Environment = "production";
            MnoHelper.App.Host = "https://mysuperapp.com";
            MnoHelper.Api.Id = "app-1";

            Settings settings = MnoHelper.Sso.SamlSettings();

            Assert.AreEqual("https://api-hub.maestrano.com/api/v1/auth/saml", settings.IdpSsoTargetUrl);
            Assert.AreEqual(MnoHelper.Api.Id, settings.Issuer);
            Assert.AreEqual(MnoHelper.Sso.NameIdFormat, settings.NameIdentifierFormat);
            Assert.AreEqual(MnoHelper.Sso.Idm + MnoHelper.Sso.ConsumePath, settings.AssertionConsumerServiceUrl);
            Assert.AreEqual(MnoHelper.Sso.X509Certificate, settings.IdpCertificate);
        }

        [Test]
        public void Sso_BuildRequest_ItShouldReturnSamlRequest()
        {
            MnoHelper.Environment = "production";
            Assert.IsInstanceOf(typeof(Request), MnoHelper.Sso.BuildRequest());
        }

        [Test]
        public void Sso_BuildResponse_ItShouldReturnSamlRequest()
        {
            MnoHelper.Environment = "production";
            string samlResponseStr = Helpers.ReadSamlSupportFiles("Responses/response1.xml.base64");
            Assert.IsInstanceOf(typeof(Response), MnoHelper.Sso.BuildResponse(samlResponseStr));
        }

        [Test]
        public void Sso_SetSession_ItShouldSetTheUserInSession()
        {
            MnoHelper.Environment = "production";

            // Build context
            var samlResp = new SsoResponseStub();
            var att = samlResp.GetAttributes();
            var user = new User(samlResp);
            var session = Helpers.FakeHttpSessionState();
            MnoHelper.Sso.SetSession(session, user);

            // Decrypt session
            var enc = System.Text.Encoding.UTF8;
            var json = enc.GetString(Convert.FromBase64String(session["maestrano"].ToString()));
            var mnoObj = JObject.Parse(json);

            Assert.AreEqual(user.SsoSession, mnoObj.Value<String>("session"));
            Assert.AreEqual(user.Uid, mnoObj.Value<String>("uid"));
            Assert.AreEqual(user.GroupUid, mnoObj.Value<String>("group_uid"));
            Assert.AreEqual(user.SsoSessionRecheck, mnoObj.Value<DateTime>("session_recheck"));
        }

        [Test]
        public void Sso_ClearSession_ItShouldDeleteTheMaestranoSession()
        {
            MnoHelper.Environment = "production";

            // Build context
            var samlResp = new SsoResponseStub();
            var att = samlResp.GetAttributes();
            var user = new User(samlResp);
            var session = Helpers.FakeHttpSessionState();
            MnoHelper.Sso.SetSession(session, user);

            // Test
            MnoHelper.Sso.ClearSession(session);
            Assert.IsNull(session["maestrano"]);

        }
    }
}
