using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Shibbolizer
{
    public class ShibbolizerOptions : AuthenticationSchemeOptions
    {
        public string UsernameHeader { get; set; }
        public IEnumerable<string> ClaimHeaders { get; set; }
        public IEnumerable<MultiClaimHeader> MultiClaimHeaders { get; set; }
    }

    public class MultiClaimHeader
    {
        public Func<string, IEnumerable<string>> Parser { get; set; }
        public string Header { get; set; }
    }
}