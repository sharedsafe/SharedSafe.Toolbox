using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Toolbox.Meta;

namespace Toolbox.Mime
{
	/**
		RFC 2047

		Note: it seems that '_' are required to be converted to ' ' (space) characters, which is
		not done in the MS implementation :(
	**/

	public static class EncodedWord
	{
		public static string decode(string word)
		{

			// fast check, most words are not encoded!
			if (!word.Contains("=?"))
				return word;

			var sb = new StringBuilder();

			int current = 0;

			foreach (Match match in EncodedWordExpression.Matches(word))
			{
				string encoded = match.Value;
				string decoded = decodeFragment(encoded);

				sb.Append(word, current, match.Index - current);
				sb.Append(decoded);
				current = match.Index + match.Length;
			}

			Debug.Assert(current <= word.Length);

			if (current != word.Length)
				sb.Append(word.Substring(current, word.Length - current));

			return sb.ToString();
		}

		static readonly Regex EncodedWordExpression =
			new Regex(@"=\?[^\?]+\?[^\?]+\?[^\?]+\?=", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		/// For proper fragment decoding we use already available .NET code.

		static string decodeFragment(string fragment)
		{
			try
			{
				return DecodeEncoding(fragment) != null ? DecodeHeaderValue(fragment) : fragment;
			}
			catch (Exception e)
			{
				Log.E("decoding error of encoded fragment: {0}".format(fragment));
				Log.D(e.Message);
				return fragment;
			}
		}

		static readonly Func<string, Encoding> DecodeEncoding =
			MethodResolver.tryResolve <Func<string, Encoding>>(
				typeof(System.Net.Mime.MediaTypeNames),
				"System.Net.Mime.MimeBasePart",
				"DecodeEncoding",
				BindingFlags.NonPublic | BindingFlags.Static);

		static readonly Func<string, string> DecodeHeaderValue =
			MethodResolver.tryResolve <Func<string, string>>(
				typeof(System.Net.Mime.MediaTypeNames),
				"System.Net.Mime.MimeBasePart",
				"DecodeHeaderValue",
				BindingFlags.NonPublic | BindingFlags.Static);
	}
}

