﻿/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/blob/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using System.Globalization;
using System.Net;
using System.Numerics;
using FirebirdSql.Data.Types;

namespace FirebirdSql.Data.Common
{
	internal static class TypeDecoder
	{
		public static decimal DecodeDecimal(object value, int scale, int sqltype)
		{
			long divisor = 1;
			var returnValue = Convert.ToDecimal(value, CultureInfo.InvariantCulture);

			if (scale < 0)
			{
				divisor = (long)Math.Pow(10, -scale);
			}

			switch (sqltype & ~1)
			{
				case IscCodes.SQL_SHORT:
				case IscCodes.SQL_LONG:
				case IscCodes.SQL_QUAD:
				case IscCodes.SQL_INT64:
					returnValue = returnValue / divisor;
					break;
			}

			return returnValue;
		}

		public static TimeSpan DecodeTime(int sqlTime)
		{
			return TimeSpan.FromTicks(sqlTime * 1000L);
		}

		public static DateTime DecodeDate(int sqlDate)
		{
			int year, month, day, century;

			sqlDate -= 1721119 - 2400001;
			century = (4 * sqlDate - 1) / 146097;
			sqlDate = 4 * sqlDate - 1 - 146097 * century;
			day = sqlDate / 4;

			sqlDate = (4 * day + 3) / 1461;
			day = 4 * day + 3 - 1461 * sqlDate;
			day = (day + 4) / 4;

			month = (5 * day - 3) / 153;
			day = 5 * day - 3 - 153 * month;
			day = (day + 5) / 5;

			year = 100 * century + sqlDate;

			if (month < 10)
			{
				month += 3;
			}
			else
			{
				month -= 9;
				year += 1;
			}

			var date = new DateTime(year, month, day);

			return date.Date;
		}

		public static bool DecodeBoolean(byte[] value)
		{
			return value[0] != 0;
		}

		public static Guid DecodeGuid(byte[] value)
		{
			var a = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(value, 0));
			var b = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(value, 4));
			var c = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(value, 6));
			var d = new[] { value[8], value[9], value[10], value[11], value[12], value[13], value[14], value[15] };
			return new Guid(a, b, c, d);
		}

		public static int DecodeInt32(byte[] value)
		{
			return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(value, 0));
		}

		public static long DecodeInt64(byte[] value)
		{
			return IPAddress.HostToNetworkOrder(BitConverter.ToInt64(value, 0));
		}

		public static FbDecFloat DecodeDec16(byte[] value)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(value);
			}
			return DecimalCodec.DecFloat16.ParseBytes(value);
		}

		public static FbDecFloat DecodeDec34(byte[] value)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(value);
			}
			return DecimalCodec.DecFloat34.ParseBytes(value);
		}

		public static BigInteger DecodeInt128(byte[] value)
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(value);
			}
			return Int128Helper.GetInt128(value);
		}
	}
}
