﻿using System;
using System.Web;
using Maestrano;
using NUnit.Framework;
using System.Web.SessionState;
using Newtonsoft.Json.Linq;
using Maestrano.Sso;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Moq;
using RestSharp;

namespace Maestrano.Tests.Sso
{


    [TestFixture]
    public class SessionTests
    {

        public SessionTests()
        {
            MnoHelper.Sso.SloEnabled = true;
            Helpers.destroyMnoSession();
        }

        [Test]
        public void ItContructsAnInstanceFromHttpSessionStateBaseObject()
        {
            Helpers.injectMnoSession();
            var httpSession = Helpers.FakeHttpSessionState();
            Session mnoSession = new Session("maestrano", httpSession);

            Assert.AreEqual(httpSession, mnoSession.HttpSession);
            Assert.AreEqual("usr-1", mnoSession.Uid);
            Assert.AreEqual("cld-1", mnoSession.GroupUid);
            Assert.AreEqual("sessiontoken", mnoSession.SessionToken);
            Assert.AreEqual(DateTime.Parse("2014-06-22T01:00:00Z").ToUniversalTime(), mnoSession.Recheck);
        }

        [Test]
        public void ItContructsAnInstanceFromHttpSessionStateObject()
        {
            Helpers.injectMnoSession();
            Session mnoSession = new Session("maestrano", Helpers.FakeHttpSessionState());

            Assert.AreEqual("usr-1", mnoSession.Uid);
            Assert.AreEqual("cld-1", mnoSession.GroupUid);
            Assert.AreEqual("sessiontoken", mnoSession.SessionToken);
            Assert.AreEqual(DateTime.Parse("2014-06-22T01:00:00Z").ToUniversalTime(), mnoSession.Recheck);
        }

        [Test]
        public void ItContructsAnInstanceFromHttpSessionStateObjectAndSsoUser()
        {
            // Http context
            Helpers.injectMnoSession();
            var httpSession = Helpers.FakeHttpSessionState();

            // User
            var samlResp = new SsoResponseStub();
            var user = new User(samlResp);


            Session mnoSession = new Session("maestrano", httpSession, user);

            Assert.AreEqual(httpSession, mnoSession.HttpSession);
            Assert.AreEqual(user.Uid, mnoSession.Uid);
            Assert.AreEqual(user.GroupUid, mnoSession.GroupUid);
            Assert.AreEqual(user.SsoSession, mnoSession.SessionToken);
            Assert.AreEqual(user.SsoSessionRecheck, mnoSession.Recheck);
        }

        [Test]
        public void IsRemoteCheckRequired_ItReturnsTrueIfRecheckIsBeforeNow()
        {
            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(-2);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsTrue(mnoSession.isRemoteCheckRequired());
        }

        [Test]
        public void IsRemoteCheckRequired_ItReturnsFalseIfRecheckIsAfterNow()
        {
            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();


            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsFalse(mnoSession.isRemoteCheckRequired());
        }

        [Test]
        public void PerformRemoteCheck_WhenValid_ItShouldReturnTrueAndAssignRecheckIfValid()
        {
            // Response preparation
            RestResponse response = new RestResponse();
            var datetime = DateTime.UtcNow;
            JObject respObj = new JObject(new JProperty("valid", "true"), new JProperty("recheck", datetime.ToString("s")));
            response.Content = respObj.ToString();
            response.ResponseStatus = ResponseStatus.Completed;

            // Client mock
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(c => c.Execute(It.IsAny<RestRequest>())).Returns(response);

            // Http context
            Helpers.injectMnoSession();
            var httpSession = Helpers.FakeHttpSessionState();
            Session mnoSession = new Session("maestrano", httpSession);

            // Tests
            Assert.IsTrue(mnoSession.PerformRemoteCheck(mockRestClient.Object));
            Assert.AreEqual(DateTime.Parse(datetime.ToString("s")), mnoSession.Recheck);
        }

        [Test]
        public void PerformRemoteCheck_WhenInvalid_ItShouldReturnFalseAndLeaveRecheckUnchanged()
        {
            // Response preparation
            RestResponse response = new RestResponse();
            var datetime = DateTime.UtcNow;
            JObject respObj = new JObject(new JProperty("valid", "false"), new JProperty("recheck", datetime.ToString("s")));
            response.Content = respObj.ToString();
            response.ResponseStatus = ResponseStatus.Completed;

            // Client mock
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(c => c.Execute(It.IsAny<RestRequest>())).Returns(response);

            // Http context
            Helpers.injectMnoSession();
            var httpSession = Helpers.FakeHttpSessionState();
            Session mnoSession = new Session("maestrano", httpSession);

            // Tests
            var recheck = mnoSession.Recheck;
            Assert.IsFalse(mnoSession.PerformRemoteCheck(mockRestClient.Object));
            Assert.AreEqual(DateTime.Parse(recheck.ToString("s")), mnoSession.Recheck);
        }


        [Test]
        public void Save_ItShouldSaveTheMaestranoSessionInHttpSession()
        {
            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // Create Mno session and save it
            Session mnoSession = new Session("maestrano", httpSession);
            mnoSession.SessionToken = "anothertoken";
            mnoSession.Save();

            // Decrypt session and test
            var enc = System.Text.Encoding.UTF8;
            var json = enc.GetString(Convert.FromBase64String(httpSession["maestrano"].ToString()));
            var mnoObj = JObject.Parse(json);

            Assert.AreEqual(mnoSession.SessionToken, mnoObj.Value<String>("session"));
            Assert.AreEqual(mnoSession.Uid, mnoObj.Value<String>("uid"));
            Assert.AreEqual(mnoSession.GroupUid, mnoObj.Value<String>("group_uid"));
            Assert.AreEqual(mnoSession.Recheck, mnoObj.Value<DateTime>("session_recheck"));
        }

        [Test]
        public void IsValid_WhenSloDisabled_ItShouldReturnTrue()
        {
            // Disable SLO
            MnoHelper.Sso.SloEnabled = false;

            // Response preparation (session not valid)
            RestResponse response = new RestResponse();
            var datetime = DateTime.UtcNow;
            JObject respObj = new JObject(new JProperty("valid", "false"), new JProperty("recheck", datetime.ToString("s")));
            response.Content = respObj.ToString();
            response.ResponseStatus = ResponseStatus.Completed;

            // Client mock
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(c => c.Execute(It.IsAny<RestRequest>())).Returns(response);

            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(-1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsTrue(mnoSession.IsValid());
        }

        [Test]
        public void IsValid_WhenIfSessionSpecifiedAndNoMnoSession_ItShouldReturnTrue()
        {
            // Http context
            Helpers.destroyMnoSession();
            var httpSession = Helpers.FakeHttpSessionState();

            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsTrue(mnoSession.IsValid(ifSession: true));
        }

        [Test]
        public void IsValid_WhenNoRecheckRequired_ItShouldReturnTrue()
        {
            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsTrue(mnoSession.IsValid());
        }

        [Test]
        public void IsValid_WhenRecheckRequiredAndValid_ItShouldReturnTrueAndSaveTheSession()
        {
            // Response preparation
            RestResponse response = new RestResponse();
            var datetime = DateTime.UtcNow;
            JObject respObj = new JObject(
              new JProperty("valid", "true"),
              new JProperty("recheck", datetime.ToString("s"))
              );
            response.Content = respObj.ToString();
            response.ResponseStatus = ResponseStatus.Completed;

            // Client mock
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(c => c.Execute(It.IsAny<RestRequest>())).Returns(response);

            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(-1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // Test mno session
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsTrue(mnoSession.IsValid(mockRestClient.Object));

            // Decrypt session and test recheck
            var enc = System.Text.Encoding.UTF8;
            var json = enc.GetString(Convert.FromBase64String(httpSession["maestrano"].ToString()));
            var mnoObj = JObject.Parse(json);

            Assert.AreEqual(datetime.ToString("s"), mnoObj.Value<DateTime>("session_recheck").ToString("s"));
        }

        [Test]
        public void IsValid_WhenRecheckRequiredAndInvalid_ItShouldReturnFalse()
        {
            // Response preparation
            RestResponse response = new RestResponse();
            var datetime = DateTime.UtcNow;
            JObject respObj = new JObject(new JProperty("valid", "false"), new JProperty("recheck", datetime.ToString("s")));
            response.Content = respObj.ToString();
            response.ResponseStatus = ResponseStatus.Completed;

            // Client mock
            var mockRestClient = new Mock<RestClient>();
            mockRestClient.Setup(c => c.Execute(It.IsAny<RestRequest>())).Returns(response);

            // Http context
            var recheck = DateTime.UtcNow.AddMinutes(-1);
            Helpers.injectMnoSession(recheck);
            var httpSession = Helpers.FakeHttpSessionState();

            // test
            Session mnoSession = new Session("maestrano", httpSession);
            Assert.IsFalse(mnoSession.IsValid(mockRestClient.Object));
        }
    }
}
