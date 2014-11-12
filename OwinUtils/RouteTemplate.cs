using System;
using System.Collections.Generic;
using Microsoft.Owin;

namespace OwinUtils
{
    public class RouteTemplate
    {
        class Token
        {
            enum TokenType { literal, optional, required };
            public string name;
            private TokenType type;
            public Token(string seg)
            {
                if (!String.IsNullOrEmpty(seg))
                {
                    Parse(seg);
                }
                else
                {
                    this.type = TokenType.literal;
                    this.name = "";
                }
            }

            private void Parse(string seg)
            {
                var first = seg[0];
                switch (first)
                {
                    case '[':
                        this.type = TokenType.optional;
                        this.name = seg.Trim('[', ']');
                        break;
                    case '<':
                        this.type = TokenType.required;
                        this.name = seg.Trim('<', '>');
                        break;
                    default:
                        this.type = TokenType.literal;
                        this.name = seg;
                        break;
                }
            }

            public bool extract(string segment, IDictionary<string, object> paraDict)
            {
                switch (this.type)
                {
                    case TokenType.literal:
                        return segment == this.name;
                        break;
                    case TokenType.optional:
                        paraDict[this.name] = segment;
                        return true;
                        break;
                    case TokenType.required:
                        paraDict[this.name] = segment;
                        return !String.IsNullOrEmpty(segment);
                }
                return false;
            }

        }

        private readonly Token[] tokens;
        private readonly bool partials;
        private string template;

        public RouteTemplate(string template, bool partials)
        {
            this.template = template;
            this.partials = partials;
            if (null == template)
            {
                tokens = null;
            }
            else if (template == String.Empty) {
                this.tokens = new Token[0];
            }
            else {
                var segs = template.Split('/');
                this.tokens = new Token[segs.Length];
                for (int i = 0; i < segs.Length; i++)
                {
                    this.tokens[i] = new Token(segs[i]);
                }
            }
        }


        public new string ToString()
        {
            return this.template;
        }

        public class MatchData
        {
            public string pathMatched = "";
            public string pathRemaining = "";
        }

        public MatchData match(string path, Dictionary<string, object> paramDict)
        {
     
            int matchedSegments = 0;
            var pathSegments = path == "" ? new string[0] : path.Split('/');
            if (pathSegments.Length > this.tokens.Length && this.partials == false) {
                return null;
            }
            for (int token = 0; token < this.tokens.Length; token++)
            {
                string value;
                var pathSeg = token < pathSegments.Length ? pathSegments[token] : null;
                if (tokens[token].extract(pathSeg, paramDict)) {
                    matchedSegments += token < pathSegments.Length ? 1 : 0;
                }
                else {
                    return null;
                }
            }
            var ret = new MatchData();
            var remainingSegments = pathSegments.Length - matchedSegments;
            if (remainingSegments > 0) { // must start with a slash
                ret.pathRemaining = "/" + String.Join("/", pathSegments, matchedSegments, remainingSegments);
            }
            else {
                ret.pathRemaining = "";
            }
            ret.pathMatched = String.Join("/", pathSegments, 0, matchedSegments);
            return ret;
        }
    }
}