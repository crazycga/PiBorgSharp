#!/usr/bin/env python
# coding: latin-1

# Import library functions we need
import Diablo
import time

# Tell the system how to drive the stepper
sequence = [[1.0, 1.0], [1.0, -1.0], [-1.0, -1.0], [-1.0, 1.0]] # Order for stepping 
stepDelay = 0.002                                               # Delay between steps

# Name the global variables
global step
global DIABLO

# Setup the Diablo
DIABLO = Diablo.Diablo()
#DIABLO.i2cAddress = 0x44                # Uncomment and change the value if you have changed the board address
DIABLO.Init()
if not DIABLO.foundChip:
    boards = Diablo.ScanForDiablo()
    if len(boards) == 0:
        print 'No Diablo found, check you are attached :)'
    else:
        print 'No Diablo at address %02X, but we did find boards:' % (DIABLO.i2cAddress)
        for board in boards:
            print '    %02X (%d)' % (board, board)
        print 'If you need to change the I?C address change the setup line so it is correct, e.g.'
        print 'DIABLO.i2cAddress = 0x%02X' % (boards[0])
    sys.exit()
#DIABLO.SetEpoIgnore(True)              # Uncomment to disable EPO latch, needed if you do not have a switch / jumper
DIABLO.ResetEpo()
step = -1

# Function to perform a sequence of steps as fast as allowed
def MoveStep(count):
    global step
    global DIABLO

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
            DIABLO.SetMotor1(drive[0])
            DIABLO.SetMotor2(drive[1])
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
            DIABLO.SetMotor1(drive[0])
            DIABLO.SetMotor2(drive[1])
        time.sleep(stepDelay)
        count -= 1

try:
    # Start by turning all drives off
    DIABLO.MotorsOff()
    # Loop forever
    while True:
        # Ask the user how many steps to move
        steps = input("Steps to move (-ve for reverse, 0 to quit): ")
        if steps == 0:
            # Turn off the drives and release the GPIO pins
            DIABLO.MotorsOff()
            print 'Goodbye'
            break
        else:
            # Move the specified amount of steps
            MoveStep(steps)
except KeyboardInterrupt:
    # CTRL+C exit, turn off the drives and release the GPIO pins
    DIABLO.MotorsOff()
    print 'Terminated'

