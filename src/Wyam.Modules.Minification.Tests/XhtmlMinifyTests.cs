﻿using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Modules.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class XhtmlMinifyTests : BaseFixture
    {
        public class ExecuteMethodTests : XhtmlMinifyTests
        {
            [Test]
            public void Minify()
            {
                // Given
                string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                string output = @"<html><head><title>Title</title></head><body><h1>Title</h1><p>This is<br />some text</p></body></html>";

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                XhtmlMinify xhtmlMinify = new XhtmlMinify();

                // When
                xhtmlMinify.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}