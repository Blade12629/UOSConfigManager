/*
 * Disclaimer:
 * This software is provided without guarantee nor warranty. Use at your own discretion. The creators of this software and the owners of this site cannot be held liable for any damages inflicted.
 * Credits: https://www.techpowerup.com/forums/threads/large-address-aware.112556/
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOSConfigManager.LargeAddressAware
{
    public static class LAA
    {
        public static async Task<bool> LaaEnable(string path)
        {
            if (!File.Exists(path))
                return false;

            LaaFile file = new LaaFile(path);

            if (file.LargeAddressAware)
                return false;

            file.WriteCharacteristics(true);

            return true;
        }

        public static async Task<bool> LaaDisable(string path)
        {
            if (!File.Exists(path))
                return false;

            LaaFile file = new LaaFile(path);

            if (!file.LargeAddressAware)
                return false;

            file.WriteCharacteristics(false);

            return true;
        }

        public static async Task<bool> LaaSwitch(string path)
        {
            if (!File.Exists(path))
                return false;

            LaaFile file = new LaaFile(path);

            if (file.LargeAddressAware)
                file.WriteCharacteristics(false);
            else
                file.WriteCharacteristics(true);

            return true;
        }
        
        [Flags]
        public enum Characteristics : ushort
        {
            IMAGE_FILE_RELOCS_STRIPPED = 0x0001,            // Relocation information was stripped from the file. The file must be loaded at its preferred base address. If the base address is not available, the loader reports an error.
            IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002,           // The file is executable (there are no unresolved external references).
            IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004,         // COFF line numbers were stripped from the file.
            IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008,        // COFF symbol table entries were stripped from file.
            IMAGE_FILE_AGGRESIVE_WS_TRIM = 0x0010,          // Aggressively trim the working set. This value is obsolete as of Windows 2000.
            IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020,        // The application can handle addresses larger than 2 GB.
            IMAGE_FILE_BYTES_REVERSED_LO = 0x0080,          // The bytes of the word are reversed. This flag is obsolete.
            IMAGE_FILE_32BIT_MACHINE = 0x0100,              // The computer supports 32-bit words.
            IMAGE_FILE_DEBUG_STRIPPED = 0x0200,             // Debugging information was removed and stored separately in another file.
            IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400,    // If the image is on removable media, copy it to and run it from the swap file.
            IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800,          // If the image is on the network, copy it to and run it from the swap file.
            IMAGE_FILE_SYSTEM = 0x1000,                     // The image is a system file.
            IMAGE_FILE_DLL = 0x2000,                        // The image is a DLL file. While it is an executable file, it cannot be run directly.
            IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000,             // The file should be run only on a uniprocessor computer.
            IMAGE_FILE_BYTES_REVERSED_HI = 0x8000           // The bytes of the word are reversed. This flag is obsolete.
        }

        public class LaaFile
        {
            private string _Path;
            public string Path
            {
                get { return _Path; }
            }

            private long _CharacteristicsOffset = -1;
            private Characteristics _Characteristics = (Characteristics)0;
            public Characteristics Characteristics
            {
                get { return _Characteristics; }
            }
            public bool LargeAddressAware
            {
                get
                {
                    if ((_Characteristics & Characteristics.IMAGE_FILE_LARGE_ADDRESS_AWARE) == Characteristics.IMAGE_FILE_LARGE_ADDRESS_AWARE)
                        return true;
                    else
                        return false;
                }
            }

            public LaaFile(string path)
            {
                _Path = path;
                ReadCharacteristics();
            }

            private bool GetOffsets()
            {
                bool output = true;

                try
                {
                    FileStream fs = new FileStream(_Path, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);

                    fs.Seek(0, SeekOrigin.Begin);
                    if (br.ReadByte() != 0x4D) // M
                        throw new Exception();

                    if (br.ReadByte() != 0x5A) // Z
                        throw new Exception();

                    fs.Seek(60, SeekOrigin.Begin);
                    long pe = br.ReadUInt32();

                    fs.Seek(pe, SeekOrigin.Begin);
                    if (br.ReadByte() != 0x50) // P ortable
                        throw new Exception();

                    if (br.ReadByte() != 0x45) // E executable
                        throw new Exception();

                    fs.Seek(20, SeekOrigin.Current);
                    _CharacteristicsOffset = fs.Position;
                    _Characteristics = (Characteristics)br.ReadUInt16();

                    br.Close();
                    fs.Close();
                }
                catch
                {
                    _CharacteristicsOffset = -1;
                    output = false;
                }

                return output;
            }
            public bool ReadCharacteristics()
            {
                if (_CharacteristicsOffset == -1)
                    return GetOffsets();
                else
                {
                    bool output = true;

                    try
                    {
                        FileStream fs = new FileStream(_Path, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);

                        fs.Seek(_CharacteristicsOffset, SeekOrigin.Begin);
                        _Characteristics = (Characteristics)br.ReadUInt16();

                        br.Close();
                        fs.Close();
                    }
                    catch
                    {
                        _CharacteristicsOffset = -1;
                        output = false;
                    }

                    return output;
                }
            }
            public bool WriteCharacteristics(Characteristics value)
            {
                if (_CharacteristicsOffset == -1)
                    return false;
                else
                {
                    bool output = true;

                    try
                    {
                        FileStream fs = new FileStream(_Path, FileMode.Open, FileAccess.Write);
                        BinaryWriter bw = new BinaryWriter(fs);

                        fs.Seek(_CharacteristicsOffset, SeekOrigin.Begin);
                        bw.Write((ushort)value);
                        bw.Flush();

                        bw.Close();
                        fs.Close();

                        _Characteristics = value; // Update the new value locally so it always matches the file.
                    }
                    catch { output = false; }

                    return output;
                }
            }
            public bool WriteCharacteristics(bool laa)
            {
                Characteristics chars = _Characteristics;
                if (laa)
                    chars = Characteristics.IMAGE_FILE_LARGE_ADDRESS_AWARE | chars;
                else
                    chars = ~Characteristics.IMAGE_FILE_LARGE_ADDRESS_AWARE & chars;

                return WriteCharacteristics(chars);
            }
        }
    }
}
