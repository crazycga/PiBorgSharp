#!/usr/bin/env python
# coding: latin-1

# Import the libraries we need
import ThunderBorg
import time
import sys

# Setup the ThunderBorg
global TB
TB = ThunderBorg.ThunderBorg()  # Create a new ThunderBorg object
#TB.i2cAddress = 0x15           # Uncomment and change the value if you have changed the board address
TB.Init()                       # Set the board up (checks the board is connected)
if not TB.foundChip:
    boards = ThunderBorg.ScanForThunderBorg()
    if len(boards) == 0:
        print 'No ThunderBorg found, check you are attached :)'
    else:
        print 'No ThunderBorg at address %02X, but we did find boards:' % (TB.i2cAddress)
        for board in boards:
            print '    %02X (%d)' % (board, board)
        print 'If you need to change the I²C address change the setup line so it is correct, e.g.'
        print 'TB.i2cAddress = 0x%02X' % (boards[0])
    sys.exit()

# Set the LEDs to show the battery level
TB.SetLedShowBattery(True)

# Loop over the sequence until the user presses CTRL+C
print 'Press CTRL+C to finish'
try:
    while True:
        # Read the battery level
        voltageBatt = TB.GetBatteryReading()
        # Display the reading
        print 'Battery level: %0.2f V' % (voltageBatt)
        # Wait between readings
        time.sleep(0.5)
except KeyboardInterrupt:
    # User has pressed CTRL+C
    print 'Done'
