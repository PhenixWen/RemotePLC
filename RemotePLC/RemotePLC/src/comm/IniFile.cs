using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace RemotePLC.src.comm
{

    /// <summary>Provides methods for reading from, and writing to, .ini files.</summary>
    public class IniFile
    {
        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            internal static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, IntPtr lpReturnedString, uint nSize, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            internal static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            internal static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);
        }

        private const int BufferSize = 32 * 1024;

        private string filename;

        /// <summary>Initializes a new instance of the <see cref="IniFile" /> class.</summary>
        /// <param name="filename">The path of the .ini file.</param>
        public IniFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            if (!File.Exists(filename))
            {
                throw new FileNotFoundException("The specified file was not found.", filename);
            }

            this.filename = Path.GetFullPath(filename);
        }

        /// <summary>Retrieves the sections in the file.</summary>
        /// <returns>An array containing the section names.</returns>
        public string[] Sections()
        {
            IntPtr buffer = Marshal.AllocCoTaskMem(BufferSize);
            try
            {
                uint bytesReturned = NativeMethods.GetPrivateProfileSectionNames(buffer, BufferSize, filename);
                if (bytesReturned == 0)
                {
                    return new string[0];
                }

                string[] sections = SplitNullDelimitedBuffer(buffer, (int)bytesReturned);
                return sections;
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>Determines whether the specified section exists in this .ini file.</summary>
        /// <param name="section">The section to look for.</param>
        /// <returns><c>true</c> if the specified section was found, <c>false</c> otherwise.</returns>
        public bool HasSection(string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            string[] sections = Sections();
            if (sections == null)
            {
                return false;
            }

            foreach (string s in sections)
            {
                if (s.Equals(section, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Retrieves a list of options in the specified section.</summary>
        /// <param name="section">The section for which to retrieve the options.</param>
        /// <returns>An array containing the option names.</returns>
        public string[] Options(string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (!HasSection(section))
            {
                throw new ArgumentException("Section does not exist.");
            }

            IntPtr buffer = Marshal.AllocCoTaskMem(BufferSize);
            try
            {
                uint bytesReturned = NativeMethods.GetPrivateProfileString(section, null, null, buffer, BufferSize, filename);
                if (bytesReturned == 0)
                {
                    return new string[0];
                }

                string[] options = SplitNullDelimitedBuffer(buffer, (int)bytesReturned);
                return options;
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>Determines whether the specified option exists in the specified section.</summary>
        /// <param name="section">The section which the option is in.</param>
        /// <param name="option">The option to look for.</param>
        /// <returns><c>true</c> if the specified option was found, <c>false</c> otherwise.</returns>
        public bool HasOption(string section, string option)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (string.IsNullOrEmpty(option))
            {
                throw new ArgumentNullException("option");
            }

            string[] options;
            try
            {
                options = Options(section);
            }
            catch
            {
                return false;
            }

            if (options == null)
            {
                return false;
            }

            foreach (string o in options)
            {
                if (o.Equals(option, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Gets a value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>string.Empty</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        public string Get(string section, string option, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (string.IsNullOrEmpty("option"))
            {
                throw new ArgumentNullException("option");
            }

            IntPtr buffer = Marshal.AllocCoTaskMem(BufferSize);
            try
            {
                uint bytesReturned = NativeMethods.GetPrivateProfileString(section, option, defaultValue, buffer, BufferSize, filename);
                if (bytesReturned == 0)
                {
                    return defaultValue;
                }

                string value = Marshal.PtrToStringAuto(buffer, (int)bytesReturned).ToString();
                return value;
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>Gets an <c>int</c> value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>0</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        public int GetInt(string section, string option, int defaultValue = 0)
        {
            string stringValue = Get(section, option, defaultValue.ToString());
            int value;
            if (!int.TryParse(stringValue, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Gets a <c>long</c> value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>0</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        public long GetLong(string section, string option, long defaultValue = 0)
        {
            string stringValue = Get(section, option, defaultValue.ToString());
            long value;
            if (!long.TryParse(stringValue, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Gets a <c>float</c> value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>0.0</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        public float GetFloat(string section, string option, float defaultValue = 0)
        {
            string stringValue = Get(section, option, defaultValue.ToString());
            float value;
            if (!float.TryParse(stringValue, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Gets a <c>double</c> value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>0.0</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        public double GetDouble(string section, string option, double defaultValue = 0)
        {
            string stringValue = Get(section, option, defaultValue.ToString());
            double value;
            if (!double.TryParse(stringValue, out value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Gets a <c>bool</c> value from the .ini file.</summary>
        /// <param name="section">The section that the option is in.</param>
        /// <param name="option">The option to read.</param>
        /// <param name="defaultValue">The default value (defaults to <c>false</c> if not specified).</param>
        /// <returns>The value from the .ini file if it could be found, or else the default value.</returns>
        /// <remarks>When parsing a <c>bool</c> the following strings are recognized: true/false, yes/no, 1/0.</remarks>
        public bool GetBoolean(string section, string option, bool defaultValue = false)
        {
            string stringValue = Get(section, option, defaultValue.ToString()).Trim().ToLower();
            if (stringValue == "true" || stringValue == "1" || stringValue == "yes")
            {
                return true;
            }
            else if (stringValue == "false" || stringValue == "0" || stringValue == "no")
            {
                return false;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>Retrieves a list of items in the specified section.</summary>
        /// <param name="section">The section for which to retrieve the items.</param>
        /// <returns>An array of <see cref="System.Collections.Generic.KeyValuePair" /> containing the options and ther values.</returns>
        public KeyValuePair<string, string>[] Items(string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            IntPtr buffer = Marshal.AllocCoTaskMem(BufferSize);
            try
            {
                uint bytesReturned = NativeMethods.GetPrivateProfileSection(section, buffer, BufferSize, filename);
                if (bytesReturned == 0)
                {
                    return null;
                }

                string[] tokens = SplitNullDelimitedBuffer(buffer, (int)bytesReturned);
                KeyValuePair<string, string>[] items = new KeyValuePair<string, string>[tokens.Length];
                for (int i = 0; i < tokens.Length; i++)
                {
                    int equalsPos = tokens[i].IndexOf('=');
                    string option = tokens[i].Substring(0, equalsPos).Trim();
                    string value = tokens[i].Substring(equalsPos + 1).Trim();
                    items[i] = new KeyValuePair<string, string>(option, value);
                }

                return items;
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>Sets a value in the .ini file.</summary>
        /// <param name="section">The section which the option is in.</param>
        /// <param name="option">The option whose value is to be set.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was successfully set, <c>false</c> otherwise.</returns>
        public bool Set(string section, string option, object value)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (string.IsNullOrEmpty("option"))
            {
                throw new ArgumentNullException("option");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            bool result = NativeMethods.WritePrivateProfileString(section, option, value.ToString(), filename);
            return result;
        }

        /// <summary>Removes the specified option from the specified section.</summary>
        /// <param name="section">The section which the option is in.</param>
        /// <param name="option">The option to remove.</param>
        /// <returns><c>true</c> if the option was successfully removed, <c>false</c> otherwise.</returns>
        public bool RemoveOption(string section, string option)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            if (string.IsNullOrEmpty("option"))
            {
                throw new ArgumentNullException("option");
            }

            bool result = NativeMethods.WritePrivateProfileString(section, option, null, filename);
            return result;
        }

        /// <summary>Removes the specified section.</summary>
        /// <param name="section">The section to remove.</param>
        /// <returns><c>true</c> if the section was successfully removed, <c>false</c> otherwise.</returns>
        public bool RemoveSection(string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentNullException("section");
            }

            bool result = NativeMethods.WritePrivateProfileString(section, null, null, filename);
            return result;
        }

        private static string[] SplitNullDelimitedBuffer(IntPtr buffer, int length)
        {
            string s = Marshal.PtrToStringAuto(buffer, length - 1).ToString();
            string[] parts = s.Split('\0');
            return parts;
        }
    }
}