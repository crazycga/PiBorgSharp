using System;
using System.Collections.Generic;
using System.Text;

namespace PiBorgSharp.UltraBorg
{
    public class UltraBorg_class
    {
        // based on original source written by Arron Churchill (I think): https://www.piborg.org/blog/piborg-arron
        public static readonly bool THROTTLE_CODE = false;

        public static readonly int I2C_SLAVE                    = 0x0703;
        public static readonly byte I2C_MAX_LEN                 = 0x04;
        public static readonly decimal USM_US_TO_MM             = 0.17500M;
        public static readonly int PWM_MIN                      = 2000;
        public static readonly int PWM_MAX                      = 4000;
        public static readonly decimal DELAY_AFTER_EPROM        = 0.01M;
        public static readonly int PWM_RESET                    = 0xFFFF;

        public static readonly byte I2C_ID_SERVO_USM            = 0x36;

        public static readonly byte COMMAND_GET_TIME_USM1       = 0x01;
        public static readonly byte COMMAND_GET_TIME_USM2       = 0x02;
        public static readonly byte COMMAND_GET_TIME_USM3       = 0x03;
        public static readonly byte COMMAND_GET_TIME_USM4       = 0x04;
        public static readonly byte COMMAND_SET_PWM1            = 0x05;
        public static readonly byte COMMAND_GET_PWM1            = 0x06;
        public static readonly byte COMMAND_SET_PWM2            = 0x07;
        public static readonly byte COMMAND_GET_PWM2            = 0x08;
        public static readonly byte COMMAND_SET_PWM3            = 0x09;
        public static readonly byte COMMAND_GET_PWM3            = 0x0A;
        public static readonly byte COMMAND_SET_PWM4            = 0x0B;
        public static readonly byte COMMAND_GET_PWM4            = 0x0C;
        public static readonly byte COMMAND_CALIBRATE_PWM1      = 0x0D;
        public static readonly byte COMMAND_CALIBRATE_PWM2      = 0x0E;
        public static readonly byte COMMAND_CALIBRATE_PWM3      = 0x0F;
        public static readonly byte COMMAND_CALIBRATE_PWM4      = 0x10;
        public static readonly byte COMMAND_GET_PWM_MIN_1       = 0x11;
        public static readonly byte COMMAND_GET_PWM_MAX_1       = 0x12;
        public static readonly byte COMMAND_GET_PWM_BOOT_1      = 0x13;
        public static readonly byte COMMAND_GET_PWM_MIN_2       = 0x14;
        public static readonly byte COMMAND_GET_PWM_MAX_2       = 0x15;
        public static readonly byte COMMAND_GET_PWM_BOOT_2      = 0x16;
        public static readonly byte COMMAND_GET_PWM_MIN_3       = 0x17;
        public static readonly byte COMMAND_GET_PWM_MAX_3       = 0x18;
        public static readonly byte COMMAND_GET_PWM_BOOT_3      = 0x19;
        public static readonly byte COMMAND_GET_PWM_MIN_4       = 0x1A;
        public static readonly byte COMMAND_GET_PWM_MAX_4       = 0x1B;
        public static readonly byte COMMAND_GET_PWM_BOOT_4      = 0x1C;
        public static readonly byte COMMAND_SET_PWM_MIN_1       = 0x1D;
        public static readonly byte COMMAND_SET_PWM_MAX_1       = 0x1E;
        public static readonly byte COMMAND_SET_PWM_BOOT_1      = 0x1F;
        public static readonly byte COMMAND_SET_PWM_MIN_2       = 0x20;
        public static readonly byte COMMAND_SET_PWM_MAX_2       = 0x21;
        public static readonly byte COMMAND_SET_PWM_BOOT_2      = 0x22;
        public static readonly byte COMMAND_SET_PWM_MIN_3       = 0x23;
        public static readonly byte COMMAND_SET_PWM_MAX_3       = 0x24;
        public static readonly byte COMMAND_SET_PWM_BOOT_3      = 0x25;
        public static readonly byte COMMAND_SET_PWM_MIN_4       = 0x26;
        public static readonly byte COMMAND_SET_PWM_MAX_4       = 0x27;
        public static readonly byte COMMAND_SET_PWM_BOOT_4      = 0x28;
        public static readonly byte COMMAND_GET_FILTER_USM1     = 0x29;
        public static readonly byte COMMAND_GET_FILTER_USM2     = 0x2A;
        public static readonly byte COMMAND_GET_FILTER_USM3     = 0x2B;
        public static readonly byte COMMAND_GET_FILTER_USM4     = 0x2C;
        public static readonly byte COMMAND_GET_ID              = 0x99;
        public static readonly byte COMMAND_SET_I2C_ADD         = 0xAA;

        public static readonly byte COMMAND_VALUE_FWD           = 0x01;
        public static readonly byte COMMAND_VALUE_REV           = 0x02;

        public static readonly byte COMMAND_VALUE_ON            = 0x01;
        public static readonly byte COMMAND_VALUE_OFF           = 0x00;

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

        public enum ValueType
        {
            Minimum = 1
            , Maximum = 2
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
            throw new NotImplementedException();
            
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

            //
            //this.PWM_MIN_1 = this.GetWithRetry(this.GetServoMinimum1, 5);

        }

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
                            tempCommand = 0xFD;
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
                        case 0xFD:
                            message += "Got unknown motor while enumerating minimum...";
                            break;
                        case 0xFE:
                            message += "Got unknown motor while enumerating maximum...";
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
                    log.WriteLog("Getting servo minimum for motor...", ILogger.Priority.Information);
                }
                using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
                {
                    bus.WriteBytes(_UltraBorgAddress, new byte[] { COMMAND_GET_PWM_MIN_1 });
                    byte[] response = bus.ReadBytes(this._UltraBorgAddress, I2C_MAX_LEN);

                    if (response[0] == COMMAND_GET_PWM_MIN_1)
                    {
                        tempreturn = (response[1] << 8) + response[2];
                    }
                }
            }


            return tempreturn;
        }


    }
}
