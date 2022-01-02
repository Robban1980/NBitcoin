using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace NBitcoin.Altcoins.Verthash.Internals
{
	public class VerthashDatFile
	{
		// Need to keep the file in RAM https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
		public static MemoryMappedFile VerthashDat { get; private set; }
		private static bool _datFileInMemory = false;
		private static string _datInMemName = "verthasdat";

		/// <summary>
		/// Big endian
		/// </summary>
		private static string _verthashDatFileHash = "0x48aa21d7afededb63976d48a8ff8ec29d5b02563af4a1110b056cd43e83155a5";
		private static uint256 _verthashDatFileHashuint256 = uint256.Parse(_verthashDatFileHash);

		private string _verthashDatFileLocation = "";


		public VerthashDatFile()
		{
			// TODO: This must be configurable some how.
			_verthashDatFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vertcoin");
		}

		public bool VerifyDatFile()
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] fileHash = null;

				if (_datFileInMemory == false)
				{
					// Load file in to memory
					LoadInRam(_verthashDatFileLocation);

					using (var viewStream = VerthashDat.CreateViewStream())
					{
						fileHash = sha256Hash.ComputeHash(viewStream);
					}
				}

				using (var viewStream = VerthashDat.CreateViewStream())
				{
					fileHash = sha256Hash.ComputeHash(viewStream);
				}

				var fileStringHash = ByteArrayToString(fileHash);

				if (fileStringHash.Equals(_verthashDatFileHash))
				{
					return true;
				}

				throw new ArgumentException($"Verthash Datafile's hash is invalid - got {fileStringHash} expected {_verthashDatFileHash}");
			}
		}

		/// <summary>
		/// Convert the array to human readable hex
		/// </summary>
		/// <param name="arrInput"></param>
		/// <returns></returns>
		public string ByteArrayToString(byte[] arrInput)
		{
			int i;
			StringBuilder sOutput = new StringBuilder(arrInput.Length);

			if (BitConverter.IsLittleEndian)
			{
				// Reverse
				Array.Reverse(arrInput);
			}

			for (i = 0; i < arrInput.Length; i++)
			{
				sOutput.Append(arrInput[i].ToString("x2"));
			}

			string result = sOutput.ToString();
			return $"0x{result}";
		}

		/// <summary>
		/// Load verthash dat file in to memory.
		/// </summary>
		public void LoadInRam(string fileLocation)
		{
			string datFile = Path.Combine(fileLocation, "verthash.dat");

			if (File.Exists(datFile))
			{
				throw new FileNotFoundException("Verthash datafile not found");
			}

			VerthashDat = MemoryMappedFile.CreateFromFile(datFile, FileMode.Open, _datInMemName);
			_datFileInMemory = true;
		}
	}
}
