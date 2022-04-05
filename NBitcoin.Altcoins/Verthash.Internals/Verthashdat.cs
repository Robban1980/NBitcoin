using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;

namespace NBitcoin.Altcoins.Verthash.Internals
{
	public sealed class Verthashdat : IDisposable
	{
		private long _fileSize = 0;
		public long FileSize
		{
			get
			{
				return _fileSize;
			}
		}

		private bool _datFileInMemory = false;
		public bool DatFileInMemory { get { return _datFileInMemory; } }

		// Need to keep the file in RAM https://docs.microsoft.com/en-us/dotnet/standard/io/memory-mapped-files
		private MemoryMappedFile _verthashFile;
		public MemoryMappedFile VerthashFile { get { return _verthashFile; } }

		private string _datInMemName = "verthasdat";

		/// <summary>
		/// Big endian
		/// </summary>
		private static string _verthashDatFileHash = "0x48aa21d7afededb63976d48a8ff8ec29d5b02563af4a1110b056cd43e83155a5";

		private string _verthashDatFileLocation = "";
		private static Verthashdat? _instance;

		private Verthashdat()
		{
			// TODO: This must be configurable some how.
			_verthashDatFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vertcoin");
		}

		private Verthashdat(string customPath)
		{
			// TODO: This must be configurable some how.
			_verthashDatFileLocation = customPath;
		}

		public static Verthashdat GetInstance(string? customPath = null)
		{
			if (_instance == null)
			{
				if (string.IsNullOrEmpty(customPath))
				{
					_instance = new Verthashdat();
				}
				else
				{
					_instance = new Verthashdat(customPath);
				}
			}

			return _instance;
		}

		public bool VerifyDatFile()
		{
			using (SHA256 sha256Hash = SHA256.Create())
			{
				byte[] fileHash = null;

				if (_datFileInMemory == false)
				{
					// Load file in to memory
					LoadInRam();

					using (var viewStream = _verthashFile.CreateViewStream())
					{
						fileHash = sha256Hash.ComputeHash(viewStream);
					}
				}

				using (var viewStream = _verthashFile.CreateViewStream())
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
		public void LoadInRam()
		{
			string datFile = Path.Combine(_verthashDatFileLocation, "verthash.dat");

			if (File.Exists(datFile) == false)
			{
				throw new FileNotFoundException("Verthash datafile not found");
			}

			using (FileStream fs = new FileStream(datFile, FileMode.Open, FileAccess.Read))
			{
				_verthashFile = MemoryMappedFile.CreateFromFile(fs, _datInMemName, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, false);
				_fileSize = fs.Length;
			}
			_datFileInMemory = true;
		}

		public void Unload()
		{
			if (_verthashFile != null)
			{
				_verthashFile.Dispose();
			}
		}

		public void Dispose()
		{
			Unload();
		}
	}
}
