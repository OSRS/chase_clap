namespace OsrsOpen.Chase.Reference.Net
{
    public static class HttpUriUtils
    {
        private static readonly char[] pathDelim = new char[] { '/' };
        private static readonly char[] queryDelim = new char[] { '?' };
        private static readonly char[] queryParamSep = new char[] { '&' };
        private static readonly char[] queryKeyValSep = new char[] { '=' };

        public static string[]? SeparateQuery(string? uri)
        {
            if (uri != null && uri.Length > 0)
            {
                string[] res = uri.Split(queryDelim);
                if (res.Length < 3)
                    return res;

                //oddly had more than 1 ? in it, so merge the rest of the parts
                string[] tmp = new string[2];
                tmp[0] = res[0];
                tmp[1] = res[1];
                for (int i = 2; i < res.Length; i++)
                {
                    if (res[i] != null)
                        tmp[1] = tmp[1] + '&' + res[i];
                    else
                        tmp[1] = tmp[1] + '&';
                }
                return tmp;
            }
            return null;
        }

        public static string[]? SplitQueryParams(string? queryString)
        {
            if (queryString != null && queryString.Length > 0)
            {
                return queryString.Split(queryParamSep);
            }
            return null;
        }

        public static KeyValuePair<string, string?>[]? KeyifyQueryParams(string[]? queryParams)
        {
            if (queryParams != null)
            {
                if (queryParams.Length > 0)
                {
                    KeyValuePair<string, string?>[] res = new KeyValuePair<string, string?>[queryParams.Length];
                    string tmp;
                    for (int i = 0; i < res.Length; i++)
                    {
                        tmp = queryParams[i];
                        if (tmp != null && tmp.Length > 0)
                        {
                            string[] vals = tmp.Split(queryKeyValSep);
                            if (vals.Length == 2)
                                res[i] = new KeyValuePair<string, string?>(vals[0], vals[1]);
                            else if (vals.Length > 2)
                            {
                                if (vals[1] == null)
                                    vals[1] = string.Empty;

                                for (int j = 2; j < vals.Length; j++)
                                {
                                    if (vals[j] != null)
                                        vals[1] = vals[1] + '=' + vals[j];
                                    else
                                        vals[1] = vals[1] + '=';
                                }
                                KeyValuePair<string, string?> t2 = new KeyValuePair<string, string?>(vals[0], vals[1]);
                            }
                            else
                                res[i] = new KeyValuePair<string, string?>(vals[i], null);
                        }
                    }
                    return res;
                }
                return Array.Empty<KeyValuePair<string, string?>>();
            }
            return null;
        }

        public static KeyValuePair<string, string?>[]? KeyifyQueryParams(string? queryString)
        {
            if (queryString != null && queryString.Length > 0)
            {
                return KeyifyQueryParams(SplitQueryParams(queryString));
            }
            return null;
        }

        public static (string[]?, KeyValuePair<string, string?>[]?) DecomposeUri(string uri)
        {
            string[]? uriQuery = SeparateQuery(uri);
            if (uriQuery != null && uriQuery.Length > 0)
            {
                return (SplitPathParts(uriQuery[0]), KeyifyQueryParams(uriQuery[1]));
            }
            return (null, null);
        }

        public static string[]? SplitPathParts(string? uriPath)
        {
            if (uriPath != null)
            {
                if (uriPath.Length > 0)
                {
                    return uriPath.Split(pathDelim);
                }
                return Array.Empty<string>();
            }
            return null;
        }

        public static int Depth(string[]? parts)
        {
            if (parts != null)
            {
                return parts.Length;
            }
            return -1;
        }

        public static int Depth((string[]?, KeyValuePair<string, string>[]?) uri)
        {
            if (uri.Item1 != null)
            {
                if (uri.Item2 != null)
                    return uri.Item1.Length + uri.Item2.Length;
                return uri.Item1.Length;
            }
            if (uri.Item2 != null)
                return uri.Item2.Length;
            return -1;
        }

        public static int DistinctKeyCount(KeyValuePair<string, string>[]? queryParams)
        {
            if (queryParams != null)
            {
                if (queryParams.Length > 0)
                {
                    HashSet<string> uniq = new HashSet<string>();
                    foreach (KeyValuePair<string, string> cur in queryParams)
                    {
                        uniq.Add(cur.Key);
                    }
                    return uniq.Count;
                }
                return 0;
            }
            return -1;
        }
    }
}
