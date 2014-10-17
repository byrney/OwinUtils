using System;

namespace OwinUtils
{
    public interface UrlBuilder
    {
        string url(string relativeUrl);
        string root { get; }
        string path(string relativePath);
    }
}

