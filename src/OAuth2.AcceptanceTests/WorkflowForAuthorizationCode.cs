﻿using System;
using System.Net;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel.Web;
using FluentAssertions;
using Moq;
using NNS.Authentication.OAuth2.Extensions;
using NUnit.Framework;
using NUnit.Mocks;

namespace NNS.Authentication.OAuth2.AcceptanceTests
{
    public class WorkflowForAuthorizationCode
    {
        private Uri _authorizationRequestUri;
        private Uri _redirectionUri;
        private string _resourceOwnerName ;
        private string _clientId;

        [SetUp]
        public void SetUp()
        {
            _resourceOwnerName = "stoeren";
            if (!ResourceOwners.ResourceOwnerExists(_resourceOwnerName))
                ResourceOwners.Add(_resourceOwnerName);

            _clientId = "268852326492238";
            _authorizationRequestUri = new Uri("http://example.com/AuthorizationRequest");
            _redirectionUri = new Uri("http://example.com/RedirectionUri");
            if (!ServersWithAuthorizationCode.ServerWithAuthorizationCodeExists(_clientId, _authorizationRequestUri, _redirectionUri))
                ServersWithAuthorizationCode.Add(_clientId, _authorizationRequestUri, _redirectionUri);
        }

        [Test]
        public void CreateServerAndUsersAndGetCorrectRedirectToAuthorizationRequest()
        {
            // Spec v2-22 4.1.1

            var resourceOwner = ResourceOwners.GetResourceOwner(_resourceOwnerName);
            var server = ServersWithAuthorizationCode.GetServerWithAuthorizationCode(_clientId, _authorizationRequestUri,
                                                                                     _redirectionUri);

            var mockContext = new Mock<IOutgoingWebResponseContext> {DefaultValue = DefaultValue.Mock};
            mockContext.SetupAllProperties();
            resourceOwner.AuthorizesMeToAccessTo(server).Should().BeFalse();


            var outgoingResponse = mockContext.Object;
            outgoingResponse.RedirectToAuthorization(server, resourceOwner);

            outgoingResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            outgoingResponse.Location.Should().NotBeNullOrEmpty();

        }

        [Test]
        public void GetAuthorizationCodeViaUserAgentAndRequestProtectedResource()
        {
            //TODO: webrequest mocken
            // diesen dann mit "Pseudo"-Auth-Code ausstatten, die SetToken(server, incommingRequest) => resoruceOwner
            // und die WebRequest.Authorize(server, resourceOwner) anschubsen
            // dabei müssen die UserCredentials richtig gesetzt sein

            var resourceOwnertmp = ResourceOwners.GetResourceOwner(_resourceOwnerName);
            var servertmp = ServersWithAuthorizationCode.GetServerWithAuthorizationCode(_clientId, _authorizationRequestUri, _redirectionUri);

            var mockContext = new Mock<IIncomingWebRequestContext> { DefaultValue = DefaultValue.Mock };
            mockContext.SetupAllProperties();
            var incommingRequest = mockContext.Object;

            incommingRequest.UriTemplateMatch.RequestUri = _redirectionUri;
            incommingRequest.UriTemplateMatch.QueryParameters.Add("code", "Splx10BeZQQYbYS6WxSbIA");
            incommingRequest.UriTemplateMatch.QueryParameters.Add("state", servertmp.Guid.ToString() + "_" + resourceOwnertmp.Guid.ToString());
            var tuple = incommingRequest.GetCredentialsFromAuthorizationRedirect();

            var server = tuple.Item1;
            var resourceOwner = tuple.Item2;

            server.Should().Be(servertmp);
            resourceOwner.Should().Be(resourceOwner);

            
            var webRequest = (HttpWebRequest) WebRequest.Create("http://example.com/ProtectedResource");
            webRequest.SignRequest(server,resourceOwner);

            //Test ob WebRequest richtig unterschrieben wurde

            Assert.Fail("Test is not completed yet");
        }
    }
}
