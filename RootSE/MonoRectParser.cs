using System;
using System.Globalization;
using System.Windows;

namespace RootSE
{
	static class MonoRectParser
	{
		public static Rect parse(string source)
		{
			var tokenizerHelper = new TokenizerHelper(source);
			string str = tokenizerHelper.NextTokenRequired();
			Rect rect = str != "Empty"
				? new Rect(Convert.ToDouble(str, CultureInfo.InvariantCulture),
					Convert.ToDouble(tokenizerHelper.NextTokenRequired(), CultureInfo.InvariantCulture),
					Convert.ToDouble(tokenizerHelper.NextTokenRequired(), CultureInfo.InvariantCulture),
					Convert.ToDouble(tokenizerHelper.NextTokenRequired(), CultureInfo.InvariantCulture))
				: Rect.Empty;
			tokenizerHelper.LastTokenRequired();
			return rect;
		}

		internal class TokenizerHelper
		{
			private char _quoteChar;
			private char _argSeparator;
			private string _str;
			private int _strLen;
			private int _charIndex;
			internal int _currentTokenIndex;
			internal int _currentTokenLength;
			private bool _foundSeparator;

			internal bool FoundSeparator
			{
				get
				{
					return this._foundSeparator;
				}
			}

			internal TokenizerHelper(string str)
			{
				char numericListSeparator = ',';
				this.Initialize(str, '\'', numericListSeparator);
			}

			internal TokenizerHelper(string str, char quoteChar, char separator)
			{
				this.Initialize(str, quoteChar, separator);
			}

			private void Initialize(string str, char quoteChar, char separator)
			{
				this._str = str;
				this._strLen = str == null ? 0 : str.Length;
				this._currentTokenIndex = -1;
				this._quoteChar = quoteChar;
				this._argSeparator = separator;
				while (this._charIndex < this._strLen && char.IsWhiteSpace(this._str, this._charIndex))
					++this._charIndex;
			}

			internal string GetCurrentToken()
			{
				if (this._currentTokenIndex < 0)
					return (string)null;
				else
					return this._str.Substring(this._currentTokenIndex, this._currentTokenLength);
			}

			internal void LastTokenRequired()
			{
				if (this._charIndex == this._strLen)
					return;
				throw new InvalidOperationException("TokenizerHelperExtraDataEncountered");
			}

			internal bool NextToken()
			{
				return this.NextToken(false);
			}

			internal string NextTokenRequired()
			{
				if (this.NextToken(false))
					return this.GetCurrentToken();
				throw new InvalidOperationException("TokenizerHelperPrematureStringTermination");
			}

			internal string NextTokenRequired(bool allowQuotedToken)
			{
				if (this.NextToken(allowQuotedToken))
					return this.GetCurrentToken();
				throw new InvalidOperationException("TokenizerHelperPrematureStringTermination");
			}

			internal bool NextToken(bool allowQuotedToken)
			{
				return this.NextToken(allowQuotedToken, this._argSeparator);
			}

			internal bool NextToken(bool allowQuotedToken, char separator)
			{
				this._currentTokenIndex = -1;
				this._foundSeparator = false;
				if (this._charIndex >= this._strLen)
					return false;
				char ch = this._str[this._charIndex];
				int num1 = 0;
				if (allowQuotedToken && (int)ch == (int)this._quoteChar)
				{
					++num1;
					++this._charIndex;
				}
				int num2 = this._charIndex;
				int num3 = 0;
				while (this._charIndex < this._strLen)
				{
					char c = this._str[this._charIndex];
					if (num1 > 0)
					{
						if ((int)c == (int)this._quoteChar)
						{
							--num1;
							if (num1 == 0)
							{
								++this._charIndex;
								break;
							}
						}
					}
					else if (char.IsWhiteSpace(c) || (int)c == (int)separator)
					{
						if ((int)c == (int)separator)
						{
							this._foundSeparator = true;
							break;
						}
						else
							break;
					}
					++this._charIndex;
					++num3;
				}
				if (num1 > 0)
				{
					throw new InvalidOperationException("TokenizerHelperMissingEndQuote");
				}
				else
				{
					this.ScanToNextToken(separator);
					this._currentTokenIndex = num2;
					this._currentTokenLength = num3;
					if (this._currentTokenLength >= 1)
						return true;
					throw new InvalidOperationException("TokenizerHelperEmptyToken");
				}
			}

			private void ScanToNextToken(char separator)
			{
				if (this._charIndex >= this._strLen)
					return;
				char c1 = this._str[this._charIndex];
				if ((int)c1 != (int)separator && !char.IsWhiteSpace(c1))
				{
					throw new InvalidOperationException("TokenizerHelperExtraDataEncountered");
				}
				else
				{
					int num = 0;
					while (this._charIndex < this._strLen)
					{
						char c2 = _str[_charIndex];
						if (c2 == separator)
						{
							_foundSeparator = true;
							++num;
							++_charIndex;
							if (num > 1)
								throw new InvalidOperationException("TokenizerHelperEmptyToken");
						}
						else if (char.IsWhiteSpace(c2))
							++_charIndex;
						else
							break;
					}
					if (num <= 0 || _charIndex < this._strLen)
						return;
					throw new InvalidOperationException("TokenizerHelperEmptyToken");
				}
			}

			internal static char GetNumericListSeparator(IFormatProvider provider)
			{
				char ch = ',';
				NumberFormatInfo instance = NumberFormatInfo.GetInstance(provider);
				if (instance.NumberDecimalSeparator.Length > 0 && (int)ch == (int)instance.NumberDecimalSeparator[0])
					ch = ';';
				return ch;
			}
		}
	}
}
