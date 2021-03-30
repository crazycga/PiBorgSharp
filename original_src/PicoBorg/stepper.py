#!/usr/bin/env python
# coding: latin-1

# Import libary functions we need
import RPi.GPIO as GPIO
GPIO.setmode(GPIO.BCM)
import time

# Set which GPIO pins the drive outputs are connected to
DRIVE_1 = 4  # Black
DRIVE_2 = 18 # Green
DRIVE_3 = 8  # Red
DRIVE_4 = 7  # Blue
# Yellow and White are +ve centre taps

# Tell the system how to drive the stepper
sequence = [DRIVE_1, DRIVE_3, DRIVE_2, DRIVE_4] # Order for stepping (see data sheet for the stepper motor)
stepDelay = 0.002                               # Delay between steps

# Set all of the drive pins as output pins
GPIO.setup(DRIVE_1, GPIO.OUT)
GPIO.setup(DRIVE_2, GPIO.OUT)
GPIO.setup(DRIVE_3, GPIO.OUT)
GPIO.setup(DRIVE_4, GPIO.OUT)

# Function to set all drives off
def MotorOff():
    global step
    GPIO.output(DRIVE_1, GPIO.LOW)
    GPIO.output(DRIVE_2, GPIO.LOW)
    GPIO.output(DRIVE_3, GPIO.LOW)
    GPIO.output(DRIVE_4, GPIO.LOW)
    step = -1

# Function to perform a sequence of steps as fast as allowed
def MoveStep(count):
    global step

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
            GPIO.output(DRIVE_4, GPIO.HIGH)
            step = 0
        else:
            step += dir

        # Wrap step when we reach the end of the sequence
        if step < 0:
            step = len(sequence) - 1
        elif step >= len(sequence):
            step = 0

        # For this step turn one of the drives off and another on
        if step < len(sequence):
            GPIO.output(sequence[step-2], GPIO.LOW)
            GPIO.output(sequence[step], GPIO.HIGH)
        time.sleep(stepDelay)
        count -= 1

try:
    # Start by turning all drives off
    MotorOff()
    raw_input("You can now turn on the power, press ENTER to continue")
    # Loop forever
    while True:
        # Ask the user how many steps to move
        steps = input("Steps to move (-ve for reverse, 0 to quit): ")
        if steps == 0:
            # Turn off the drives and release the GPIO pins
            MotorOff()
            raw_input("Turn the power off now, press ENTER to continue")
            GPIO.cleanup()
            print 'Goodbye'
            break
        else:
            # Move the specified amount of steps
            MoveStep(steps)
except KeyboardInterrupt:
    # CTRL+C exit, turn off the drives and release the GPIO pins
    print 'Terminated'
    MotorOff()
    raw_input("Turn the power off now, press ENTER to continue")
    GPIO.cleanup()

