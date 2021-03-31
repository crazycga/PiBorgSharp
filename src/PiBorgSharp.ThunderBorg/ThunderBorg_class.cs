using System;
using PiBorgSharp;

namespace PiBorgSharp.ThunderBorg
{
    public class ThunderBorg_class
    {
        // test build
        // based on original source written by Arron Churchill (I think): https://www.piborg.org/blog/piborg-arron
        public static readonly bool THROTTLE_CODE               = false;     // added to introduce code throttling; maybe the code is too fast?

        public static readonly ushort I2C_SLAVE                 = 0x0703;
        public static readonly byte PWN_MAX                     = 0xFF;
        public static readonly byte I2C_MAX_LEN                 = 0x06;

        public static readonly decimal VOLTAGE_PIN_MAX          = 36.3M;
        public static readonly decimal VOLTAGE_PIN_CORRECTION   = 0.0M;
        public static readonly decimal BATTERY_MIN_DEFAULT      = 7.0M;
        public static readonly decimal BATTERY_MAX_DEFAULT      = 35.0M;

        public static readonly byte I2C_ID_THUNDERBORG          = 0x15;

        public static readonly byte COMMAND_SET_LED1            = 0x01;
        public static readonly byte COMMAND_GET_LED1            = 0x02;
        public static readonly byte COMMAND_SET_LED2            = 0x03;
        public static readonly byte COMMAND_GET_LED2            = 0x04;
        public static readonly byte COMMAND_SET_LEDS            = 0x05;
        public static readonly byte COMMAND_SET_LED_BATT_MON    = 0x06;
        public static readonly byte COMMAND_GET_LED_BATT_MON    = 0x07;
        public static readonly byte COMMAND_SET_A_FWD           = 0x08;
        public static readonly byte COMMAND_SET_A_REV           = 0x09;
        public static readonly byte COMMAND_GET_A               = 0x0A;
        public static readonly byte COMMAND_SET_B_FWD           = 0x0B;
        public static readonly byte COMMAND_SET_B_REV           = 0x0C;
        public static readonly byte COMMAND_GET_B               = 0x0D;
        public static readonly byte COMMAND_ALL_OFF             = 0x0E;
        public static readonly byte COMMAND_GET_DRIVE_A_FAULT   = 0x0F;
        public static readonly byte COMMAND_GET_DRIVE_B_FAULT   = 0x10;
        public static readonly byte COMMAND_SET_ALL_FWD         = 0x11;
        public static readonly byte COMMAND_SET_ALL_REV         = 0x12;
        public static readonly byte COMMAND_SET_FAILSAFE        = 0x13;
        public static readonly byte COMMAND_GET_FAILSAFE        = 0x14;
        public static readonly byte COMMAND_GET_BATT_VOLT       = 0x15;
        public static readonly byte COMMAND_SET_BATT_LIMITS     = 0x16;
        public static readonly byte COMMAND_GET_BATT_LIMITS     = 0x17;
        public static readonly byte COMMAND_WRITE_EXTERNAL_LED  = 0x18;
        public static readonly byte COMMAND_GET_ID              = 0x99;
        public static readonly byte COMMAND_SET_I2C_ADD         = 0xAA;

        public static readonly byte COMMAND_VALUE_FWD           = 0x01;
        public static readonly byte COMMAND_VALUE_REV           = 0x02;

        public static readonly byte COMMAND_VALUE_ON            = 0x01;
        public static readonly byte COMMAND_VALUE_OFF           = 0x00;

        public static readonly decimal SECONDS_PER_METER        = 0.85M;    // calculated by PiBorg team; confirmed by crazycga
        public static readonly decimal SECONDS_PER_SPIN         = 1.10M;    // calulcated by PiBorg team; unconfirmed, likely to be affected by batteries more than straight motion

        public static readonly ushort COMMAND_ANALOG_MAX        = 0x3FF;

        private int _bus = 0x01;
        private int _TBorgAddress = 0x00;
        private ILogger _log = null;

        /// <summary>
        /// Scans the I2C bus for the ThunderBorg board.  It will scan bus 1 by default, and return the port number when it gets a ThunderBorg response to the board ID request.
        /// </summary>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Port number found for ThunderBorg; -1 if no board found</returns>
        public static int ScanForThunderBorg(int busNumber = 1, ILogger log = null)
        {
            int tempReturn = -1;

            if (log != null)
            {
                log.WriteLog("Starting scan for ThunderBorg_class board...");
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
                            if (response[1] == I2C_ID_THUNDERBORG)
                            {
                                tempReturn = port;
                                if (log != null)
                                {
                                    log.WriteLog("FOUND ThunderBorg board on port: " + port.ToString("X2"));
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

            if (log!=null)
            {
                log.WriteLog("Finished port scan...");
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets a new address for the ThunderBorg.  CAUTION: this setting is persistent and will continue after powering down your ThunderBorg.
        /// </summary>
        /// <param name="newAddress">The new port address for the ThunderBorg</param>
        /// <param name="oldAddress">Optional - if specified the method will use this address as opposed to scanning for the ThunderBorg</param>
        /// <param name="busNumber">Default: 1; this parameter will map to /dev/i2c-n where n is the bus number</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        /// <returns>The new port number for the ThunderBorg</returns>
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
                _oldAddress = ThunderBorg_class.ScanForThunderBorg(Convert.ToInt32(busNumber), logger);
                if (_oldAddress < 0) 
                {
                    throw new Exception("ThunderBorg board not found.");
                }
            }
            else
            {
                _oldAddress = Convert.ToInt32(oldAddress);
            }

            if (logger != null)
            {
                logger.WriteLog("Attempting change of ThunderBorg address from " + _oldAddress.ToString("X2") + " to " + newAddress.ToString("X2"), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + busNumber.ToString()))
            {
                bus.WriteBytes(_oldAddress, new byte[] { COMMAND_SET_I2C_ADD, newAddress });

                System.Threading.Thread.Sleep(200);         // let the I2C bus catch up

                int tempCheck = ThunderBorg_class.ScanForThunderBorg(Convert.ToInt32(busNumber), logger);

                if (tempCheck == newAddress)
                {
                    logger.WriteLog("CHANGED BOARD ADDRESS FROM " + _oldAddress.ToString("X2") + " TO " + newAddress.ToString("X2"), ILogger.Priority.Critical);
                    logger.WriteLog("This change will be persistent even after a reboot; keep track of it.", ILogger.Priority.Information);
                }
                else
                {
                    logger.WriteLog("**FAILED** to change ThunderBorg address.  Current address: " + tempCheck.ToString("X2"));
                }
                return tempCheck;
            }
        }

        /// <summary>
        /// Main ThunderBorg class
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <param name="tryOtherBus">CURRENTLY NO EFFECT</param>
        public ThunderBorg_class(ILogger log = null, bool tryOtherBus = false)
        {
            if (log != null)
            {
                this._log = log;
                log.WriteLog("THROTTLE_CODE: " + THROTTLE_CODE.ToString(), ILogger.Priority.Information);
                log.WriteLog("Instantiating ThunderBorg_class...", ILogger.Priority.Information);
            }

            _TBorgAddress = ThunderBorg_class.ScanForThunderBorg(1, log);

            if (log != null)
            {
                log.WriteLog("Loading ThunderBorg on bus " + _bus.ToString("X2") + ", address " + _TBorgAddress.ToString("X2"));
            }
        }

        public int CurrentAddress
        {
            get
            {
                return this._TBorgAddress;
            }
        }

        public decimal VoltagePinMax
        {
            get
            {
                return ThunderBorg_class.VOLTAGE_PIN_MAX;
            }
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

            if ((power > 255) || (power<-255))
            {
                if (log!=null)
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
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_A_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_A_REV, parsedPower });
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
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_B_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_B_REV, parsedPower });
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

            if (log!=null)
            {
                log.WriteLog("Getting power level for A motors...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_A });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (response == null)
                {
                    if (log != null)
                    {
                        log.WriteLog("*** ERROR: no response from ThunderBorg board...");
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
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_B });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (response == null)
                {
                    if (log != null)
                    {
                        log.WriteLog("*** ERROR: no response from ThunderBorg board...");
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
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_ALL_FWD, parsedPower });
                }
                else
                {
                    bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_ALL_REV, parsedPower });
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

            if (log!=null)
            {
                log.WriteLog("Calling all stop.");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_ALL_OFF });
            }
        }

        /// <summary>
        /// Polls the A motors for fault.  Response bytes after constant indicate an error as been detected.  Faults may indicate power problems, such as under-voltage (not enough power), and may be cleared
        /// by setting a lower drive power.
        /// Faults will self-clear, they do not need to be reset, however some faults require both motors to be moving at less than 100% to clear.  NOTE: the fault state may indicate a fault att power up; this is normal
        /// and should clear when the motors are run.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>A byte array of the exact response from the board to this command - max length = I2C_MAX_LEN (default 6)</returns>
        public byte[] GetDriveFaultA(ILogger log = null)
        {
            if (!_CheckInit(log, true))
            {
                return null;
            }

            byte[] tempReturn;

            if (log != null) 
            {
                log.WriteLog("Getting drive fault A status...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_DRIVE_A_FAULT });
                tempReturn = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);

                if (log != null) 
                {
                    log.WriteLog("Raw Response: " + BytesToString(tempReturn));
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Polls the B motors for fault.  Response bytes after constant indicate an error as been detected.  Faults may indicate power problems, such as under-voltage (not enough power), and may be cleared
        /// by setting a lower drive power.
        /// Faults will self-clear, they do not need to be reset, however some faults require both motors to be moving at less than 100% to clear.  NOTE: the fault state may indicate a fault att power up; this is normal
        /// and should clear when the motors are run.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>A byte array of the exact response from the board to this command - max length = I2C_MAX_LEN (default 6)</returns>
        public byte[] GetDriveFaultB(ILogger log = null)
        {
            if (!_CheckInit(log, true))
            {
                return null;
            }

            byte[] tempReturn;

            if (log != null)
            {
                log.WriteLog("Getting drive fault B status...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_DRIVE_B_FAULT });
                tempReturn = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);

                if (log != null) 
                {
                    log.WriteLog("Raw Response: " + BytesToString(tempReturn));
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets the communications failsafe state.
        /// The failsafe will turn the motors off unless the ThunderBorg receives a command from the computer at least once every 1/4 seconds.  This is used to turn the ThunderBorg off
        /// if the computer stops sending communications to the ThunderBorg, such as in a power outage situation.  (NOTE: this makes the LED(s) blink red, and disables the ability to change
        /// the LED colors.)
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
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_FAILSAFE, Convert.ToByte(!currentState) });
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
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            bool tempReturn = false;

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_FAILSAFE });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
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
        /// Polls the current color setting of LED1 (on the ThunderBorg board.)
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>A byte array of I2C_MAX_LEN (default 6) bytes containing the command, and three bytes of colors corresponding to red, green and blue</returns>
        public byte[] GetLED1(ILogger log = null)
        {
            byte[] tempReturn;

            if (!_CheckInit())
            {
                return null;
            }

            if (log != null)
            {
                log.WriteLog("Calling results for get LED 1...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_LED1 });

                tempReturn = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (tempReturn[0] == COMMAND_GET_LED1)
                {
                    if (log != null) 
                    {
                        log.WriteLog("Get LED1 response: " + this.BytesToString(tempReturn));
                    }

                    return tempReturn;
                }
                else
                {
                    if (log != null)
                    {
                        log.WriteLog("Got a nonsense reponse from COMMAND_GET_LED1..." + this.BytesToString(tempReturn));
                    }

                    throw new InvalidProgramException("Nonsense response during COMMAND_GET_LED1.");
                }
            }
        }

        /// <summary>
        /// Polls the current color setting of LED2 (on the ThunderBorg lid.)
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>A byte array of I2C_MAX_LEN (default 6) bytes containing the command, and three bytes of colors corresponding to red, green and blue</returns>
        public byte[] GetLED2(ILogger log = null)
        {
            byte[] tempReturn;

            if (!_CheckInit())
            {
                return null;
            }

            if (log != null)
            {
                log.WriteLog("Calling results for get LED 2...");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_LED2 });

                tempReturn = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (tempReturn[0] == COMMAND_GET_LED2)
                {
                    if (log != null)
                    {
                        log.WriteLog("Get LED2 response: " + this.BytesToString(tempReturn));
                    }

                    return tempReturn;
                }
                else
                {
                    if (log != null)
                    {
                        log.WriteLog("Got a nonsense reponse from COMMAND_GET_LED2..." + this.BytesToString(tempReturn));
                    }

                    throw new InvalidProgramException("Nonsense response during COMMAND_GET_LED2.");
                }
            }
        }

        /// <summary>
        /// Polls the current setting of the LED system to determine if it is showing the battery monitor
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Current battery monitor setting: true - monitoring battery; false - not monitoring battery</returns>
        public bool GetLEDBatteryMonitor(ILogger log = null)
        {
            bool tempReturn = false;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_LED_BATT_MON });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (log != null)
                {
                    log.WriteLog("Response from COMMAND_GET_LED_BATT_MON: " + this.BytesToString(response));
                }
                if (response[0] == COMMAND_GET_LED_BATT_MON)
                {
                    if (response[1] == COMMAND_VALUE_ON)
                    {
                        tempReturn = true;
                        return tempReturn;
                    }
                    else if (response[1] == COMMAND_VALUE_OFF)
                    {
                        tempReturn = false;
                        return tempReturn;
                    }
                }
                else
                {
                    throw new InvalidProgramException("Nonsense response during COMMAND_GET_LED_BATT_MON.");
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Sets the LED(s) to show a color corresponding to the current voltage available from the battery.  NOTE: this will disabled manually setting LED colors.
        /// </summary>
        /// <param name="setting">True - monitor battery; false - do not monitor battery</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetLEDBatteryMonitor(bool setting, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if (log != null) 
            {
                log.WriteLog("Setting LED battery monitor to: " + setting.ToString(), ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_LED_BATT_MON, Convert.ToByte(setting) });
            }
        }

        /// <summary>
        /// Sets the LED1 (on the ThunderBorg board) to the color specified by the red, green and blue values.
        /// NOTE: this will not work if either battery monitoring is on, or the failsafe mode is on.
        /// </summary>
        /// <param name="red">Red value: 0 <= n <= 255</param>
        /// <param name="green">Green value: 0 <= n <= 255</param>
        /// <param name="blue">Blue value: 0 <= n <= 255</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetLED1(byte red, byte green, byte blue, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if (log != null)
            {
                log.WriteLog("Setting LED 1...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_LED1, red, green, blue });
            }
        }

        /// <summary>
        /// Sets the LED2 (on the ThunderBorg lid) to the color specified by the red, green and blue values.
        /// NOTE: this will not work if either battery monitoring is on, or the failsafe mode is on.
        /// </summary>
        /// <param name="red">Red value: 0 <= n <= 255</param>
        /// <param name="green">Green value: 0 <= n <= 255</param>
        /// <param name="blue">Blue value: 0 <= n <= 255</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetLED2(byte red, byte green, byte blue, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if (log != null)
            {
                log.WriteLog("Setting LED 2...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_LED2, red, green, blue });
            }
        }

        /// <summary>
        /// Sets all LEDs (on the ThunderBorg board AND ThunderBorg lid) to the color specified by the red, green and blue values.
        /// NOTE: this will not work if either battery monitoring is on, or the failsafe mode is on.
        /// </summary>
        /// <param name="red">Red value: 0 <= n <= 255</param>
        /// <param name="green">Green value: 0 <= n <= 255</param>
        /// <param name="blue">Blue value: 0 <= n <= 255</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetLEDs(byte red, byte green, byte blue, ILogger log = null)
        {
            if (!_CheckInit())
            {
                return;
            }

            if (log != null)
            {
                log.WriteLog("Setting LEDs to " + red.ToString() + ", " + green.ToString() + ", " + blue.ToString() + "...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_LEDS, red, green, blue });

                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
            }
        }

        /// <summary>
        /// Polls the ThunderBorg to determine current voltage supplied to the ThunderBorg
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Current voltage recorded at the ThunderBorg</returns>
        public decimal GetBatteryVoltage(ILogger log = null)
        {
            int tempIntReturn = 0;
            decimal tempReturn = 0.00M;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            if (log != null)
            {
                log.WriteLog("Getting battery voltage...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_BATT_VOLT });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);

                if (log != null)
                {
                    log.WriteLog("GET_BATT_VOLT response: " + this.BytesToString(response), ILogger.Priority.Information);
                }

                if (response[0] == COMMAND_GET_BATT_VOLT)
                {
                    tempIntReturn = ((int)response[1] << 8) + (int)response[2];
                }
            }

            tempReturn = Convert.ToDecimal(tempIntReturn) / Convert.ToDecimal(COMMAND_ANALOG_MAX);
            
            tempReturn *= VOLTAGE_PIN_MAX;
            tempReturn += VOLTAGE_PIN_CORRECTION;
            tempReturn = Math.Round(tempReturn, 2);

            return tempReturn;
        }

        /// <summary>
        /// Gets the board ID of the board
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Board ID (ThunderBorg is 0x15)</returns>
        public byte GetBoardID (ILogger log = null)
        {
            byte tempReturn = 0x00;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            if (log != null)
            {
                log.WriteLog("Checking board ID...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_ID });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (response[0] == COMMAND_GET_ID)
                {
                    tempReturn = response[1];
                }
            }

            return tempReturn;
        }

        /// <summary>
        /// Gets the current minimum and maximum voltage that the battery monitoring system uses.  See SetBatteryMonitoringLimits for more information.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        /// <returns>Array of two decimal values: the minimum and maximum voltages</returns>
        public decimal[] GetBatteryMonitoringLimits(ILogger log = null)
        {
            decimal tempMin = 0.00M;
            decimal tempMax = 0.00M;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_GET_BATT_LIMITS });
                byte[] response = bus.ReadBytes(_TBorgAddress, I2C_MAX_LEN);
                if (response[0] == COMMAND_GET_BATT_LIMITS)
                {
                    tempMin = response[1];
                    tempMax = response[2];

                    tempMin /= Convert.ToDecimal(0xFF);
                    tempMax /= Convert.ToDecimal(0xFF);

                    tempMin *= VOLTAGE_PIN_MAX;
                    tempMax *= VOLTAGE_PIN_MAX;

                    tempMin = Math.Round(tempMin, 2);
                    tempMax = Math.Round(tempMax, 2);

                    decimal[] tempReturn = { tempMin, tempMax };

                    if (log != null)
                    {
                        log.WriteLog("Voltages found - min: " + tempMin.ToString() + " max: " + tempMax.ToString(), ILogger.Priority.Information);
                    }

                    return tempReturn;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the battery monitoring limits used for setting the LED color.  The colors shown range from full red at minimum (or below), yellow at half, and green at maximum or higher.  These values are stored in the EEPROM and reloaded when the board is powered.
        /// </summary>
        /// <param name="minimum">Minimum voltage (0 V minimum)</param>
        /// <param name="maximum">Maximum voltage (36.3 V maximum)</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void SetBatteryMonitoringLimits(decimal minimum, decimal maximum, ILogger log = null)
        {
            // my original values were 6.98 / 35.02; I don't know what the defaults are
            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not instantiated.");
            }

            minimum /= Convert.ToDecimal(VOLTAGE_PIN_MAX);
            maximum /= Convert.ToDecimal(VOLTAGE_PIN_MAX);

            byte levelMin = Math.Max(Convert.ToByte(0x00), Math.Min(Convert.ToByte(0xFF), Convert.ToByte(minimum * 0xFF)));
            byte levelMax = Math.Max(Convert.ToByte(0x00), Math.Min(Convert.ToByte(0xFF), Convert.ToByte(maximum * 0xFF)));

            if (log != null)
            {
                log.WriteLog("Trying to set battery monitoring limits to: 0x" + levelMin.ToString("X2") + " V. min and 0x" + levelMax.ToString("X2") + " V. max...", ILogger.Priority.Information);
            }

            using (var bus = I2CBus.Open("/dev/i2c-" + this._bus.ToString()))
            {
                bus.WriteBytes(_TBorgAddress, new byte[] { COMMAND_SET_BATT_LIMITS, levelMin, levelMax });
                System.Threading.Thread.Sleep(200);     // note: this was recommended in the Python version for EEPROM writing
            }
        }

        /// <summary>
        /// Demonstrates LED color changes.  NOTE: this turns OFF battery monitoring.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void WaveLEDs(ILogger log = null)
        {
            bool batSetting = false;
            batSetting = this.GetLEDBatteryMonitor(log);
            byte red = 0;
            byte green = 0;
            byte blue = 0;

            while (batSetting)
            {
                log.WriteLog("Battery was monitoring; resetting to off.", ILogger.Priority.Information);
                this.SetLEDBatteryMonitor(false, log);
                batSetting = this.GetLEDBatteryMonitor(log);
            }

            System.Diagnostics.Stopwatch myStop = new System.Diagnostics.Stopwatch();
            myStop.Start();
            int timeOut = 0;
            log.WriteLog("This routine will run through the colors of the LEDs.  Press any key to stop.  Will stop automatically after a while...", ILogger.Priority.Medium);
            while ((!Console.KeyAvailable) && (timeOut < 10)) 
            {
                for (int hueCycle = 0; hueCycle <= 765; hueCycle++)
                {
                    switch (hueCycle)
                    {
                        case int hue when (hueCycle <= 255):
                            red = Convert.ToByte(255 - hue);
                            green = Convert.ToByte(hue);
                            blue = 0;
                            break;
                        case int hue when ((hueCycle <= 510) && (hueCycle > 255)):
                            red = 0;
                            green = Convert.ToByte(510 - hue);
                            blue = Convert.ToByte(hue - 255);
                            break;
                        case int hue when (hueCycle >= 510):
                            red = Convert.ToByte(hue - 510);
                            green = 0;
                            blue = Convert.ToByte(765 - hue);
                            break;
                        default:
                            red = 0;
                            green = 0;
                            blue = 0;
                            break;
                    }
                    this.SetLEDs(red, green, blue, log);
                    if (Console.KeyAvailable) break;
                    System.Threading.Thread.Sleep(10);
                }
                if (Console.KeyAvailable) break;
                timeOut++;
            }

            if (Console.KeyAvailable) Console.ReadKey(true);
            myStop.Stop();
            log.WriteLog("Ran LED tests for total timee of " + myStop.Elapsed.ToString(), ILogger.Priority.Medium);
        }

        /// <summary>
        /// Executes a test run on all wheels from slow to fast to slow again.  NOTE: ensure you have room for this test, or that the ThunderBorg is off the wheels.
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void TestSpeeds(ILogger log = null)
        {
            if (log != null)
            {
                log.WriteLog("Testing a slow to fast speed on both wheels.  Press any key to stop.", ILogger.Priority.Medium);
            }

            for(int i = 0; i < 256; i++)
            {
                this.SetAllMotors(i);
                System.Threading.Thread.Sleep(10);
                if (Console.KeyAvailable) break;
            }

            for (int i = 255; i > 0; i--)
            {
                this.SetAllMotors(i);
                System.Threading.Thread.Sleep(10);
                if (Console.KeyAvailable) break;
            }

            this.AllStop();
            if (Console.KeyAvailable) Console.ReadKey(true);
        }

        /// <summary>
        /// Executes a distance test as set out by the PiBorg company; used for benchmarking
        /// </summary>
        /// <param name="meters">Length in meters to travel (roughly estimated)</param>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void TestDistance(decimal meters, ILogger log = null)
        {
            decimal calculatedSpeed = SECONDS_PER_METER;
            decimal estimatedTime = meters * SECONDS_PER_METER;

            if (log != null)
            {
                log.WriteLog("Moving forward for " + estimatedTime.ToString() + " seconds to go " + meters.ToString() + " meter(s).");
            }

            this.SetAllMotors(255, log);
            System.Threading.Thread.Sleep(Convert.ToInt32(estimatedTime * 1000));

            this.AllStop();
        }

        /// <summary>
        /// Executes a spin test as set out by the PiBorg company; used for benchmarking
        /// </summary>
        /// <param name="log">Default: null; the ILogger interface used in this library</param>
        public void TestSpin(ILogger log = null)
        {
            decimal calculatedSpin = SECONDS_PER_SPIN;

            if (log != null)
            {
                log.WriteLog("Attemping 360 degree spin.");
            }

            this.SetMotorA(-255, log);
            this.SetMotorB(255, log);
            System.Threading.Thread.Sleep(Convert.ToInt32(calculatedSpin * 1000));

            this.AllStop();
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

            if (this._TBorgAddress == 0x00)
            {
                if (log != null)
                {
                    log.WriteLog("ThunderBorg_class not instantiated...");
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
        public string BytesToString (byte[] incoming)
        {
            string tempReturn = string.Empty;

            for (int i = 0; i < incoming.Length; i++)
            {
                tempReturn += incoming[i].ToString("X2") + " ";
            }

            return tempReturn;
        }
    }
}
