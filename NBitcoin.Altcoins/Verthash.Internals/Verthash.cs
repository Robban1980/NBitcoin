namespace NBitcoin.Altcoins.Verthash.Internals
{
	public class Verthash
	{
		private static readonly uint HEADER_SIZE = 80;
		private static readonly uint HASH_OUT_SIZE = 32;
		private static readonly uint P0_SIZE = 64;
		private static readonly uint N_ITER = 8;
		private static readonly uint N_SUBSET P0_SIZE* N_ITER;
		private static readonly uint N_ROT = 32;
		private static readonly uint N_INDEXES = 4096;
		private static readonly uint BYTE_ALIGNMENT = 16;

		public Verthash()
		{
			
		}

		public unsafe int* Hash(int* input)
		{
			int* output;

			byte[] input_header = new byte[HEADER_SIZE];

			fixed (pInput_header = input_header)
			{

				memcpy(&pInput_header[0], input, HEADER_SIZE);
			}

			byte[] p1 = new byte[HASH_OUT_SIZE];

			sha3(&input_header[0], HEADER_SIZE, &p1[0], HASH_OUT_SIZE);

			byte[] p0 = new byte[N_SUBSET];

			for (size_t i = 0; i < N_ITER; i++)
			{
				input_header[0] += 1;
				sha3(&input_header[0], HEADER_SIZE, p0 + i * P0_SIZE, P0_SIZE);
			}

			uint32_t* p0_index = (uint32_t*)p0;
			uint32_t seek_indexes[N_INDEXES];

			for (size_t x = 0; x < N_ROT; x++)
			{
				memcpy(seek_indexes + x * (N_SUBSET / sizeof(uint32_t)), p0, N_SUBSET);
				for (size_t y = 0; y < N_SUBSET / sizeof(uint32_t); y++)
				{

					*(p0_index + y) = (*(p0_index + y) << 1) | (1 & (*(p0_index + y) >> 31));
				}
			}

			size_t datfile_sz = datFileSize;
			FILE* VerthashDatFile;

			if (!datFileInRam)
			{
				fs::path dataFile = GetDataDir() / "verthash.dat";
				if (!boost::filesystem::exists(dataFile))
				{
					throw std::runtime_error("Verthash datafile not found");
				}
				VerthashDatFile = fsbridge::fopen(dataFile.c_str(), "rb");
				fseek(VerthashDatFile, 0, SEEK_END);
				datfile_sz = ftell(VerthashDatFile);
			}

			uint32_t* p1_32 = (uint32_t*)p1;
			uint32_t value_accumulator = 0x811c9dc5;
			const uint32_t mdiv = ((datfile_sz - HASH_OUT_SIZE) / BYTE_ALIGNMENT) + 1;

			if (!datFileInRam)
			{
				size_t read_len = -1;
				for (size_t i = 0; i < N_INDEXES; i++)
				{
					const long offset = (fnv1a(seek_indexes[i], value_accumulator) % mdiv) * BYTE_ALIGNMENT;
					fseek(VerthashDatFile, offset, SEEK_SET);
					for (size_t i2 = 0; i2 < HASH_OUT_SIZE / sizeof(uint32_t); i2++)
					{
						uint32_t value = 0;
						read_len = fread(&value, sizeof(uint32_t), 1, VerthashDatFile);
						assert(read_len == 1);
						uint32_t* p1_ptr = p1_32 + i2;
						*p1_ptr = fnv1a(*p1_ptr, value);

						value_accumulator = fnv1a(value_accumulator, value);
					}
				}
			}
			else
			{
				uint32_t* blob_bytes_32 = (uint32_t*)datFile;
				for (size_t i = 0; i < N_INDEXES; i++)
				{
					const uint32_t offset = (fnv1a(seek_indexes[i], value_accumulator) % mdiv) * BYTE_ALIGNMENT / sizeof(uint32_t);
					for (size_t i2 = 0; i2 < HASH_OUT_SIZE / sizeof(uint32_t); i2++)
					{
						const uint32_t value = *(blob_bytes_32 + offset + i2);
						uint32_t* p1_ptr = p1_32 + i2;
						*p1_ptr = fnv1a(*p1_ptr, value);
						value_accumulator = fnv1a(value_accumulator, value);
					}
				}
			}

			memcpy(output, &p1[0], HASH_OUT_SIZE);
			if (!datFileInRam)
			{
				fclose(VerthashDatFile);
			}
		}
	}
}
