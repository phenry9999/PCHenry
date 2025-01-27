using System.Security.Cryptography.X509Certificates;
using FileProcesses;

namespace MyClassesTests;

[TestClass]
public class FileProcessTests : TestBase
{
	[ClassInitialize]
	public static void ClassInitialize(TestContext tc)
	{
		tc.WriteLine("In the FileProcessTests ClassInitialize() method");
	}

	[ClassCleanup]
	public static void ClassCleanup()
	{
	}

	[TestInitialize]
	public void TestInitialize()
	{
		TestContext?.WriteLine("In the TestInitialize() method");
		WriteDescription(this.GetType());
		WriteOwner(this.GetType());

		if (GetTestName() == "FileNameDoesExist")
		{
			string fileName = GetFileName("GoodFileName", TestConstants.GOOD_FILE_NAME);

			WriteOutput($"Creating the file: {fileName}");
			File.AppendAllText(fileName, "Some Text");
		}
	}

	[TestCleanup]
	public void TestCleanup()
	{
		TestContext?.WriteLine("In the TestCleanup() method");

		if (GetTestName() == "FileNameDoesExist")
		{
			string fileName = GetFileName("GoodFileName", TestConstants.GOOD_FILE_NAME);

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			WriteOutput($"Deleting the file: {fileName}");
		}
	}

	[TestMethod]
	[Description("Check to see if a file does not exists.")]
	[Priority(3)]
	[TestCategory("Smoketest")]
	[TestCategory("NoException")]
	public void FileNameDoesExist()
	{
		FileProcess fp = new();
		bool fromCall;
		string fileName = GetFileName("GoodFileName", TestConstants.GOOD_FILE_NAME);

		fromCall = fp.FileExists(fileName);

		Assert.IsTrue(fromCall, "File '{0}' does NOT exist", fileName);
	}

	[TestMethod]
	[Description("Check to see if a file exists.")]
	[Owner("Tom")]
	[Priority(2)]
	[TestCategory("NoException")]
	public void FileNameDoesNotExist()
	{
		FileProcess fp = new();

		string fileName = GetFileName("BadFileName", TestConstants.BAD_FILE_NAME);
		bool fromCall;

		WriteOutput($"Checking for File: {fileName}");

		fromCall = fp.FileExists(fileName);

		Assert.IsFalse(fromCall);
	}

	[TestMethod]
	[Owner("Dick")]
	[Priority(99)]
	[TestCategory("Exeption")]
	[Description("Check for a thrown ArgumentNullException using the try-catch block")]
	public void FileNameNullOrEmpty_UsingTryCatch_ShouldThrowArgumentNullException()
	{
		FileProcess fp;
		string fileName = string.Empty;
		bool fromCall = false;

		try
		{
			fp = new();
			WriteOutput(TestConstants.EMPTY_FILE_MSG);
			fromCall = fp.FileExists(fileName);

			Assert.Fail(GetTestSetting<string>("EmptyFileFailMsg", TestConstants.EMPTY_FILE_MSG));
		}
		catch (ArgumentException)
		{
			Assert.IsFalse(fromCall);
		}
	}

	[TestMethod]
	[Owner("Dick")]
	[Priority(1)]
	[TestCategory("Exeption")]
	[ExpectedException(typeof(ArgumentNullException))]
	public void FileNameNullOrEmpty_UsingAttribute_ShouldThrowArgumentNullException()
	{
		FileProcess fp;
		string fileName = string.Empty;
		bool fromCall = false;

		fp = new();
		WriteOutput(TestContext?.Properties["EmptyFileMsg"]?.ToString() ?? TestConstants.EMPTY_FILE_MSG);
		fromCall = fp.FileExists(fileName);

		Assert.Fail(GetTestSetting<string>("EmptyFileFailMsg", TestConstants.EMPTY_FILE_MSG));
	}

	[TestMethod]
	[Timeout(3000)]
	public void SimulateTimeoutFinishesBefore()
	{
		Thread.Sleep(2000);

		Assert.IsTrue(true);
	}

	[Ignore]
	[TestMethod]
	[Timeout(3000)]
	public void SimulateTimeoutFinishesAfterAndTimesout()
	{
		Thread.Sleep(4000);

		Assert.Fail("Shouldn't see this as it should timeout");
	}

	[TestMethod]
	[DeploymentItem("FileToDeploy.txt")]
	[Description("Check to see if a file does not exists using the [DeploymentItem] attribute.")]
	[Priority(3)]
	[TestCategory("Smoketest")]
	[TestCategory("NoExeption")]
	public void FileNameDoesExistUsingDeploymentItem()
	{
		FileProcess fp = new();
		bool fromCall;
		string fileName = "FileToDeploy.txt";

		WriteOutput($"Checking for file '{fileName}' in folder '{TestContext?.DeploymentDirectory}'");

		fromCall = fp.FileExists(fileName);

		Assert.IsTrue(fromCall, "File '{0}' does NOT exist", fileName);
	}

	[TestMethod]
	[Description("Check to see if a file exists.")]
	[Owner("Tom")]
	[Priority(2)]
	[TestCategory("NoExeption")]
	[DataRow("FilenameNope.txt")]
	[DataRow("NopeNotThere.txt")]
	[DataRow("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA.txt")]
	[DataRow("txt.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
	public void FileNameDoesNotExistWithDataRows(string fileNameToTest)
	{
		FileProcess fp = new();

		string fileName = fileNameToTest;
		bool fromCall;

		WriteOutput($"Checking for File: {fileName}");

		fromCall = fp.FileExists(fileName);

		Assert.IsFalse(fromCall);
	}

	[TestMethod]
	[Description("Check to see if a file exists.")]
	[Owner("Tom")]
	[Priority(2)]
	[TestCategory("NoExeption")]
	[DataRow("FilenameNope.txt", false)]
	[DataRow("NopeNotThere.txt", false)]
	[DataRow("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA.txt", false)]
	[DataRow("txt.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", false)]
	[DataRow("Test1Name.txt", false)]
	[DataRow("Test2Name.txt", false)]
	[DataRow("Test3Name.txt", false)]
	public void FileNameDoesNotExistWithDataRowsWithMultipleParameters(string fileNameToTest, bool ignoreForNow)
	{
		FileProcess fp = new();

		string fileName = fileNameToTest;
		bool fromCall;

		WriteOutput($"Checking for File: {fileName}");

		fromCall = fp.FileExists(fileName);

		Assert.IsFalse(fromCall);
	}

	[TestMethod]
	[Ignore]
	public void MakeTestFail()
	{
		Assert.Fail();
	}
}