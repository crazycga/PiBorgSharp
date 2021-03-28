using System;

namespace PiBorgSharp.ThunderBorg
{
    public class ThunderBorg_class
    {
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
                                    log.WriteLog("FOUND ThunderBorg_class board on port: " + port.ToString("X2"));
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

        public ThunderBorg_class(ILogger log = null, bool tryOtherBus = false)
        {
            if (log != null)
            {
                this._log = log;
                log.WriteLog("THROTTLE_CODE: " + THROTTLE_CODE.ToString());
                log.WriteLog();
                log.WriteLog("Finding ThunderBorg_class...");
                _TBorgAddress = ThunderBorg_class.ScanForThunderBorg(1, log);
            }
            else
            {
                _TBorgAddress = ThunderBorg_class.ScanForThunderBorg();
            }

            if (log != null)
            {
                log.WriteLog("Loding ThunderBorg_class on bus " + _bus.ToString("X2") + ", address " + _TBorgAddress.ToString("X2"));
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
                        log.WriteLog("*** ERROR: no response from ThunderBorg_class...");
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
                        log.WriteLog("*** ERROR: no response from ThunderBorg_class...");
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

        public bool GetFailsafe(ILogger log = null)
        {
            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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

        public bool GetLEDBatteryMonitor(ILogger log = null)
        {
            bool tempReturn = false;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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

        public decimal GetBatteryVoltage(ILogger log = null)
        {
            int tempIntReturn = 0;
            decimal tempReturn = 0.00M;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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

        public byte GetBoardID (ILogger log = null)
        {
            byte tempReturn = 0x00;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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

        public decimal[] GetBatteryMonitoringLimits(ILogger log = null)
        {
            decimal tempMin = 0.00M;
            decimal tempMax = 0.00M;

            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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
        /// <param name="log">Optional logging output routine</param>
        public void SetBatteryMonitoringLimits(decimal minimum, decimal maximum, ILogger log = null)
        {
            // my original values were 6.98 / 35.02; I don't know what the defaults are
            if (!_CheckInit())
            {
                throw new NullReferenceException("ThunderBorg_class not initiated.");
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

        private bool _CheckInit(ILogger log = null, bool throwException = false)
        {
            bool tempReturn = false;

            if (this._TBorgAddress == 0x00)
            {
                if (log != null)
                {
                    log.WriteLog("ThunderBorg_class not initiated...");
                }

                if (throwException)
                {
                    throw new InvalidOperationException("ThunderBorg_class not initialized.");
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
