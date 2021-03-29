using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace PiBorgSharp.ThunderBorg
{
    /// <summary>
    /// This object contains all of the settings pertinent to the ThunderBorg environment
    /// </summary>
    public class ThunderBorgSettings_class : INotifyPropertyChanged
    {
        private int _Id = 0;
        private int _boardAddress = 0x00;
        private decimal _voltagePinMax = 0.00M;
        private decimal _batteryMonitorMin = 0.00M;
        private decimal _batteryMonitorMax = 0.00M;
        private byte _LED1_Red = 0x00;
        private byte _LED1_Green = 0x00;
        private byte _LED1_Blue = 0x00;
        private byte _LED2_Red = 0x00;
        private byte _LED2_Green = 0x00;
        private byte _LED2_Blue = 0x00;
        private bool _batteryMonitoringState = false;
        private bool _failSafeState = false;
        private bool _IsUsed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// ID field, intended to be used in a database environment
        /// </summary>
        public int Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this._Id = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Address of the ThunderBorg board
        /// </summary>
        public int BoardAddress
        {
            get
            {
                return this._boardAddress;
            }
            set
            {
                this._boardAddress = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Calculation value set by the constant VOLTAGE_PIN_MAX in the actual code
        /// </summary>
        public decimal VoltagePinMax
        {
            get
            {
                return this._voltagePinMax;
            }
            set
            {
                this._voltagePinMax = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The minimum voltage used by the LED battery monitor
        /// </summary>
        public decimal BatteryMonitorMin
        {
            get
            {
                return this._batteryMonitorMin;
            }
            set
            {
                this._batteryMonitorMin = value;
                this._IsUsed = true;
                RaisePropertyChanged();                   
            }
        }

        /// <summary>
        /// The maximum voltage used by the LED battery monitor
        /// </summary>
        public decimal BatteryMonitorMax
        {
            get
            {
                return this._batteryMonitorMax;
            }
            set
            {
                this._batteryMonitorMax = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED1 red setting
        /// </summary>
        public byte LED1_Red
        {
            get
            {
                return this._LED1_Red;
            }
            set
            {
                this._LED1_Red = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED1 green setting
        /// </summary>
        public byte LED1_Green
        {
            get
            {
                return this._LED1_Green;
            }
            set
            {
                this._LED1_Green = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED1 blue setting
        /// </summary>
        public byte LED1_Blue
        {
            get
            {
                return this._LED1_Blue;
            }
            set
            {
                this._LED1_Blue = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED2 red setting
        /// </summary>
        public byte LED2_Red
        {
            get
            {
                return this._LED2_Red;
            }
            set
            {
                this._LED2_Red = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED2 green setting
        /// </summary>
        public byte LED2_Green
        {
            get
            {
                return this._LED2_Green;
            }
            set
            {
                this._LED2_Green = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// LED2 blue setting
        /// </summary>
        public byte LED2_Blue
        {
            get
            {
                return this._LED2_Blue;
            }
            set
            {
                this._LED2_Blue = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The LED battery monitoring state; true - on / false - off
        /// </summary>
        public bool BatteryMonitoringState
        {
            get
            {
                return this._batteryMonitoringState;
            }
            set
            {
                this._batteryMonitoringState = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The failsafe state; true - on / false - off
        /// </summary>
        public bool FailSafeState
        {
            get
            {
                return this._failSafeState;
            }
            set
            {
                this._failSafeState = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// [Read only] Indicates whether the object has been updated since instantiation
        /// </summary>
        public bool IsUsed
        {
            get
            {
                return this._IsUsed;
            }
        }

        /// <summary>
        /// Enumerates the [ThunderBorg_class] board passed into it, and records the settings it finds.
        /// </summary>
        /// <param name="borg">The _intiialized_ ThunderBorg board to be enumerated</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        public void GetCurrentEnvironment(ThunderBorg_class borg, ILogger logger = null)
        {
            Stopwatch elapsedTime = new Stopwatch();
            elapsedTime.Start();

            bool log = false;
            if (logger != null) 
            {
                log = true;
            }

            if (log) logger.WriteLog("Starting board enumeration...");

            if (log) logger.WriteLog("\tIssuing all stop, first...");
            borg.AllStop();

            this.BoardAddress = borg.CurrentAddress;
            if (log) logger.WriteLog("\tCurrent board address: 0x" + this.BoardAddress.ToString("X2"));

            this.VoltagePinMax = borg.VoltagePinMax;
            if (log) logger.WriteLog("\tCurrent VOLTAGE_PIN_MAX: " + this.VoltagePinMax.ToString());

            decimal[] batteryMonitorResponse = borg.GetBatteryMonitoringLimits();
            this.BatteryMonitorMin = batteryMonitorResponse[0];
            this.BatteryMonitorMax = batteryMonitorResponse[1];
            if (log) logger.WriteLog("\tBattery monitoring min: " + this.BatteryMonitorMin.ToString() + " max: " + this.BatteryMonitorMax.ToString());

            byte[] LED1Settings = borg.GetLED1();
            this.LED1_Red = LED1Settings[1];
            this.LED1_Green = LED1Settings[2];
            this.LED1_Blue = LED1Settings[3];

            byte[] LED2Settings = borg.GetLED2();
            this.LED2_Red = LED2Settings[1];
            this.LED2_Green = LED2Settings[2];
            this.LED2_Blue = LED2Settings[3];

            if (log) logger.WriteLog("\tLED1: R: 0x" + this.LED1_Red.ToString("X2") + " G: 0x" + this.LED1_Green.ToString("X2") + " B: 0x" + this.LED1_Blue.ToString("X2"));
            if (log) logger.WriteLog("\tLED2: R: 0x" + this.LED2_Red.ToString("X2") + " G: 0x" + this.LED2_Green.ToString("X2") + " B: 0x" + this.LED2_Blue.ToString("X2"));

            this.BatteryMonitoringState = borg.GetLEDBatteryMonitor();
            if (log) logger.WriteLog("\tCurrent battery LED monitor state: " + this.BatteryMonitoringState.ToString());

            this.FailSafeState = borg.GetFailsafe();
            if (log) logger.WriteLog("\tCurrent failsafe state: " + this.FailSafeState.ToString());

            if (log) logger.WriteLog("\tEnumeration complete...");

            elapsedTime.Stop();
            if (log) logger.WriteLog("\tEnumeration took " + elapsedTime.ElapsedMilliseconds.ToString() + " milliseconds...");
        }

        /// <summary>
        /// Sets the _initialized_ [ThunderBorg_class] board to the settings contained in this object.  **NOTE** if the board address differs from this object's board address, this method will not run.
        /// </summary>
        /// <param name="borg">The _intiialized_ ThunderBorg board to be enumerated</param>
        /// <param name="logger">Default: null; the ILogger interface used in this library</param>
        public void SetCurrentEnvironment(ThunderBorg_class borg, ILogger logger = null)
        {
            Stopwatch elapsedTime = new Stopwatch();
            elapsedTime.Start();

            bool log = false;
            if (logger != null) 
            {
                log = true;
            }

            if (!this.IsUsed)
            {
                if (log) logger.WriteLog("Not writing these values; the settings haven't been used.");
                throw new ArgumentException("ThunderBorgSettings class not used; no values contained therein.", "ThunderBorgSettings");
            }

            if (log) logger.WriteLog("Setting board values to current enumeration...");

            if (log) logger.WriteLog("Issuing all stop, first...");
            borg.AllStop();

            if (borg.CurrentAddress != this.BoardAddress)
            {
                if (log) logger.WriteLog("Possibly invalid settings; this board's address is 0x" + borg.CurrentAddress.ToString("X2") + " but the setting says 0x" + this.BoardAddress.ToString("X2"));
                throw new ArgumentException("ThunderBorg_class address doesn't match settings address.", "ThunderBorgSettings.BoardAddress");
            }
            else
            {
                if (log) logger.WriteLog("Address is confirmed at 0x" + borg.CurrentAddress);
            }

            if (log) logger.WriteLog("VOLTAGE_PIN_MAX is a constant set by code; cannot change; current value: " + this.VoltagePinMax.ToString());

            borg.SetBatteryMonitoringLimits(this.BatteryMonitorMin, this.BatteryMonitorMax);
            if (log) logger.WriteLog("\tSet battery monitoring min: " + this.BatteryMonitorMin.ToString() + " max: " + this.BatteryMonitorMax.ToString());

            borg.SetLED1(this.LED1_Red, this.LED1_Green, this.LED1_Blue);
            borg.SetLED2(this.LED2_Red, this.LED2_Green, this.LED2_Blue);

            if (log) logger.WriteLog("\tSet LED1: R: 0x" + this.LED1_Red.ToString("X2") + " G: 0x" + this.LED1_Green.ToString("X2") + " B: 0x" + this.LED1_Blue.ToString("X2"));
            if (log) logger.WriteLog("\tSet LED2: R: 0x" + this.LED2_Red.ToString("X2") + " G: 0x" + this.LED2_Green.ToString("X2") + " B: 0x" + this.LED2_Blue.ToString("X2"));

            borg.SetLEDBatteryMonitor(this.BatteryMonitoringState);
            if (log) logger.WriteLog("\tCurrent battery LED monitor state: " + this.BatteryMonitoringState.ToString());

            borg.SetFailsafe(this.FailSafeState);
            if (log) logger.WriteLog("\tSet failsafe state: " + this.FailSafeState.ToString());

            if (log) logger.WriteLog("\tSettings complete...");

            elapsedTime.Stop();
            if (log) logger.WriteLog("\tBoard setting took " + elapsedTime.ElapsedMilliseconds.ToString() + " milliseconds...");
        }
    }
}
