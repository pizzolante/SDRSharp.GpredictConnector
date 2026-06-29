using System;
using System.Threading;

namespace SDRSharp.GpredictConnector
{
    class Rigctrld
    {
        /// <summary>
        /// Parses a rigctld-compatible command and returns the response.
        /// Supported commands:
        ///   f               — read current frequency (Hz)
        ///   F 123456789     — set frequency (integer Hz)
        ///   F 7074055.000   — set frequency (decimal Hz, as sent by WSJT-X / gpredict)
        /// </summary>
        public string ExecCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return GenerateReturn(HamlibErrorcode.RIG_EPROTO);

            // Normalize: strip all whitespace, \r, \n from both ends
            command = command.Trim();

            // --- Read frequency: exact "f" ---
            if (command == "f")
                return FrequencyInHz.ToString() + "\n";

            // --- Set frequency: "F <number>" or "F<number>" ---
            if (command.Length >= 1 && (command[0] == 'F' || command[0] == 'f'))
            {
                // If command is just "F" (or "f") with no frequency, it's a protocol error
                if (command.Length < 2)
                    return GenerateReturn(HamlibErrorcode.RIG_EPROTO);

                string payload;
                if (command[1] == ' ')
                    payload = command.Substring(2);  // skip "F "
                else
                    payload = command.Substring(1);  // skip "F"

                payload = payload.Trim();
                if (payload.Length == 0)
                    return GenerateReturn(HamlibErrorcode.RIG_EPROTO);

                if (TryParseFrequency(payload, out long hz))
                {
                    FrequencyInHz = hz;
                    return GenerateReturn(HamlibErrorcode.RIG_OK);
                }

                return GenerateReturn(HamlibErrorcode.RIG_EPROTO);
            }

            // --- Unknown command ---
            return GenerateReturn(HamlibErrorcode.RIG_ENIMPL);
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
