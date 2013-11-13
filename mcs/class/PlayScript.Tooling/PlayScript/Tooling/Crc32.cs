// Copyright (c) Damien Guard.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Originally published at http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net

using System;
using System.Collections.Generic;

namespace PlayScript.Tooling
{
	/// <summary>
	/// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
	/// </summary>
	/// <remarks>
	/// Crc32 should only be used for backward compatibility with older file formats
	/// and algorithms. It is not secure enough for new applications.
	/// If you need to call multiple times for the same data either use the HashAlgorithm
	/// interface or remember that the result of one Compute call needs to be ~ (XOR) before
	/// being passed in as the seed for the next Compute call.
	/// </remarks>
	public static class Crc32
	{
		public const uint DefaultPolynomial = 0xedb88320u;
		public const uint DefaultSeed = 0xffffffffu;

		public static uint[] Table;

		public static void InitializeTable()
		{
			uint polynomial = DefaultPolynomial;
			var createTable = new uint[256];
			for (var i = 0; i < 256; i++)
			{
				var entry = (uint)i;
				for (var j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
				else
					entry = entry >> 1;
				createTable[i] = entry;
			}

			Table = createTable;
		}

		public static uint Calculate(string buffer)
		{
			if (buffer == null)
				return 0;
			if (Table == null)
				Table = InitializeTable (DefaultPolynomial);
			uint crc = DefaultSeed;
			int size = buffer.Length;
			for (var i = 0; i < size; i++)
				crc = (crc >> 8) ^ Table[(byte)buffer[i] ^ crc & 0xff];
			return ~crc;
		}

	}
}