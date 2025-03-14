namespace ABSProject
{
    public static class Utility
    {
        public static string Normalize(string s)
        {
            s = s.ToLowerInvariant().Trim();
            if (s.StartsWith("the "))
                s = s.Substring(4);
            char[] punctuation = { ',', '.', '!', '?', ':', ';' };
            foreach (var p in punctuation)
                s = s.Replace(p.ToString(), "");
            return s;
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return (t != null) ? t.Length : 0;
            if (string.IsNullOrEmpty(t))
                return s.Length;
            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;
            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = System.Math.Min(
                        System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }
            return d[s.Length, t.Length];
        }

        public static double Similarity(string s1, string s2)
        {
            s1 = Normalize(s1);
            s2 = Normalize(s2);
            int maxLen = System.Math.Max(s1.Length, s2.Length);
            if (maxLen == 0) return 1.0;
            int distance = LevenshteinDistance(s1, s2);
            return 1.0 - (double)distance / maxLen;
        }

        public static bool IsMatch(Book a, Book b, double threshold = 0.8)
        {
            string titleA = a.media?.metadata?.title ?? "";
            string titleB = b.media?.metadata?.title ?? "";
            string authorA = a.media?.metadata?.authorName ?? "";
            string authorB = b.media?.metadata?.authorName ?? "";
            return (Similarity(titleA, titleB) >= threshold) &&
                   (Similarity(authorA, authorB) >= threshold);
        }
    }
}
