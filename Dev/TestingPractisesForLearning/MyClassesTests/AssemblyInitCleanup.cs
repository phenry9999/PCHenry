using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClassesTests;

[TestClass]
public class AssemblyInitCleanup
{
	[AssemblyInitialize]
	public static void AssemblyInitialize(TestContext tc)
	{
		tc.WriteLine("In MyClassesTests.AssemblyInitCleanup.AssemblyInitialize");
	}

	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{

	}
}
