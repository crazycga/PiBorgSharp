#!/usr/bin/env python
# coding: latin-1

# Import the libraries we need
import UltraBorg
import time

# Settings
servoMin = -1.0                 # Smallest servo position to use
servoMax = +1.0                 # Largest servo position to use
startupDelay = 0.5              # Delay before making the initial move
stepDelay = 0.1                 # Delay between steps
rateStart = 0.05                # Step distance for all servos during initial move
rateServo1 = 0.01               # Step distance for servo #1
rateServo2 = 0.02               # Step distance for servo #2
rateServo3 = 0.04               # Step distance for servo #3
rateServo4 = 0.08               # Step distance for servo #4

# Start the UltraBorg
UB = UltraBorg.UltraBorg()      # Create a new UltraBorg object
UB.Init()                       # Set the board up (checks the board is connected)

# Loop over the sequence until the user presses CTRL+C
print 'Press CTRL+C to finish'
try:
    print 'Move to central'
    # Initial settings
    servo1 = 0.0
    servo2 = 0.0
    servo3 = 0.0
    servo4 = 0.0
    # Set our initial servo positions
    UB.SetServoPosition1(servo1)
    UB.SetServoPosition2(servo2)
    UB.SetServoPosition3(servo3)
    UB.SetServoPosition4(servo4)
    # Wait a while to be sure the servos have caught up
    time.sleep(startupDelay)
    print 'Sweep to start position'
    while servo1 > servoMin:
        # Reduce the servo positions
        servo1 -= rateStart
        servo2 -= rateStart
        servo3 -= rateStart
        servo4 -= rateStart
        # Check if they are too small
        if servo1 < servoMin:
            servo1 = servoMin
            servo2 = servoMin
            servo3 = servoMin
            servo4 = servoMin
        # Set our new servo positions
        UB.SetServoPosition1(servo1)
        UB.SetServoPosition2(servo2)
        UB.SetServoPosition3(servo3)
        UB.SetServoPosition4(servo4)
        # Wait until the next step
        time.sleep(stepDelay)
    print 'Sweep all servos through the range'
    while True:
        # Increase the servo positions at separate rates
        servo1 += rateServo1
        servo2 += rateServo2
        servo3 += rateServo3
        servo4 += rateServo4
        # Check if any of them are too large, if so wrap to the over end
        if servo1 > servoMax:
            servo1 -= (servoMax - servoMin)
        if servo2 > servoMax:
            servo2 -= (servoMax - servoMin)
        if servo3 > servoMax:
            servo3 -= (servoMax - servoMin)
        if servo4 > servoMax:
            servo4 -= (servoMax - servoMin)
        # Set our new servo positions
        UB.SetServoPosition1(servo1)
        UB.SetServoPosition2(servo2)
        UB.SetServoPosition3(servo3)
        UB.SetServoPosition4(servo4)
        # Wait until the next step
        time.sleep(stepDelay)
except KeyboardInterrupt:
    # User has pressed CTRL+C
    print 'Done'
