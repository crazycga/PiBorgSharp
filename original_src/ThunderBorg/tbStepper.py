#!/usr/bin/env python
# coding: latin-1

# Import library functions we need
import ThunderBorg
import time
import sys

# Tell the system how to drive the stepper
maxPower = 1.00                         # Output to drive the stepper
holdingPower = 0.50                     # Output to drive the stepper when holding
sequence = [                            # Order for stepping 
        [+maxPower, +maxPower],
        [+maxPower, -maxPower],
        [-maxPower, -maxPower],
        [-maxPower, +maxPower]] 
sequenceHold = [                        # Order for stepping at holding power
        [+holdingPower, +holdingPower],
        [+holdingPower, -holdingPower],
        [-holdingPower, -holdingPower],
        [-holdingPower, +holdingPower]] 
stepDelay = 0.002                       # Delay between steps

# Name the global variables
global step
global TB

# Setup the ThunderBorg
global TB
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
step = -1

# Function to perform a sequence of steps as fast as allowed
def MoveStep(count):
    global step
    global TB

    # Choose direction based on sign (+/-)
    if count < 0:
        dir = -1
        count *= -1
    else:
        dir = 1

    # Loop through the steps
    while count > 0:
        # Set a starting position if this is the first move
        if step == -1:
            drive = sequence[-1]
            TB.SetMotor1(drive[0])
            TB.SetMotor2(drive[1])
            step = 0
        else:
            step += dir

        # Wrap step when we reach the end of the sequence
        if step < 0:
            step = len(sequence) - 1
        elif step >= len(sequence):
            step = 0

        # For this step set the required drive values
        if step < len(sequence):
            drive = sequence[step]
            TB.SetMotor1(drive[0])
            TB.SetMotor2(drive[1])
        time.sleep(stepDelay)
        count -= 1

# Function to switch to holding power
def HoldPosition():
    global step
    global TB

    # For the current step set the required holding drive values
    if step < len(sequence):
        drive = sequenceHold[step]
        TB.SetMotor1(drive[0])
        TB.SetMotor2(drive[1])

try:
    # Start by turning all drives off
    TB.MotorsOff()
    # Loop forever
    while True:
        # Ask the user how many steps to move
        steps = input("Steps to move (-ve for reverse, 0 to quit): ")
        if steps == 0:
            # Turn off the drives
            TB.MotorsOff()
            print 'Goodbye'
            break
        else:
            # Move the specified amount of steps
            MoveStep(steps)
            # Turn the motor to holding power after we have finished moving
            HoldPosition()
except KeyboardInterrupt:
    # CTRL+C exit, turn off the drives
    TB.MotorsOff()
    print 'Terminated'

