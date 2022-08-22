using System.Diagnostics;
using System.Net;

namespace OsrsOpen.Chase.Reference.Net
{
    public static class DomainUtils
    {
        [DebuggerStepThrough]
        public static bool IsDomain(string? value)
        {
            if (value != null && value.Length > 0)
            {
                if (value[value.Length - 1] == '.')
                    value = value.Substring(0, value.Length - 1);

                string[] terms = value.Split('.');
                if (terms.Length == 4 || terms.Length == 1)
                {
                    if (IPAddress.TryParse(value, out _))
                        return false;
                }

                string term;
                for (int i = 0; i < terms.Length; i++)
                {
                    term = terms[i];
                    if (term.Length > 0 && char.IsLetterOrDigit(term[0]))
                    {
                        for (int j = 1; j < term.Length; j++)
                        {
                            if (!LegalDomainChar(term[j]))
                                return false;
                        }
                    }
                    else
                        return false;
                }
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool LegalDomainChar(char value)
        {
            return (value > 64 && value < 91) || (value > 44 && value < 58 && value != 47) || (value > 96 && value < 123); //lowercase, - . digits, uppercase
        }

        [DebuggerStepThrough]
        public static string[] Explode(string host)
        {
            return host.Split('.');
        }

        [DebuggerStepThrough]
        public static string? Normalize(string? host)
        {
            host = Clean(host);
            if (host != null && host.Length > 0)
            {
                return host.ToLowerInvariant();
            }
            return host;
        }

        [DebuggerStepThrough]
        public static string? Clean(string? host)
        {
            if (host != null && host.Length > 0)
            {
                if (host.EndsWith("."))
                    return host.Substring(0, host.Length - 1);
            }
            return host;
        }

        [DebuggerStepThrough]
        public static bool IsIp(string host)
        {
            if (host != null)
            {
                return IPAddress.TryParse(host, out _);
            }
            return false;
        }

        [DebuggerStepThrough]
        public static int GetNumLevels(string host)
        {
            int res = 1;
            for (int i = 0; i < host.Length; i++)
            {
                if (host[i] == '.')
                    res++;
            }
            return res;
        }

        [DebuggerStepThrough]
        public static int GetNumSubDomains(string host) => GetNumLevels(host);

        [DebuggerStepThrough]
        public static string GetRootDomain(string host)
        {
            int count = GetNumLevels(host);
            if (count > 2)
            {
                int sofar = 0;
                for (int i = 0; i < host.Length; i++)
                {
                    if (host[i] == '.')
                        sofar++;
                    if (count - sofar == 2)
                        return host.Substring(i + 1);
                }
            }
            return host;
        }

        [DebuggerStepThrough]
        public static string GetTLD(string host)
        {
            string[] tokens = host.Split('.');
            return tokens[tokens.Length - 1];
        }

        [DebuggerStepThrough]
        public static string? HostNameFromUrlString(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? tmp))
                {
                    string loc = tmp.Host;
                    if (!string.IsNullOrEmpty(loc))
                    {
                        if (loc.Contains(":"))
                            return loc.Substring(0, loc.IndexOf(':'));
                        return loc;
                    }
                }
                else
                {
                    if (Uri.CheckHostName(url) != UriHostNameType.Dns)
                        return null;
                    return url;
                }
            }
            return null;
        }

        [DebuggerStepThrough]
        public static int HostLength(string host)
        {
            return host.Length;
        }
    }
}
