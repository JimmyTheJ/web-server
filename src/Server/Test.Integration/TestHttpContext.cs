using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace VueServer.Test.Integration
{
    public class TestHttpContext : HttpContext
    {
        private ISession _session;
        private ClaimsPrincipal _user;

        public TestHttpContext ()
        {
            _session = new TestSession();
            _user = new TestUserClaimsPrinciple();
        }

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override HttpRequest Request => throw new NotImplementedException();

        public override HttpResponse Response => throw new NotImplementedException();

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override ClaimsPrincipal User 
        { 
            get => _user;
            set => _user = value; 
        }

        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        
        public override ISession Session 
        { 
            get => _session; 
            set => _session = value; 
        }

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
