﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NNS.Authentication.OAuth2
{
    public class ServerWithAuthorizationCode : Server
    {
        public String ClientId { get; private set; }
        public Uri RedirectionUri { get; private set; }

        internal ServerWithAuthorizationCode(string clientId, Uri authorizationRequestUri, Uri redirectionUri)
        {
            ClientId = clientId;
            AuthorizationRequestUri = authorizationRequestUri;
            RedirectionUri = redirectionUri;
            Guid = Guid.NewGuid();
        }

        internal XElement ToXElement()
        {
            var element = new XElement("Server");
            element.Add(new XAttribute("type","AuthorizationCode"));
            element.Add(new XElement("ClientId",ClientId));
            element.Add(new XElement("AuthorizationUri",AuthorizationRequestUri.ToString()));
            element.Add(new XElement("RedirectionUri", RedirectionUri.ToString()));
            return element;
        }

        public static ServerWithAuthorizationCode FromXElement(XElement element)
        {
            var server = new ServerWithAuthorizationCode(
                element.Element("ClientId").Value,
                new Uri(element.Element("AuthorizationUri").Value),
                new Uri(element.Element("RedirectionUri").Value));
            return server;

        }
    }
}
