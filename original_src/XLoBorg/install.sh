#!/bin/bash

echo '=== Installing prerequisites ==='
sudo apt-get -y install i2c-tools python-smbus

echo '=== Removing I2C devices from the blacklisting ==='
sudo cp /etc/modprobe.d/raspi-blacklist.conf /etc/modprobe.d/raspi-blacklist.conf.old
sudo sed -i 's/^blacklist i2c-bcm2708/#\0    # We need this enabled for I2C add-ons, e.g. XLoBorg/g' /etc/modprobe.d/raspi-blacklist.conf

echo '=== Adding I2C devices to auto-load at boot time ==='
sudo cp /etc/modules /etc/modules.old
sudo sed -i '/^\s*i2c-dev\s*/d' /etc/modules
sudo sed -i '/^\s*i2c-bcm2708\s*/d' /etc/modules
sudo sed -i '/^#.*XLoBorg.*/d' /etc/modules
sudo bash -c "echo '' >> /etc/modules"
sudo bash -c "echo '# Kernel modules needed for I2C add-ons, e.g. XLoBorg' >> /etc/modules"
sudo bash -c "echo 'i2c-dev' >> /etc/modules"
sudo bash -c "echo 'i2c-bcm2708' >> /etc/modules"

echo '=== Adding user "pi" to the I2C permissions list ==='
sudo adduser pi i2c

echo '=== Make scripts executable ==='
chmod a+x *.py

echo '=== Finished ==='
echo ''
echo 'Your Raspberry Pi should now be setup for running XLoBorg'
echo 'Please restart your Raspberry Pi to ensure the I2C driver is running'
