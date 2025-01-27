using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClassesTests;

[TestClass]
public class AssertEqualityTests
{
	[TestMethod]
	public void AreNumbersEqual()
	{
		int value1 = 42;
		int value2 = 42;

		Assert.AreEqual(value1, value2);
	}

	[TestMethod]
	public void AreNumbersNotEqualTest()
	{
		object value1 = 42L;
		object value2 = 21;

		Assert.AreNotEqual(value1, value2);
	}

	[TestMethod]
	public void AreStringsEqual()
	{
		string value1 = "Paul";
		string value2 = "Paul";

		Assert.AreEqual(value1, value2);
	}

	[TestMethod]
	public void AreStringsEqualCaseInsensitive()
	{
		string value1 = "Paul";
		string value2 = "paul";

		Assert.AreEqual(value1, value2, true);
	}

	[TestMethod]
	public void AreStringsNotEqualTest()
	{
		string value1 = "Paul";
		string value2 = "John";

		Assert.AreNotEqual(value1, value2);
	}
}
