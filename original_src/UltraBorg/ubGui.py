#!/usr/bin/env python
# coding: latin-1

# Import library functions we need 
import UltraBorg
import Tkinter

# Start the UltraBorg
global UB
UB = UltraBorg.UltraBorg()      # Create a new UltraBorg object
UB.Init()                       # Set the board up (checks the board is connected)

# Class representing the GUI dialog
class UltraBorg_tk(Tkinter.Tk):
    # Constructor (called when the object is first created)
    def __init__(self, parent):
        Tkinter.Tk.__init__(self, parent)
        self.parent = parent
        self.protocol("WM_DELETE_WINDOW", self.OnExit) # Call the OnExit function when user closes the dialog
        self.Initialise()

    # Initialise the dialog
    def Initialise(self):
        global UB
        self.title('UltraBorg Example GUI')

        # Setup a grid of 4 sliders which command each servo output, plus 4 readings for the servo positions and distances
        self.grid()

        # The servo sliders
        self.sld1 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld1_move)
        self.sld1.set(0)
        self.sld1.grid(column = 0, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')
        self.sld2 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld2_move)
        self.sld2.set(0)
        self.sld2.grid(column = 1, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')
        self.sld3 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld3_move)
        self.sld3.set(0)
        self.sld3.grid(column = 2, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')
        self.sld4 = Tkinter.Scale(self, from_ = +100, to = -100, orient = Tkinter.VERTICAL, command = self.sld4_move)
        self.sld4.set(0)
        self.sld4.grid(column = 3, row = 0, rowspan = 1, columnspan = 1, sticky = 'NSEW')

        # The servo values (read from the controller)
        self.lblServo1 = Tkinter.Label(self, text = '-')
        self.lblServo1['font'] = ('Arial', 20, 'bold')
        self.lblServo1.grid(column = 0, row = 1, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblServo2 = Tkinter.Label(self, text = '-')
        self.lblServo2['font'] = ('Arial', 20, 'bold')
        self.lblServo2.grid(column = 1, row = 1, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblServo3 = Tkinter.Label(self, text = '-')
        self.lblServo3['font'] = ('Arial', 20, 'bold')
        self.lblServo3.grid(column = 2, row = 1, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblServo4 = Tkinter.Label(self, text = '-')
        self.lblServo4['font'] = ('Arial', 20, 'bold')
        self.lblServo4.grid(column = 3, row = 1, columnspan = 1, rowspan = 1, sticky = 'NSEW')

        # The distance readings and a heading
        self.lblDistanceHeading = Tkinter.Label(self, text = 'Distances (mm)')
        self.lblDistanceHeading['font'] = ('Arial', 20, 'bold')
        self.lblDistanceHeading.grid(column = 0, row = 2, columnspan = 4, rowspan = 1, sticky = 'NSEW')
        self.lblDistance1 = Tkinter.Label(self, text = '-')
        self.lblDistance1['font'] = ('Arial', 20, 'bold')
        self.lblDistance1.grid(column = 0, row = 3, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblDistance2 = Tkinter.Label(self, text = '-')
        self.lblDistance2['font'] = ('Arial', 20, 'bold')
        self.lblDistance2.grid(column = 1, row = 3, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblDistance3 = Tkinter.Label(self, text = '-')
        self.lblDistance3['font'] = ('Arial', 20, 'bold')
        self.lblDistance3.grid(column = 2, row = 3, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblDistance4 = Tkinter.Label(self, text = '-')
        self.lblDistance4['font'] = ('Arial', 20, 'bold')
        self.lblDistance4.grid(column = 3, row = 3, columnspan = 1, rowspan = 1, sticky = 'NSEW')

        # The grid sizing
        self.grid_columnconfigure(0, weight = 1)
        self.grid_columnconfigure(1, weight = 1)
        self.grid_columnconfigure(2, weight = 1)
        self.grid_columnconfigure(3, weight = 1)
        self.grid_rowconfigure(0, weight = 4)
        self.grid_rowconfigure(1, weight = 1)
        self.grid_rowconfigure(2, weight = 1)
        self.grid_rowconfigure(3, weight = 1)

        # Set the size of the dialog
        self.resizable(True, True)
        self.geometry('400x600')

        # Start polling for readings
        self.poll()

    # Polling function
    def poll(self):
        global UB

        # Read the servo positions
        servo1 = UB.GetServoPosition1()
        servo2 = UB.GetServoPosition2()
        servo3 = UB.GetServoPosition3()
        servo4 = UB.GetServoPosition4()

        # Read the ultrasonic distances
        distance1 = int(UB.GetDistance1())
        distance2 = int(UB.GetDistance2())
        distance3 = int(UB.GetDistance3())
        distance4 = int(UB.GetDistance4())

        # Set the servo displays
        self.lblServo1['text'] = '%.0f %%' % (servo1 * 100.0)
        self.lblServo2['text'] = '%.0f %%' % (servo2 * 100.0)
        self.lblServo3['text'] = '%.0f %%' % (servo3 * 100.0)
        self.lblServo4['text'] = '%.0f %%' % (servo4 * 100.0)

        # Set the ultrasonic displays
        if distance1 == 0:
            self.lblDistance1['text'] = 'None'
        else:
            self.lblDistance1['text'] = '%4d' % (distance1)
        if distance2 == 0:
            self.lblDistance2['text'] = 'None'
        else:
            self.lblDistance2['text'] = '%4d' % (distance2)
        if distance3 == 0:
            self.lblDistance3['text'] = 'None'
        else:
            self.lblDistance3['text'] = '%4d' % (distance3)
        if distance4 == 0:
            self.lblDistance4['text'] = 'None'
        else:
            self.lblDistance4['text'] = '%4d' % (distance4)

        # Prime the next poll
        self.after(200, self.poll)

    # Called when the user closes the dialog
    def OnExit(self):
        # End the program
        self.quit()

    # Called when sld1 is moved
    def sld1_move(self, value):
        global UB
        UB.SetServoPosition1(float(value) / 100.0)

    # Called when sld2 is moved
    def sld2_move(self, value):
        global UB
        UB.SetServoPosition2(float(value) / 100.0)

    # Called when sld3 is moved
    def sld3_move(self, value):
        global UB
        UB.SetServoPosition3(float(value) / 100.0)

    # Called when sld4 is moved
    def sld4_move(self, value):
        global UB
        UB.SetServoPosition4(float(value) / 100.0)

# if we are the main program (python was passed a script) load the dialog automatically
if __name__ == "__main__":
    app = UltraBorg_tk(None)
    app.mainloop()

