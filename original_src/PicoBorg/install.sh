#!/bin/bash

# Grab the directory location of these examples
DPICO=`pwd`

# Setup Python GPIO ppackages, RPi.GPIO and wiringpi
sudo apt-get -y install python-dev python-setuptools python-rpi.gpio python-pip
sudo pip install wiringpi

# Change scripts to have execute permissions
chmod +x *.py

# Create desktop shortcut
PICO_SHORTCUT="${HOME}/Desktop/picoborg.desktop"
echo "[Desktop Entry]" > ${PICO_SHORTCUT}
echo "Encoding=UTF-8" >> ${PICO_SHORTCUT}
echo "Version=1.0" >> ${PICO_SHORTCUT}
echo "Type=Application" >> ${PICO_SHORTCUT}
echo "Exec=gksudo ${DPICO}/4dc_gui.py" >> ${PICO_SHORTCUT}
echo "Icon=${DPICO}/piborg.ico" >> ${PICO_SHORTCUT}
echo "Terminal=false" >> ${PICO_SHORTCUT}
echo "Name=PicoBorg Demo GUI" >> ${PICO_SHORTCUT}
echo "Comment=PicoBorg demonstration GUI for 3 drives + 1 PWM drive" >> ${PICO_SHORTCUT}
echo "Categories=Application;Development;" >> ${PICO_SHORTCUT}
