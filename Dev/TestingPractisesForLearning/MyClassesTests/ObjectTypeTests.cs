using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyClasses;

namespace MyClassesTests;

[TestClass]
public class ObjectTypeTests
{
	[TestMethod]
	public void AreNotSame()
	{
		Person x = new();
		Person y = new();

		Assert.AreNotSame(x, y);
	}

	[TestMethod]
	public void AreSame()
	{
		Person x = new();
		Person y = x;

		Assert.AreSame(x, y);
	}

}
