﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace GLSLhelper.Test
{
	[TestClass()]
	public class GlslTokenizerTests
	{
		[DataTestMethod()]
		[DynamicData(nameof(GetSingleTokenData), DynamicDataSourceType.Method)]
		public void TokenizeSingleTest(string text, TokenType expectedType)
		{
			var tokenizer = new GlslParser();
			var token = tokenizer.Tokenize(text).First();
			Assert.AreEqual(expectedType, token.Type);
			Assert.AreEqual(text, text.Substring(token.Start, token.Length));
		}

		[DataTestMethod()]
		[DynamicData(nameof(GetTokensData), DynamicDataSourceType.Method)]
		public void TokenizeTest(string text, TokenType[] expectedTypes)
		{
			var tokenizer = new GlslParser();
			var tokens = tokenizer.Tokenize(text);
			var types = tokens.Select(token => token.Type).ToArray();

			static string Print<Type>(IEnumerable<Type> types) => string.Join(',', types);
			CollectionAssert.AreEqual(types, expectedTypes, $"\n\tExpected={Print(expectedTypes)}\n\tActual={Print(types)}");
		}

		private static IEnumerable<object[]> GetSingleTokenData()
		{
			yield return new object[] { "// comment stuff", TokenType.Comment };
			yield return new object[] { "/* comment stuff\n\n # preprocessor */", TokenType.Comment };
			yield return new object[] { "max", TokenType.Function };
			yield return new object[] { "a", TokenType.Identifier };
			yield return new object[] { "_x123", TokenType.Identifier };
			yield return new object[] { "vec4", TokenType.Keyword };
			yield return new object[] { "1", TokenType.Number };
			yield return new object[] { ".1", TokenType.Number };
			yield return new object[] { "1.", TokenType.Number };
			yield return new object[] { "#warning", TokenType.Preprocessor };
			yield return new object[] { "gl_FragCoord", TokenType.Variable };
			foreach(var op in GlslSpecification.Operators)
			{
				yield return new object[] { op.ToString(), TokenType.Operator };
			}
		}

		private static IEnumerable<object[]> GetTokensData()
		{
			yield return new object[] { "//#include \"test 12.5 \\ / * löä \"\n //comment", new TokenType[] { TokenType.Comment, TokenType.Comment } };
			yield return new object[] { "//#include \"test 12.5 \\ / * löä \" //comment", new TokenType[] { TokenType.Comment } };
			yield return new object[] { "#include \"test 12.5 \\ / * löä \" //comment", new TokenType[] { TokenType.Preprocessor, TokenType.QuotedString, TokenType.Comment } };
			yield return new object[] { "#include \"test\" //comment", new TokenType[] { TokenType.Preprocessor, TokenType.QuotedString, TokenType.Comment } };
			yield return new object[] { "#version 330\\//comment", new TokenType[] { TokenType.Preprocessor, TokenType.Number, TokenType.Operator, TokenType.Comment } };
			yield return new object[] { "#version 330 \\	  //comment", new TokenType[] { TokenType.Preprocessor, TokenType.Number, TokenType.Operator, TokenType.Comment } };
			yield return new object[] { "// comment stuff\n#prepor", new TokenType[] { TokenType.Comment, TokenType.Preprocessor } };
			yield return new object[] { "#prepor\r\n/* comment stuff uniform float;\n\n*/", new TokenType[] { TokenType.Preprocessor, TokenType.Comment } };
			yield return new object[] { "#version\n uniform float test", new TokenType[] { TokenType.Preprocessor, TokenType.Keyword, TokenType.Keyword, TokenType.Identifier } };
			yield return new object[] { "gl_FragCoord = vec4(1.)", new TokenType[] { TokenType.Variable, TokenType.Operator, TokenType.Keyword, TokenType.Operator, TokenType.Number, TokenType.Operator } };
		}
	}
}