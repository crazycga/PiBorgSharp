#!/usr/bin/env python
# coding: latin-1

# Import the libraries we need
import UltraBorg
import time

# Settings
distanceMin = 100.0             # Minimum distance in mm, corresponds to servo at -100%
distanceMax = 300.0             # Maximum distance in mm, corresponds to servo at +100%

# Start the UltraBorg
UB = UltraBorg.UltraBorg()      # Create a new UltraBorg object
UB.Init()                       # Set the board up (checks the board is connected)

# Calculate our divisor
distanceDiv = (distanceMax - distanceMin) / 2.0

# Loop over the sequence until the user presses CTRL+C
print 'Press CTRL+C to finish'
try:
    # Initial settings
    servo1 = 0.0
    servo2 = 0.0
    servo3 = 0.0
    servo4 = 0.0
    while True:
        # Read all four ultrasonic values, we use the raw values so we respond quickly
        usm1 = UB.GetRawDistance1()
        usm2 = UB.GetRawDistance2()
        usm3 = UB.GetRawDistance3()
        usm4 = UB.GetRawDistance4()
        # Convert to the nearest millimeter
        usm1 = int(usm1)
        usm2 = int(usm2)
        usm3 = int(usm3)
        usm4 = int(usm4)
        # Generate the servo positions based on the distance readings
        if usm1 != 0:
            servo1 = ((usm1 - distanceMin) / distanceDiv) - 1.0
            if servo1 > 1.0:
                servo1 = 1.0
            elif servo1 < -1.0:
                servo1 = -1.0
        if usm2 != 0:
            servo2 = ((usm2 - distanceMin) / distanceDiv) - 1.0
            if servo2 > 1.0:
                servo2 = 1.0
            elif servo2 < -1.0:
                servo2 = -1.0
        if usm3 != 0:
            servo3 = ((usm3 - distanceMin) / distanceDiv) - 1.0
            if servo3 > 1.0:
                servo3 = 1.0
            elif servo3 < -1.0:
                servo3 = -1.0
        if usm4 != 0:
            servo4 = ((usm4 - distanceMin) / distanceDiv) - 1.0
            if servo4 > 1.0:
                servo4 = 1.0
            elif servo4 < -1.0:
                servo4 = -1.0
        # Display our readings
        print '%4d mm -> %.1f %%' % (usm1, servo1 * 100.0)
        print '%4d mm -> %.1f %%' % (usm2, servo2 * 100.0)
        print '%4d mm -> %.1f %%' % (usm3, servo3 * 100.0)
        print '%4d mm -> %.1f %%' % (usm4, servo4 * 100.0)
        print
        # Set our new servo positions
        UB.SetServoPosition1(servo1)
        UB.SetServoPosition2(servo2)
        UB.SetServoPosition3(servo3)
        UB.SetServoPosition4(servo4)
        # Wait between readings
        time.sleep(.1)
except KeyboardInterrupt:
    # User has pressed CTRL+C
    print 'Done'
