#!/bin/bash

DUB=`pwd`

echo '=== Installing prerequisites ==='
sudo apt-get update
sudo apt-get -y install i2c-tools python-smbus tix

echo '=== Removing I2C devices from the blacklisting ==='
sudo cp /etc/modprobe.d/raspi-blacklist.conf /etc/modprobe.d/raspi-blacklist.conf.old
sudo sed -i 's/^blacklist i2c-bcm2708/#\0    # We need this enabled for I2C add-ons, e.g. PicoBorg Reverse/g' /etc/modprobe.d/raspi-blacklist.conf

echo '=== Adding I2C devices to auto-load at boot time ==='
sudo cp /etc/modules /etc/modules.old
sudo sed -i '/^\s*i2c-dev\s*/d' /etc/modules
sudo sed -i '/^\s*i2c-bcm2708\s*/d' /etc/modules
sudo sed -i '/^#.*UltraBorg.*/d' /etc/modules
sudo bash -c "echo '' >> /etc/modules"
sudo bash -c "echo '# Kernel modules needed for I2C add-ons, e.g. UltraBorg' >> /etc/modules"
sudo bash -c "echo 'i2c-dev' >> /etc/modules"
sudo bash -c "echo 'i2c-bcm2708' >> /etc/modules"

echo '=== Adding user "pi" to the I2C permissions list ==='
sudo adduser pi i2c

echo '=== Make scripts executable ==='
chmod a+x *.py
chmod a+x *.sh

echo '=== Create a desktop shortcut for the GUI example ==='
UB_SHORTCUT="${HOME}/Desktop/UltraBorg.desktop"
echo "[Desktop Entry]" > ${UB_SHORTCUT}
echo "Encoding=UTF-8" >> ${UB_SHORTCUT}
echo "Version=1.0" >> ${UB_SHORTCUT}
echo "Type=Application" >> ${UB_SHORTCUT}
echo "Exec=${DUB}/ubGui.py" >> ${UB_SHORTCUT}
echo "Icon=${DUB}/piborg.ico" >> ${UB_SHORTCUT}
echo "Terminal=false" >> ${UB_SHORTCUT}
echo "Name=UltraBorg Demo GUI" >> ${UB_SHORTCUT}
echo "Comment=UltraBorg demonstration GUI" >> ${UB_SHORTCUT}
echo "Categories=Application;Development;" >> ${UB_SHORTCUT}

echo '=== Create a desktop shortcut for the tuning GUI ==='
UB_SHORTCUT="${HOME}/Desktop/UltraBorgTuning.desktop"
echo "[Desktop Entry]" > ${UB_SHORTCUT}
echo "Encoding=UTF-8" >> ${UB_SHORTCUT}
echo "Version=1.0" >> ${UB_SHORTCUT}
echo "Type=Application" >> ${UB_SHORTCUT}
echo "Exec=${DUB}/ubTuningGui.py" >> ${UB_SHORTCUT}
echo "Icon=${DUB}/piborg.ico" >> ${UB_SHORTCUT}
echo "Terminal=false" >> ${UB_SHORTCUT}
echo "Name=UltraBorg Tuning GUI" >> ${UB_SHORTCUT}
echo "Comment=UltraBorg Tuning GUI" >> ${UB_SHORTCUT}
echo "Categories=Application;Development;" >> ${UB_SHORTCUT}

echo '=== Finished ==='
echo ''
echo 'Your Raspberry Pi should now be setup for running UltraBorg'
echo 'Please restart your Raspberry Pi to ensure the I2C driver is running'
