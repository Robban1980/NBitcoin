using NBitcoin.Altcoins.HashX11.Crypto.SHA3;
using Skein256 = NBitcoin.Altcoins.HashX11.Crypto.SHA3.Custom.Skein256;

namespace NBitcoin.Altcoins.GincoinInternals.Lyra2
{
	/// <summary>
	/// Implements Lyra2RE.
	/// </summary>
	public static class Lyra2RE
	{
		/// <summary>
		/// Computes a Lyra2RE hash.
		/// </summary>
		/// <param name="input">The input buffer.</param>
		/// <returns>The computed hash.</returns>
		public static byte[] ComputeHash(byte[] input)
		{
			var output = new Blake256().ComputeBytes(input).GetBytes();
			output = new Keccak256().ComputeBytes(output).GetBytes();
			output = new Lyra2(Lyra2Version.v1).ComputeBytes(32, output, output, 1, 8, 8);
			output = new Skein256().ComputeBytes(output).GetBytes();
			output = new Groestl256().ComputeBytes(output).GetBytes();
			return output;
		}

		/// <summary>
		/// Computes a Lyra2 hash and returns the resulting hash..
		/// </summary>
		/// <param name="lyra2">The <see cref="Lyra2"/> instance to use.</param>
		/// <param name="kLen">The output buffer length.</param>
		/// <param name="pwd">The password buffer.</param>
		/// <param name="salt">The salt buffer.</param>
		/// <param name="timeCost">The time cost parameter.</param>
		/// <param name="nRows">The number of rows in each block.</param>
		/// <param name="nCols">The number of columns in each block.</param>
		/// <returns>The Lyra2 hash.</returns>
		public static byte[] ComputeBytes(this Lyra2 lyra2, int kLen, byte[] pwd, byte[] salt, ulong timeCost, ulong nRows, ulong nCols)
		{
			var hash = new byte[kLen];
			lyra2.Calculate(hash, pwd, salt, timeCost, nRows, nCols);
			return hash;
		}
	}

	/// <summary>
	/// Implements Lyra2REv2.
	/// </summary>
	public static class Lyra2REv2
	{
		/// <summary>
		/// Computes a Lyra2REv2 hash.
		/// </summary>
		/// <param name="input">The input buffer.</param>
		/// <returns>The computed hash.</returns>
		public static byte[] ComputeHash(byte[] input)
		{
			var output = new Blake256().ComputeBytes(input).GetBytes();
			output = new Keccak256().ComputeBytes(output).GetBytes();
			output = new CubeHash256().ComputeBytes(output).GetBytes();
			output = new Lyra2(Lyra2Version.v2).ComputeBytes(32, output, output, 1, 4, 4);
			output = new Skein256().ComputeBytes(output).GetBytes();
			output = new CubeHash256().ComputeBytes(output).GetBytes();
			output = new BlueMidnightWish256().ComputeBytes(output).GetBytes();
			return output;
		}
	}

	/// <summary>
	/// Implements Lyra2REv3.
	/// </summary>
	public static class Lyra2REv3
	{
		/// <summary>
		/// Computes a Lyra2REv3 hash.
		/// </summary>
		/// <param name="input">The input buffer.</param>
		/// <returns>The computed hash.</returns>
		public static byte[] ComputeHash(byte[] input)
		{
			var output = new Blake256().ComputeBytes(input).GetBytes();
			output = new Lyra2(Lyra2Version.v3).ComputeBytes(32, output, output, 1, 4, 4);
			output = new CubeHash256().ComputeBytes(output).GetBytes();
			output = new Lyra2(Lyra2Version.v3).ComputeBytes(32, output, output, 1, 4, 4);
			output = new BlueMidnightWish256().ComputeBytes(output).GetBytes();
			return output;
		}
	}
}
