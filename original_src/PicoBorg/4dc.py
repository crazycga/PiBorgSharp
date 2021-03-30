#!/usr/bin/env python
# coding: latin-1

# Import libary functions we need
import RPi.GPIO as GPIO
GPIO.setmode(GPIO.BCM)

# Set which GPIO pins the drive outputs are connected to
DRIVE_1 = 4
DRIVE_2 = 18
DRIVE_3 = 8
DRIVE_4 = 7

# Set all of the drive pins as output pins
GPIO.setup(DRIVE_1, GPIO.OUT)
GPIO.setup(DRIVE_2, GPIO.OUT)
GPIO.setup(DRIVE_3, GPIO.OUT)
GPIO.setup(DRIVE_4, GPIO.OUT)

# Map current on/off state to command state
dInvert = {}
dInvert[True] = GPIO.LOW
dInvert[False] = GPIO.HIGH

# Map the on/off state to nicer names for display
dName = {}
dName[True] = 'ON '
dName[False] = 'OFF'

# Function to set all drives off
def MotorOff():
    GPIO.output(DRIVE_1, GPIO.LOW)
    GPIO.output(DRIVE_2, GPIO.LOW)
    GPIO.output(DRIVE_3, GPIO.LOW)
    GPIO.output(DRIVE_4, GPIO.LOW)

try:
    # Start by turning all drives off
    MotorOff()
    raw_input("You can now turn on the power, press ENTER to continue")
    while True:
        # Print the current state of all 4 drives
        print dName[GPIO.input(DRIVE_1)] + ' ' + dName[GPIO.input(DRIVE_2)] + ' ' + dName[GPIO.input(DRIVE_3)] + ' ' + dName[GPIO.input(DRIVE_4)]
        # Ask the user which drives they would like to toggle (switch on/off)
        toToggle = raw_input('Drives to toggle (Q to quit)? ')
        toToggle = toToggle.upper()
        # Toggle the appropriate drive lines
        if toToggle.find('Q') != -1:
            # Turn off the drives and release the GPIO pins
            MotorOff()
            raw_input("Turn the power off now, press ENTER to continue")
            GPIO.cleanup()
            print 'Goodbye'
            break
        if toToggle.find('1') != -1:
            # Invert drive 1
            GPIO.output(DRIVE_1, dInvert[GPIO.input(DRIVE_1)])
        if toToggle.find('2') != -1:
            # Invert drive 2
            GPIO.output(DRIVE_2, dInvert[GPIO.input(DRIVE_2)])
        if toToggle.find('3') != -1:
            # Invert drive 3
            GPIO.output(DRIVE_3, dInvert[GPIO.input(DRIVE_3)])
        if toToggle.find('4') != -1:
            # Invert drive 4
            GPIO.output(DRIVE_4, dInvert[GPIO.input(DRIVE_4)])
except KeyboardInterrupt:
    # CTRL+C exit, turn off the drives and release the GPIO pins
    print 'Terminated'
    MotorOff()
    raw_input("Turn the power off now, press ENTER to continue")
    GPIO.cleanup()

