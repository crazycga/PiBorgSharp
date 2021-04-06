using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace PiBorgSharp.UltraBorg
{
    public class UBReading_class : INotifyPropertyChanged
    {
        private int _bus = 0x01;
        private int _UltraBorgAddress = 0x00;
        private ILogger _log = null;
        private bool _successfulInit = false;

        private uint _sensor1Reading = 0;
        private uint _sensor2Reading = 0;
        private uint _sensor3Reading = 0;
        private uint _sensor4Reading = 0;
        
        private ushort _servo1Position = 0;
        private ushort _servo2Position = 0;
        private ushort _servo3Position = 0;
        private ushort _servo4Position = 0;

        private int _servo1Max = 0;
        private int _servo2Max = 0;
        private int _servo3Max = 0;
        private int _servo4Max = 0;

        private int _servo1Min = 0;
        private int _servo2Min = 0;
        private int _servo3Min = 0;
        private int _servo4Min = 0;

        private int _servo1Boot = 0;
        private int _servo2Boot = 0;
        private int _servo3Boot = 0;
        private int _servo4Boot = 0;

        private bool _IsUsed = false;

        public enum ScanType
        {
            FastSensorsOnly = 1
            , SensorsOnly = 2
            , QuickScan = 3
            , FullScan = 4
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(caller));
            }
        }

        public UBReading_class(UltraBorg_class myBorg, ILogger log = null)
        {
            if (myBorg == null)
            {
                throw new ArgumentOutOfRangeException("The UltraBorg class passed to UBReading_class is null");
            }

            this._bus = myBorg.BusNumber;
            this._UltraBorgAddress = myBorg.UltraBorgAddress;
            this._log = log;
            _successfulInit = true;
        }

        public uint sensor1Reading
        {
            get
            {
                return this._sensor1Reading;
            }
            set
            {
                this._sensor1Reading = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public uint sensor2Reading
        {
            get
            {
                return this._sensor2Reading;
            }
            set
            {
                this._sensor2Reading = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public uint sensor3Reading
        {
            get
            {
                return this._sensor3Reading;
            }
            set
            {
                this._sensor3Reading = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public uint sensor4Reading
        {
            get
            {
                return this._sensor4Reading;
            }
            set
            {
                this._sensor4Reading = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public ushort servo1Position
        {
            get
            {
                return this._servo1Position;
            }
            set
            {
                this._servo1Position = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public ushort servo2Position
        {
            get
            {
                return this._servo2Position;
            }
            set
            {
                this._servo2Position = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public ushort servo3Position
        {
            get
            {
                return this._servo3Position;
            }
            set
            {
                this._servo3Position = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public ushort servo4Position
        {
            get
            {
                return this._servo4Position;
            }
            set
            {
                this._servo4Position = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo1Min
        {
            get
            {
                return this._servo1Min;
            }
            set
            {
                this._servo1Min = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo2Min
        {
            get
            {
                return this._servo2Min;
            }
            set
            {
                this._servo2Min = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo3Min
        {
            get
            {
                return this._servo3Min;
            }
            set
            {
                this._servo3Min = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo4Min
        {
            get
            {
                return this._servo4Min;
            }
            set
            {
                this._servo4Min = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo1Max
        {
            get
            {
                return this._servo1Max;
            }
            set
            {
                this._servo1Max = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo2Max
        {
            get
            {
                return this._servo2Max;
            }
            set
            {
                this._servo2Max = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo3Max
        {
            get
            {
                return this._servo3Max;
            }
            set
            {
                this._servo3Max = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo4Max
        {
            get
            {
                return this._servo4Max;
            }
            set
            {
                this._servo4Max = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo1Boot
        {
            get
            {
                return this._servo1Boot;
            }
            set
            {
                this._servo1Boot = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo2Boot
        {
            get
            {
                return this._servo2Boot;
            }
            set
            {
                this._servo2Boot = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo3Boot
        {
            get
            {
                return this._servo3Boot;
            }
            set
            {
                this._servo3Boot = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public int servo4Boot
        {
            get
            {
                return this._servo4Boot;
            }
            set
            {
                this._servo4Boot = value;
                this._IsUsed = true;
                RaisePropertyChanged();
            }
        }

        public bool IsUsed
        {
            get
            {
                return this._IsUsed;
            }
        }




        public void EnumerateCurrentEnvironment(UltraBorg_class myBorg, ScanType scan, ILogger log = null)
        {
            if (!_successfulInit)
            {
                return;
            }

            if (log != null)
            {
                log.WriteLog("Enumerating environment on a scan type: " + scan.ToString());
            }

            if (scan == ScanType.FastSensorsOnly)
            {
                _sensor1Reading = myBorg.GetDistance(1, UltraBorg_class.FilterType.Unfiltered, log);
                _sensor2Reading = myBorg.GetDistance(2, UltraBorg_class.FilterType.Unfiltered, log);
                _sensor3Reading = myBorg.GetDistance(3, UltraBorg_class.FilterType.Unfiltered, log);
                _sensor4Reading = myBorg.GetDistance(4, UltraBorg_class.FilterType.Unfiltered, log);
            }

            _sensor1Reading = myBorg.GetDistance(1, UltraBorg_class.FilterType.Filtered, log);
            _sensor2Reading = myBorg.GetDistance(2, UltraBorg_class.FilterType.Filtered, log);
            _sensor3Reading = myBorg.GetDistance(3, UltraBorg_class.FilterType.Filtered, log);
            _sensor4Reading = myBorg.GetDistance(4, UltraBorg_class.FilterType.Filtered, log);

            if (scan == ScanType.SensorsOnly)
            {
                return;
            }

            _servo1Position = myBorg.GetRawServoPosition(1, log);
            _servo2Position = myBorg.GetRawServoPosition(2, log);
            _servo3Position = myBorg.GetRawServoPosition(3, log);
            _servo4Position = myBorg.GetRawServoPosition(4, log);

            if (scan == ScanType.QuickScan)
            {
                return;
            }

            _servo1Min = myBorg.GetServo(1, UltraBorg_class.ValueType.Minimum, log);
            _servo1Max = myBorg.GetServo(1, UltraBorg_class.ValueType.Maximum, log);
            _servo1Boot = myBorg.GetServo(1, UltraBorg_class.ValueType.Boot, log);

            _servo2Min = myBorg.GetServo(2, UltraBorg_class.ValueType.Minimum, log);
            _servo2Max = myBorg.GetServo(2, UltraBorg_class.ValueType.Maximum, log);
            _servo2Boot = myBorg.GetServo(2, UltraBorg_class.ValueType.Boot, log);

            _servo3Min = myBorg.GetServo(3, UltraBorg_class.ValueType.Minimum, log);
            _servo3Max = myBorg.GetServo(3, UltraBorg_class.ValueType.Maximum, log);
            _servo3Boot = myBorg.GetServo(3, UltraBorg_class.ValueType.Boot, log);

            _servo4Min = myBorg.GetServo(4, UltraBorg_class.ValueType.Minimum, log);
            _servo4Max = myBorg.GetServo(4, UltraBorg_class.ValueType.Maximum, log);
            _servo4Boot = myBorg.GetServo(4, UltraBorg_class.ValueType.Boot, log);
        }

    }
}
