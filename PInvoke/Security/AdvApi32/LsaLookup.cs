using System;
using System.Runtime.InteropServices;
using Vanara.Extensions;
using static Vanara.PInvoke.NetSecApi;

namespace Vanara.PInvoke
{
	public static partial class AdvApi32
	{
		/// <summary>
		/// The LSA_OBJECT_ATTRIBUTES structure is used with the LsaOpenPolicy function to specify the attributes of the connection to the Policy object. When
		/// you call LsaOpenPolicy, initialize the members of this structure to NULL or zero because the function does not use the information.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct LSA_OBJECT_ATTRIBUTES
		{
			/// <summary>Specifies the size, in bytes, of the LSA_OBJECT_ATTRIBUTES structure.</summary>
			public int Length;

			/// <summary>Should be <c>NULL</c>.</summary>
			public IntPtr RootDirectory;

			/// <summary>Should be <c>NULL</c>.</summary>
			public IntPtr ObjectName;

			/// <summary>Should be zero.</summary>
			public int Attributes;

			/// <summary>Should be <c>NULL</c>.</summary>
			public IntPtr SecurityDescriptor;

			/// <summary>Should be <c>NULL</c>.</summary>
			public IntPtr SecurityQualityOfService;

			/// <summary>
			/// Returns a completely empty reference. This value should be used when calling <see cref="LsaOpenPolicy(string, ref LSA_OBJECT_ATTRIBUTES, LsaPolicyRights, out SafeLsaPolicyHandle)"/>.
			/// </summary>
			/// <value>An <see cref="LSA_OBJECT_ATTRIBUTES"/> instance with all members set to <c>NULL</c> or zero.</value>
			public static LSA_OBJECT_ATTRIBUTES Empty { get; } = new LSA_OBJECT_ATTRIBUTES();
		}

		/// <summary>
		/// The LSA_STRING structure is used by various Local Security Authority (LSA) functions to specify a string. Also an example of
		/// unnecessary over-engineering and re-engineering.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 8)]
		[PInvokeData("LsaLookup.h", MSDNShortId = "aa378522")]
		public struct LSA_STRING
		{
			/// <summary>
			/// Specifies the length, in bytes, of the string pointed to by the Buffer member, not including the terminating null character, if any.
			/// </summary>
			public ushort length;

			/// <summary>
			/// Specifies the total size, in bytes, of the memory allocated for Buffer. Up to MaximumLength bytes can be written into the buffer without
			/// trampling memory.
			/// </summary>
			public ushort MaximumLength;

			/// <summary>Pointer to a string. Note that the strings returned by the various LSA functions might not be null-terminated.</summary>
			public string Buffer;

			/// <summary>Initializes a new instance of the <see cref="LSA_STRING"/> struct from a string.</summary>
			/// <param name="s">The string value.</param>
			/// <exception cref="ArgumentException">String exceeds 32Kb of data.</exception>
			public LSA_STRING(string s)
			{
				if (s == null)
				{
					length = MaximumLength = 0;
					Buffer = null;
				}
				else
				{
					var l = s.Length;
					if (l >= ushort.MaxValue)
						throw new ArgumentException("String too long");
					Buffer = s;
					length = (ushort)l;
					MaximumLength = (ushort)(l + 1);
				}
			}

			/// <summary>Gets the number of characters in the string.</summary>
			public int Length => length;

			/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
			/// <returns>A <see cref="string"/> that represents this instance.</returns>
			public override string ToString() => Buffer;

			/// <summary>Performs an implicit conversion from <see cref="LSA_STRING"/> to <see cref="string"/>.</summary>
			/// <param name="value">The value.</param>
			/// <returns>The result of the conversion.</returns>
			public static implicit operator string(LSA_STRING value) => value.ToString();
		}

		/// <summary>The LSA_TRANSLATED_NAME structure is used with the LsaLookupSids function to return information about the account identified by a SID.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct LSA_TRANSLATED_NAME
		{
			/// <summary>
			/// An SID_NAME_USE enumeration value that identifies the type of SID.
			/// </summary>
			public SID_NAME_USE Use;

			/// <summary>An LSA_UNICODE_STRING structure that contains the isolated name of the translated SID. An isolated name is a user, group, or local group account name without the domain name (for example, user_name, rather than Acctg\user_name).</summary>
			public LSA_UNICODE_STRING Name;

			/// <summary>
			/// The index of an entry in a related LSA_REFERENCED_DOMAIN_LIST data structure which describes the domain that owns the account. If there is no
			/// corresponding reference domain for an entry, then DomainIndex will contain a negative value.
			/// </summary>
			public int DomainIndex;
		}

		/// <summary>Contains SIDs that are retrieved based on account names. This structure is used by the LsaLookupNames2 function.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct LSA_TRANSLATED_SID2
		{
			/// <summary>
			/// An SID_NAME_USE enumeration value that identifies the use of the SID. If this value is SidTypeUnknown or SidTypeInvalid, the rest of the
			/// information in the structure is not valid and should be ignored.
			/// </summary>
			public SID_NAME_USE Use;

			/// <summary>The complete SID of the account.</summary>
			public IntPtr Sid;

			/// <summary>
			/// The index of an entry in a related LSA_REFERENCED_DOMAIN_LIST data structure which describes the domain that owns the account. If there is no
			/// corresponding reference domain for an entry, then DomainIndex will contain a negative value.
			/// </summary>
			public int DomainIndex;

			/// <summary>Not used.</summary>
			public uint Flags;
		}

		/// <summary>A custom marshaler for functions using LSA_STRING so that managed strings can be used.</summary>
		/// <seealso cref="ICustomMarshaler"/>
		internal class LsaStringMarshaler : ICustomMarshaler
		{
			public static ICustomMarshaler GetInstance(string cookie) => new LsaStringMarshaler();

			public void CleanUpManagedData(object ManagedObj)
			{
			}

			public void CleanUpNativeData(IntPtr pNativeData)
			{
				if (pNativeData == IntPtr.Zero) return;
				Marshal.FreeCoTaskMem(pNativeData);
				pNativeData = IntPtr.Zero;
			}

			public int GetNativeDataSize() => Marshal.SizeOf(typeof(LSA_STRING));

			public IntPtr MarshalManagedToNative(object ManagedObj)
			{
				var s = ManagedObj as string;
				if (s == null) return IntPtr.Zero;
				var str = new LSA_STRING(s);
				return str.StructureToPtr(Marshal.AllocCoTaskMem, out int _);
			}

			public object MarshalNativeToManaged(IntPtr pNativeData)
			{
				if (pNativeData == IntPtr.Zero) return null;
				var ret = pNativeData.ToStructure<LSA_STRING>();
				var s = (string)ret.ToString().Clone();
				//LsaFreeMemory(pNativeData);
				return s;
			}
		}
	}
}