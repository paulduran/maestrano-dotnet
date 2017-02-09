using NUnit.Framework;
using System;

namespace Maestrano.Tests
{
    [TestFixture]
    public class MaestranoConfigurationTests
    {

     
        [Test]
        public void itHasTheRightDefaultProductionConfig()
        {
            MnoHelper.ClearPreset("maestrano");
            MnoHelper.Environment = "production";

            // App
            Assert.AreEqual("http://localhost", MnoHelper.App.Host);

            // API
            Assert.AreEqual("https://api-hub.maestrano.com", MnoHelper.Api.Host);
            Assert.AreEqual("/api/v1/", MnoHelper.Api.Base);
            Assert.AreEqual("C#", MnoHelper.Api.Lang);
            Assert.AreEqual(MnoHelper.Version, MnoHelper.Api.Version);
            Assert.AreEqual(Environment.OSVersion.ToString() + " - " + Environment.Version.ToString(), MnoHelper.Api.LangVersion);

            // SSO
            Assert.IsTrue(MnoHelper.Sso.Enabled);
            Assert.IsTrue(MnoHelper.Sso.SloEnabled);
            Assert.AreEqual("https://api-hub.maestrano.com", MnoHelper.Sso.Idp);
            Assert.AreEqual("http://localhost", MnoHelper.Sso.Idm);
            Assert.AreEqual("virtual", MnoHelper.Sso.CreationMode);
            Assert.AreEqual("urn:oasis:names:tc:SAML:2.0:nameid-format:persistent", MnoHelper.Sso.NameIdFormat);
            Assert.AreEqual("2f:57:71:e4:40:19:57:37:a6:2c:f0:c5:82:52:2f:2e:41:b7:9d:7e", MnoHelper.Sso.X509Fingerprint);
            Assert.AreEqual("-----BEGIN CERTIFICATE-----\nMIIDezCCAuSgAwIBAgIJAPFpcH2rW0pyMA0GCSqGSIb3DQEBBQUAMIGGMQswCQYD\nVQQGEwJBVTEMMAoGA1UECBMDTlNXMQ8wDQYDVQQHEwZTeWRuZXkxGjAYBgNVBAoT\nEU1hZXN0cmFubyBQdHkgTHRkMRYwFAYDVQQDEw1tYWVzdHJhbm8uY29tMSQwIgYJ\nKoZIhvcNAQkBFhVzdXBwb3J0QG1hZXN0cmFuby5jb20wHhcNMTQwMTA0MDUyNDEw\nWhcNMzMxMjMwMDUyNDEwWjCBhjELMAkGA1UEBhMCQVUxDDAKBgNVBAgTA05TVzEP\nMA0GA1UEBxMGU3lkbmV5MRowGAYDVQQKExFNYWVzdHJhbm8gUHR5IEx0ZDEWMBQG\nA1UEAxMNbWFlc3RyYW5vLmNvbTEkMCIGCSqGSIb3DQEJARYVc3VwcG9ydEBtYWVz\ndHJhbm8uY29tMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQD3feNNn2xfEz5/\nQvkBIu2keh9NNhobpre8U4r1qC7h7OeInTldmxGL4cLHw4ZAqKbJVrlFWqNevM5V\nZBkDe4mjuVkK6rYK1ZK7eVk59BicRksVKRmdhXbANk/C5sESUsQv1wLZyrF5Iq8m\na9Oy4oYrIsEF2uHzCouTKM5n+O4DkwIDAQABo4HuMIHrMB0GA1UdDgQWBBSd/X0L\n/Pq+ZkHvItMtLnxMCAMdhjCBuwYDVR0jBIGzMIGwgBSd/X0L/Pq+ZkHvItMtLnxM\nCAMdhqGBjKSBiTCBhjELMAkGA1UEBhMCQVUxDDAKBgNVBAgTA05TVzEPMA0GA1UE\nBxMGU3lkbmV5MRowGAYDVQQKExFNYWVzdHJhbm8gUHR5IEx0ZDEWMBQGA1UEAxMN\nbWFlc3RyYW5vLmNvbTEkMCIGCSqGSIb3DQEJARYVc3VwcG9ydEBtYWVzdHJhbm8u\nY29tggkA8WlwfatbSnIwDAYDVR0TBAUwAwEB/zANBgkqhkiG9w0BAQUFAAOBgQDE\nhe/18oRh8EqIhOl0bPk6BG49AkjhZZezrRJkCFp4dZxaBjwZTddwo8O5KHwkFGdy\nyLiPV326dtvXoKa9RFJvoJiSTQLEn5mO1NzWYnBMLtrDWojOe6Ltvn3x0HVo/iHh\nJShjAn6ZYX43Tjl1YXDd1H9O+7/VgEWAQQ32v8p5lA==\n-----END CERTIFICATE-----", MnoHelper.Sso.X509Certificate);

            // Connec
            Assert.AreEqual("https://api-connec.maestrano.com", MnoHelper.Connec.Host);
            Assert.AreEqual("/api/v2", MnoHelper.Connec.BasePath);

            // Webhook
            Assert.AreEqual("/maestrano/account/groups/:id", MnoHelper.Webhook.Account.GroupsPath);
            Assert.AreEqual("/maestrano/account/groups/:group_id/users/:id", MnoHelper.Webhook.Account.GroupUsersPath);
            Assert.AreEqual("/maestrano/connec/notifications", MnoHelper.Webhook.Connec.NotificationsPath);

            // Webhook - Connec! Subscriptions
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Accounts);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Company);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Invoices);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.SalesOrders);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.PurchaseOrders);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Quotes);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Payments);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Journals);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Items);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Organizations);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.People);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Projects);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.TaxCodes);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.TaxRates);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Events);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Venues);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.EventOrders);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.WorkLocations);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.PayItems);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.Employees);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.PaySchedules);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.TimeSheets);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.TimeActivities);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.PayRuns);
            Assert.IsFalse(MnoHelper.Webhook.Connec.Subscriptions.PayStubs);

        }

        [Test]
        public void itSetsTheHostAndIdmProperlyIfDefined()
        {
            MnoHelper.ClearPreset("maestrano");
            MnoHelper.Environment = "production";

            string expected = "https://mysuperapp.com";
            MnoHelper.App.Host = expected;

            Assert.AreEqual(expected, MnoHelper.App.Host);
            Assert.AreEqual(expected, MnoHelper.Sso.Idm);
        }

        [Test]
        public void itSetsTheApiTokenProperly()
        {
            MnoHelper.Environment = "production";
            MnoHelper.Api.Id = "app-1";
            MnoHelper.Api.Key = "bla";

            Assert.AreEqual("app-1:bla", MnoHelper.Api.Token);
        }

        [Test]
        public void itGeneratesTheMetadataWithoutError()
        {
            MnoHelper.Environment = "production";
            MnoHelper.Api.Id = "app-1";
            MnoHelper.Api.Key = "bla";

            Assert.IsNotNull(MnoHelper.ToMetadata());
        }


        [Test]
        public void itRetrieveTheDevPlatformParameters()
        {
            var config = Configuration.DevPlatform.Load();
            Assert.AreEqual("https://wrong-url.com", config.Host);
            Assert.AreEqual("/and-wrong-path/too", config.ApiPath);
            Assert.AreEqual("[your environment nid]", config.Environment.Name);
            Assert.AreEqual("[your environment key]", config.Environment.ApiKey);
            Assert.AreEqual("[your environment secret]", config.Environment.ApiSecret);
        }

    }
}
