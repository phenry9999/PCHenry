using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyClassesTests;
public class TestBase
{
	public TestContext? TestContext { get; set; }
	public string OutputMessage { get; set; } = string.Empty;

	protected T GetTestSetting<T>(string name, T defaultValue)
	{
		T ret = defaultValue;

		try
		{
			var temp = TestContext?.Properties[name];

			if (temp != null)
			{
				ret = (T)Convert.ChangeType(temp, typeof(T));
			}
		}
		catch
		{
			//swallow exception and return defaultValue
		}

		return ret;
	}

	protected void WriteOutput()
	{
		TestContext?.WriteLine(OutputMessage);
	}

	protected void WriteOutput(string message)
	{
		TestContext?.WriteLine(message);
	}

	protected string GetTestName()
	{
		var ret = TestContext?.TestName ?? string.Empty;
		return ret;
	}

	protected string GetFileName(string name, string defaultValue)
	{
		string fileName = GetTestSetting<string>(name, defaultValue);
		fileName = fileName.Replace("[AppDataPath]", System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));
		WriteOutput($"Checking for File: {fileName}");
		return fileName;
	}

	protected T? GetAttribute<T>(Type typ)
	{
		string testName = GetTestName();

		Attribute? attr = typ.GetMethod(testName)?.GetCustomAttribute(typeof(T));

		if (attr != null)
		{
			return (T)Convert.ChangeType(attr, typeof(T));
		}
		else
		{
			return default;
		}
	}

	protected void WriteDescription(Type typ)
	{
		DescriptionAttribute? attr = GetAttribute<DescriptionAttribute>(typ);

		if (attr != null)
		{
			string sep = new('-', 40);
			WriteOutput($"Test Purpose: {attr.Description} {sep}");
		}
	}

	protected void WriteOwner(Type typ)
	{
		OwnerAttribute? attr = GetAttribute<OwnerAttribute>(typ);

		if (attr != null)
		{
			string sep = new('=', 40);
			WriteOutput($"Test Owner: {attr.Owner}{sep}");
		}
	}
}
