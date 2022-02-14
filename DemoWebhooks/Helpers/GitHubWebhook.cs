namespace DemoWebhooks.Helpers
{
    public class GitHubWebhook
    {
        const string sha256Prefix = "sha256=";
        public static bool IsGitHubSignatureValid(string payload, string signatureWithPrefix, string _gitHubWebhookSecret)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrWhiteSpace(signatureWithPrefix))
                throw new ArgumentNullException(nameof(signatureWithPrefix));

            if (signatureWithPrefix.StartsWith(sha256Prefix, StringComparison.OrdinalIgnoreCase))
            {
                var signature = signatureWithPrefix.Substring(sha256Prefix.Length);
                var secret = Encoding.ASCII.GetBytes(_gitHubWebhookSecret);
                var payloadBytes = Encoding.ASCII.GetBytes(payload);

                using (var hmacsha256 = new HMACSHA256(secret))
                {
                    var hash = hmacsha256.ComputeHash(payloadBytes);

                    var hashString = ToHexString(hash);

                    if (hashString.Equals(signature))
                        return true;
                }
            }

            return false;
        }

        public static string ToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }

    }
}
