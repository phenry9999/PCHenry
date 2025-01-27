using System;
using System.Linq.Expressions;

namespace FloatToString;

internal class Program
{
	static void Main(string[] args)
	{

		//float[] floats = new float[] { 0.0f, 1.0f, 2.0f, 2.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.7f, 1, 8f, 1.9f };

		//string[] strings = new string[] { "0.0", "1.0", "2.0", "2.9", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9" };

		//string[] unicodes = new string[] { "\0\0\0\0", "\0\0€?", "\0\0\0@", "š™9@", "ÍÌŒ?", "š™™?", "ff¦?", "33³?", "\0\0À?", "", "", "", "" };

		//string[] unicodes2 = new string[] { "\u0000\u0000\u0000\u0000", "\u0000\u0000\u0128\u0063", "\0\0\0\u0064", "š™9@", "ÍÌŒ?", "š™™?", "ff¦?", "33³?", "\0\0À?", "", "", "", "" };


		//foreach (var u in unicodes2)
		//{
		//	Console.Write(u);
		//	Console.Write("\t\t = ");

		//	byte[] b = new byte[4];

		//	b[0] = (byte)u[0];
		//	b[1] = (byte)u[1];
		//	b[2] = (byte)u[2];
		//	b[3] = (byte)u[3];

		//	float f1 = BitConverter.ToSingle(b, 0);

		//	Console.WriteLine(f1);



		DoThisTest();


		try
		{

			DoAnotherTest();
		}
		catch
		{
			Console.WriteLine("Duh!");
		}

		Console.WriteLine();
	}

	private static void DoAnotherTest()
	{
		throw new DivideByZeroException("You can'd do that here");
	}

	private static string DoThisTest()
	{
		var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
		return now;
	}

	//foreach (string unicode in unicodes2)
	//{
	//	if (!string.IsNullOrEmpty(unicode))
	//	{
	//		byte[] bytes = new byte[4];
	//		for (int i = 0; i < 4; i++)
	//		{
	//			bytes[i] = (byte)unicode[i];
	//		}

	//		float floatValue = BitConverter.ToSingle(bytes, 0);
	//		Console.WriteLine(floatValue);
	//	}
	//}

	//Console.ReadLine();
	//}
}