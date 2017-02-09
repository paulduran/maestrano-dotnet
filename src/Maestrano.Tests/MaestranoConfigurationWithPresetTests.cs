using NUnit.Framework;
using System;

namespace Maestrano.Tests
{
    [TestFixture]
    public class MaestranoConfigurationWithPresetTests
    {



        [Test]
        public void itSetsTheHostAndIdmProperlyIfDefined()
        {
            MnoHelper.ClearPreset("sometenant");
            MnoHelper.With("sometenant").Environment = "production";

            string expected = "https://somerandomhost.com";
            MnoHelper.With("sometenant").App.Host = expected;
            MnoHelper.With("sometenant").Sso.Idm = expected;

        }

        [Test]
        public void itSetsTheApiTokenProperly()
        {
            MnoHelper.With("sometenant").Environment = "production";
            MnoHelper.With("sometenant").Api.Id = "app-1";
            MnoHelper.With("sometenant").Api.Key = "bla";

            Assert.AreEqual("app-1:bla", MnoHelper.With("sometenant").Api.Token);
        }

        [Test]
        public void itGeneratesTheMetadataWithoutError()
        {
            MnoHelper.With("sometenant").Environment = "production";
            MnoHelper.With("sometenant").Api.Id = "app-1";
            MnoHelper.With("sometenant").Api.Key = "bla";

            Assert.IsNotNull(MnoHelper.With("sometenant").ToMetadata());
        }

        [Test]
        public void Sso_ItBuildsTheRightSamlRequest()
        {
            MnoHelper.With("sometenant").Environment = "production";

            var ssoIdpUrl = MnoHelper.With("sometenant").Sso.BuildRequest(null).RedirectUrl();
            Assert.IsTrue(ssoIdpUrl.StartsWith("https://idp.sometenant.com"));
        }

        [Test]
        public void Sso_ItBuildsTheRightSamlSettings()
        {
            MnoHelper.With("sometenant").Environment = "production";
            MnoHelper.With("sometenant").Api.Id = "app-tenant1";
            MnoHelper.With("sometenant").Sso.Idp = "https://idp.sometenantspecificendpoint.com";
            MnoHelper.With("sometenant").Sso.Idm = "https://somespecificapphost.com";
            MnoHelper.With("sometenant").Sso.ConsumePath = "/somespecifictenant/auth/saml/consume";

            var samlSettings = MnoHelper.With("sometenant").Sso.SamlSettings();
            Assert.AreEqual("app-tenant1", samlSettings.Issuer);
            Assert.AreEqual("https://idp.sometenantspecificendpoint.com/api/v1/auth/saml", samlSettings.IdpSsoTargetUrl);
            Assert.AreEqual("https://somespecificapphost.com/somespecifictenant/auth/saml/consume", samlSettings.AssertionConsumerServiceUrl);
        }

    }
}
