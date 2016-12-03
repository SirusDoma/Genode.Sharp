﻿#region MIT License
/*Copyright (c) 2012-2013 Robert Rouhani <robert.rouhani@gmail.com>

SharpFont based on Tao.FreeType, Copyright (c) 2003-2007 Tao Framework Team

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/
#endregion

using System;
using System.Runtime.InteropServices;

using SharpFont.TrueType.Internal;

namespace SharpFont.TrueType
{
	/// <summary>
	/// A structure used to model an SFNT ‘name’ table entry.
	/// </summary>
	/// <remarks>
	/// Possible values for ‘platform_id’, ‘encoding_id’, ‘language_id’, and ‘name_id’ are given in the file
	/// ‘ttnameid.h’. For details please refer to the TrueType or OpenType specification.
	/// </remarks>
	/// <see cref="PlatformId"/>
	/// <see cref="AppleEncodingId"/>
	/// <see cref="MacEncodingId"/>
	/// <see cref="MicrosoftEncodingId"/>
	public class SfntName
	{
		#region Fields

		private IntPtr reference;
		private SfntNameRec rec;

		#endregion

		#region Constructors

		internal SfntName(IntPtr reference)
		{
			Reference = reference;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the platform ID for ‘string’.
		/// </summary>
		[CLSCompliant(false)]
		public PlatformId PlatformId
		{
			get
			{
				return rec.platform_id;
			}
		}

		/// <summary>
		/// Gets the encoding ID for ‘string’.
		/// </summary>
		[CLSCompliant(false)]
		public ushort EncodingId
		{
			get
			{
				return rec.encoding_id;
			}
		}

		/// <summary>
		/// Gets the language ID for ‘string’.
		/// </summary>
		[CLSCompliant(false)]
		public ushort LanguageId
		{
			get
			{
				return rec.language_id;
			}
		}

		/// <summary>
		/// Gets an identifier for ‘string’.
		/// </summary>
		[CLSCompliant(false)]
		public ushort NameId
		{
			get
			{
				return rec.name_id;
			}
		}

		/// <summary><para>
		/// Gets the ‘name’ string. Note that its format differs depending on the (platform,encoding) pair. It can be a
		/// Pascal String, a UTF-16 one, etc.
		/// </para><para>
		/// Generally speaking, the string is not zero-terminated. Please refer to the TrueType specification for
		/// details.
		/// </para></summary>
		public string String
		{
			get
			{
				//TODO look at TrueType specs, interpret string based on platform, encoding pair.
				//return string.Empty;
				return Marshal.PtrToStringUni(rec.@string, (int)rec.string_len);
			}
		}

		internal IntPtr Reference
		{
			get
			{
				return reference;
			}

			set
			{
				reference = value;
				rec = PInvokeHelper.PtrToStructure<SfntNameRec>(reference);
			}
		}

		#endregion
	}
}
