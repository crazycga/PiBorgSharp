using System;
using System.Collections.Generic;
using System.Text;
using PiBorgSharp;

namespace PiBorgSharp.Diabolo
{
    class Diabolo_class
    {
        // based on original source written by Arron Churchill (I think): https://www.piborg.org/blog/piborg-arron
        public static readonly bool THROTTLE_CODE               = false;

        public static readonly int I2C_SLAVE                    = 0x0703;
        public static readonly byte PWM_MAX                     = 255;
        public static readonly byte I2C_MAX_LEN                 = 4;

        public static readonly byte I2C_ID_DIABOLO              = 0x37;

        public static readonly byte COMMAND_SET_A_FWD           = 0x03;
        public static readonly byte COMMAND_SET_A_REV           = 0x04;
        public static readonly byte COMMAND_GET_A               = 0x05;
        public static readonly byte COMMAND_SET_B_FWD           = 0x06;
        public static readonly byte COMMAND_SET_B_REV           = 0x07;
        public static readonly byte COMMAND_GET_B               = 0x08;
        public static readonly byte COMMAND_ALL_OFF             = 0x09;
        public static readonly byte COMMAND_RESET_EPO           = 0x0A;
        public static readonly byte COMMAND_GET_EPO             = 0x0B;
        public static readonly byte COMMAND_SET_EPO_IGNORE      = 0x0C;
        public static readonly byte COMMAND_GET_EPO_IGNORE      = 0x0D;
        public static readonly byte COMMAND_SET_ALL_FWD         = 0x0F;
        public static readonly byte COMMAND_SET_ALL_REV         = 0x10;
        public static readonly byte COMMAND_SET_FAILSAFE        = 0x11;
        public static readonly byte COMMAND_GET_FAILSAFE        = 0x12;
        public static readonly byte COMMAND_SET_ENC_MODE        = 0x13;
        public static readonly byte COMMAND_GET_ENC_MODE        = 0x14;
        public static readonly byte COMMAND_MOVE_A_FWD          = 0x15;
        public static readonly byte COMMAND_MOVE_A_REV          = 0x16;
        public static readonly byte COMMAND_MOVE_B_FWD          = 0x17;
        public static readonly byte COMMAND_MOVE_B_REV          = 0x18;
        public static readonly byte COMMAND_MOVE_ALL_FWD        = 0x19;
        public static readonly byte COMMAND_MOVE_ALL_REV        = 0x1A;
        public static readonly byte COMMAND_GET_ENC_MOVING      = 0x1B;
        public static readonly byte COMMAND_SET_ENC_SPEED       = 0x1C;
        public static readonly byte COMMAND_GET_ENC_SPEED       = 0x1D;
        public static readonly byte COMMAND_SET_ENABLED         = 0x1E;
        public static readonly byte COMMAND_GET_ENABLED         = 0x1F;
        public static readonly byte COMMAND_GET_ID              = 0x99;
        public static readonly byte COMMAND_SET_I2C_ADD         = 0xAA;

        public static readonly byte COMMAND_VALUE_FWD           = 0x01;
        public static readonly byte COMMAND_VALUE_REV           = 0x02;

        public static readonly byte COMMAND_VALUE_ON            = 0x01;
        public static readonly byte COMMAND_VALUE_OFF           = 0x00;

        private int _bus = 0x01;
        private int _DiaboloAddress = 0x00;
        private ILogger _log = null;

        /// <summary>
        /// Scans the I2C bus for the Diabolo board.  It will scan bus 1 by default, and return the port number when it gets a Diabolo response to the board ID request.
        /// </summary>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Port number found for Diabolo; -1 if no board found</returns>
        public static int ScanForDiabolo(int busNumber = 1, ILogger log = null)
        {
            int tempReturn = -1;

            if (log != null)
            {
                log.WriteLog("Starting scan for Diabolo_class board...");
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
                            if (response[1] == I2C_ID_DIABOLO)
                            {
                                tempReturn = port;
                                if (log != null)
                                {
                                    log.WriteLog("FOUND Diabolo_class board on port: " + port.ToString("X2"));
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
        /// Sets a new address for the Diabolo.  CAUTION: this setting is persistent and will continue after powering down your Diabolo.
        /// </summary>
        /// <param name="newAddress">The new port address for the Diabolo</param>
        /// <param name="oldAddress">Optional - if specified the method will use this address as opposed to scanning for the Diabolo</param>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        /// <returns>The new port number for the Diabolo</returns>
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
                _oldAddress = Diabolo_class.ScanForDiabolo(Convert.ToInt32(busNumber), logger);
                if (_oldAddress < 0)
                {
                    throw new Exception("Diabolo board not found.");
                }
            }
            else
            {
                _oldAddress = Convert.ToInt32(oldAddress);
            }

            if (logger != null)
            {
                logger.WriteLog("Attempting change of Diabolo address from " + _oldAddress.ToString("X2") + " to " + newAddress.ToString("X2"), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                bus.WriteBytes(_oldAddress, new byte[] { COMMAND_SET_I2C_ADD, newAddress });

                System.Threading.Thread.Sleep(200);         // let the I2C bus catch up

                int tempCheck = Diabolo_class.ScanForDiabolo(Convert.ToInt32(busNumber), logger);

                if (tempCheck == newAddress)
                {
                    logger.WriteLog("CHANGED BOARD ADDRESS FROM " + _oldAddress.ToString("X2") + " TO " + newAddress.ToString("X2"), ILogger.Priority.Critical);
                    logger.WriteLog("This change will be persistent even after a reboot; keep track of it.", ILogger.Priority.Information);
                }
                else
                {
                    logger.WriteLog("**FAILED** to change Diabolo address.  Current address: " + tempCheck.ToString("X2"));
                }
                return tempCheck;
            }
        }

        /// <summary>
        /// Main Diabolo class
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <param name="tryOtherBus">CURRENTLY NO EFFECT</param>
        public Diabolo_class(ILogger log = null, bool tryOtherBus = false)
        {
            if (log != null)
            {
                this._log = log;
                log.WriteLog("THROTTLE_CODE: " + THROTTLE_CODE.ToString(), ILogger.Priority.Information);
                log.WriteLog("Instantiating Diabolo_class...", ILogger.Priority.Information);
            }

            _DiaboloAddress = Diabolo_class.ScanForDiabolo(1, log);

            if (log != null)
            {
                log.WriteLog("Loading Diabolo on bus " + _bus.ToString("X2") + ", address " + _DiaboloAddress.ToString("X2"), ILogger.Priority.Medium);
            }
        }

        public int CurrentAddress
        {
            get
            {
                return this._DiaboloAddress;
            }
        }

        /// <summary>
        /// Internal process used to determine if the board is initialized or not
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <param name="throwException">True - throw a new exception; false - suppress exception</param>
        /// <returns></returns>
        private bool _CheckInit(ILogger log = null, bool throwException = false)
        {
            bool tempReturn = false;

            if (this._DiaboloAddress == 0x00)
            {
                if (log != null)
                {
                    log.WriteLog("ThunderBorg_class not instantiated...", ILogger.Priority.Critical);
                }

                if (throwException)
                {
                    throw new InvalidOperationException("ThunderBorg_class not instantiated.");
                }
                else
                {
                    tempReturn = false;
                }
            }
            else
            {
                tempReturn = true;
            }

            return tempReturn;
        }

        /// <summary>
        /// Helper routine to output the contents of a byte array as a hexadecimal string
        /// </summary>
        /// <param name="incoming">Byte array to parse into a string</param>
        /// <returns>String representing hexadecimal bytes in array</returns>
        public string BytesToString(byte[] incoming)
        {
            string tempReturn = string.Empty;

            for (int i = 0; i < incoming.Length; i++)
            {
                tempReturn += incoming[i].ToString("X2") + " ";
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets the motor power for the A motors.  Range of options is from -255 < n < 255.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="power">Power setting: -255 < n < 255; if outside this range, it will use the maximum</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetMotorA(int power, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if ((power > 255) || (power < -255))
            {
                if (log != null)
                {
                    log.WriteLog("Power level out of range -255 <= power <= 255; rejecting command...");
                }
                throw new IndexOutOfRangeException("Invalid power setting to motor; range outside of -255 <= power <= 255.");
            }

            if (log != null)
            {
                log.WriteLog("Setting A motors to: " + power.ToString("X2"));
            }

            byte parsedPower = 0;
            if (power > 0)
            {
                parsedPower = Convert.ToByte(power);
            }
            else
            {
                parsedPower = Convert.ToByte(-power);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (power > 0)
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_A_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_A_REV, parsedPower });
                }
            }
        }

        /// <summary>
        /// Sets the motor power for the B motors.  Range of options is from -255 < n < 255.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="power">Power setting: -255 < n < 255; if outside this range, it will use the maximum</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetMotorB(int power, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if ((power > 255) || (power < -255))
            {
                if (log != null)
                {
                    log.WriteLog("Power level out of range -255 <= power <= 255; rejecting command...");
                }
                throw new IndexOutOfRangeException("Invalid power setting to motor; range outside of -255 <= power <= 255.");
            }

            if (log != null)
            {
                log.WriteLog("Setting B motors to: " + power.ToString("X2"));
            }

            byte parsedPower = 0;
            if (power > 0)
            {
                parsedPower = Convert.ToByte(power);
            }
            else
            {
                parsedPower = Convert.ToByte(-power);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (power > 0)
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_B_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_B_REV, parsedPower });
                }
            }
        }

        /// <summary>
        /// Gets the current power setting of the A motors.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Power setting of motor: -255 < n < 255 where negative values indicate reverse</returns>
        public int GetMotorA(ILogger log = null)
        {
            int tempReturn = 0;

            if (!_CheckInit(log, true))
            {
                return 0;
            }

            if (log != null)
            {
                log.WriteLog("Getting power level for A motors...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_GET_A });
                byte[] response = bus.ReadBytes(_DiaboloAddress, I2C_MAX_LEN);
                if (response == null)
                {
                    if (log != null)
                    {
                        log.WriteLog("*** ERROR: no response from Diabolo board...");
                    }

                    throw new NullReferenceException("No parseable response from A motors on GetMotorA request.");
                }
                else if (response[0] != COMMAND_GET_A)
                {
                    if (log != null)
                    {
                        log.WriteLog("Didn't get an expected response from the A motors on GetMotorA request.");
                    }

                    throw new IndexOutOfRangeException("Unexpected response from A motors on GetMotorA request.");
                }
                else
                {
                    byte power_direction = response[1];
                    byte power_level = response[2];

                    if (log != null)
                    {
                        log.WriteLog("Raw response: " + BytesToString(response));
                    }

                    if (power_direction == COMMAND_VALUE_FWD)
                    {
                        tempReturn = power_level;
                    }
                    else if (power_direction == COMMAND_VALUE_REV)
                    {
                        tempReturn = -power_level;
                    }
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Gets the current power setting of the B motors.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Power setting of motor: -255 < n < 255 where negative values indicate reverse</returns>
        public int GetMotorB(ILogger log = null)
        {
            int tempReturn = 0;

            if (!_CheckInit(log, true))
            {
                return 0;
            }

            if (log != null)
            {
                log.WriteLog("Getting power level for B motors...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_GET_B });
                byte[] response = bus.ReadBytes(_DiaboloAddress, I2C_MAX_LEN);
                if (response == null)
                {
                    if (log != null)
                    {
                        log.WriteLog("*** ERROR: no response from Diabolo board...");
                    }

                    throw new NullReferenceException("No parseable response from B motors on GetMotorB request.");
                }
                else if (response[0] != COMMAND_GET_B)
                {
                    if (log != null)
                    {
                        log.WriteLog("Didn't get an expected response from the B motors on GetMotorB request.");
                    }

                    throw new IndexOutOfRangeException("Unexpected response from B motors on GetMotorB request.");
                }
                else
                {
                    byte power_direction = response[1];
                    byte power_level = response[2];

                    if (log != null)
                    {
                        log.WriteLog("Raw response: " + BytesToString(response));
                    }

                    if (power_direction == COMMAND_VALUE_FWD)
                    {
                        tempReturn = power_level;
                    }
                    else if (power_direction == COMMAND_VALUE_REV)
                    {
                        tempReturn = -power_level;
                    }
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets the motor power for the all motors.  Range of options is from -255 < n < 255.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="power">Power setting: -255 < n < 255; if outside this range, it will use the maximum</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetAllMotors(int power, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if ((power > 255) || (power < -255))
            {
                if (log != null)
                {
                    log.WriteLog("Power level out of range -255 <= power <= 255; rejecting command...");
                }
                throw new IndexOutOfRangeException("Invalid power setting to motors; range outside of -255 <= power <= 255.");
            }

            if (log != null)
            {
                log.WriteLog("Setting all motors to: " + power.ToString("X2"));
            }

            byte parsedPower = 0;
            if (power > 0)
            {
                parsedPower = Convert.ToByte(power);
            }
            else
            {
                parsedPower = Convert.ToByte(-power);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                if (power > 0)
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_ALL_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_ALL_REV, parsedPower });
                }
            }
        }

        /// <summary>
        /// Command to set all motors to stop.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void AllStop(ILogger log = null)
        {
            if (!_CheckInit(log, true))
            {
                return;
            }

            if (log != null)
            {
                log.WriteLog("Calling all stop.");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_ALL_OFF });
            }
        }

        /// <summary>
        /// Sets the communications failsafe state.
        /// The failsafe will turn the motors off unless the Diabolo receives a command from the computer at least once every 1/4 seconds.  This is used to turn the Diabolo off
        /// if the computer stops sending communications to the Diabolo, such as in a power outage situation.
        /// </summary>
        /// <param name="setting">True - failsafe engaged; false - failsafe disengaged</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetFailsafe(bool setting, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            bool currentState = this.GetFailsafe(log);

            if (currentState == setting)
            {
                if (log != null)
                {
                    log.WriteLog("Called setting for failsafe: " + setting.ToString() + " but it already was...");
                }
                return;
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_SET_FAILSAFE, Convert.ToByte(!currentState) });
                if (log != null)
                {
                    log.WriteLog("Set failsafe to " + (!currentState).ToString());
                }
            }
        }

        /// <summary>
        /// Polls the current failsafe setting
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Failsafe state: true - failsafe on; false - failsafe off</returns>
        public bool GetFailsafe(ILogger log = null)
        {
            if (!_CheckInit())
            {
                throw new NullReferenceException("Diabolo_class not instantiated.");
            }

            bool tempReturn = false;

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_DiaboloAddress, new byte[] { COMMAND_GET_FAILSAFE });
                byte[] response = bus.ReadBytes(_DiaboloAddress, I2C_MAX_LEN);
                if (log != null)
                {
                    log.WriteLog("Got response on failsafe: " + BytesToString(response));
                }

                if (response[0] == COMMAND_GET_FAILSAFE)
                {
                    tempReturn = Convert.ToBoolean(response[1]);
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Reset the EPO latch status.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void ResetEPO(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current EPO latch setting.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - latch has been tripped and movement is disabled unless overriden; false - latch is not tripped</returns>
        public bool GetEPO(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the system to ignore or use the EPO latch; set to false if you have an EPO latch, set to true if not.
        /// </summary>
        /// <param name="State">True - ignore EPO latch; false - respect EPO latch</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetEPOIgnore(bool State, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the system setting for the EPO latch override.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - system is ignoring EPO latch; false - system is respecting EPO latch</returns>
        public bool GetEPOIgnore(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the encoder movement mode.  If this mode is enabled, the EncoderMoveMotor commands are available to move fixed distances
        /// In non-encoder move (disabled) the SetMotor commands are available to set drive speeds
        /// NOTE: the encoder movement mode requires that the encoder feedback is attached to an encoder signal; see www.piborg.org/picoborgrev for wiring instructions
        /// NOTE: encoder movement mode is DISABLED at power on
        /// </summary>
        /// <param name="State">True - set encoder mode; false - disable encoder mode</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetEncoderMoveMode(bool State, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the encoder movement state of the Diabolo; see SetEncodeMoveMode for details on encoder movement mode.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - Diabolo is in encoder mode; false - Diabolo is in movement mode</returns>
        public bool GetEncoderMoveMode(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the A motors until the specified encoder counts have been reached.  Range of options is -32,767 < n < 32,767.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="StepCount">Number of encoder counts to step</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void EncoderMoveMotorA(int StepCount, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves the B motors until the specified encoder counts have been reached.  Range of options is -32,767 < n < 32,767.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="StepCount">Number of encoder counts to step</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void EncoderMoveMotorB(int StepCount, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Moves all motors until the specified encoder counts have been reached.  Range of options is -32,767 < n < 32,767.  Negative numbers indicate reverse.
        /// </summary>
        /// <param name="StepCount">Number of encoder counts to step</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void EncoderMoveAllMotors(int StepCount, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks the current state of the encoder motion.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - encoder movement detected; false - no encoder movement detected</returns>
        public bool CheckEncoderMovement(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Waits until all encoder movement has completed.  If a timeout is provided the function will return false if motors are still in motion at the end of the timeout.
        /// </summary>
        /// <param name="TimeOut">Number of milliseconds to wait before returning false if motors are still moving</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void WaitWhileEncoderMoving(int TimeOut = -1, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the power level for the motors for encoder-based movement.  Range of options is 0 < n < 255.
        /// </summary>
        /// <param name="Power">Power level of motors during encoder movement</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetEncoderSpeed(byte Power, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the power level for the motors for encoder-based movement.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Power level of motors</returns>
        public byte GetEncoderSpeed(ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets whether or not the Diabolo is powering the motor drive pins.  If true all motor pins are either low, high or PWNed (powered); false indicates all motor pins are tri-stated (unpowered.)
        /// </summary>
        /// <param name="State">True - all motor pins are either low, high, or PWMed; false - all motor pins are tri-stated</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetPowerEnabled(bool State, ILogger log = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the state of Diabolo providing power to the motors.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>True - all motors are either low, high or PWMed; false - motors are tri-stated</returns>
        public bool GetPowerEnabled(ILogger log = null)
        {
            throw new NotImplementedException();
        }
    }
}
