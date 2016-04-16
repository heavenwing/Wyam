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
    public class JsMinifyTests : BaseFixture
    {
        public class ExecuteMethodTests : JsMinifyTests
        {
            [Test]
            public void Minify()
            {
                // Given
                // Example taken from https://raw.githubusercontent.com/douglascrockford/JSMin/master/README
                string input = @"
var is = {
    ie:      navigator.appName == 'Microsoft Internet Explorer',
    java:    navigator.javaEnabled(),
    ns:      navigator.appName == 'Netscape',
    ua:      navigator.userAgent.toLowerCase(),
    version: parseFloat(navigator.appVersion.substr(21)) ||
             parseFloat(navigator.appVersion),
    win:     navigator.platform == 'Win32'
}

is.mac = is.ua.indexOf('mac') >= 0;

if (is.ua.indexOf('opera') >= 0) {
    is.ie = is.ns = false;
    is.opera = true;
}

if (is.ua.indexOf('gecko') >= 0) {
    is.ie = is.ns = false;
    is.gecko = true;
}";
                string output = @"var is={ie:navigator.appName=='Microsoft Internet Explorer',java:navigator.javaEnabled(),ns:navigator.appName=='Netscape',ua:navigator.userAgent.toLowerCase(),version:parseFloat(navigator.appVersion.substr(21))||parseFloat(navigator.appVersion),win:navigator.platform=='Win32'}
is.mac=is.ua.indexOf('mac')>=0;if(is.ua.indexOf('opera')>=0){is.ie=is.ns=false;is.opera=true;}
if(is.ua.indexOf('gecko')>=0){is.ie=is.ns=false;is.gecko=true;}";

                // HACK: The JS minifier will use \n new-lines, but in this test Windows/VS uses \r\n for new-lines
                output = output.Replace(System.Environment.NewLine, "\n");

                IExecutionContext context = Substitute.For<IExecutionContext>();
                IDocument document = Substitute.For<IDocument>();
                document.Content.Returns(input);

                JsMinify jsMinify = new JsMinify();

                // When
                jsMinify.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received(1).GetDocument(Arg.Any<IDocument>(), Arg.Any<string>());
                context.Received().GetDocument(document, output);
            }
        }
    }
}