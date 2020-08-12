// !! this file should not be modified

using System;
using System.Runtime.InteropServices;	// so we can marshal the DRIS as a block of memory
using System.Text;						// so we can extract / set strings in the DRIS
using System.Windows.Forms;				// for MessageBox

[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack=1)]
public class DRIS
{
	// the first 4 fields are never encrypted
	public byte header1;				// should be set to "DRIS"
	public byte header2;
	public byte header3;
	public byte header4;
	// inputs
	public int size;					// size of this structure
	public int seed1;					// seed for data/dris encryption
	public int seed2;					// as above
	// (maybe encrypted from now on)
	public int function;				// specify only one function
	public int flags;					// options for the function selected. To use more than one OR them together: OPTION1 | OPTION2...
	public uint execs_decrement;		// amount by which to dec execs if we use flag: DEC_MANY_EXECS
	public int data_crypt_key_num;		// number of the key (1-3) that the dongle uses to encrypt or decrypt user data
	public int rw_offset;				// offset in the dongle data area to read or write data
	public int rw_length;				// length of data are to read/write/encrypt/decrypt
	public IntPtr DoNotUse;				// do not use the rw_data_ptr element use the "Data" argument of the DDProtCheck function
	[MarshalAs(UnmanagedType.ByValArray, SizeConst=256)] // NB access this field by using alg_licence_name (see below)
	private byte[] _alt_licence_name = null; // protection check for different licence instead of this one
	public int var_a;					// variable values for user algorithm
	public int var_b;						
	public int var_c;
	public int var_d;
	public int var_e;
	public int var_f;
	public int var_g;
	public int var_h;
	public int alg_number;				// the number of the user algorithm that you want to execute

	// outputs
	public int ret_code;				// return code from the protection check
	public int ext_err;					// extended error
	public int type;					// type of dongle detected. 1 = Pro, 2 = FD
	public int model;					// model of dongle detected. 1 = Lite, 2 = Plus, 4 = Net 5, 7 = Net Unlimited
	public int sdsn;					// Software Developer's Serial Number
	[MarshalAs(UnmanagedType.ByValArray, SizeConst=12)]	// NB access this field by using prodcode (see below)
	private byte[] _prodcode = null;	// product code (null-terminated)
	public uint dongle_number;		
	public int update_number;				
	public uint data_area_size;			// size of the data area in the dongle detected
	public int max_alg_num;				// number of algorithms in the dongle detected
	public int execs;					// executions left: -1 indicates 'no limit'
	public int exp_day;					// expiry day: -1 indicates 'no limit'
	public int exp_month;				// expiry month: -1 indicates 'no limit'
	public int exp_year;				// expiry year: -1 indicates 'no limit'
	public uint features;				// features value 
	public int net_users;				// maximum number of network users for the dongle detected: -1 indicates 'mo limit'
	public int alg_answer;				// answer to the user algorithm executed with the given variable values
	public uint fd_capacity;			// capacity of the data area in FD dongle. Currently fixed at ~10MB but may change in the future.
	[MarshalAs(UnmanagedType.ByValArray, SizeConst=128)]	// NB access this field by using fd_drive (see below)
	private byte[] _fd_drive = null;	// fd drive letter (null-terminated)
	public int swkey_type;				// 0 = no swkey detected, 1 = temporary software key, 2 = demo software key
	public int swkey_exp_day;			// software key expiry date (if software key detected)
	public int swkey_exp_month;
	public int swkey_exp_year;

	// NB we have to do it this way because we cannot encrypt strings correctly unless they are byte arrays
	public string prodcode
	{
		get 
		{
			StringBuilder sb = new StringBuilder(12);
			foreach (byte b in _prodcode)
			{
				if (b == 0)
					return sb.ToString();
				else
					sb.Append((char)b);
			}
			return sb.ToString();
		}
	}

	public string fd_drive
	{
		get 
		{
			StringBuilder sb = new StringBuilder(128);
			foreach (byte b in _fd_drive)
			{
				if (b == 0)
					return sb.ToString();
				else
					sb.Append((char)b);
			}
			return sb.ToString();
		}
	}

	// NB we have to do it this way because we cannot encrypt strings correctly unless they are byte arrays
	public string alt_licence_name
	{
		set 
		{
			int i;
			StringBuilder sb = new StringBuilder(value, 256);
			for (i=0; i<sb.Length; i++)
			{
				_alt_licence_name[i] = (byte)sb[i];
			}
			_alt_licence_name[i] = 0;				// null terminate
		}
	}


	// sets 4 bytes from an integer at the specified offset in a data array
	public void Set4Bytes(byte[] data, int offset, int value) 
	{
		data[offset] = (byte)(value & 0xff);
		data[offset+1] = (byte)((value >> 8) & 0xff);
		data[offset+2] = (byte)((value >> 16) & 0xff);
		data[offset+3] = (byte)((value >> 24) & 0xff);
		return;
	}

	// converts to DRIS structure to a byte array (so we can do encryption)
	public void DrisToByteArray(DRIS dris, byte[] data) 
	{
		GCHandle MyGC = GCHandle.Alloc(data, GCHandleType.Pinned);
		Marshal.StructureToPtr(dris, MyGC.AddrOfPinnedObject(), false);
		MyGC.Free();
		return;
	}

	// converts a byte array to the DRIS structure (so we can do encryption)
	public void ByteArrayToDris(byte[] data, DRIS dris)
	{
		GCHandle MyGC = GCHandle.Alloc(data, GCHandleType.Pinned);
		Marshal.PtrToStructure(MyGC.AddrOfPinnedObject(), dris);
		MyGC.Free();
		return;
	}	

	// to make a new instance of this class, initialise every element to a random value and then set the header
	public DRIS() 
	{
		Random rnd = new Random();
		Byte[] temp = new Byte[Marshal.SizeOf(this)];
		rnd.NextBytes(temp);
		ByteArrayToDris(temp, this);
		this.header1 = (byte)'D';
		this.header2 = (byte)'R';
		this.header3 = (byte)'I';
		this.header4 = (byte)'S';
	}

    // functions - must specify only one
    public const int PROTECTION_CHECK = 1;		// checks for dongle, check program params...
    public const int EXECUTE_ALGORITHM = 2;		// protection check + calculate answer for specified algorithm with specified inputs
    public const int WRITE_DATA_AREA = 3;		// protection check + writes dongle data area
    public const int READ_DATA_AREA = 4;		// protection check + reads dongle data area
    public const int ENCRYPT_USER_DATA = 5;		// protection check + the dongle will encrypt user data
    public const int DECRYPT_USER_DATA = 6;		// protection check + the dongle will decrypt user data
    public const int FAST_PRESENCE_CHECK = 7;	// checks for the presence of the correct dongle only with minimal security, no flags allowed.
    public const int STOP_NET_USER = 8;			// stops a network user (a protection check is NOT performed)

    // flags - can specify as many as you like
    public const int DEC_ONE_EXEC = 1;			      // decrement execs by 1 
    public const int DEC_MANY_EXECS = 2;		      // decrement execs by number specified in execs_decrement
    public const int START_NET_USER = 4;		      // starts a network user
    public const int USE_FUNCTION_ARGUMENT = 16;      // use the extra argument in the function for pointers
    public const int CHECK_LOCAL_FIRST = 32;	      // always look in local ports before looking in network ports
    public const int CHECK_NETWORK_FIRST = 64;	      // always look on the network before looking in local ports
    public const int USE_ALT_LICENCE_NAME = 128;	  // use name specified in alt_licence_name instead of the default one
    public const int DONT_SET_MAXDAYS_EXPIRY = 256;   // if the max days expiry date has not been calculated then do not do it this time
    public const int MATCH_DONGLE_NUMBER = 512;		  // restrict the search to match the dongle number specified in the DRIS
    public const int DONT_RETURN_FD_DRIVE = 1024;     // if an FD dongle has been detected then don't return the flash drive/mount name in the DRIS  
    
}

[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi, Pack=1)]
public struct NU_INFO
{
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
	public string licenceName;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=50)]
	public string userName;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
	public string computerName;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)]
	public string ipAddress;
}

// 32-bit protection check (loads 32-bit DLL)
class DDProtCheck32
{
    [DllImport("dpwin32.dll", CharSet = CharSet.Ansi)]
    public static extern int DDProtCheck([In, Out, MarshalAs(UnmanagedType.LPStruct)]DRIS dris, byte[] data);
}

// 64-bit protection check (loads 64-bit DLL)
class DDProtCheck64
{
    [DllImport("dpwin64.dll", CharSet = CharSet.Ansi)]
    public static extern int DDProtCheck([In, Out, MarshalAs(UnmanagedType.LPStruct)]DRIS dris, byte[] data);
}

// 32-bit get network user info (loads 32-bit DLL)
class DDGetNetUserList32
{
    [DllImport("dpwin32.dll", CharSet = CharSet.Ansi)]
    public static extern int DDGetNetUserList(string licence_name, out int num_net_users, byte[] nu_info_bytes, int num_info_structs, out int extended_error);
}

// 64-bit get network user info (loads 64-bit DLL)
class DDGetNetUserList64
{
    [DllImport("dpwin64.dll", CharSet = CharSet.Ansi)]
    public static extern int DDGetNetUserList(string licence_name, out int num_net_users, byte[] nu_info_bytes, int num_info_structs, out int extended_error);
}

// call our API - we only want to load the correct DLL for the bit-ness of the computer. The other DLL may not exist.
class DinkeyPro
{
	// calls the DDProtCheck function in the appropriate DLL
    public static int DDProtCheck(DRIS dris, byte[] data)
    {
        int ret_code = -1;

        if (IntPtr.Size == 4)
        {
            try
            {
                ret_code = DDProtCheck32.DDProtCheck(dris, data);
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Error! Cannot find dpwin32.dll. This should be in the same folder as DllTest.", "Sample", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        else {
            try
            {
                ret_code = DDProtCheck64.DDProtCheck(dris, data);
            }
            catch (DllNotFoundException)
            {
                MessageBox.Show("Error! Cannot find dpwin64.dll. This should be in the same folder as DllTest.",
                    "Sample", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        return ret_code;
    }

    public static int DDGetNetUserList(string licence_name, out int num_net_users, out NU_INFO[] nu_info, int num_info_structs, out int extended_error)
    {
        int ret_code = -1;
		int i;
		extended_error = num_net_users = 0;
		byte[] nu_info_bytes = new byte[num_info_structs * Marshal.SizeOf(typeof(NU_INFO))];
		byte[] nu_info_one_struct = new byte[Marshal.SizeOf(typeof(NU_INFO))];
		GCHandle MyGC;
		nu_info = null;

		// call the function and get info as a byte array
		if (IntPtr.Size == 4)
			ret_code = DDGetNetUserList32.DDGetNetUserList(licence_name, out num_net_users, nu_info_bytes, num_info_structs, out extended_error);
		else
			ret_code = DDGetNetUserList64.DDGetNetUserList(licence_name, out num_net_users, nu_info_bytes, num_info_structs, out extended_error);

		if (ret_code != 0)
			return ret_code;

		// convert byte array to an array of structs
		nu_info = new NU_INFO[num_net_users];
		for (i=0; i<num_net_users; i++)
		{
			Array.Copy(nu_info_bytes, i*Marshal.SizeOf(typeof(NU_INFO)), nu_info_one_struct, 0, Marshal.SizeOf(typeof(NU_INFO)));
			MyGC = GCHandle.Alloc(nu_info_one_struct, GCHandleType.Pinned);
			nu_info[i] = (NU_INFO)Marshal.PtrToStructure(MyGC.AddrOfPinnedObject(), typeof(NU_INFO));
			MyGC.Free();
		}
		
		return 0;
    }
	
}

