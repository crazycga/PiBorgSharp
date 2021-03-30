#!/usr/bin/env python
# coding: latin-1

# Import library functions we need (we are using wiringpi so we can use the software PWM)
import wiringpi2 as wiringpi
wiringpi.wiringPiSetup()
import Tkinter
import tkMessageBox

# Set which GPIO pins the drive outputs are connected to
DRIVE_1 = 7
DRIVE_2 = 1
DRIVE_3 = 10
DRIVE_4 = 11

# Setup software PWMs on the GPIO pins
PWM_MAX = 100
wiringpi.softPwmCreate(DRIVE_1, 0, PWM_MAX)
wiringpi.softPwmCreate(DRIVE_2, 0, PWM_MAX)
wiringpi.softPwmCreate(DRIVE_3, 0, PWM_MAX)
wiringpi.softPwmCreate(DRIVE_4, 0, PWM_MAX)
wiringpi.softPwmWrite(DRIVE_1, 0)
wiringpi.softPwmWrite(DRIVE_2, 0)
wiringpi.softPwmWrite(DRIVE_3, 0)
wiringpi.softPwmWrite(DRIVE_4, 0)

# Internal state for managing the PWM levels
global pwmOn
global pwmLevel
pwmOn = {
    DRIVE_1:False,
    DRIVE_2:False,
    DRIVE_3:False,
    DRIVE_4:False
}
pwmLevel = {
    DRIVE_1:PWM_MAX,
    DRIVE_2:PWM_MAX,
    DRIVE_3:PWM_MAX,
    DRIVE_4:PWM_MAX
}

# Class representing the GUI dialog
class PicoBorg_tk(Tkinter.Tk):
    # Constructor (called when the object is first created)
    def __init__(self, parent):
        Tkinter.Tk.__init__(self, parent)
        self.parent = parent
        self.protocol("WM_DELETE_WINDOW", self.OnExit) # Call the OnExit function when user closes the dialog
        self.Initialise()

    # Initialise the dialog
    def Initialise(self):
        self.title('PicoBorg Example GUI')
        # Setup a grid of 4 buttons which command each drive, plus a PWM slider for drive 2
        self.grid()
        self.but1 = Tkinter.Button(self, text = '1', command = self.but1_click)
        self.but1['fg'] = '#FFFFFF'
        self.but1['font'] = ("Arial", 60, "bold")
        self.but1.grid(column = 0, row = 0, rowspan = 1, sticky = 'NSEW')
        self.sld1 = Tkinter.Scale(self, from_ = 0, to = PWM_MAX, orient = Tkinter.HORIZONTAL, command = self.sld1_move)
        self.sld1.set(pwmLevel[DRIVE_1])
        self.sld1.grid(column = 0, row = 1, rowspan = 1, sticky = 'NSEW')
        self.but2 = Tkinter.Button(self, text = '2', command = self.but2_click)
        self.but2['fg'] = '#FFFFFF'
        self.but2['font'] = ("Arial", 60, "bold")
        self.but2.grid(column = 1, row = 0, rowspan = 1, sticky = 'NSEW')
        self.sld2 = Tkinter.Scale(self, from_ = 0, to = PWM_MAX, orient = Tkinter.HORIZONTAL, command = self.sld2_move)
        self.sld2.set(pwmLevel[DRIVE_2])
        self.sld2.grid(column = 1, row = 1, rowspan = 1, sticky = 'NSEW')
        self.but3 = Tkinter.Button(self, text = '3', command = self.but3_click)
        self.but3['fg'] = '#FFFFFF'
        self.but3['font'] = ("Arial", 60, "bold")
        self.but3.grid(column = 2, row = 0, rowspan = 1, sticky = 'NSEW')
        self.sld3 = Tkinter.Scale(self, from_ = 0, to = PWM_MAX, orient = Tkinter.HORIZONTAL, command = self.sld3_move)
        self.sld3.set(pwmLevel[DRIVE_3])
        self.sld3.grid(column = 2, row = 1, rowspan = 1, sticky = 'NSEW')
        self.but4 = Tkinter.Button(self, text = '4', command = self.but4_click)
        self.but4['fg'] = '#FFFFFF'
        self.but4['font'] = ("Arial", 60, "bold")
        self.but4.grid(column = 3, row = 0, rowspan = 1, sticky = 'NSEW')
        self.sld4 = Tkinter.Scale(self, from_ = 0, to = PWM_MAX, orient = Tkinter.HORIZONTAL, command = self.sld4_move)
        self.sld4.set(pwmLevel[DRIVE_4])
        self.sld4.grid(column = 3, row = 1, rowspan = 1, sticky = 'NSEW')
        self.grid_columnconfigure(0, weight = 1)
        self.grid_columnconfigure(1, weight = 1)
        self.grid_columnconfigure(2, weight = 1)
        self.grid_columnconfigure(3, weight = 1)
        self.grid_rowconfigure(0, weight = 4)
        self.grid_rowconfigure(1, weight = 1)
        # Set the size of the dialog
        self.resizable(True, True)
        self.geometry('800x200')
        # Start with the motors off
        self.MotorOff()
        # Set the button colours based on drive level
        self.SetColourDrive(self.but1, DRIVE_1)
        self.SetColourDrive(self.but2, DRIVE_2)
        self.SetColourDrive(self.but3, DRIVE_3)
        self.SetColourDrive(self.but4, DRIVE_4)

    # Called when the user closes the dialog
    def OnExit(self):
        # Turn drives off, release GPIO and end the program
        self.MotorOff()
        self.quit()

    # Turn all the drives off
    def MotorOff(self):
        global pwmOn
        wiringpi.softPwmWrite(DRIVE_1, 0)
        wiringpi.softPwmWrite(DRIVE_2, 0)
        wiringpi.softPwmWrite(DRIVE_3, 0)
        wiringpi.softPwmWrite(DRIVE_4, 0)
        pwmOn[DRIVE_1] = False
        pwmOn[DRIVE_2] = False
        pwmOn[DRIVE_3] = False
        pwmOn[DRIVE_4] = False

    # Set a button to be coloured based on a GPIO state
    def SetColourDrive(self, button, drive):
        global pwmOn
        if pwmOn[drive]:
            button['bg'] = '#008000'
        else:
            button['bg'] = '#400000'
        button['activebackground'] = button['bg']

    # Toggle a drive pin on/off
    def ToggleDrive(self, button, drive):
        global pwmOn
        global pwmLevel
        # See if the drive is flagged as on and toggle between level and 0
        if pwmOn[drive]:
            pwmOn[drive] = False
            wiringpi.softPwmWrite(drive, 0)
        else:
            pwmOn[drive] = True
            wiringpi.softPwmWrite(drive, pwmLevel[drive])
        self.SetColourDrive(button, drive)

    # Change a PWM level
    def SetNewPwm(self, drive, value):
        global pwmOn
        global pwmLevel
        pwmLevel[drive] = int(value)
        if pwmOn[drive]:
            wiringpi.softPwmWrite(drive, pwmLevel[drive])
            pass

    # Called when but1 is clicked
    def but1_click(self):
        self.ToggleDrive(self.but1, DRIVE_1)

    # Called when sld1 is moved
    def sld1_move(self, value):
        self.SetNewPwm(DRIVE_1, value)

    # Called when but2 is clicked
    def but2_click(self):
        self.ToggleDrive(self.but2, DRIVE_2)

    # Called when sld2 is moved
    def sld2_move(self, value):
        self.SetNewPwm(DRIVE_2, value)

    # Called when but3 is clicked
    def but3_click(self):
        self.ToggleDrive(self.but3, DRIVE_3)

    # Called when sld3 is moved
    def sld3_move(self, value):
        self.SetNewPwm(DRIVE_3, value)

    # Called when but4 is clicked
    def but4_click(self):
        self.ToggleDrive(self.but4, DRIVE_4)

    # Called when sld4 is moved
    def sld4_move(self, value):
        self.SetNewPwm(DRIVE_4, value)

# if we are the main program (python was passed a script) load the dialog automatically
if __name__ == "__main__":
    app = PicoBorg_tk(None)
    app.mainloop()

