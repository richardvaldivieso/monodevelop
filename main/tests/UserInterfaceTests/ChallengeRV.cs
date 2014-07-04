//
// ChallengeRV.cs
//
// Author:
//       richardvaldivieso <${AuthorEmail}>
//
// Copyright (c) 2014 richardvaldivieso
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MonoDevelop.Core;
using MonoDevelop.Ide.Commands;
using NUnit.Framework;

namespace UserInterfaceTests
{
	public class ChallengeRV: UITestBase
	{
		public ChallengeRV ()
		{
		}

		[Test]
		public void OpenEditCompile ()
		{
			var slnFile = Ide.OpenTestSolution ("ConsoleApp-VS2010/ConsoleApplication.sln");
			var slnDir = slnFile.ParentDirectory;

			var exe = slnDir.Combine ("bin", "Debug", "XamarinChallenge.exe");
			Assert.IsFalse (File.Exists (exe));

			Ide.OpenFile (slnFile.ParentDirectory.Combine ("Program.cs"));

			Ide.BuildSolution ();
			AssertExeHasOutput (exe, "");

			Session.SelectActiveWidget ();
			for (int n = 0; n < 10; n++)
				Session.ExecuteCommand (TextEditorCommands.LineDown);
			Session.ExecuteCommand (TextEditorCommands.LineEnd);

			//List<string> s = new List<string> () {"one", "two", "three"};
			//Session.TypeText("\nList<string> s = new List<string>(){\"one\"};");
			Session.TypeText("\nConsole.WriteLine(\"Hello Xamarin\");\n");

			Ide.BuildSolution ();
			AssertExeHasOutput (exe, "Hello Xamarin");

			Ide.CloseAll ();
		}
		void AssertExeHasOutput (string exe, string expectedOutput)
		{
			var sw = new StringWriter ();
			var p = ProcessUtils.StartProcess (new ProcessStartInfo (exe), sw, sw, CancellationToken.None);
			Assert.AreEqual (0, p.Result);
			string output = sw.ToString ();

			Assert.AreEqual (expectedOutput, output.Trim ());
		}
		[Test]
		public void CreateBuildProject ()
		{
			string projectName = "XamarinChallenge";
			string projectCategory = "C#";
			string projectKind = "Console Project";

			var projectDirectory = Util.CreateTmpDir (projectName);

			Ide.CreateProject (projectName, projectCategory, projectKind, projectDirectory);

			Ide.BuildSolution ();

			Ide.CloseAll ();
		}
	}
}

