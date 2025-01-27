using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyClassesTests;
public class TestConstants
{
	public const string GOOD_FILE_NAME = @"[AppDataPath]\TestFile.TestConstants.txt";
	public const string BAD_FILE_NAME = @"C:\BogusNameDoesNotExist.txt";
	public const string EMPTY_FILE_FAIL_MSG = "The call to fhe FileExists() method did NOT throw an ArgumentNullException and it SHOULD have.";
	public const string EMPTY_FILE_MSG = "Checking for an empty file name.";
}
