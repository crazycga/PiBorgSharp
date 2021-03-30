#!/usr/bin/env python
# coding: latin-1

# Import library functions we need 
import ThunderBorg
import Tkinter
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

# Class representing the GUI dialog
class ThunderBorg_tk(Tkinter.Tk):
    # Constructor (called when the object is first created)
    def __init__(self, parent):
        Tkinter.Tk.__init__(self, parent)
        self.parent = parent
        self.protocol("WM_DELETE_WINDOW", self.OnExit) # Call the OnExit function when user closes the dialog
        self.Initialise()

    # Initialise the dialog
    def Initialise(self):
        global TB
        self.title('ThunderBorg Example GUI')
        # Add 2 sliders which command each motor output, plus a stop button for both motors
        self.grid()
        self.sld1 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld1_move)
        self.sld1.set(0)
        self.sld1.grid(column = 1, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')
        self.sld2 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld2_move)
        self.sld2.set(0)
        self.sld2.grid(column = 2, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')
        self.butOff = Tkinter.Button(self, text = 'All Off', command = self.butOff_click)
        self.butOff['font'] = ("Arial", 20, "bold")
        self.butOff.grid(column = 0, row = 1, rowspan = 1, columnspan = 4, sticky = 'NSEW')
        # Add a display for the battery level
        self.lblBattery = Tkinter.Label(self, text = 'Power input: ??.?? V', justify = Tkinter.CENTER, bg = '#000', fg = '#FFF')
        self.lblBattery['font'] = ('Trebuchet', 20, 'bold')
        self.lblBattery.grid(column = 0, row = 2, rowspan = 1, columnspan = 4, sticky = 'NSEW')
        # Setup the grid scaling
        self.grid_columnconfigure(0, weight = 1)
        self.grid_columnconfigure(1, weight = 1)
        self.grid_columnconfigure(2, weight = 1)
        self.grid_columnconfigure(3, weight = 1)
        self.grid_rowconfigure(0, weight = 4)
        self.grid_rowconfigure(1, weight = 1)
        self.grid_rowconfigure(2, weight = 1)
        # Set the size of the dialog
        self.resizable(True, True)
        self.geometry('500x600')
        # Setup the initial motor state
        TB.MotorsOff()
        # Get the monitoring limits
        self.battMin, self.battMax = TB.GetBatteryMonitoringLimits()
        # Start polling for readings
        self.after(1, self.Poll)

    # Polling function
    def Poll(self):
        battery = TB.GetBatteryReading()
        monitorLevel = int(((battery - self.battMin) / (self.battMax - self.battMin)) * 511)
        if monitorLevel < 0:
            # Below minimum
            monitorColour = '#FF0000'
        elif monitorLevel < 256:
            # Low range
            monitorColour = '#FF%02X00' % (monitorLevel)
        elif monitorLevel < 512:
            # High range
            monitorColour = '#%02XFF00' % (511 - monitorLevel)
        else:
            # Above maximum
            monitorColour = '#00FF00'
        self.lblBattery['text'] = 'Power input: %02.2f V' % (battery)
        self.lblBattery['fg'] = monitorColour
        # Re-run the poll after 100 ms
        self.after(100, self.Poll)

    # Called when the user closes the dialog
    def OnExit(self):
        global TB
        # Turn drives off and end the program
        TB.MotorsOff()
        self.quit()

    # Called when sld1 is moved
    def sld1_move(self, value):
        global TB
        TB.SetMotor1(float(value) / 100.0)

    # Called when sld2 is moved
    def sld2_move(self, value):
        global TB
        TB.SetMotor2(float(value) / 100.0)

    # Called when butOff is clicked
    def butOff_click(self):
        global TB
        TB.MotorsOff()
        self.sld1.set(0)
        self.sld2.set(0)

# if we are the main program (python was passed a script) load the dialog automatically
if __name__ == "__main__":
    app = ThunderBorg_tk(None)
    app.mainloop()

