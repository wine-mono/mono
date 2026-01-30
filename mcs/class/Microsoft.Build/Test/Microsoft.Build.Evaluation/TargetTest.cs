//
// TargetTest.cs
//
// Authors:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Andres G. Aragoneses (knocte@gmail.com)
//
// (C) 2006 Marek Sieradzki
// (C) 2012 Andres G. Aragoneses
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MonoTests.Microsoft.Build.Tasks;
using NUnit.Framework;
using System.IO;
using System.Xml;

using MonoTests.Helpers;

namespace MonoTests.Microsoft.Build.Evaluation {
	[TestFixture]
	public class TargetTest {

		bool Build (string projectXml, ILogger logger)
		{
			var reader = new StringReader (projectXml);
			var xml = XmlReader.Create (reader);
			var coll = ProjectCollection.GlobalProjectCollection;
			var proj = coll.LoadProject (xml, "4.0");
			return proj.Build ("Main", new ILogger[] { logger });
		}

		TestMessageLogger CreateLogger (string projectXml)
		{
			var logger = new TestMessageLogger ();
			var result = Build (projectXml, logger);

			if (!result) {
				logger.DumpMessages ();
				Assert.Fail ("Build failed");
			}

			return logger;
		}

		void ItemGroupInsideTarget (string xml, params string[] messages)
		{
			ItemGroupInsideTarget (xml, 1, messages);
		}

		void ItemGroupInsideTarget (string xml, int expectedTargetCount, params string[] messages)
		{
			var logger = CreateLogger (xml);
			
			try
			{
				Assert.AreEqual(messages.Length, logger.NormalMessageCount, "Expected number of messages");
				for (int i = 0; i < messages.Length; i++)
					logger.CheckLoggedMessageHead (messages [i], i.ToString ());
				Assert.AreEqual(0, logger.NormalMessageCount, "Extra messages found");
				Assert.AreEqual(0, logger.WarningMessageCount, "Extra warningmessages found");
				
				Assert.AreEqual(expectedTargetCount, logger.TargetStarted, "TargetStarted count");
				Assert.AreEqual(expectedTargetCount, logger.TargetFinished, "TargetFinished count");
				Assert.AreEqual(messages.Length, logger.TaskStarted, "TaskStarted count");
				Assert.AreEqual(messages.Length, logger.TaskFinished, "TaskFinished count");
			}
			catch (AssertionException)
			{
				logger.DumpMessages();
				throw;
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void BuildProjectWithItemGroupInsideTarget ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
					<fruit Include=""apple""/>
						<fruit Include=""apricot""/>
					</ItemGroup>

					<Target Name=""Main"">
						<ItemGroup>
							<fruit Include=""raspberry"" />
						</ItemGroup>
						<Message Text=""%(fruit.Identity)""/>
					</Target>
				</Project>", "apple", "apricot", "raspberry");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void BuildProjectWithItemGroupInsideTarget2 ()
		{
			ItemGroupInsideTarget (
				@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<ItemGroup>
						<A Include='1'>
							<Sub>Foo</Sub>
						</A>
					</ItemGroup>
					<PropertyGroup>
						<Foo>Bar</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<A Include='2'>
								<Sub>$(Foo);Hello</Sub>
							</A>
						</ItemGroup>
				
						<Message Text='@(A)' />
						<Message Text='%(A.Sub)' />
					</Target>
				</Project>", "1;2", "Foo", "Bar;Hello");
		}
		
		[Test]
		public void BuildProjectWithPropertyGroupInsideTarget ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>Foo</A>
						<B>Bar</B>
					</PropertyGroup>

					<Target Name=""Main"">
						<Message Text='$(A)' />
						<PropertyGroup>
							<A>$(B)</A>
						</PropertyGroup>
						<Message Text='$(A)' />
					</Target>
				</Project>", "Foo", "Bar");
		}

		[Test]
		public void BuildProjectWithPropertyGroupInsideTarget2 ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<PropertyGroup>
						<A>Foo</A>
						<B>Bar</B>
					</PropertyGroup>

					<Target Name=""Main"">
						<Message Text='$(A)' />
						<PropertyGroup Condition='true'>
							<B Condition='false'>False</B>
						</PropertyGroup>
						<PropertyGroup Condition='true'>
							<A>$(B)</A>
						</PropertyGroup>
						<Message Text='$(A)' />
						<Message Text='$(B)' />
						<PropertyGroup>
							<A Condition='$(A) == $(B)'>Equal</A>
						</PropertyGroup>
						<Message Text='$(A)' />
					</Target>
				</Project>", "Foo", "Bar", "Bar", "Equal");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_ModifyMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Server Include='Server1'>
							<AdminContact>Mono</AdminContact>
						</Server>
						<Server Include='Server2'>
							<AdminContact>Mono</AdminContact>
						</Server>
						<Server Include='Server3'>
							<AdminContact>Root</AdminContact>
						</Server>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Server Condition=""'%(Server.AdminContact)' == 'Mono'"">
								<AdminContact>Monkey</AdminContact>
							</Server>
						</ItemGroup>
					
						<Message Text='%(Server.Identity) : %(Server.AdminContact)' />
						</Target>
					</Project>", "Server1 : Monkey", "Server2 : Monkey", "Server3 : Root");
		}

		[Test]
		public void ItemGroupInsideTarget_RemoveItem ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A;B;C;D' />
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo Remove='B' />
						</ItemGroup>

						<Message Text='@(Foo)' />
					</Target>
				</Project>", "A;C;D");
		}

		[Test]
		public void ItemGroupInsideTarget_DontKeepDuplicates ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A;B' />
						<Foo Include='C'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='D'>
							<Hello>Boston</Hello>
						</Foo>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo Include='B;C;D' KeepDuplicates='false'>
								<Hello>Boston</Hello>
							</Foo>
						</ItemGroup>
				
						<Message Text='@(Foo)' />
					</Target>
				</Project>", "A;B;C;D;B;C");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_RemoveMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>
					<PropertyGroup>
						<Foo>Hello</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Bar Include='@(Foo)' RemoveMetadata='$(Foo)' />
							<Bar Include='E'>
								<Hello>Monkey</Hello>
							</Bar>
						</ItemGroup>
				
						<Message Text='%(Bar.Identity)' Condition=""'%(Bar.Hello)' != ''""/>
					</Target>
				</Project>", "E");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_RemoveMetadata2 ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>
					<PropertyGroup>
					<Foo>Hello</Foo>
					</PropertyGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo RemoveMetadata='$(Foo)' />
							<Foo Include='E'>
								<Hello>Monkey</Hello>
							</Foo>
						</ItemGroup>
				
					<Message Text='%(Foo.Identity)' Condition=""'%(Foo.Hello)' != ''""/>
					</Target>
				</Project>", "E");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_KeepMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<Foo Include='A' />
						<Foo Include='B'>
							<Hello>World</Hello>
						</Foo>
						<Foo Include='C'>
							<Hello>Boston</Hello>
						</Foo>
						<Foo Include='D'>
							<Test>Monkey</Test>
						</Foo>
					</ItemGroup>

					<Target Name='Main'>
						<ItemGroup>
							<Foo KeepMetadata='Test' />
							<Foo Include='E'>
								<Hello>Monkey</Hello>
							</Foo>
						</ItemGroup>
				
						<Message Text='%(Foo.Identity)' Condition=""'%(Foo.Test)' != ''""/>
					</Target>
				</Project>", "D");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_UpdateMetadata ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<ItemGroup>
						<ProjectReference Include='xyz'/>
					</ItemGroup>

					<Target Name='Main' DependsOnTargets='CreateBar'>
						<Message Text='Before: $(Bar)'/>
						<ItemGroup>
							<ProjectReference>
								<AdditionalProperties>A=b</AdditionalProperties>
							</ProjectReference>
						</ItemGroup>
						<Message Text='After: $(Bar)'/>
					</Target>

					<Target Name='CreateBar'>
						<PropertyGroup>
							<Bar>Bar01</Bar>
						</PropertyGroup>
					</Target>
				</Project>", 2, "Before: Bar01", "After: Bar01");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_Batching ()
		{
			ItemGroupInsideTarget (
				@"<Project ToolsVersion=""4.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Target Name='Main'>
						<ItemGroup>
							<Foo Include='A;B' />
							<All Include='%(Foo.Identity)' />
						</ItemGroup>
						<Message Text='%(All.Identity)' />
					</Target>
				</Project>", "A", "B");
		}

		[Test]
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_Condition ()
		{
			ItemGroupInsideTarget (
				@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<PropertyGroup>
						<Summer>true</Summer>
					</PropertyGroup>
					<ItemGroup>
						<Weather Include='Sun;Rain' />
					</ItemGroup>
				
					<Target Name='Main'>
						<ItemGroup Condition=""'$(Summer)' != 'true'"">
							<Weather Include='Snow' />
						</ItemGroup>
						<Message Text='%(Weather.Identity)' />
					</Target>
				</Project>", "Sun", "Rain");
		}

		[Test]
		[Category ("NotWorking")]
		public void PropertyGroupInsideTarget_Condition ()
		{
			ItemGroupInsideTarget (
				@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
					<ItemGroup>
						<Shells Include=""/bin/sh;/bin/bash;/bin/false"" />
					</ItemGroup>

					<Target Name='Main'>
						<PropertyGroup>
							<HasBash Condition=""'%(Shells.Filename)' == 'bash'"">true</HasBash>
						</PropertyGroup>

						<ItemGroup Condition=""'$(HasBash)' == 'true'"">
							<Weather Include='Rain' />
						</ItemGroup>
						<Message Text='%(Weather.Identity)' />
					</Target>
				</Project>", "Rain");
		}

		[Test]
		// Bug #14661
		[Category ("NotWorking")]
		public void ItemGroupInsideTarget_Expression_in_Metadata ()
		{
			ItemGroupInsideTarget (
			@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""4.0"">
				<ItemGroup>
					<Foo Include='output1'>
						<Inputs>input1a;input1b</Inputs>
					</Foo>
					<Foo Include='output2'>
						<Inputs>input2a;input2b</Inputs>
					</Foo>
				</ItemGroup>

				<Target Name='Main' DependsOnTargets='_PrepareItems' Inputs='@(_Foo)' Outputs='%(Result)'>
					<Message Text='COMPILE: @(_Foo) - %(_Foo.Result)' />
				</Target>

				<Target Name='_PrepareItems'>
					<ItemGroup>
						<_Foo Include='%(Foo.Inputs)'>
							<Result>%(Foo.Identity)</Result>
						</_Foo>
					</ItemGroup>
				</Target>
			</Project>",
			3, "COMPILE: input1a;input1b - output1", "COMPILE: input2a;input2b - output2");
		}
	}
}
