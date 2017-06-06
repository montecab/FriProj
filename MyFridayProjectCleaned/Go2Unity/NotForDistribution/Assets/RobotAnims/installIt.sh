#!/bin/bash

# run this after doit.sh if things seem sane.

# ./doit.sh
rm -rf ../../../../../ChromeIOS/ChromeIOSResource/firmware/resources/Files/AN_D*
cp -r output/Animations_OnRobot/AN* ../../../../../ChromeIOS/ChromeIOSResource/firmware/resources/Files/
cp output/Animations_InApp/*.txt ../../../Assets/Resources/RobotResources/Animations/

echo "Copied files to places."
echo
echo "If animation, sound, or default spark files have changed,"
echo "  then cd to ChromeIOS/ChromeIOSResource/firmware"
echo "  and Run these:"
echo "  ../firmwareTools/updateResourceFileListings.sh"
echo "  ../firmwareTools/currentBundleVersion.sh"
echo "  ../firmwareTools/replaceBundleVersion.sh 23 24"
echo "  Where 23 is the current resource version, and 24 is the resource version you want to change it to"
echo
echo "Now do a sanity check and CHECK IN THE CHANGES"
