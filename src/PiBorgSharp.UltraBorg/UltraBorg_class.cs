using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace PiBorgSharp.UltraBorg
{
    public class UltraBorg_class
    {
        // based on original source written by Arron Churchill (I think): https://www.piborg.org/blog/piborg-arron
        public static bool THROTTLE_CODE = false;
        public static readonly uint THROTTLE_SPEED = 10;           // NOTE: this is the fatest speed at which the sensors can run or they do not reset distances

        public static readonly int I2C_SLAVE = 0x0703;
        public static readonly byte I2C_MAX_LEN = 0x04;
        public static readonly decimal USM_US_TO_MM = 0.171497M;    // = 1 / 29.155 (speed of sound in microseconds per centimeter) / 2 (there and back) * 10 (centimeters to millimeters) = 0.1714971702966901
        public static readonly decimal USM_US_TO_IN = 0.006752M;    // USM_US_TO_MM * 0.0393701 = 0.0067518607442977
        public static readonly int PWM_MIN = 2000;
        public static readonly int PWM_MAX = 4000;
        public static readonly decimal DELAY_AFTER_EEPROM = 0.01M;
        public static readonly int PWM_RESET = 0xFFFF;

        public static readonly byte I2C_ID_SERVO_USM = 0x36;

        public static readonly byte COMMAND_GET_TIME_USM1 = 0x01;
        public static readonly byte COMMAND_GET_TIME_USM2 = 0x02;
        public static readonly byte COMMAND_GET_TIME_USM3 = 0x03;
        public static readonly byte COMMAND_GET_TIME_USM4 = 0x04;
        public static readonly byte COMMAND_SET_PWM1 = 0x05;
        public static readonly byte COMMAND_GET_PWM1 = 0x06;
        public static readonly byte COMMAND_SET_PWM2 = 0x07;
        public static readonly byte COMMAND_GET_PWM2 = 0x08;
        public static readonly byte COMMAND_SET_PWM3 = 0x09;
        public static readonly byte COMMAND_GET_PWM3 = 0x0A;
        public static readonly byte COMMAND_SET_PWM4 = 0x0B;
        public static readonly byte COMMAND_GET_PWM4 = 0x0C;
        public static readonly byte COMMAND_CALIBRATE_PWM1 = 0x0D;
        public static readonly byte COMMAND_CALIBRATE_PWM2 = 0x0E;
        public static readonly byte COMMAND_CALIBRATE_PWM3 = 0x0F;
        public static readonly byte COMMAND_CALIBRATE_PWM4 = 0x10;
        public static readonly byte COMMAND_GET_PWM_MIN_1 = 0x11;
        public static readonly byte COMMAND_GET_PWM_MAX_1 = 0x12;
        public static readonly byte COMMAND_GET_PWM_BOOT_1 = 0x13;
        public static readonly byte COMMAND_GET_PWM_MIN_2 = 0x14;
        public static readonly byte COMMAND_GET_PWM_MAX_2 = 0x15;
        public static readonly byte COMMAND_GET_PWM_BOOT_2 = 0x16;
        public static readonly byte COMMAND_GET_PWM_MIN_3 = 0x17;
        public static readonly byte COMMAND_GET_PWM_MAX_3 = 0x18;
        public static readonly byte COMMAND_GET_PWM_BOOT_3 = 0x19;
        public static readonly byte COMMAND_GET_PWM_MIN_4 = 0x1A;
        public static readonly byte COMMAND_GET_PWM_MAX_4 = 0x1B;
        public static readonly byte COMMAND_GET_PWM_BOOT_4 = 0x1C;
        public static readonly byte COMMAND_SET_PWM_MIN_1 = 0x1D;
        public static readonly byte COMMAND_SET_PWM_MAX_1 = 0x1E;
        public static readonly byte COMMAND_SET_PWM_BOOT_1 = 0x1F;
        public static readonly byte COMMAND_SET_PWM_MIN_2 = 0x20;
        public static readonly byte COMMAND_SET_PWM_MAX_2 = 0x21;
        public static readonly byte COMMAND_SET_PWM_BOOT_2 = 0x22;
        public static readonly byte COMMAND_SET_PWM_MIN_3 = 0x23;
        public static readonly byte COMMAND_SET_PWM_MAX_3 = 0x24;
        public static readonly byte COMMAND_SET_PWM_BOOT_3 = 0x25;
        public static readonly byte COMMAND_SET_PWM_MIN_4 = 0x26;
        public static readonly byte COMMAND_SET_PWM_MAX_4 = 0x27;
        public static readonly byte COMMAND_SET_PWM_BOOT_4 = 0x28;
        public static readonly byte COMMAND_GET_FILTER_USM1 = 0x29;
        public static readonly byte COMMAND_GET_FILTER_USM2 = 0x2A;
        public static readonly byte COMMAND_GET_FILTER_USM3 = 0x2B;
        public static readonly byte COMMAND_GET_FILTER_USM4 = 0x2C;
        public static readonly byte COMMAND_GET_ID = 0x99;
        public static readonly byte COMMAND_SET_I2C_ADD = 0xAA;

        public static readonly byte COMMAND_VALUE_FWD = 0x01;
        public static readonly byte COMMAND_VALUE_REV = 0x02;

        public static readonly byte COMMAND_VALUE_ON = 0x01;
        public static readonly byte COMMAND_VALUE_OFF = 0x00;

        public int PWM_MIN_1 { get; set; }
        public int PWM_MAX_1 { get; set; }
        public int PWM_MIN_2 { get; set; }
        public int PWM_MAX_2 { get; set; }
        public int PWM_MIN_3 { get; set; }
        public int PWM_MAX_3 { get; set; }
        public int PWM_MIN_4 { get; set; }
        public int PWM_MAX_4 { get; set; }

        private int _bus = 0x01;
        private int _UltraBorgAddress = 0x00;
        private ILogger _log = null;
        private Stopwatch _internalClock_Sensor1 = new Stopwatch();
        private Stopwatch _internalClock_Sensor2 = new Stopwatch();
        private Stopwatch _internalClock_Sensor3 = new Stopwatch();
        private Stopwatch _internalClock_Sensor4 = new Stopwatch();

        public enum ValueType
        {
            Minimum = 1
            , Maximum = 2
            , Boot = 3
        }

        public enum FilterType
        {
            Filtered = 1
            , Unfiltered = 2
        }

        /// <summary>
        /// Scans the I2C bus for the UltraBorg board.  It will scan bus 1 by default, and return the port number when it gets a UltraBorg response to the board ID request.
        /// </summary>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Port number found for UltraBorg; -1 if no board found</returns>
        public static int ScanForUltraBorg(int busNumber = 1, ILogger log = null)
        {
            int tempReturn = -1;

            if (log != null)
            {
                log.WriteLog("Starting scan for UltraBorg_class board...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                for (byte port = 0x03; port < 0x78; port++)
                {
                    try
                    {
                        bus.WriteByte(port, COMMAND_GET_ID);
                        byte[] response = bus.ReadBytes(port, I2C_MAX_LEN);
                        if (response[0] == 0x99)
                        {
                            if (response[1] == I2C_ID_SERVO_USM)
                            {
                                tempReturn = port;
                                if (log != null)
                                {
                                    log.WriteLog("FOUND UltraBorg_class board on port: " + port.ToString("X2"));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // do nothing
                    }
                }
            }

            if (log != null)
            {
                log.WriteLog("Finished port scan...");
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets a new address for the UltraBorg.  CAUTION: this setting is persistent and will continue after powering down your UltraBorg.
        /// </summary>
        /// <param name="newAddress">The new port address for the UltraBorg</param>
        /// <param name="oldAddress">Optional - if specified the method will use this address as opposed to scanning for the UltraBorg</param>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        /// <returns>The new port number for the UltraBorg</returns>
        public static int SetNewAddress(byte newAddress, byte? oldAddress = null, int? busNumber = 0, ILogger logger = null)
        {
            int _oldAddress;

            if ((newAddress <= 0x03) || (newAddress > 0x77))
            {
                logger.WriteLog("Error: I2C addresses below 3 (0x03) and above 119 (0x77) are reserved.  Please use a different address.");
                throw new ArgumentOutOfRangeException("newAddress", "New port number must be between 0x03 and 0x77.");
            }

            if (oldAddress == null)
            {
                _oldAddress = UltraBorg_class.ScanForUltraBorg(Convert.ToInt32(busNumber), logger);
                if (_oldAddress < 0)
                {
                    throw new Exception("UltraBorg board not found.");
                }
            }
            else
            {
                _oldAddress = Convert.ToInt32(oldAddress);
            }

            if (logger != null)
            {
                logger.WriteLog("Attempting change of UltraBorg address from " + _oldAddress.ToString("X2") + " to " + newAddress.ToString("X2"), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                bus.WriteBytes(_oldAddress, new byte[] { COMMAND_SET_I2C_ADD, newAddress });

                System.Threading.Thread.Sleep(200);         // let the I2C bus catch up

                int tempCheck = UltraBorg_class.ScanForUltraBorg(Convert.ToInt32(busNumber), logger);

                if (tempCheck == newAddress)
                {
                    logger.WriteLog("CHANGED BOARD ADDRESS FROM " + _oldAddress.ToString("X2") + " TO " + newAddress.ToString("X2"), ILogger.Priority.Critical);
                    logger.WriteLog("This change will be persistent even after a reboot; keep track of it.", ILogger.Priority.Information);
                }
                else
                {
                    logger.WriteLog("**FAILED** to change UltraBorg address.  Current address: " + tempCheck.ToString("X2"));
                }
                return tempCheck;
            }
        }

        /// <summary>
        /// Main UltraBorg class
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <param name="tryOtherBus">CURRENTLY NO EFFECT</param>
        public UltraBorg_class(ILogger log = null, bool tryOtherBus = false)
        {
            if (log != null)
            {
                this._log = log;
                log.WriteLog("THROTTLE_CODE: " + THROTTLE_CODE.ToString(), ILogger.Priority.Information);
                log.WriteLog("Instantiating UltraBorg_class...", ILogger.Priority.Information);
            }

            this.PWM_MIN_1 = PWM_MIN;
            this.PWM_MAX_1 = PWM_MAX;
            this.PWM_MIN_2 = PWM_MIN;
            this.PWM_MAX_2 = PWM_MAX;
            this.PWM_MIN_3 = PWM_MIN;
            this.PWM_MAX_3 = PWM_MAX;
            this.PWM_MIN_4 = PWM_MIN;
            this.PWM_MAX_4 = PWM_MAX;

            this._UltraBorgAddress = UltraBorg_class.ScanForUltraBorg(1, log);

            if (log != null)
            {
                log.WriteLog("Loading UltraBorg on bus " + _bus.ToString("X2") + ", address " + _UltraBorgAddress.ToString("X2"), ILogger.Priority.Medium);
            }

            _internalClock_Sensor1.Start();
            _internalClock_Sensor2.Start();
            _internalClock_Sensor3.Start();
            _internalClock_Sensor4.Start();
        }

        public int BusNumber
        {
            get
            {
                return this._bus;
            }
        }

        public int UltraBorgAddress
        {
            get
            {
                return this._UltraBorgAddress;
            }
        }

        /// <summary>
        /// Gets the PWM level for a servo output, and returns an integer value where 2000 represents a 1 millisecond servo burst.  Examples: 2000 -> 1 ms: typical shortest burst; 5000 -> 2.5 ms burst: higher than typical longest burst
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="checkBoundary">Available options: minimum, maximum or boot (startup)</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Integer value where n / 2000 = 1 millisecond</returns>
        public int GetServo(byte motor, ValueType checkBoundary, ILogger log = null)
        {
            int tempreturn = -1;
            byte tempCommand = 0x00;

            switch(checkBoundary)
            {
                case ValueType.Minimum:
                    switch(motor)
                    {
                        case 1:
                            tempCommand = COMMAND_GET_PWM_MIN_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_GET_PWM_MIN_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_GET_PWM_MIN_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_GET_PWM_MIN_4;
                            break;
                        default:
                            tempCommand = 0xFC;
                            break;
                    }
                    break;

                case ValueType.Maximum:
                    switch (motor)
                    {
                        case 1:
                            tempCommand = COMMAND_GET_PWM_MAX_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_GET_PWM_MAX_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_GET_PWM_MAX_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_GET_PWM_MAX_4;
                            break;
                        default:
                            tempCommand = 0xFD;
                            break;
                    }
                    break;

                case ValueType.Boot:
                    switch(motor)
                    {
                        case 1: 
                            tempCommand = COMMAND_GET_PWM_BOOT_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_GET_PWM_BOOT_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_GET_PWM_BOOT_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_GET_PWM_BOOT_4;
                            break;
                        default:
                            tempCommand = 0xFE;
                            break;
                    }
                    break;

                default:
                    tempCommand = 0xFF;
                    break;
            }

            if (tempCommand > 0xF1)
            {
                if (log != null)
                {
                    string message = string.Empty;

                    message = "An error was returned trying to enumerate GetServo; passed " + checkBoundary.ToString() + " and " + motor.ToString() + " as parameters...\n";
                    
                    switch (tempCommand)
                    {
                        case 0xFC:
                            message += "Got unknown motor while enumerating minimum...";
                            break;
                        case 0xFD:
                            message += "Got unknown motor while enumerating maximum...";
                            break;
                        case 0xFE:
                            message += "Got unknown motor while enumerating boot...";
                            break;
                        case 0xFF:
                            message += "Got a check for a boundary that doesn't exist...";
                            break;
                    }

                    log.WriteLog(message, ILogger.Priority.Critical);
                }
            }

            try
            {
                if (log != null)
                {
                    string tempMessage = "Getting servo ";
                    switch(checkBoundary)
                    {
                        case ValueType.Minimum:
                            tempMessage += "minimum ";
                            break;
                        case ValueType.Maximum:
                            tempMessage += "maximum ";
                            break;
                        case ValueType.Boot:
                            tempMessage += "boot ";
                            break;
                    }

                    tempMessage += "for motor " + motor.ToString() + "...";

                    log.WriteLog(tempMessage, ILogger.Priority.Information);
                }
                using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
                {
                    bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand });
                    byte[] response = bus.ReadBytes(this._UltraBorgAddress, I2C_MAX_LEN);

                    if (response[0] == tempCommand)
                    {
                        tempreturn = (response[1] << 8) + response[2];
                    }
                }
            }
            catch (Exception ex)
            {
                // do something here
            }

            return tempreturn;
        }

        /// <summary>
        /// Sets the raw PWM level for the motor referenced by *motor*.  The value is 0 <= n <= 65535 where 0 is a 0% duty cycle and 65535 is a 100% duty cycle.
        /// NOTE: although this routine *DOES* reject values outside this range, there is *no limit checking on the board*.  It is possible to set values OUTSIDE
        /// this range, but doing so for extended periods may damage the board or the servo.  PiBorg and the PiBorgSharp project recommend using the tuning GUI 
        /// for setting the servo limits.
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="powerLevel">0 for a 0% duty cycle <= n <= 65535 for a 100% duty cycle</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void CalibrateServoPosition(byte motor, ushort powerLevel, ILogger log = null)
        {
            byte pwmDutyLow = (byte)(powerLevel & 0xFF);
            byte pwmDutyHigh = (byte)((powerLevel >> 8) & 0xFF);
            byte tempCommand = 0xFF;

            if ((pwmDutyHigh > 255) | (pwmDutyHigh < 0) | (pwmDutyLow<0) | (pwmDutyLow > 255))
            {
                if (log != null)
                {
                    log.WriteLog("Received a value outside the boundaries of 0 < n < 65535 (but I don't know how...)", ILogger.Priority.Critical);
                }
                
                throw new ArgumentException("powerLevel setting - argument out of bounds");
            }

            if (log != null)
            {
                log.WriteLog("Received word: " + powerLevel.ToString("X2") + " and parsed low: " + pwmDutyLow.ToString("X2") + " and high: " + pwmDutyHigh.ToString("X2") + "...", ILogger.Priority.Information);
            }

            switch (motor)
            {
                case 1:
                    tempCommand = COMMAND_CALIBRATE_PWM1;
                    break;
                case 2:
                    tempCommand = COMMAND_CALIBRATE_PWM2;
                    break;
                case 3:
                    tempCommand = COMMAND_CALIBRATE_PWM3;
                    break;
                case 4:
                    tempCommand = COMMAND_CALIBRATE_PWM4;
                    break;
                default:
                    tempCommand = 0xFF;
                    break;
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand, pwmDutyHigh, pwmDutyLow });
            }
        }

        /// <summary>
        /// Gets the raw PWM level for the servo output.  Return value is anywhere between 0 for a 0% duty cycle to 65535 for a 100% duty cycle.
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>0 <= n <= 65535 representing the duty cycle of the selected servo</returns>
        public ushort GetRawServoPosition(byte motor, ILogger log = null)
        {
            byte tempCommand = 0xFF;
            ushort tempReturn = 0;

            switch (motor)
            {
                case 1:
                    tempCommand = COMMAND_GET_PWM1;
                    break;
                case 2:
                    tempCommand = COMMAND_GET_PWM2;
                    break;
                case 3:
                    tempCommand = COMMAND_GET_PWM3;
                    break;
                case 4:
                    tempCommand = COMMAND_GET_PWM4;
                    break;
                default:
                    tempCommand = 0xFE;
                    break;
            }

            if (log != null)
            {
                log.WriteLog("Polling motor " + motor.ToString() + " for raw servo position...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand });
                byte[] response = bus.ReadBytes(_UltraBorgAddress, I2C_MAX_LEN);
                tempReturn = (ushort)((response[1] << 8) + response[2]);
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets the servo levels for minimum, maximum and boot level.  NOTE: trying to set a boot value outside the min/max (default, or as previously set) will fail and throw an ArgumentException.
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="checkBoundary">Available options: minimum, maximum or boot (startup)</param>
        /// <param name="powerLevel">0 <= n <= 65535 representing the duty cycle of the selected servo</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetServo(byte motor, ValueType checkBoundary, ushort powerLevel, ILogger log = null) 
        {
            byte tempCommand = 0x00;

            switch (checkBoundary)
            {
                case ValueType.Minimum:
                    switch (motor)
                    {
                        case 1:
                            tempCommand = COMMAND_SET_PWM_MIN_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_SET_PWM_MIN_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_SET_PWM_MIN_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_SET_PWM_MIN_4;
                            break;
                        default:
                            tempCommand = 0xFC;
                            break;
                    }
                    break;

                case ValueType.Maximum:
                    switch (motor)
                    {
                        case 1:
                            tempCommand = COMMAND_SET_PWM_MAX_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_SET_PWM_MAX_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_SET_PWM_MAX_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_SET_PWM_MAX_4;
                            break;
                        default:
                            tempCommand = 0xFD;
                            break;
                    }
                    break;

                case ValueType.Boot:
                    switch (motor)
                    {
                        case 1:
                            tempCommand = COMMAND_SET_PWM_BOOT_1;
                            break;
                        case 2:
                            tempCommand = COMMAND_SET_PWM_BOOT_2;
                            break;
                        case 3:
                            tempCommand = COMMAND_SET_PWM_BOOT_3;
                            break;
                        case 4:
                            tempCommand = COMMAND_SET_PWM_BOOT_4;
                            break;
                        default:
                            tempCommand = 0xFE;
                            break;
                    }
                    break;

                default:
                    tempCommand = 0xFF;
                    break;
            }

            if (checkBoundary == ValueType.Boot)
            {
                bool passBoundaryValidation = false;
                int checkMin = 0;
                int checkMax = 65535;

                // original author assumes that in the case of PWM_MIN_x being greater than PWM_MAX_x then the system would reverse the check; to accomplish
                // this, I'm going to set my own boundaries to ensure the smaller is the minimum

                switch (motor)
                {
                    case 1:
                        checkMin = Math.Min(this.PWM_MIN_1, this.PWM_MAX_1);
                        checkMax = Math.Max(this.PWM_MIN_1, this.PWM_MAX_1);
                        if ((powerLevel >= checkMin) && (powerLevel <= checkMax))
                        {
                            passBoundaryValidation = true;
                        }
                        break;
                    case 2:
                        checkMin = Math.Min(this.PWM_MIN_2, this.PWM_MAX_2);
                        checkMax = Math.Max(this.PWM_MIN_2, this.PWM_MAX_2);
                        if ((powerLevel >= checkMin) && (powerLevel <= checkMax))
                        {
                            passBoundaryValidation = true;
                        }
                        break;
                    case 3:
                        checkMin = Math.Min(this.PWM_MIN_3, this.PWM_MAX_3);
                        checkMax = Math.Max(this.PWM_MIN_3, this.PWM_MAX_3);
                        if ((powerLevel >= checkMin) && (powerLevel <= checkMax))
                        {
                            passBoundaryValidation = true;
                        }
                        break;
                    case 4:
                        checkMin = Math.Min(this.PWM_MIN_4, this.PWM_MAX_4);
                        checkMax = Math.Max(this.PWM_MIN_4, this.PWM_MAX_4);
                        if ((powerLevel >= checkMin) && (powerLevel <= checkMax))
                        {
                            passBoundaryValidation = true;
                        }
                        break;
                    default:
                        passBoundaryValidation = false;
                        break;
                }

                if (!passBoundaryValidation)
                {
                    if (log != null)
                    {
                        log.WriteLog("Setting boot value for motor " + motor.ToString() + " failed validation check...", ILogger.Priority.Critical);
                    }

                    throw new ArgumentException("Boot power level out of range for motor " + motor.ToString());
                }
            }

            if (tempCommand > 0xF1)
            {
                if (log != null)
                {
                    string message = string.Empty;

                    message = "An error was returned trying to prepare SetServo; passed " + checkBoundary.ToString() + " and " + motor.ToString() + " as parameters...\n";

                    switch (tempCommand)
                    {
                        case 0xFC:
                            message += "Got unknown motor while setting minimum...";
                            break;
                        case 0xFD:
                            message += "Got unknown motor while setting maximum...";
                            break;
                        case 0xFE:
                            message += "Got unknown motor while setting boot...";
                            break;
                        case 0xFF:
                            message += "Got a boundary that doesn't exist...";
                            break;
                    }

                    log.WriteLog(message, ILogger.Priority.Critical);
                }
            }

            byte pwmDutyLow = (byte)(powerLevel & 0xFF);
            byte pwmDutyHigh = (byte)((powerLevel >> 8) & 0xFF);

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand, pwmDutyHigh, pwmDutyLow });

                // note: the system has to sleep for a brief time to write changes to the EEPROM
                System.Threading.Thread.Sleep(Convert.ToInt32(DELAY_AFTER_EEPROM * 1000));

                switch (tempCommand)
                {
                    case var _ when tempCommand == COMMAND_SET_PWM_MIN_1:
                        this.PWM_MIN_1 = this.GetServo(1, ValueType.Minimum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MIN_2:
                        this.PWM_MIN_2 = this.GetServo(2, ValueType.Minimum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MIN_3:
                        this.PWM_MIN_3 = this.GetServo(3, ValueType.Minimum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MIN_4:
                        this.PWM_MIN_4 = this.GetServo(4, ValueType.Minimum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MAX_1:
                        this.PWM_MAX_1 = this.GetServo(1, ValueType.Maximum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MAX_2:
                        this.PWM_MAX_2 = this.GetServo(2, ValueType.Maximum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MAX_3:
                        this.PWM_MAX_3 = this.GetServo(3, ValueType.Maximum, log);
                        break;
                    case var _ when tempCommand == COMMAND_SET_PWM_MAX_4:
                        this.PWM_MAX_4 = this.GetServo(4, ValueType.Maximum, log);
                        break;
                    default:
                        break;
                }

            }


        }

        /// <summary>
        /// Gets the distance for the ultrasonic modules in two forms: filtered and unfiltered.  Filtered responses are slower but more stable; unfiltered is more jumpy, but more responsive.  
        /// Response examples: 0 - no object in range; 25 - object 25mm away; 3500 - object 3.5 m away
        /// </summary>
        /// <param name="sensor">Selected sensor: 1, 2, 3, or 4</param>
        /// <param name="filter">Filtered - slower but more stable responses; unfiltered - faster but more jumpy responses</param>
        /// <param name="log"></param>
        /// <returns>Default: null; the ILogger interface used in this library</returns>
        public uint GetDistance(byte sensor, FilterType filter, ILogger log = null)
        {
            uint tempReturn = 0;
            byte tempCommand = 0xFF;

            if (filter == FilterType.Filtered)
            {
                switch (sensor)
                {
                    case 1:
                        tempCommand = COMMAND_GET_FILTER_USM1;
                        break;
                    case 2:
                        tempCommand = COMMAND_GET_FILTER_USM2;
                        break;
                    case 3:
                        tempCommand = COMMAND_GET_FILTER_USM3;
                        break;
                    case 4:
                        tempCommand = COMMAND_GET_FILTER_USM4;
                        break;
                    default:
                        tempCommand = 0xFE;
                        break;
                }
            }
            else
            {
                switch (sensor)
                {
                    case 1:
                        tempCommand = COMMAND_GET_TIME_USM1;
                        break;
                    case 2:
                        tempCommand = COMMAND_GET_TIME_USM2;
                        break;
                    case 3:
                        tempCommand = COMMAND_GET_TIME_USM3;
                        break;
                    case 4:
                        tempCommand = COMMAND_GET_TIME_USM4;
                        break;
                    default:
                        tempCommand = 0xFD;
                        break;
                }
            }

            if (log != null)
            {
                string tempMessage = "Getting ";
                if (filter == FilterType.Filtered)
                {
                    tempMessage += "filtered ";
                }
                else
                {
                    tempMessage += "unfiltered ";
                }
                tempMessage += "distance from sensor #" + sensor.ToString();

                log.WriteLog(tempMessage, ILogger.Priority.Information);
            }

            if (THROTTLE_CODE)
            {
                switch (sensor)
                {
                    case 1:
                        {
                            while (_internalClock_Sensor1.ElapsedMilliseconds < THROTTLE_SPEED)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                        break;
                    case 2:
                        {
                            while (_internalClock_Sensor2.ElapsedMilliseconds < THROTTLE_SPEED)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                        break;
                    case 3:
                        {
                            while (_internalClock_Sensor3.ElapsedMilliseconds < THROTTLE_SPEED)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                        break;
                    case 4:
                        {
                            while (_internalClock_Sensor4.ElapsedMilliseconds < THROTTLE_SPEED)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                        }
                        break;
                }
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand });
                byte[] response = bus.ReadBytes(_UltraBorgAddress, I2C_MAX_LEN);
                if (response[0] == tempCommand)
                {
                    tempReturn = (uint)((response[1] << 8) + response[2]);
                    if (log != null)
                    {
                        string tempMessage = "Returned " + tempReturn.ToString() + " as a raw result...";
                        log.WriteLog(tempMessage, ILogger.Priority.Information);
                    }
                    if (tempReturn == 65535)
                    {
                        tempReturn = 0;
                    }
                    else
                    {
                        tempReturn = Convert.ToUInt32(tempReturn * USM_US_TO_MM);
                    }
                }
            }

            if (THROTTLE_CODE)
            {
                switch (sensor)
                {
                    case 1:
                        {
                            _internalClock_Sensor1.Restart();
                        }
                        break;
                    case 2:
                        {
                            _internalClock_Sensor2.Restart();
                        }
                        break;
                    case 3:
                        {
                            _internalClock_Sensor3.Restart();
                        }
                        break;
                    case 4:
                        {
                            _internalClock_Sensor4.Restart();
                        }
                        break;
                }
            }

            return tempReturn;
        }

        public void SetServoPosition(byte motor, ushort powerLevel, ILogger log = null)
        {
            byte highLevel = 0x00;
            byte lowLevel = 0x00;
            byte tempCommand = 0xFF;

            switch (motor)
            {
                case 1:
                    tempCommand = COMMAND_SET_PWM1;
                    break;
                case 2:
                    tempCommand = COMMAND_SET_PWM2;
                    break;
                case 3:
                    tempCommand = COMMAND_SET_PWM3;
                    break;
                case 4:
                    tempCommand = COMMAND_SET_PWM4;
                    break;
            }

            lowLevel = (byte)(powerLevel & 0xFF);
            highLevel = (byte)((powerLevel >> 8) & 0xFF);

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_UltraBorgAddress, new byte[] { tempCommand, highLevel, lowLevel });
            }
        }

        public long Internal1TimePassed()
        {
            return _internalClock_Sensor1.ElapsedMilliseconds;
        }

        public bool Internal1TimerActive()
        {
            return _internalClock_Sensor1.IsRunning;
        }
    }
}
