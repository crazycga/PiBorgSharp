#!/usr/bin/env python
# coding: Latin-1

# Import library functions we need
import ThunderBorg
import time
import sys

# Setup the ThunderBorg
TB = ThunderBorg.ThunderBorg()     # Create a new ThunderBorg object
#TB.i2cAddress = 0x15              # Uncomment and change the value if you have changed the board address
TB.Init()                          # Set the board up (checks the board is connected)
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

# Disable the colour by battery level
TB.SetLedShowBattery(False)

# Loop over the sequence until the user presses CTRL+C
print 'Press CTRL+C to finish'
try:
    while True:
        # Loop over a set of different hues:
        for hue in range(300):
            # Get hue into the 0 to 3 range
            hue /= 100.0
            # Decide which two channels we are between
            if hue < 1.0:
                # Red to Green
                red = 1.0 - hue
                green = hue
                blue = 0.0
            elif hue < 2.0:
                # Green to Blue
                red = 0.0
                green = 2.0 - hue
                blue = hue - 1.0
            else:
                # Blue to Red
                red = hue - 2.0
                green = 0.0
                blue = 3.0 - hue
            # Set the chosen colour for both LEDs
            TB.SetLeds(red, green, blue)
            # Wait a short while
            time.sleep(0.01)
except KeyboardInterrupt:
    # User has pressed CTRL+C, set the LEDs to battery monitoring mode
    TB.SetLedShowBattery(True)
    print 'Done'
