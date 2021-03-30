#!/usr/bin/env python
# coding: latin-1

# Import library functions we need
import ThunderBorg
import pygtk
pygtk.require('2.0')
import gtk


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

# Disable the colour by battery level
TB.SetLedShowBattery(False)

# Class representing the GUI dialog
class ThunderBorgLed_gtk:
    # Constructor (called when the object is first created)
    def __init__(self):
        self.window = gtk.Window(gtk.WINDOW_TOPLEVEL)
        self.window.set_title("ThunderBorg LED control")
        self.window.connect("destroy", self.destroy)

        self.vbxGrid = gtk.VBox(False, 0)
        self.window.add(self.vbxGrid)

        self.lblThunderBorgColour = gtk.Label("ThunderBorg LED colour")
        self.lblThunderBorgColour.set_use_markup(gtk.TRUE)
        self.lblThunderBorgColour.set_markup("\n<b><big><big>ThunderBorg LED colour\n</big></big></b>")
        self.vbxGrid.pack_start(self.lblThunderBorgColour, True, True, 0)
        self.lblThunderBorgColour.show()

        self.clrThunderBorg = gtk.ColorSelection()
        self.clrThunderBorg.set_has_palette(False)
        self.clrThunderBorg.connect("color_changed", self.ThunderBorgColourChanged)
        self.vbxGrid.pack_start(self.clrThunderBorg, True, True, 0)
        self.clrThunderBorg.show()

        self.lblThunderBorgLidColour = gtk.Label("Lid LED colour")
        self.lblThunderBorgLidColour.set_use_markup(gtk.TRUE)
        self.lblThunderBorgLidColour.set_markup("<b><big><big>\nLid LED colour\n</big></big></b>")
        self.vbxGrid.pack_start(self.lblThunderBorgLidColour, True, True, 0)
        self.lblThunderBorgLidColour.show()

        self.clrThunderBorgLid = gtk.ColorSelection()
        self.clrThunderBorgLid.set_has_palette(False)
        self.clrThunderBorgLid.connect("color_changed", self.ThunderBorgLidColourChanged)
        self.vbxGrid.pack_start(self.clrThunderBorgLid, True, True, 0)
        self.clrThunderBorgLid.show()

        self.vbxGrid.show()
        self.window.show()

    # Main loop
    def main(self):
        gtk.main()

    # ThunderBorg colour changed
    def ThunderBorgColourChanged(self, widget):
        global TB
        # Get current colour
        colour = self.clrThunderBorg.get_current_color()
        # Set the ThunderBorg colours
        TB.SetLed1(colour.red_float, colour.green_float, colour.blue_float)

    # ThunderBorg Lid colour changed
    def ThunderBorgLidColourChanged(self, widget):
        global TB
        # Get current colour
        colour = self.clrThunderBorgLid.get_current_color()
        # Set the ThunderBorg colours
        TB.SetLed2(colour.red_float, colour.green_float, colour.blue_float)

    # Exit event
    def destroy(self, widget, data=None):
        gtk.main_quit()

if __name__ == "__main__":
    app = ThunderBorgLed_gtk()
    app.main()
