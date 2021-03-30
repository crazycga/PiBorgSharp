#!/usr/bin/env python
# coding: latin-1

# Import library functions we need 
import UltraBorg
import Tkinter
import Tix

# Start the UltraBorg
global UB
UB = UltraBorg.UltraBorg()      # Create a new UltraBorg object
UB.Init()                       # Set the board up (checks the board is connected)

# Calibration settings
CAL_PWM_MIN = 0                 # Minimum selectable calibration burst (2000 = 1 ms)
CAL_PWM_MAX = 6000              # Maximum selectable calibration burst (2000 = 1 ms)
CAL_PWM_START = 3000            # Startup value for the calibration burst (2000 = 1 ms)
STD_PWM_MIN = 2000              # Default minimum position
STD_PWM_MAX = 4000              # Default maximum position
STD_PWM_START = 0xFFFF          # Default startup position (unset, calculates centre)

# Class representing the GUI dialog
class UltraBorg_tk(Tkinter.Tk):
    # Constructor (called when the object is first created)
    def __init__(self, parent):
        Tkinter.Tk.__init__(self, parent)
        self.tk.call('package', 'require', 'Tix')
        self.parent = parent
        self.protocol("WM_DELETE_WINDOW", self.OnExit) # Call the OnExit function when user closes the dialog
        self.Initialise()

    # Initialise the dialog
    def Initialise(self):
        global UB
        self.title('UltraBorg Tuning GUI')

        # Setup a grid of 4 sliders which command each servo output, plus 4 readings for the servo positions and distances
        self.grid()

        # The heading labels
        self.lblHeadingTask = Tkinter.Label(self, text = 'Task to perform')
        self.lblHeadingTask['font'] = ('Arial', 18, 'bold')
        self.lblHeadingTask.grid(column = 0, row = 0, columnspan = 1, rowspan = 1, sticky = 'NSEW')
        self.lblHeadingServo1 = Tkinter.Label(self, text = 'Servo #1')
        self.lblHeadingServo1['font'] = ('Arial', 18, 'bold')
        self.lblHeadingServo1.grid(column = 1, row = 0, columnspan = 2, rowspan = 1, sticky = 'NSEW')
        self.lblHeadingServo2 = Tkinter.Label(self, text = 'Servo #2')
        self.lblHeadingServo2['font'] = ('Arial', 18, 'bold')
        self.lblHeadingServo2.grid(column = 3, row = 0, columnspan = 2, rowspan = 1, sticky = 'NSEW')
        self.lblHeadingServo3 = Tkinter.Label(self, text = 'Servo #3')
        self.lblHeadingServo3['font'] = ('Arial', 18, 'bold')
        self.lblHeadingServo3.grid(column = 5, row = 0, columnspan = 2, rowspan = 1, sticky = 'NSEW')
        self.lblHeadingServo4 = Tkinter.Label(self, text = 'Servo #4')
        self.lblHeadingServo4['font'] = ('Arial', 18, 'bold')
        self.lblHeadingServo4.grid(column = 7, row = 0, columnspan = 2, rowspan = 1, sticky = 'NSEW')

        # The task descriptions
        self.lblTaskMaximum = Tkinter.Label(self, text = 
                'Hover over the buttons\n' + 
                'for more help\n\n' +
                'Set the servo maximums')
        self.lblTaskMaximum['font'] = ('Arial', 14, '')
        self.lblTaskMaximum.grid(column = 0, row = 1, columnspan = 1, rowspan = 2, sticky = 'NEW')
        self.lblTaskStartup = Tkinter.Label(self, text = 'Set the servo startup positions')
        self.lblTaskStartup['font'] = ('Arial', 14, '')
        self.lblTaskStartup.grid(column = 0, row = 3, columnspan = 1, rowspan = 2, sticky = 'NSEW')
        self.lblTaskMinimum = Tkinter.Label(self, text = 'Set the servo minimums')
        self.lblTaskMinimum['font'] = ('Arial', 14, '')
        self.lblTaskMinimum.grid(column = 0, row = 5, columnspan = 1, rowspan = 2, sticky = 'NSEW')
        self.lblTaskCurrent = Tkinter.Label(self, text = 'Current servo position')
        self.lblTaskCurrent['font'] = ('Arial', 18, 'bold')
        self.lblTaskCurrent.grid(column = 0, row = 7, columnspan = 1, rowspan = 1, sticky = 'NSEW')

        # The servo sliders
        self.sld1 = Tkinter.Scale(self, from_ = CAL_PWM_MAX, to = CAL_PWM_MIN, orient = Tkinter.VERTICAL, command = self.sld1_move, showvalue = 0)
        self.sld1.set(CAL_PWM_START)
        self.sld1.grid(column = 1, row = 1, rowspan = 6, columnspan = 1, sticky = 'NSE')
        self.sld2 = Tkinter.Scale(self, from_ = CAL_PWM_MAX, to = CAL_PWM_MIN, orient = Tkinter.VERTICAL, command = self.sld2_move, showvalue = 0)
        self.sld2.set(CAL_PWM_START)
        self.sld2.grid(column = 3, row = 1, rowspan = 6, columnspan = 1, sticky = 'NSE')
        self.sld3 = Tkinter.Scale(self, from_ = CAL_PWM_MAX, to = CAL_PWM_MIN, orient = Tkinter.VERTICAL, command = self.sld3_move, showvalue = 0)
        self.sld3.set(CAL_PWM_START)
        self.sld3.grid(column = 5, row = 1, rowspan = 6, columnspan = 1, sticky = 'NSE')
        self.sld4 = Tkinter.Scale(self, from_ = CAL_PWM_MAX, to = CAL_PWM_MIN, orient = Tkinter.VERTICAL, command = self.sld4_move, showvalue = 0)
        self.sld4.set(CAL_PWM_START)
        self.sld4.grid(column = 7, row = 1, rowspan = 6, columnspan = 1, sticky = 'NSE')

        # The servo maximums
        self.lblServoMaximum1 = Tkinter.Label(self, text = '-')
        self.lblServoMaximum1['font'] = ('Arial', 14, '')
        self.lblServoMaximum1.grid(column = 2, row = 1, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMaximum2 = Tkinter.Label(self, text = '-')
        self.lblServoMaximum2['font'] = ('Arial', 14, '')
        self.lblServoMaximum2.grid(column = 4, row = 1, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMaximum3 = Tkinter.Label(self, text = '-')
        self.lblServoMaximum3['font'] = ('Arial', 14, '')
        self.lblServoMaximum3.grid(column = 6, row = 1, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMaximum4 = Tkinter.Label(self, text = '-')
        self.lblServoMaximum4['font'] = ('Arial', 14, '')
        self.lblServoMaximum4.grid(column = 8, row = 1, columnspan = 1, rowspan = 1, sticky = 'SW')

        # The servo maximum set buttons
        self.butServoMaximum1 = Tkinter.Button(self, text = 'Save\nmaximum', command = self.butServoMaximum1_click)
        self.butServoMaximum1['font'] = ('Arial', 12, '')
        self.butServoMaximum1.grid(column = 2, row = 2, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMaximum2 = Tkinter.Button(self, text = 'Save\nmaximum', command = self.butServoMaximum2_click)
        self.butServoMaximum2['font'] = ('Arial', 12, '')
        self.butServoMaximum2.grid(column = 4, row = 2, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMaximum3 = Tkinter.Button(self, text = 'Save\nmaximum', command = self.butServoMaximum3_click)
        self.butServoMaximum3['font'] = ('Arial', 12, '')
        self.butServoMaximum3.grid(column = 6, row = 2, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMaximum4 = Tkinter.Button(self, text = 'Save\nmaximum', command = self.butServoMaximum4_click)
        self.butServoMaximum4['font'] = ('Arial', 12, '')
        self.butServoMaximum4.grid(column = 8, row = 2, columnspan = 1, rowspan = 1, sticky = 'NW')

        # The servo startups
        self.lblServoStartup1 = Tkinter.Label(self, text = '-')
        self.lblServoStartup1['font'] = ('Arial', 14, '')
        self.lblServoStartup1.grid(column = 2, row = 3, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoStartup2 = Tkinter.Label(self, text = '-')
        self.lblServoStartup2['font'] = ('Arial', 14, '')
        self.lblServoStartup2.grid(column = 4, row = 3, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoStartup3 = Tkinter.Label(self, text = '-')
        self.lblServoStartup3['font'] = ('Arial', 14, '')
        self.lblServoStartup3.grid(column = 6, row = 3, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoStartup4 = Tkinter.Label(self, text = '-')
        self.lblServoStartup4['font'] = ('Arial', 14, '')
        self.lblServoStartup4.grid(column = 8, row = 3, columnspan = 1, rowspan = 1, sticky = 'SW')

        # The servo startup set buttons
        self.butServoStartup1 = Tkinter.Button(self, text = 'Save\nstartup', command = self.butServoStartup1_click)
        self.butServoStartup1['font'] = ('Arial', 12, '')
        self.butServoStartup1.grid(column = 2, row = 4, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoStartup2 = Tkinter.Button(self, text = 'Save\nstartup', command = self.butServoStartup2_click)
        self.butServoStartup2['font'] = ('Arial', 12, '')
        self.butServoStartup2.grid(column = 4, row = 4, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoStartup3 = Tkinter.Button(self, text = 'Save\nstartup', command = self.butServoStartup3_click)
        self.butServoStartup3['font'] = ('Arial', 12, '')
        self.butServoStartup3.grid(column = 6, row = 4, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoStartup4 = Tkinter.Button(self, text = 'Save\nstartup', command = self.butServoStartup4_click)
        self.butServoStartup4['font'] = ('Arial', 12, '')
        self.butServoStartup4.grid(column = 8, row = 4, columnspan = 1, rowspan = 1, sticky = 'NW')

        # The servo minimums
        self.lblServoMinimum1 = Tkinter.Label(self, text = '-')
        self.lblServoMinimum1['font'] = ('Arial', 14, '')
        self.lblServoMinimum1.grid(column = 2, row = 5, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMinimum2 = Tkinter.Label(self, text = '-')
        self.lblServoMinimum2['font'] = ('Arial', 14, '')
        self.lblServoMinimum2.grid(column = 4, row = 5, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMinimum3 = Tkinter.Label(self, text = '-')
        self.lblServoMinimum3['font'] = ('Arial', 14, '')
        self.lblServoMinimum3.grid(column = 6, row = 5, columnspan = 1, rowspan = 1, sticky = 'SW')
        self.lblServoMinimum4 = Tkinter.Label(self, text = '-')
        self.lblServoMinimum4['font'] = ('Arial', 14, '')
        self.lblServoMinimum4.grid(column = 8, row = 5, columnspan = 1, rowspan = 1, sticky = 'SW')

        # The servo minimum set buttons
        self.butServoMinimum1 = Tkinter.Button(self, text = 'Save\nminimum', command = self.butServoMinimum1_click)
        self.butServoMinimum1['font'] = ('Arial', 12, '')
        self.butServoMinimum1.grid(column = 2, row = 6, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMinimum2 = Tkinter.Button(self, text = 'Save\nminimum', command = self.butServoMinimum2_click)
        self.butServoMinimum2['font'] = ('Arial', 12, '')
        self.butServoMinimum2.grid(column = 4, row = 6, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMinimum3 = Tkinter.Button(self, text = 'Save\nminimum', command = self.butServoMinimum3_click)
        self.butServoMinimum3['font'] = ('Arial', 12, '')
        self.butServoMinimum3.grid(column = 6, row = 6, columnspan = 1, rowspan = 1, sticky = 'NW')
        self.butServoMinimum4 = Tkinter.Button(self, text = 'Save\nminimum', command = self.butServoMinimum4_click)
        self.butServoMinimum4['font'] = ('Arial', 12, '')
        self.butServoMinimum4.grid(column = 8, row = 6, columnspan = 1, rowspan = 1, sticky = 'NW')

        # The servo values (read from the controller)
        self.lblServo1 = Tkinter.Label(self, text = '-')
        self.lblServo1['font'] = ('Arial', 18, '')
        self.lblServo1.grid(column = 2, row = 7, columnspan = 1, rowspan = 1, sticky = 'NSW')
        self.lblServo2 = Tkinter.Label(self, text = '-')
        self.lblServo2['font'] = ('Arial', 18, '')
        self.lblServo2.grid(column = 4, row = 7, columnspan = 1, rowspan = 1, sticky = 'NSW')
        self.lblServo3 = Tkinter.Label(self, text = '-')
        self.lblServo3['font'] = ('Arial', 18, '')
        self.lblServo3.grid(column = 6, row = 7, columnspan = 1, rowspan = 1, sticky = 'NSW')
        self.lblServo4 = Tkinter.Label(self, text = '-')
        self.lblServo4['font'] = ('Arial', 18, '')
        self.lblServo4.grid(column = 8, row = 7, columnspan = 1, rowspan = 1, sticky = 'NSW')

        # The major operations
        self.butReset = Tkinter.Button(self, text = 'Reset and save all to default values', command = self.butReset_click)
        self.butReset['font'] = ("Arial", 20, "bold")
        self.butReset.grid(column = 0, row = 8, rowspan = 1, columnspan = 9, sticky = 'NSEW')

        # Balloon help pop-up
        self.tipStatus = Tix.Balloon(self)
        self.servoSliderHelp = ('Use this slider to move servo #%d.\n' +
                                'Hover over each button for more help.\n' +
                                'The current position of servo #%d is shown at the bottom.')
        self.servoMaxHelp = ('Set the maximum for servo #%d.\n' + 
                             'Slowly move the servo #%d slider up until the servo stops moving,\n' + 
                             'then move the slider back down slightly to where it moves again.\n' +
                             'This will become +100%%.')
        self.servoStartupHelp = ('Set the startup position for servo #%d.\n' +
                                 'When UltraBorg powers up, servo #%d will move to this position.\n' + 
                                 'This position must be between the set maximum and minimum.\n' +
                                 'If unset then 0%% is used instead.')
        self.servoMinHelp = ('Set the minimum for servo #%d.\n' + 
                             'Slowly move the servo #%d slider down until the servo stops moving,\n' + 
                             'then move the slider back up slightly to where it moves again.\n' +
                             'This will become -100%%.')
        self.tipStatus.bind_widget(self.sld1,             balloonmsg = self.servoSliderHelp  % (1, 1))
        self.tipStatus.bind_widget(self.butServoMaximum1, balloonmsg = self.servoMaxHelp     % (1, 1))
        self.tipStatus.bind_widget(self.butServoStartup1, balloonmsg = self.servoStartupHelp % (1, 1))
        self.tipStatus.bind_widget(self.butServoMinimum1, balloonmsg = self.servoMinHelp     % (1, 1))
        self.tipStatus.bind_widget(self.sld2,             balloonmsg = self.servoSliderHelp  % (2, 2))
        self.tipStatus.bind_widget(self.butServoMaximum2, balloonmsg = self.servoMaxHelp     % (2, 2))
        self.tipStatus.bind_widget(self.butServoStartup2, balloonmsg = self.servoStartupHelp % (2, 2))
        self.tipStatus.bind_widget(self.butServoMinimum2, balloonmsg = self.servoMinHelp     % (2, 2))
        self.tipStatus.bind_widget(self.sld3,             balloonmsg = self.servoSliderHelp  % (3, 3))
        self.tipStatus.bind_widget(self.butServoMaximum3, balloonmsg = self.servoMaxHelp     % (3, 3))
        self.tipStatus.bind_widget(self.butServoStartup3, balloonmsg = self.servoStartupHelp % (3, 3))
        self.tipStatus.bind_widget(self.butServoMinimum3, balloonmsg = self.servoMinHelp     % (3, 3))
        self.tipStatus.bind_widget(self.sld4,             balloonmsg = self.servoSliderHelp  % (4, 4))
        self.tipStatus.bind_widget(self.butServoMaximum4, balloonmsg = self.servoMaxHelp     % (4, 4))
        self.tipStatus.bind_widget(self.butServoStartup4, balloonmsg = self.servoStartupHelp % (4, 4))
        self.tipStatus.bind_widget(self.butServoMinimum4, balloonmsg = self.servoMinHelp     % (4, 4))


        # The grid sizing
        self.grid_columnconfigure(0, weight = 1)
        self.grid_columnconfigure(1, weight = 1)
        self.grid_columnconfigure(2, weight = 2)
        self.grid_columnconfigure(3, weight = 1)
        self.grid_columnconfigure(4, weight = 2)
        self.grid_columnconfigure(5, weight = 1)
        self.grid_columnconfigure(6, weight = 2)
        self.grid_columnconfigure(7, weight = 1)
        self.grid_columnconfigure(8, weight = 2)
        self.grid_rowconfigure(0, weight = 1)
        self.grid_rowconfigure(1, weight = 1)
        self.grid_rowconfigure(2, weight = 1)
        self.grid_rowconfigure(3, weight = 1)
        self.grid_rowconfigure(4, weight = 1)
        self.grid_rowconfigure(5, weight = 1)
        self.grid_rowconfigure(6, weight = 1)
        self.grid_rowconfigure(7, weight = 1)
        self.grid_rowconfigure(8, weight = 1)

        # Set the size of the dialog
        self.resizable(True, True)
        self.geometry('1000x700')

        # Read the current settings for each servo
        self.ReadAllCalibration()

        # Start polling for readings
        self.poll()

    # Polling function
    def poll(self):
        global UB

        # Read the servo positions
        servo1 = UB.GetRawServoPosition1()
        servo2 = UB.GetRawServoPosition2()
        servo3 = UB.GetRawServoPosition3()
        servo4 = UB.GetRawServoPosition4()

        # Set the servo displays
        self.lblServo1['text'] = '%d' % (servo1)
        self.lblServo2['text'] = '%d' % (servo2)
        self.lblServo3['text'] = '%d' % (servo3)
        self.lblServo4['text'] = '%d' % (servo4)

        # Prime the next poll
        self.after(200, self.poll)

    # Reads all of the current calibration settings
    def ReadAllCalibration(self):
        self.SetLabelValue(self.lblServoMaximum1, UB.PWM_MAX_1)
        self.SetLabelValue(self.lblServoMaximum2, UB.PWM_MAX_2)
        self.SetLabelValue(self.lblServoMaximum3, UB.PWM_MAX_3)
        self.SetLabelValue(self.lblServoMaximum4, UB.PWM_MAX_4)
        self.SetLabelValue(self.lblServoMinimum1, UB.PWM_MIN_1)
        self.SetLabelValue(self.lblServoMinimum2, UB.PWM_MIN_2)
        self.SetLabelValue(self.lblServoMinimum3, UB.PWM_MIN_3)
        self.SetLabelValue(self.lblServoMinimum4, UB.PWM_MIN_4)
        self.SetLabelValue(self.lblServoStartup1, UB.GetWithRetry(UB.GetServoStartup1, 5))
        self.SetLabelValue(self.lblServoStartup2, UB.GetWithRetry(UB.GetServoStartup2, 5))
        self.SetLabelValue(self.lblServoStartup3, UB.GetWithRetry(UB.GetServoStartup3, 5))
        self.SetLabelValue(self.lblServoStartup4, UB.GetWithRetry(UB.GetServoStartup4, 5))

    # Takes a label and PWM drive level for display
    def SetLabelValue(self, label, pwmLevel):
        if pwmLevel == None:
            label['text'] = 'Unset'
        elif pwmLevel == 0x0000:
            label['text'] = 'Unset'
        elif pwmLevel == 0xFFFF:
            label['text'] = 'Unset'
        else:
            label['text'] = '%d' % (pwmLevel)

    # Takes a label and returns a PWM drive level or 0
    def GetLabelValue(self, label):
        try:
            return int(label['text'])
        except:
            return 0

    # Called when the user closes the dialog
    def OnExit(self):
        # End the program
        self.quit()

    # Called when sld1 is moved
    def sld1_move(self, value):
        global UB
        UB.CalibrateServoPosition1(int(value))

    # Called when sld2 is moved
    def sld2_move(self, value):
        global UB
        UB.CalibrateServoPosition2(int(value))

    # Called when sld3 is moved
    def sld3_move(self, value):
        global UB
        UB.CalibrateServoPosition3(int(value))

    # Called when sld4 is moved
    def sld4_move(self, value):
        global UB
        UB.CalibrateServoPosition4(int(value))

    # Called when butReset is clicked
    def butReset_click(self):
        global UB
        # Set all values back to standard
        UB.SetWithRetry(UB.SetServoMaximum1, UB.GetServoMaximum1, STD_PWM_MAX, 5)
        UB.SetWithRetry(UB.SetServoMinimum1, UB.GetServoMinimum1, STD_PWM_MIN, 5)
        UB.SetWithRetry(UB.SetServoStartup1, UB.GetServoStartup1, STD_PWM_START, 5)
        UB.SetWithRetry(UB.SetServoMaximum2, UB.GetServoMaximum2, STD_PWM_MAX, 5)
        UB.SetWithRetry(UB.SetServoMinimum2, UB.GetServoMinimum2, STD_PWM_MIN, 5)
        UB.SetWithRetry(UB.SetServoStartup2, UB.GetServoStartup2, STD_PWM_START, 5)
        UB.SetWithRetry(UB.SetServoMaximum3, UB.GetServoMaximum3, STD_PWM_MAX, 5)
        UB.SetWithRetry(UB.SetServoMinimum3, UB.GetServoMinimum3, STD_PWM_MIN, 5)
        UB.SetWithRetry(UB.SetServoStartup3, UB.GetServoStartup3, STD_PWM_START, 5)
        UB.SetWithRetry(UB.SetServoMaximum4, UB.GetServoMaximum4, STD_PWM_MAX, 5)
        UB.SetWithRetry(UB.SetServoMinimum4, UB.GetServoMinimum4, STD_PWM_MIN, 5)
        UB.SetWithRetry(UB.SetServoStartup4, UB.GetServoStartup4, STD_PWM_START, 5)
        # Move back to centre
        self.sld1.set(CAL_PWM_START)
        self.sld2.set(CAL_PWM_START)
        self.sld3.set(CAL_PWM_START)
        self.sld4.set(CAL_PWM_START)
        # Re-read calibration settings
        self.ReadAllCalibration()

    # Called when butServoMaximum1 is clicked
    def butServoMaximum1_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo1)
        if pwmLevel == 0:
            self.lblServoMaximum1['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMaximum1['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMaximum1, UB.GetServoMaximum1, pwmLevel, 5)
            if okay:
                self.lblServoMaximum1['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMaximum1['fg'] = '#000000'
            else:
                self.lblServoMaximum1['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMaximum1['fg'] = '#A00000'

    # Called when butServoMinimum1 is clicked
    def butServoMinimum1_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo1)
        if pwmLevel == 0:
            self.lblServoMinimum1['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMinimum1['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMinimum1, UB.GetServoMinimum1, pwmLevel, 5)
            if okay:
                self.lblServoMinimum1['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMinimum1['fg'] = '#000000'
            else:
                self.lblServoMinimum1['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMinimum1['fg'] = '#A00000'

    # Called when butServoStartup1 is clicked
    def butServoStartup1_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo1)
        if pwmLevel == 0:
            self.lblServoStartup1['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoStartup1['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoStartup1, UB.GetServoStartup1, pwmLevel, 5)
            if okay:
                self.lblServoStartup1['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoStartup1['fg'] = '#000000'
            else:
                self.lblServoStartup1['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoStartup1['fg'] = '#A00000'

    # Called when butServoMaximum2 is clicked
    def butServoMaximum2_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo2)
        if pwmLevel == 0:
            self.lblServoMaximum2['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMaximum2['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMaximum2, UB.GetServoMaximum2, pwmLevel, 5)
            if okay:
                self.lblServoMaximum2['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMaximum2['fg'] = '#000000'
            else:
                self.lblServoMaximum2['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMaximum2['fg'] = '#A00000'

    # Called when butServoMinimum2 is clicked
    def butServoMinimum2_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo2)
        if pwmLevel == 0:
            self.lblServoMinimum2['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMinimum2['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMinimum2, UB.GetServoMinimum2, pwmLevel, 5)
            if okay:
                self.lblServoMinimum2['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMinimum2['fg'] = '#000000'
            else:
                self.lblServoMinimum2['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMinimum2['fg'] = '#A00000'

    # Called when butServoStartup2 is clicked
    def butServoStartup2_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo2)
        if pwmLevel == 0:
            self.lblServoStartup2['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoStartup2['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoStartup2, UB.GetServoStartup2, pwmLevel, 5)
            if okay:
                self.lblServoStartup2['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoStartup2['fg'] = '#000000'
            else:
                self.lblServoStartup2['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoStartup2['fg'] = '#A00000'

    # Called when butServoMaximum3 is clicked
    def butServoMaximum3_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo3)
        if pwmLevel == 0:
            self.lblServoMaximum3['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMaximum3['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMaximum3, UB.GetServoMaximum3, pwmLevel, 5)
            if okay:
                self.lblServoMaximum3['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMaximum3['fg'] = '#000000'
            else:
                self.lblServoMaximum3['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMaximum3['fg'] = '#A00000'

    # Called when butServoMinimum3 is clicked
    def butServoMinimum3_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo3)
        if pwmLevel == 0:
            self.lblServoMinimum3['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMinimum3['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMinimum3, UB.GetServoMinimum3, pwmLevel, 5)
            if okay:
                self.lblServoMinimum3['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMinimum3['fg'] = '#000000'
            else:
                self.lblServoMinimum3['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMinimum3['fg'] = '#A00000'

    # Called when butServoStartup3 is clicked
    def butServoStartup3_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo3)
        if pwmLevel == 0:
            self.lblServoStartup3['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoStartup3['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoStartup3, UB.GetServoStartup3, pwmLevel, 5)
            if okay:
                self.lblServoStartup3['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoStartup3['fg'] = '#000000'
            else:
                self.lblServoStartup3['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoStartup3['fg'] = '#A00000'

    # Called when butServoMaximum4 is clicked
    def butServoMaximum4_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo4)
        if pwmLevel == 0:
            self.lblServoMaximum4['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMaximum4['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMaximum4, UB.GetServoMaximum4, pwmLevel, 5)
            if okay:
                self.lblServoMaximum4['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMaximum4['fg'] = '#000000'
            else:
                self.lblServoMaximum4['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMaximum4['fg'] = '#A00000'

    # Called when butServoMinimum4 is clicked
    def butServoMinimum4_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo4)
        if pwmLevel == 0:
            self.lblServoMinimum4['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoMinimum4['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoMinimum4, UB.GetServoMinimum4, pwmLevel, 5)
            if okay:
                self.lblServoMinimum4['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoMinimum4['fg'] = '#000000'
            else:
                self.lblServoMinimum4['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoMinimum4['fg'] = '#A00000'

    # Called when butServoStartup4 is clicked
    def butServoStartup4_click(self):
        global UB
        pwmLevel = self.GetLabelValue(self.lblServo4)
        if pwmLevel == 0:
            self.lblServoStartup4['text'] = '%d\nCannot save!' % (pwmLevel)
            self.lblServoStartup4['fg'] = '#A00000'
        else:
            okay = UB.SetWithRetry(UB.SetServoStartup4, UB.GetServoStartup4, pwmLevel, 5)
            if okay:
                self.lblServoStartup4['text'] = '%d\nSaved' % (pwmLevel)
                self.lblServoStartup4['fg'] = '#000000'
            else:
                self.lblServoStartup4['text'] = '%d\nSave failed!' % (pwmLevel)
                self.lblServoStartup4['fg'] = '#A00000'

# if we are the main program (python was passed a script) load the dialog automatically
if __name__ == "__main__":
    app = UltraBorg_tk(None)
    app.mainloop()

