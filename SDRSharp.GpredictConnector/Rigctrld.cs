using System;
using System.Threading;

namespace SDRSharp.GpredictConnector
{
    class Rigctrld
    {
        /// <summary>
        /// Parses a rigctld-compatible command and returns the response.
        /// Supports Hamlib mode codes (AM=1, CW=2, USB=3, LSB=4, FM=6, WFM=7, CWR=8, DSB=25)
        /// and mode names (AM, FM, NFM, WFM, LSB, USB, DSB, CW, CWR, RAW).
        /// </summary>
        public string ExecCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return GenerateReturn(HamlibErrorcode.RIG_EPROTO);

            // Normalize: strip all whitespace, \r, \n from both ends
            command = command.Trim();

            // Log received command for diagnostics
            LogCommand("RX", command);

            string response;

            // --- Read frequency: exact "f" ---
            if (command == "f")
            {
                response = FrequencyInHz.ToString() + "\n";
                LogCommand("TX", response);
                return response;
            }

            // --- Set frequency: "F <number>" or "F<number>" ---
            if (command.Length >= 1 && command[0] == 'F')
            {
                // If command is just "F" with no frequency, it's a protocol error
                if (command.Length < 2)
                {
                    response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                    LogCommand("TX", response);
                    return response;
                }

                string payload;
                if (command[1] == ' ')
                    payload = command.Substring(2);  // skip "F "
                else
                    payload = command.Substring(1);  // skip "F"

                payload = payload.Trim();
                if (payload.Length == 0)
                {
                    response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                    LogCommand("TX", response);
                    return response;
                }

                if (TryParseFrequency(payload, out long hz))
                {
                    FrequencyInHz = hz;
                    response = GenerateReturn(HamlibErrorcode.RIG_OK);
                    LogCommand("TX", response);
                    return response;
                }

                response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                LogCommand("TX", response);
                return response;
            }

            // --- Read mode: exact "m" ---
            if (command == "m")
            {
                string mode = string.IsNullOrEmpty(mode_) ? defaultMode_ : mode_;
                int pb = string.IsNullOrEmpty(mode_) ? defaultPassband_ : passband_;
                response = mode + "\n" + pb.ToString() + "\n";
                LogCommand("TX", response);
                return response;
            }

            // --- Set mode: "M <mode> [<passband>]" (string or numeric) ---
            if (command.Length >= 1 && command[0] == 'M')
            {
                if (command.Length < 2)
                {
                    response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                    LogCommand("TX", response);
                    return response;
                }

                string payload;
                if (command[1] == ' ')
                    payload = command.Substring(2);  // skip "M "
                else
                    payload = command.Substring(1);  // skip "M"

                payload = payload.Trim();
                if (payload.Length == 0)
                {
                    response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                    LogCommand("TX", response);
                    return response;
                }

                // Split mode and optional passband
                string[] parts = payload.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string rawMode = parts[0];
                int newPassband = 0;
                if (parts.Length >= 2)
                    int.TryParse(parts[1], out newPassband);

                // Try string mode first, then numeric Hamlib mode code
                string newMode = rawMode.ToUpperInvariant();
                if (!IsValidMode(newMode))
                {
                    // Try numeric Hamlib mode code (1=AM, 2=CW, 3=USB, 4=LSB, 6=FM, 7=WFM, 8=CWR, 25=DSB)
                    newMode = MapModeNumber(rawMode);
                }

                if (IsValidMode(newMode))
                {
                    Passband = newPassband;
                    Mode = newMode;
                    response = GenerateReturn(HamlibErrorcode.RIG_OK);
                    LogCommand("TX", response);
                    return response;
                }

                // Unknown mode
                response = GenerateReturn(HamlibErrorcode.RIG_EPROTO);
                LogCommand("TX", response);
                return response;
            }

            // --- Read VFO: exact "v" ---
            if (command == "v")
            {
                response = "VFOA\n";
                LogCommand("TX", response);
                return response;
            }

            // --- Set VFO: "V <vfo>" ---
            if (command.Length >= 1 && command[0] == 'V')
            {
                response = GenerateReturn(HamlibErrorcode.RIG_OK);
                LogCommand("TX", response);
                return response;
            }

            // --- Read PTT: exact "t" ---
            if (command == "t")
            {
                response = "0\n";
                LogCommand("TX", response);
                return response;
            }

            // --- Set PTT: "T <ptt>" ---
            if (command.Length >= 1 && command[0] == 'T')
            {
                response = GenerateReturn(HamlibErrorcode.RIG_OK);
                LogCommand("TX", response);
                return response;
            }

            // --- Read split VFO: exact "s" ---
            if (command == "s")
            {
                response = "0\nVFOA\n";
                LogCommand("TX", response);
                return response;
            }

            // --- Set split VFO: "S <split> <vfo>" ---
            if (command.Length >= 1 && command[0] == 'S')
            {
                response = GenerateReturn(HamlibErrorcode.RIG_OK);
                LogCommand("TX", response);
                return response;
            }

            // --- Dump info / extended dump_state: "_" or "\dump_state" ---
            if (command == "_" || command == "\\dump_state")
            {
                response = string.Format(
                    "0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n" +
                    "10\n11\n12\n13\n14\n15\n" +
                    "{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n" +
                    "0\n0\n0\n" +
                    "0\n",
                    GetCurrentModeAsNumber(),
                    GetCurrentModeAsNumber(),
                    "0",       // freq
                    FrequencyInHz.ToString(),
                    "VFOA",
                    "0"
                );
                LogCommand("TX", response);
                return response;
            }

            // --- Check VFO: "\chk_vfo" ---
            if (command == "\\chk_vfo")
            {
                response = "CHKVFO 1\n";
                LogCommand("TX", response);
                return response;
            }

            // --- VFO select by number: "1", "2", ... ---
            if (command.Length == 1 && char.IsDigit(command[0]))
            {
                response = GenerateReturn(HamlibErrorcode.RIG_OK);
                LogCommand("TX", response);
                return response;
            }

            // --- Unknown command ---
            response = GenerateReturn(HamlibErrorcode.RIG_ENIMPL);
            LogCommand("TX", response);
            return response;
        }

        /// <summary>
        /// Parses a frequency string that may be integer ("7074055") or
        /// decimal ("7074055.000000") — both accepted by rigctld protocol.
        /// Leading zeros and optional decimal part are handled.
        /// </summary>
        private static bool TryParseFrequency(string input, out long hz)
        {
            hz = 0;
            if (string.IsNullOrEmpty(input))
                return false;

            // Must start with a digit
            if (!char.IsDigit(input[0]))
                return false;

            // Walk the string manually to validate format: digits, optionally one dot, optionally more digits
            bool hasDot = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '.')
                {
                    if (hasDot)
                        return false; // only one dot allowed
                    hasDot = true;
                }
                else if (!char.IsDigit(c))
                {
                    return false; // invalid character
                }
            }

            // Parse as double, then truncate to long (Hz integers only)
            try
            {
                double d = double.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
                hz = (long)d;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateReturn(HamlibErrorcode errorcode)
        {
            return "RPRT " + ((int)errorcode).ToString() + "\n";
        }

        /// <summary>
        /// Maps a Hamlib numeric mode code (as string) to a mode name string.
        /// Hamlib mode codes: 0=NONE, 1=AM, 2=CW, 3=USB, 4=LSB, 5=RTTY, 6=FM, 7=WFM, 8=CWR, 25=DSB
        /// </summary>
        private static string MapModeNumber(string rawMode)
        {
            if (!int.TryParse(rawMode, out int modeNum))
                return rawMode.ToUpperInvariant();

            switch (modeNum)
            {
                case 1: return "AM";
                case 2: return "CW";
                case 3: return "USB";
                case 4: return "LSB";
                case 6: return "FM";
                case 7: return "WFM";
                case 8: return "CWR";
                case 25: return "DSB";
                default: return modeNum.ToString(); // unknown, will fail IsValidMode
            }
        }

        /// <summary>
        /// Returns the current mode as Hamlib numeric code for \dump_state response.
        /// </summary>
        private int GetCurrentModeAsNumber()
        {
            string mode = string.IsNullOrEmpty(mode_) ? defaultMode_ : mode_;
            switch (mode)
            {
                case "AM": return 1;
                case "CW": return 2;
                case "USB": return 3;
                case "LSB": return 4;
                case "FM": return 6;
                case "NFM": return 6;
                case "WFM": return 7;
                case "CWR": return 8;
                case "DSB": return 25;
                default: return 1; // default AM
            }
        }

        /// <summary>
        /// Writes command log to %TEMP%\SDRSharp.GpredictConnector.log for diagnostics.
        /// </summary>
        private static void LogCommand(string direction, string data)
        {
            try
            {
                string logPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "SDRSharp.GpredictConnector.log");
                string line = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}",
                    DateTime.Now, direction,
                    data.Replace("\r", "\\r").Replace("\n", "\\n"));
                System.IO.File.AppendAllText(logPath, line + Environment.NewLine);
            }
            catch
            {
                // Silently ignore logging errors
            }
        }
        public long FrequencyInHz
        {
            get
            {
                return frequency_;
            }
            set
            {
                if (frequency_ != value)
                {
                    frequency_ = value;
                    FrequencyInHzChanged?.Invoke(frequency_);
                }
                
            }
        }

        public string FrequencyInHzString
        {
            get
            {
                return FrequencyInHz.ToString();
            }
            private set
            {
                try
                {
                    // Parse as double first to handle decimal frequencies from WSJT-X (e.g. "7074055.000000")
                    FrequencyInHz = (long)double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch { }
            }
        }

        public event Action<long> FrequencyInHzChanged;

        public string Mode
        {
            get
            {
                return mode_;
            }
            set
            {
                if (mode_ != value)
                {
                    mode_ = value;
                    ModeChanged?.Invoke(mode_, passband_);
                }
            }
        }

        public int Passband
        {
            get
            {
                return passband_;
            }
            set
            {
                passband_ = value;
            }
        }

        public event Action<string, int> ModeChanged;

        private static bool IsValidMode(string mode)
        {
            switch (mode)
            {
                case "AM":
                case "FM":
                case "NFM":
                case "WFM":
                case "LSB":
                case "USB":
                case "DSB":
                case "CW":
                case "CWR":
                case "RAW":
                    return true;
                default:
                    return false;
            }
        }

        private string mode_ = "FM";  // default so rigctld 'm' command never returns error
        private int passband_ = 0;
        private const string defaultMode_ = "FM";
        private const int defaultPassband_ = 0;

        private long frequency_ = 0;
        private Thread frequency_set_thread_ = null;
        public Thread FrequencySetThread
        {
            get
            {
                return frequency_set_thread_;
            }
        }


        enum HamlibErrorcode
        {
            RIG_OK = 0,     /*!< No error, operation completed successfully */
            RIG_EINVAL = -1,     /*!< invalid parameter */
            RIG_ECONF = -2,      /*!< invalid configuration (serial,..) */
            RIG_ENOMEM= -3,     /*!< memory shortage */
            RIG_ENIMPL = -4,     /*!< function not implemented, but will be */
            RIG_ETIMEOUT = -5,   /*!< communication timed out */
            RIG_EIO = -6,        /*!< IO error, including open failed */
            RIG_EINTERNAL = -7,  /*!< Internal Hamlib error, huh! */
            RIG_EPROTO = -8,     /*!< Protocol error */
            RIG_ERJCTED = -9,    /*!< Command rejected by the rig */
            RIG_ETRUNC = -10,     /*!< Command performed, but arg truncated */
            RIG_ENAVAIL = -11,    /*!< function not available */
            RIG_ENTARGET = -12,   /*!< VFO not targetable */
            RIG_BUSERROR = -13,   /*!< Error talking on the bus */
            RIG_BUSBUSY = -14,    /*!< Collision on the bus */
            RIG_EARG = -15,       /*!< NULL RIG handle or any invalid pointer parameter in get arg */
            RIG_EVFO = -16,       /*!< Invalid VFO */
            RIG_EDOM = -17       /*!< Argument out of domain of func */
        };
    }
}
