using System;
using System.Collections.Generic;
using System.Text;

namespace PiBorgSharp.ZeroBorg
{
    class ZeroBorg_class
    {
        // based on original source written by Arron Churchill (I think): https://www.piborg.org/blog/piborg-arron
        public static readonly bool THROTTLE_CODE               = false;     // added to introduce code throttling; maybe the code is too fast?

        public static readonly int I2C_SLAVE                    = 0x0703;
        public static readonly byte PWM_MAX                     = 0xFF;
        public static readonly byte I2C_NORM_LEN                = 0x04;
        public static readonly byte I2C_LONG_LEN                = 0x18;

        public static readonly byte I2C_ID_ZEROBORG             = 0x40;

        public static readonly byte COMMAND_SET_LED             = 0x01;
        public static readonly byte COMMAND_GET_LED             = 0x02;
        public static readonly byte COMMAND_SET_A_FWD           = 0x03;
        public static readonly byte COMMAND_SET_A_REV           = 0x04;
        public static readonly byte COMMAND_GET_A               = 0x05;
        public static readonly byte COMMAND_SET_B_FWD           = 0x06;
        public static readonly byte COMMAND_SET_B_REV           = 0x07;
        public static readonly byte COMMAND_GET_B               = 0x08;
        public static readonly byte COMMAND_SET_C_FWD           = 0x09;
        public static readonly byte COMMAND_SET_C_REV           = 0x0A;
        public static readonly byte COMMAND_GET_C               = 0x0B;
        public static readonly byte COMMAND_SET_D_FWD           = 0x0C;
        public static readonly byte COMMAND_SET_D_REV           = 0x0D;
        public static readonly byte COMMAND_GET_D               = 0x0E;
        public static readonly byte COMMAND_ALL_OFF             = 0x0F;
        public static readonly byte COMMAND_SET_ALL_FWD         = 0x10;
        public static readonly byte COMMAND_SET_ALL_REV         = 0x11;
        public static readonly byte COMMAND_SET_FAILSAFE        = 0x12;
        public static readonly byte COMMAND_GET_FAILSAFE        = 0x13;
        public static readonly byte COMMAND_RESET_EPO           = 0x14;
        public static readonly byte COMMAND_GET_EPO             = 0x15;
        public static readonly byte COMMAND_SET_EPO_IGNORE      = 0x16;
        public static readonly byte COMMAND_GET_EPO_IGNORE      = 0x17;
        public static readonly byte COMMAND_GET_NEW_IR          = 0x18;
        public static readonly byte COMMAND_GET_LAST_IR         = 0x19;
        public static readonly byte COMMAND_SET_LED_IR          = 0x1A;
        public static readonly byte COMMAND_GET_LED_IR          = 0x1B;
        public static readonly byte COMMAND_GET_ANALOG_1        = 0x1C;
        public static readonly byte COMMAND_GET_ANALOG_2        = 0x1D;
        public static readonly byte COMMAND_GET_ID              = 0x99;
        public static readonly byte COMMAND_SET_I2C_ADD         = 0xAA;

        public static readonly byte COMMAND_VALUE_FWD           = 0x01;
        public static readonly byte COMMAND_VALUE_REV           = 0x02;

        public static readonly byte COMMAND_VALUE_ON            = 0x01;
        public static readonly byte COMMAND_VALUE_OFF           = 0x00;

        public static readonly ushort COMMAND_ANALOG_MAX        = 0x3FF;

        public static readonly byte IR_MAX_BYTES                = (byte)(I2C_LONG_LEN - 2);

        private int _bus = 0x01;
        private int _ZeroBorgAddress = 0x00;
        private ILogger _log = null;

        /// <summary>
        /// Scans the I2C bus for the ZeroBorg board.  It will scan bus 1 by default, and return the port number when it gets a ZeroBorg response to the board ID request.
        /// </summary>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Port number found for ZeroBorg; -1 if no board found</returns>
        public static int ScanForZeroBorg(int busNumber = 1, ILogger log = null)
        {
            int tempReturn = -1;

            if (log != null)
            {
                log.WriteLog("Starting scan for ZeroBorg_class board...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                for (byte port = 0x03; port < 0x78; port++)
                {
                    try
                    {
                        bus.WriteByte(port, COMMAND_GET_ID);
                        byte[] response = bus.ReadBytes(port, I2C_NORM_LEN);
                        if (response[0] == 0x99)
                        {
                            if (response[1] == I2C_ID_ZEROBORG)
                            {
                                tempReturn = port;
                                if (log != null)
                                {
                                    log.WriteLog("FOUND ZeroBorg_class board on port: " + port.ToString("X2"));
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
        /// Sets a new address for the ZeroBorg.  CAUTION: this setting is persistent and will continue after powering down your ZeroBorg.
        /// </summary>
        /// <param name="newAddress">The new port address for the ZeroBorg</param>
        /// <param name="oldAddress">Optional - if specified the method will use this address as opposed to scanning for the ZeroBorg</param>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        /// <returns>The new port number for the ZeroBorg</returns>
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
                _oldAddress = ZeroBorg_class.ScanForZeroBorg(Convert.ToInt32(busNumber), logger);
                if (_oldAddress < 0)
                {
                    throw new Exception("ZeroBorg board not found.");
                }
            }
            else
            {
                _oldAddress = Convert.ToInt32(oldAddress);
            }

            if (logger != null)
            {
                logger.WriteLog("Attempting change of ZeroBorg address from " + _oldAddress.ToString("X2") + " to " + newAddress.ToString("X2"), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                bus.WriteBytes(_oldAddress, new byte[] { COMMAND_SET_I2C_ADD, newAddress });

                System.Threading.Thread.Sleep(200);         // let the I2C bus catch up

                int tempCheck = ZeroBorg_class.ScanForZeroBorg(Convert.ToInt32(busNumber), logger);

                if (tempCheck == newAddress)
                {
                    logger.WriteLog("CHANGED BOARD ADDRESS FROM " + _oldAddress.ToString("X2") + " TO " + newAddress.ToString("X2"), ILogger.Priority.Critical);
                    logger.WriteLog("This change will be persistent even after a reboot; keep track of it.", ILogger.Priority.Information);
                }
                else
                {
                    logger.WriteLog("**FAILED** to change ZeroBorg address.  Current address: " + tempCheck.ToString("X2"));
                }
                return tempCheck;
            }
        }

        /// <summary>
        /// Main ZeroBorg class
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <param name="tryOtherBus">CURRENTLY NO EFFECT</param>
        public ZeroBorg_class(ILogger log = null, bool tryOtherBus = false)
        {
            throw new NotImplementedException();

            if (log != null)
            {
                this._log = log;
                log.WriteLog("THROTTLE_CODE: " + THROTTLE_CODE.ToString(), ILogger.Priority.Information);
                log.WriteLog("Instantiating UltraBorg_class...", ILogger.Priority.Information);
            }

            this._ZeroBorgAddress = ZeroBorg_class.ScanForZeroBorg(1, log);

            if (log != null)
            {
                log.WriteLog("Loading UltraBorg on bus " + _bus.ToString("X2") + ", address " + _ZeroBorgAddress.ToString("X2"), ILogger.Priority.Medium);
            }
        }

        /// <summary>
        /// Set the selected motor level to forward or reverse.  Power levels are -255 < n < 255.
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="powerLevel">Power setting of selected motor; -255 < n < 255</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetMotor(byte motor, short powerLevel, ILogger log = null)
        {
            byte tempCommand = 0xFF;
            byte parsedPower = 0;

            //if (!_CheckInit())
            //{
            //    return;
            //}

            if ((powerLevel > 255) || (powerLevel < -255))
            {
                if (log != null)
                {
                    log.WriteLog("Power level out of range -255 <= power <= 255; rejecting command...");
                }
                throw new IndexOutOfRangeException("Invalid power setting to motor; range outside of -255 <= power <= 255.");
            }

            if (powerLevel >= 0)
            {
                switch (motor)
                {
                    case 1:
                        tempCommand = COMMAND_SET_A_FWD;
                        break;
                    case 2:
                        tempCommand = COMMAND_SET_B_FWD;
                        break;
                    case 3:
                        tempCommand = COMMAND_SET_C_FWD;
                        break;
                    case 4:
                        tempCommand = COMMAND_SET_D_FWD;
                        break;
                    default:
                        tempCommand = 0xFE;
                        break;
                }
            }
            else
            {
                switch (motor)
                {
                    case 1:
                        tempCommand = COMMAND_SET_A_REV;
                        break;
                    case 2:
                        tempCommand = COMMAND_SET_B_REV;
                        break;
                    case 3:
                        tempCommand = COMMAND_SET_C_REV;
                        break;
                    case 4:
                        tempCommand = COMMAND_SET_D_REV;
                        break;
                    default:
                        tempCommand = 0xFD;
                        break;
                }
            }

            if (powerLevel > 0)
            {
                parsedPower = Convert.ToByte(powerLevel);
            }
            else
            {
                parsedPower = Convert.ToByte(-powerLevel);
            }

            if (log != null)
            {
                log.WriteLog("Setting motor " + motor.ToString() + " to: " + powerLevel.ToString("X2") + " " + PiBorgSharp.Utilities.ParseDirection(powerLevel));
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (powerLevel > 0)
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { tempCommand, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { tempCommand, parsedPower });
                }
            }
        }

        /// <summary>
        /// Gets the selected motor level; positive is forward, negative is reverse
        /// </summary>
        /// <param name="motor">Selected motor: 1, 2, 3, or 4</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns></returns>
        public short GetMotor(byte motor, ILogger log = null)
        {
            short tempReturn = 0;
            byte tempCommand = 0xFF;

            switch (motor)
            {
                case 1:
                    tempCommand = COMMAND_GET_A;
                    break;
                case 2:
                    tempCommand = COMMAND_GET_B;
                    break;
                case 3:
                    tempCommand = COMMAND_GET_C;
                    break;
                case 4:
                    tempCommand = COMMAND_GET_D;
                    break;
                default:
                    tempCommand = 0xFE;
                    break;
            }

            if (log != null)
            {
                log.WriteLog("Getting value from motor " + motor.ToString() + "...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { tempCommand });
                byte[] response = bus.ReadBytes(_ZeroBorgAddress, I2C_NORM_LEN);

                if (response[0] == tempCommand)
                {
                    if (response[1] == COMMAND_VALUE_FWD)
                    {
                        tempReturn = response[2];
                    }
                    else if (response[1] == COMMAND_VALUE_REV)
                    {
                        tempReturn = (short)-response[2];
                    }
                }

                if (log != null)
                {
                    log.WriteLog("Retrieved power setting of " + tempReturn.ToString() + " from motor " + motor.ToString() + "...", ILogger.Priority.Information);
                }

                return tempReturn;
            }
        }

        /// <summary>
        /// Sets all motors to the speed specified by powerLevel where -255 < powerLevel < 255; positive power is forward, negative power is reverse.
        /// </summary>
        /// <param name="powerLevel">Positive forward, negative reverse where -255 < n < 255</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetMotors(short powerLevel, ILogger log = null)
        {
            byte tempCommand = 0xFF;
            byte parsedPower = 0;

            //if (!_CheckInit())
            //{
            //    return;
            //}

            if ((powerLevel > 255) || (powerLevel < -255))
            {
                if (log != null)
                {
                    log.WriteLog("Power level out of range -255 <= power <= 255; rejecting command...");
                }
                throw new IndexOutOfRangeException("Invalid power setting to motor; range outside of -255 <= power <= 255.");
            }

            if (powerLevel >= 0)
            {
                tempCommand = COMMAND_SET_ALL_FWD;
            }
            else if (powerLevel < 0) 
            {
                tempCommand = COMMAND_SET_ALL_REV;
            }

            if (powerLevel > 0)
            {
                parsedPower = Convert.ToByte(powerLevel);
            }
            else
            {
                parsedPower = Convert.ToByte(-powerLevel);
            }

            if (log != null)
            {
                log.WriteLog("Setting all motors to: " + powerLevel.ToString("X2") + " " + PiBorgSharp.Utilities.ParseDirection(powerLevel));
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (powerLevel > 0)
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { tempCommand, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { tempCommand, parsedPower });
                }
            }
        }

        /// <summary>
        /// Command to set all motors to stop.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void AllStop(ILogger log = null)
        {
            if (log != null)
            {
                log.WriteLog("Sending all stop command...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_ALL_OFF });
            }
        }

        /// <summary>
        /// Sets the LED state
        /// </summary>
        /// <param name="state">True - on; false - off</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetLEDState(bool state, ILogger log = null)
        {
            if (log != null)
            {
                log.WriteLog("Setting LED to state " + state.ToString() + "...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (state)
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_SET_LED, COMMAND_VALUE_ON });
                }
                else
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_SET_LED, COMMAND_VALUE_OFF });
                }
            }
        }

        /// <summary>
        /// Gets the current LED state
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - on; false - off</returns>
        public bool GetLEDState(ILogger log = null)
        {
            bool tempReturn = false;

            if (log != null)
            {
                log.WriteLog("Retreiving LED state...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_GET_LED });
                byte[] response = bus.ReadBytes(_ZeroBorgAddress, I2C_NORM_LEN);

                if (response[0] == COMMAND_GET_LED)
                {
                    if (response[1] == COMMAND_VALUE_ON)
                    {
                        tempReturn = true;
                    }
                    else if(response[1] == COMMAND_VALUE_OFF)
                    {
                        tempReturn = false;
                    }
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Resets the EPO switch; used to allow movement again after the EPO has been tripped
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void ResetEPO(ILogger log = null)
        {
            if (log != null)
            {
                log.WriteLog("Resetting the EPO switch...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_RESET_EPO });
            }
        }

        /// <summary>
        /// Gets the current EPO state.  If the EPO state is true, it will prevent movement until reset or ignored.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - current state is TRIPPED; false - current state is off</returns>
        public bool GetEPO(ILogger log = null)
        {
            bool tempReturn = false;

            if (log != null)
            {
                log.WriteLog("Getting EPO state...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_GET_EPO });
                byte[] response = bus.ReadBytes(_ZeroBorgAddress, I2C_NORM_LEN);
                if (response[0] == COMMAND_GET_EPO)
                {
                    if (response[1] == COMMAND_VALUE_ON)
                    {
                        tempReturn = true;
                    }
                    else if (response[1] == COMMAND_VALUE_OFF)
                    {
                        tempReturn = false;
                    }
                }
            }

            return tempReturn;
        }


        /// <summary>
        /// Sets the EPO ignore flag to the desired state
        /// </summary>
        /// <param name="state">True - ignore EPO; false - respect EPO</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetEPOIgnore(bool state, ILogger log = null)
        {
            if (log != null)
            {
                log.WriteLog("Setting EPO ignore flag to " + state.ToString(), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (state)
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_SET_EPO_IGNORE, COMMAND_VALUE_ON });
                }
                else if (!state)
                {
                    bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_SET_EPO_IGNORE, COMMAND_VALUE_OFF });
                }
            }
        }

        /// <summary>
        /// Gets the current state of the EPO ignore flag
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - ignore flag is active and EPO is ignored; false - ignore flag is inactive and EPO is respected</returns>
        public bool GetEPOIgnore(ILogger log = null)
        {
            bool tempReturn = false;

            if (log != null)
            {
                log.WriteLog("Getting EPO Ignore flag status...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_GET_EPO_IGNORE });
                byte[] response = bus.ReadBytes(_ZeroBorgAddress, I2C_NORM_LEN);

                if (response[0] == COMMAND_GET_EPO_IGNORE)
                {
                    if (response[1] == COMMAND_VALUE_ON)
                    {
                        tempReturn = true;
                    }
                    else if (response[1] == COMMAND_VALUE_OFF)
                    {
                        tempReturn = false;
                    }
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Detects if there is an IR message that has not yet been read since the last read
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - message received since last read; false - no messages received since last read</returns>
        public bool HasNewIRMessage(ILogger log = null)
        {
            bool tempReturn = true;     // note: this violates my normal method of coding just in case the COMMAND_GET_NEW_IR returns the number of messages received

            if (log != null)
            {
                log.WriteLog("Checking to see if there is an unread IR message...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_ZeroBorgAddress, new byte[] { COMMAND_GET_NEW_IR });
                byte[] response = bus.ReadBytes(_ZeroBorgAddress, I2C_NORM_LEN);

                if (response[0] == COMMAND_GET_NEW_IR)
                {
                    if (response[1] == COMMAND_VALUE_OFF)
                    {
                        tempReturn = false;
                    }
                    if (log != null)
                    {
                        log.WriteLog("Got a response of " + response[1].ToString("X2") + " from the response...", ILogger.Priority.Information);
                    }
                }
            }

            return tempReturn;
        }
    }
}
