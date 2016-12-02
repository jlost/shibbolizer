using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Shibbolizer
{
    public class ShibbolizerOptions : AuthenticationOptions
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