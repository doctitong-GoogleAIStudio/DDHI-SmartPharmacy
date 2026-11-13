# Barcode Scanner Setup

## Optional and scalable
No scanner is required for startup, login, lookup or permitted inventory functions. Scanner profiles are stored centrally by workstation, with no hardcoded scanner or workstation count. Multiple HID scanners share keyboard focus and cannot be reliably distinguished by keystrokes alone.

## HID and Bluetooth
Configure the device as keyboard wedge, preserve ASCII group separator 29 for GS1 variable fields and configure its suffix. Scan into the barcode entry field. Do not report a device as connected merely because a profile exists. The application classifies HID keystrokes and manual entry as unverified. The hospital must approve the resulting manual dispensing workflow before use.

## Serial and images
COM diagnostic lists available serial ports, without claiming they are scanners. Read COM barcode captures carriage-return-terminated input at the chosen baud rate with a 15 second timeout. Test the actual scanner communication settings before acceptance. Decode barcode image supports readable image files via ZXing and records unverified image input. Vendor SDK integration is a disabled extension boundary pending vendor support.

## GS1 test procedure
Test valid EAN-8, EAN-13, UPC-A, GTIN-14, GS1-128 and GS1 DataMatrix. AI 01 identifies GTIN; AI 10 lot; AI 17 expiration; AI 21 serial. Verify FNC1/AIM prefix and group separator handling, invalid check digits, duplicate fields, missing optional fields and wrong lot/expiry. Batch and expiry must never be invented when absent. Internal codes use DDHI- prefix.

## Hardware acceptance record
For each workstation and device record model, firmware, interface, profile, suffix, GS separator output, scanned examples, unplug/reconnect behavior and operator approval. Test zero scanners, shared HID devices and multiple devices. Actual hardware testing has not been performed by the automated software tests.